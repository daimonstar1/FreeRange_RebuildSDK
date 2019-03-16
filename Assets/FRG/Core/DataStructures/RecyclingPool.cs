
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

namespace FRG.Core
{
    /// <summary>
    /// Reusing objects in both Unity3D and non-Unity contexts.
    /// Unity contexts must be single-threaded.
    /// </summary>
    public static class RecyclingPool
    {

        public const int MaxBufferCapacity = 8192;
        public const int MaxCollectionCapacity = 256;

        public static void Seed<T>(int count, Func<T> objectGenerator)
            where T : class
        {
            if (objectGenerator == null)
                throw new ArgumentNullException("objectGenerator");

            //using (ProfileUtil.PushSample("RecyclingPool.Seed"))
            {
                OrderedHashSet<object> set = CacheStatics.GetCache(typeof(T), true);
                for (int i = 0; i < count; i++)
                {
                    T obj = objectGenerator();
                    if (obj is IRecyclable)
                    {
                        if (!((IRecyclable)obj).Recycle())
                        {
                            continue;
                        }
                    }
                    // Lists are filled up to capacity right away so they don't spike while growing naturally
                    if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var genericType = typeof(T).GetGenericArguments()[0];
                        var propertyInfo = typeof(List<>).MakeGenericType(genericType).GetProperty("Capacity");
                        if (propertyInfo != null)
                        {
                            var setter = propertyInfo.GetSetMethod();
                            if (setter != null)
                            {
                                setter.Invoke(obj, CacheStatics.CachedArgument);
                            }
                        }
                    }
                    set.Add(obj);
                }
            }
        }

        public static void Seed<T>(int count)
            where T : class, new()
        {
            Seed<T>(count, () => { return new T(); });
        }

        public static void SeedList<T>(int count)
        {
            Seed<List<T>>(count, () => { return new List<T>(MaxCollectionCapacity); });
        }

        #region SpawnPooled

        public static Pooled<T> SpawnPooled<T>()
            where T : class, new()
        {
            return SpawnPooledInternal<T>(0);
        }

        public static Pooled<T> SpawnPooled<T>(int capacity)
            where T : class, ICapacity, new()
        {
            return SpawnPooledInternal<T>(capacity);
        }

        private static Pooled<T> SpawnPooledInternal<T>(int capacity)
            where T : class, new()
        {
            Pooled<T> pooled = new Pooled<T>(SpawnRaw<T>(), DespawnStatics.DespawnRaw);
            if (pooled.Value is ICapacity) {
                //using (ProfileUtil.PushSample("RecyclingPool.SpawnPooled (ICapacity.EnsureCapacity)"))
                {
                    ((ICapacity)pooled.Value).EnsureCapacity(Math.Max(capacity, GetDefaultCapacity(pooled.Value)));
                }
            }
            return pooled;
        }

        #endregion

        #region Stream Helpers

        /// <summary>
        /// Spawns an empty pooled memory stream.
        /// </summary>
        public static Pooled<MemoryStream> SpawnMemoryStream()
        {
            return SpawnPooled<ReusableMemoryStream>().DisownAs<MemoryStream>();
        }

        /// <summary>
        /// Spawns an empty pooled memory stream with optional capacity.
        /// </summary>
        /// <param name="capacity">The requested capacity to reserve. Will always stay about a maximum anyway.</param>
        public static Pooled<MemoryStream> SpawnMemoryStream(int capacity)
        {
            return SpawnPooled<ReusableMemoryStream>(capacity).DisownAs<MemoryStream>();
        }

