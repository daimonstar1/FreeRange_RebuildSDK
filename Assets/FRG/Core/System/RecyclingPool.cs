using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace VRC.Core
{
    /// <summary>
    /// Reusing objects in both Unity3D and non-Unity contexts.
    /// </summary>
    public static class RecyclingPool
    {
        public interface IRecyclable
        {
            /// <summary>
            /// Returns true if object can be recycled and put into pool.
            /// </summary>
            bool Recycle();
        }

        public const int MaxBufferCapacity = 8192;
        public const int MaxCollectionCapacity = 256;

        public static void Seed<T>(int count, Func<T> objectGenerator)
            where T : class
        {
            if (objectGenerator == null)
                throw new ArgumentNullException("objectGenerator");

            //Profiler.BeginSample("RecyclingPool.Seed");
            var typePool = CacheStatics.GetCache(typeof(T), true);
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
                typePool.Push(obj);
            }
            //Profiler.EndSample();
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

        private static void RecycleStringBuilder(StringBuilder builder)
        {
            builder.Length = 0;
            if (builder.Capacity > MaxBufferCapacity) {
                builder.Capacity = 0;
            }
        }

        /// <summary>
        /// Spawns a simple C# class, preferably one that implements <see cref="IRecyclable"/>.
        /// You do not absolutely need to call DespawnSimple(object) when done, but you should.
        /// </summary>
        /// <typeparam name="T">The C# class to spawn.</typeparam>
        /// <returns>The default-constructed or reused C# class instance.</returns>
        /// <remarks>You should prefer <see cref="SpawnPooled{T}"/>, except for lists.</remarks>
        public static T Spawn<T>()
            where T : class, new()
        {
            T value;
            if (TrySpawn<T>(out value)) {
                return value;
            }

            //Profiler.BeginSample("RecyclingPool.Spawn(noncached)");
            //Profiler.BeginSample("RecyclingPool.Spawn (noncached): " + typeof(T).Name);
            //Profiler.EndSample();
            return new T();
        }

        /// <summary>
        /// Attempts to spawn a recycled object. Returns null if there are none available.
        /// </summary>
        /// <remarks>You should prefer <see cref="SpawnPooled{T}"/>, except for lists.</remarks>
        public static bool TrySpawn<T>(out T value)
            where T : class
        {
            var typePool = CacheStatics.GetCache(typeof(T), true);
            if (typePool.Count > 0)
            {
                T result = (T)typePool.Pop();
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
        public static void Despawn<T>(T obj)
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
            var set = CacheStatics.GetCache(type, allowCreate: false);
            if (set == null)
            {
                Debug.LogError("Attempting to despawn an object of type \"" + type.Name + "\" that was not created by the Pool.");

                attemptToRecycle = false;
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
                            throw new ArgumentException("Cannot recycle an object of type " + obj.GetType().Name);
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
                    set.Push(obj);
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
                //Profiler.BeginSample("RecyclingPool.DespawnAll");
                for (int i = 0; i < list.Count; i++) {
                    if (list[i] != null) {
                        Despawn(list[i]);
                    }
                }
                list.Clear();
                //Profiler.EndSample();
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
            Despawn(list);
        }

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
            public const int CacheCapacity = 128;

            private static readonly Dictionary<Type, Stack<object>> CacheLookup = new Dictionary<Type, Stack<object>>();

            public static Stack<object> GetCache(Type type, bool allowCreate)
            {
                Stack<object> value;
                if (!CacheLookup.TryGetValue(type, out value)) {
                    if (allowCreate) {
                        value = new Stack<object>();
                        CacheLookup.Add(type, value);
                    }
                }
                return value;
            }
        }
    }
}