#if !GAME_SERVER
#if NET_2_0

using System.Collections.Generic;
using System.Diagnostics;

namespace System.Threading
{

    /// <summary>
    /// Compatibility class for use in Unity3D.
    /// </summary>
    /// <typeparam name="T">Any data type.</typeparam>
    public class ThreadLocal<T> : IDisposable
    {
        private Func<T> valueFactory;
        private bool trackAllValues;

        private object sync = new object();
        private Dictionary<Thread, T> valueLookup = new Dictionary<Thread, T>();

        private static bool isUnityLocalValueInitialized = false;
        private static T unityLocalValue = default(T);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public T Value
        {
            get
            {
                if (FRG.Core.ApplicationContext.IsUnityThreadCurrent)
                {
                    if (!isUnityLocalValueInitialized)
                    {
                        T value;
                        Thread current = Thread.CurrentThread;
                        lock (sync)
                        {
                            if (valueLookup.TryGetValue(current, out value))
                            {
                                unityLocalValue = value;
                            }
                            else
                            {
                                unityLocalValue = CreateValue();
                            }
                        }
                        isUnityLocalValueInitialized = true;
                    }

                    return unityLocalValue;
                }
                else
                {
                    lock (sync)
                    {
                        if (valueLookup == null) throw new ObjectDisposedException("ThreadLocal object was disposed.");

                        T value;
                        Thread current = Thread.CurrentThread;
                        if (!valueLookup.TryGetValue(current, out value))
                        {
                            value = CreateValue();
                            if (valueLookup.ContainsKey(current))
                            {
                                throw new InvalidOperationException("ThreadLocal.Value was created by factory method.");
                            }

                            valueLookup[current] = value;
                        }
                        return value;
                    }
                }
            }

            set
            {
                if (FRG.Core.ApplicationContext.IsUnityThreadCurrent)
                {
                    unityLocalValue = value;
                    isUnityLocalValueInitialized = true;
                }
                else
                {
                    lock (sync)
                    {
                        if (valueLookup == null) throw new ObjectDisposedException("ThreadLocal object was disposed.");

                        Thread current = Thread.CurrentThread;
                        valueLookup[current] = value;
                    }
                }
            }
        }

        public IList<T> Values
        {
            get
            {
                if (!trackAllValues) throw new InvalidOperationException();

                lock (sync)
                {
                    if (valueLookup == null) throw new ObjectDisposedException("ThreadLocal object was disposed.");

                    T[] values = new T[valueLookup.Count];
                    valueLookup.Values.CopyTo(values, 0);
                    return values;
                }
            }
        }

        public bool IsValueCreated
        {
            get
            {
                lock (sync)
                {
                    if (valueLookup == null) throw new ObjectDisposedException("ThreadLocal object was disposed.");

                    Thread current = Thread.CurrentThread;
                    return valueLookup.ContainsKey(current);
                }
            }
        }

        public ThreadLocal()
            : this(null, false)
        {
        }

        public ThreadLocal(bool trackAllValues)
            : this(null, trackAllValues)
        {
        }

        public ThreadLocal(Func<T> valueFactory)
            : this(valueFactory, false)
        {
        }

        public ThreadLocal(Func<T> valueFactory, bool trackAllValues)
        {
            this.valueFactory = valueFactory;
            this.trackAllValues = trackAllValues;
        }

        private T CreateValue()
        {
            if (valueFactory != null)
            {
                return valueFactory();
            }
            else
            {
                return default(T);
            }
        }

        public void Dispose()
        {
            lock(sync)
            {
                valueLookup = null;
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
#endif
#endif