        /// <summary>
        /// Spawns a memory stream with the specified buffer copied into the stream.
        /// </summary>
        public static Pooled<MemoryStream> SpawnMemoryStream(byte[] buffer)
        {
            Debug.Assert(buffer != null);

            return SpawnMemoryStream(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Spawns a memory stream with the specified buffer copied into the stream.
        /// </summary>
        public static Pooled<MemoryStream> SpawnMemoryStream(byte[] buffer, int startIndex, int length)
        {
            Debug.Assert(buffer != null);

            Pooled<ReusableMemoryStream> pooled = SpawnPooledInternal<ReusableMemoryStream>(length);
            pooled.Value.SetBuffer(buffer, startIndex, length);
            return pooled.DisownAs<MemoryStream>();
        }

        public static Pooled<MemoryStream> SpawnMemoryStream(Stream streamToCopy)
        {
            Debug.Assert(streamToCopy != null);

            MemoryStream memoryStream = streamToCopy as MemoryStream;
            if (memoryStream != null) {
                return SpawnMemoryStream(memoryStream.GetBuffer() ?? ArrayUtil.Empty<byte>(), (int)memoryStream.Position, (int)memoryStream.Length - (int)memoryStream.Position);
            }

            Pooled<ReusableMemoryStream> pooled = SpawnPooled<ReusableMemoryStream>();
            pooled.Value.CopyFrom(streamToCopy);
            return pooled.DisownAs<MemoryStream>();
        }

        /// <summary>
        /// Spawns a redirection stream that can switch what stream it points at.
        /// </summary>
        /// <param name="baseStream">The base stream to direct all operations to.</param>
        /// <param name="streamDestructor">The operation to run on the stream when the <see cref="Pooled{}"/> is disposed.</param>
        public static Pooled<RedirectionStream> SpawnRedirectionStream(Pooled<Stream> baseStream)
        {
            Pooled<RedirectionStream> redirectionStream = SpawnPooled<RedirectionStream>();
            redirectionStream.Value.TransferStream(baseStream);
            return redirectionStream;
        }

        /// <summary>
        /// Spawns a <see cref="StreamWriter"/> that is attached (indirectly) to the specified stream.
        /// </summary>
        /// <param name="baseStream">The stream to attach to. Will not necessary be the same as <see cref="StreamWriter.BaseStream"/>.</param>
        public static Pooled<StreamWriter> SpawnStreamWriter(Stream baseStream)
        {
            return GetPooledStreamWriter(GetRedirectionStreamUnsafe(baseStream));
        }

        /// <summary>
        /// Spawns a <see cref="StreamReader"/> that is attached (indirectly) to the specified stream.
        /// </summary>
        /// <param name="baseStream">The stream to attach to. Will not necessary be the same as <see cref="StreamReader.BaseStream"/>.</param>
        public static Pooled<StreamReader> SpawnStreamReader(Stream baseStream)
        {
            return GetPooledStreamReader(GetRedirectionStreamUnsafe(baseStream));
        }

        /// <summary>
        /// Spawns a <see cref="BinaryWriter"/> that is attached (indirectly) to the specified stream.
        /// </summary>
        /// <param name="baseStream">The stream to attach to. Will not necessary be the same as <see cref="BinaryWriter.BaseStream"/>.</param>
        public static Pooled<BinaryWriter> SpawnBinaryWriter(Stream baseStream)
        {
            return GetPooledBinaryWriter(GetRedirectionStreamUnsafe(baseStream));
        }

        /// <summary>
        /// Spawns a <see cref="BinaryReader"/> that is attached (indirectly) to the specified stream.
        /// </summary>
        /// <param name="baseStream">The stream to attach to. Will not necessary be the same as <see cref="cascadeClose.BaseStream"/>.</param>
        public static Pooled<BinaryReader> SpawnBinaryReader(Stream baseStream)
        {
            return GetPooledBinaryReader(GetRedirectionStreamUnsafe(baseStream));
        }

        /// <summary>
        /// Spawns a <see cref="StreamReader"/> with an initial buffer.
        /// </summary>
        /// <param name="buffer">A buffer to duplicate into the stream.</param>
        public static Pooled<StreamReader> SpawnStreamReader(byte[] buffer)
        {
            Debug.Assert(buffer != null);

            return GetPooledStreamReader(GetRedirectedMemoryStreamUnsafe(buffer, 0, buffer.Length));
        }

        /// <summary>
        /// Spawns a <see cref="StreamReader"/> with an initial buffer.
        /// </summary>
        /// <param name="buffer">A buffer to duplicate into the stream.</param>
        public static Pooled<StreamReader> SpawnStreamReader(byte[] buffer, int startIndex, int length)
        {
            return GetPooledStreamReader(GetRedirectedMemoryStreamUnsafe(buffer, startIndex, length));
        }

        /// <summary>
        /// Spawns a <see cref="BinaryReader"/> with an initial buffer.
        /// </summary>
        /// <param name="buffer">A buffer to duplicate into the stream.</param>
        public static Pooled<BinaryReader> SpawnBinaryReader(byte[] buffer)
        {
            Debug.Assert(buffer != null);

            return GetPooledBinaryReader(GetRedirectedMemoryStreamUnsafe(buffer, 0, buffer.Length));
        }

        /// <summary>
        /// Spawns a <see cref="BinaryReader"/> with an initial buffer.
        /// </summary>
        /// <param name="buffer">A buffer to duplicate into the stream.</param>
        public static Pooled<BinaryReader> SpawnBinaryReader(byte[] buffer, int startIndex, int length)
        {
            return GetPooledBinaryReader(GetRedirectedMemoryStreamUnsafe(buffer, startIndex, length));
        }

        #endregion

        #region Private Stream Helpers

        private static Pooled<RedirectionStream> SpawnRedirectedMemoryStream(byte[] buffer, int startIndex, int length)
        {
            Debug.Assert(length >= 0);

            Pooled<MemoryStream> stream = SpawnMemoryStream(buffer, startIndex, length);
            return SpawnRedirectionStream(stream.DisownAs<Stream>());
        }

        private static RedirectionStream GetRedirectionStreamUnsafe(Stream baseStream)
        {
            Pooled<RedirectionStream> redirectionStream = SpawnRedirectionStream(new Pooled<Stream>(baseStream, null));

            // Unsafe; doesn't dispose. Only OK if you're going to manually recycle
            return redirectionStream.UnsafeRelease();
        }

        private static RedirectionStream GetRedirectedMemoryStreamUnsafe(byte[] buffer, int startIndex, int length)
        {
            Pooled<RedirectionStream> redirectionStream = SpawnRedirectedMemoryStream(buffer, startIndex, length);

            // Unsafe; only for if you're going to manually recycle
            return redirectionStream.UnsafeRelease();
        }

        private static Pooled<StreamWriter> GetPooledStreamWriter(RedirectionStream redirectionStream)
        {
            return new Pooled<StreamWriter>(redirectionStream.StreamWriter, DespawnStatics.DespawnStreamWriter);
        }

        private static Pooled<StreamReader> GetPooledStreamReader(RedirectionStream redirectionStream)
        {
            return new Pooled<StreamReader>(redirectionStream.StreamReader, DespawnStatics.DespawnStreamReader);
        }

        private static Pooled<BinaryWriter> GetPooledBinaryWriter(RedirectionStream redirectionStream)
        {
            return new Pooled<BinaryWriter>(redirectionStream.BinaryWriter, DespawnStatics.DespawnBinaryWriter);
        }

        private static Pooled<BinaryReader> GetPooledBinaryReader(RedirectionStream redirectionStream)
        {
            return new Pooled<BinaryReader>(redirectionStream.BinaryReader, DespawnStatics.DespawnBinaryReader);
        }

        #endregion

        #region String Helpers

        /// <summary>
        /// Spawns a new <see cref="StringReader"/> that reads from the specified string.
        /// </summary>
        public static Pooled<StringReader> SpawnStringReader(string text)
        {
            Pooled<ReusableStringReader> reader = SpawnPooled<ReusableStringReader>();
            reader.Value.ResetString(text);
            return reader.DisownAs<StringReader>();
        }

        /// <summary>
        /// Spawns a new <see cref="StringReader"/> that reads from the specified string.
        /// </summary>
        public static Pooled<StringReader> SpawnStringReader(string text, int startPosition)
        {
            Pooled<ReusableStringReader> reader = SpawnPooled<ReusableStringReader>();
            reader.Value.ResetString(text, startPosition, (text ?? "").Length);
            return reader.DisownAs<StringReader>();
        }

        /// <summary>
        /// Spawns a new <see cref="StringReader"/> that reads from the specified string.
        /// </summary>
        public static Pooled<StringReader> SpawnStringReader(string text, int startPosition, int length)
        {
            Pooled<ReusableStringReader> reader = SpawnPooled<ReusableStringReader>();
            reader.Value.ResetString(text, startPosition, length);
            return reader.DisownAs<StringReader>();
        }

        /// <summary>
        /// Spawns a new <see cref="StringWriter"/>. Prefer this class to <see cref="StringBuilder"/> when not making a one-off string.
        /// </summary>
        public static Pooled<StringWriter> SpawnStringWriter()
        {
            return SpawnPooled<ReusableStringWriter>().DisownAs<StringWriter>();
        }

        /// <summary>
        /// Spawns a new <see cref="StringWriter"/>. Prefer this class to <see cref="StringBuilder"/> when not making a one-off string.
        /// </summary>
        public static Pooled<StringWriter> SpawnStringWriter(int capacity)
        {
            return SpawnPooled<ReusableStringWriter>(capacity).DisownAs<StringWriter>();
        }

        /// <summary>
        /// Spawns a new <see cref="StringWriter"/> that uses the specified <see cref="StringBuilder"/> as its backing buffer.
        /// </summary>
        public static Pooled<StringWriter> SpawnStringWriter(StringBuilder builder)
        {
            Pooled<ReusableStringWriter> writer = SpawnPooled<ReusableStringWriter>();
            writer.Value.ResetBuilder(builder);
            return writer.DisownAs<StringWriter>();
        }

        /// <summary>
        /// Spawns a new <see cref="StringBuilder"/>. You may want to use <see cref="SpawnStringWriter"/> instead.
        /// </summary>
        public static Pooled<StringBuilder> SpawnStringBuilder()
        {
            return SpawnStringBuilder(MaxBufferCapacity);
        }

        /// <summary>
        /// Spawns a new <see cref="StringBuilder"/>. You may want to use <see cref="SpawnStringWriter"/> instead.
        /// </summary>
        public static Pooled<StringBuilder> SpawnStringBuilder(int capacity)
        {
            int targetCapacity = Math.Max(capacity, MaxBufferCapacity);

            StringBuilder builder;
            if (!TrySpawnRaw<StringBuilder>(out builder)) {
                //using (ProfileUtil.PushSample("RecyclingPool.SpawnStringBuilder (noncached)"))
                {
                    builder = new StringBuilder(targetCapacity);
                }
            }
            else {
                // Just in case.
                builder.Length = 0;

                if (capacity > builder.Capacity) {
                    builder.EnsureCapacity(targetCapacity);
                }
            }
            return new Pooled<StringBuilder>(builder, DespawnStatics.DespawnRaw);
        }

        private static void RecycleStringBuilder(StringBuilder builder)
        {
            builder.Length = 0;
            if (builder.Capacity > MaxBufferCapacity) {
                builder.Capacity = 0;
            }
        }

        #endregion

        #region SpawnRaw

        /// <summary>
        /// Spawns a simple C# class, preferably one that implements <see cref="IRecyclable"/>.
        /// You do not absolutely need to call DespawnSimple(object) when done, but you should.
        /// </summary>
        /// <typeparam name="T">The C# class to spawn.</typeparam>
        /// <returns>The default-constructed or reused C# class instance.</returns>
        /// <remarks>You should prefer <see cref="SpawnPooled{T}"/>, except for lists.</remarks>
        public static T SpawnRaw<T>()
            where T : class, new()
        {
            T value;
            if (TrySpawnRaw<T>(out value)) {
                return value;
            }

            //using (ProfileUtil.PushSample("RecyclingPool.Spawn (noncached)"))
            //using (ProfileUtil.Profile("RecyclingPool.Spawn (noncached): " + ReflectionUtil.GetCSharpName(typeof(T))))
            {
                return new T();
            }
        }

        /// <summary>
        /// Attempts to spawn a recycled object. Returns null if there are none available.
        /// </summary>
        /// <remarks>You should prefer <see cref="SpawnPooled{T}"/>, except for lists.</remarks>
        public static bool TrySpawnRaw<T>(out T value)
            where T : class
        {

            OrderedHashSet<object> set = CacheStatics.GetCache(typeof(T), true);
            if (set.Count > 0)
            {
                T result = (T)set[set.Count - 1];
                set.RemoveAt(set.Count - 1);
                value = result;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Puts a simple C# class back into pool, preferably one that implements <see cref="IRecyclable"/>.
        /// Only call this method if you are sure nothing else references the object.
        /// You do not have to return all objects to the pool.
        /// </summary>
        /// <param name="obj">The object to reuse.</param>
        /// <remarks>
        /// <para>If it derives from <see cref="IRecyclable"/>, it may call <see cref="IRecyclable.Recycle"/>.</para>
        /// <para>On the client, it may only be called on the main Unity thread.</para>
        /// <para>Will likely not be reused on the server, though we may try locked or thread-local implementations.</para>
        /// </remarks>
        public static void DespawnRaw<T>(T obj)
            where T : class
        {
            DespawnRawInternal(obj);
        }

        public static void DespawnRawInternal(object obj)
        {
            if (ReferenceEquals(obj, null))
                return;

            bool attemptToRecycle;

            Type type = obj.GetType();
            OrderedHashSet<object> set = CacheStatics.GetCache(type, allowCreate: false);
            if (set == null)
            {
                Debug.LogError("Attempting to despawn an object of type \"" + ReflectionUtil.CSharpFullName(type) + "\" that was not created by the Pool.");

                attemptToRecycle = false;
            }
            // Was still in use...
            else if (set.Contains(obj))
            {
                Debug.LogError("Attempting to despawn an object of type \"" + ReflectionUtil.CSharpFullName(type) + "\" that is already in Pool.");

                attemptToRecycle = true;
            }
            else if (set.Count > CacheStatics.CacheCapacity)
            {
                // Too many unused; throw away.
                attemptToRecycle = false;
            }
            else
            {
                attemptToRecycle = true;
            }
            if (attemptToRecycle)
            {
                bool shouldReuse;

                if (obj is IRecyclable)
                {
                    // Don't reuse if it returns false
                    shouldReuse = ((IRecyclable)obj).Recycle();
                }
                else if (obj is ICollection)
                {
                    if (obj is IList)
                    {
                        // Don't recycle arrays
                        if (((IList)obj).IsFixedSize)
                        {
                            throw new ArgumentException("Cannot recycle an object of type " + ReflectionUtil.CSharpFullName(obj.GetType()) + ".", "obj");
                        }
                        ((IList)obj).Clear();
                        shouldReuse = true;
                    }
                    else if (obj is IDictionary)
                    {
                        ((IDictionary)obj).Clear();
                        shouldReuse = true;
                    }
                    else
                    {
                        Debug.Assert(false, "Can't recycle unknown collection type.");
                        shouldReuse = false;
                    }
                }
                else if (obj is StringBuilder)
                {
                    RecycleStringBuilder((StringBuilder)obj);
                    shouldReuse = true;
                }
                else if (obj is IDisposable)
                {
                    // Disposable types tend to be things we shouldn't recycle
                    Debug.Assert(false, "Can't recycle unknown disposable type.");
                    shouldReuse = false;
                }
                else
                {
                    // Allow unknown
                    shouldReuse = true;
                }

                if (shouldReuse)
                {
                    if (obj is ICapacity)
                    {
                        ICapacity capacitor = (ICapacity)obj;
                        if (capacitor.Capacity > GetDefaultCapacity(obj))
                        {
                            //using (ProfileUtil.PushSample("RecyclingPool.Despawn (ICapacity.TrimExcess)"))
                            {
                                capacitor.TrimExcess();
                            }
                        }
                    }

                    // May already exist, but that is a harmless no-op.
                    set.Add(obj);
                }
            }

            // Don't dispose. Some types now use IDisposable as a convenient way to despawn.
        }
        /// <summary>
        /// Loops through the list and despawns objects. List is cleared.
        /// </summary>
        public static void DespawnAll<T>(IList<T> list)
            where T : class
        {
            if (list != null) {
                //using (ProfileUtil.PushSample("RecyclingPool.DespawnAll"))
                {
                    for (int i = 0; i < list.Count; i++) {
                        if (list[i] != null) {
                            DespawnRaw(list[i]);
                        }
                    }
                    list.Clear();
                }
            }
        }

        /// <summary>
        /// Does DespawnAll then DespawnRaw
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void DespawnAllAndRaw<T>(IList<T> list)
            where T : class
        {
            DespawnAll(list);
            DespawnRaw(list);
        }
        #endregion

        public static int GetDefaultCapacity(object obj)
        {
            if (obj is ICollection) {
                return MaxCollectionCapacity;
            }
            else {
                return MaxBufferCapacity;
            }
        }

        private static class CacheStatics
        {
            public static readonly object[] CachedArgument = new object[] { 128 };

            private static readonly ThreadLocal<Dictionary<Type, OrderedHashSet<object>>> CacheLookup = new ThreadLocal<Dictionary<Type, OrderedHashSet<object>>>(() => new Dictionary<Type, OrderedHashSet<object>>());

            public const int CacheCapacity = 100;

            public static OrderedHashSet<object> GetCache(Type type, bool allowCreate)
            {
                Dictionary<Type, OrderedHashSet<object>> cacheLookup = CacheLookup.Value;

                OrderedHashSet<object> value;
                if (!cacheLookup.TryGetValue(type, out value)) {
                    if (allowCreate) {
                        value = new OrderedHashSet<object>();
                        cacheLookup.Add(type, value);
                    }
                }
                return value;
            }
        }

        private static class DespawnStatics
        {
            public static readonly Action<object> DespawnRaw = DespawnRawInternal;

            public static readonly Action<object> DespawnStreamWriter = writer => DespawnRawInternal(((StreamWriter)writer).BaseStream);
            public static readonly Action<object> DespawnStreamReader = reader => DespawnRawInternal(((StreamReader)reader).BaseStream);
            public static readonly Action<object> DespawnBinaryWriter = writer => DespawnRawInternal(((BinaryWriter)writer).BaseStream);
            public static readonly Action<object> DespawnBinaryReader = reader => DespawnRawInternal(((BinaryReader)reader).BaseStream);
        }
    }
}
