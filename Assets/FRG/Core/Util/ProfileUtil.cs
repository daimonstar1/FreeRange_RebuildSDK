using System;
#if !GAME_SERVER
using UnityEngine;
using UnityEngine.Profiling;
using UnityObject = UnityEngine.Object;
#else
using UnityObject = FRG.SharedCore.SharedScriptableObject;
#endif

namespace FRG.Core
{
    public class ProfileUtil
    {

        public static SampleScope PushSample(string name)
        {
            return new SampleScope(name);
        }

        public static SampleScope PushSample(string name, UnityEngine.Object targetObject)
        {
            return new SampleScope(name, targetObject);
        }

        /// <summary>
        /// Calls <see cref="//Profiler.BeginSample(string, UnityEngine.Object)"/> on the client and
        /// <see cref="//Profiler.EndSample"/> on dispose, so you can use it in a using block. 
        /// </summary>
        public struct SampleScope : IDisposable
        {

#if ENABLE_PROFILER
          //  bool _disposed;
#endif

            public SampleScope(string name)
                : this(name, null)
            {
            }

            public SampleScope(string name, UnityEngine.Object targetObject)
            {
#if ENABLE_PROFILER
                //if(targetObject == null) {
                //    Profiler.BeginSample(name);
                //}
                //else {
                //    Profiler.BeginSample(name, targetObject);
                //}
         //       _disposed = false;
#endif
            }

            //[System.Runtime.CompilerServices.MethodImpl((System.Runtime.CompilerServices.MethodImplOptions)0x0100)]
            public void Dispose()
            {
#if ENABLE_PROFILER
                // Imperfect check because it relies on a mutable struct
                //if(!_disposed) {
                //    _disposed = true;
                //    Profiler.EndSample();
                //}
#endif
            }
        }
    }
}
