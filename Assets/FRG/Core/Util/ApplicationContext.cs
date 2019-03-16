
using System;
using System.Threading;
using UnityEngine;

namespace FRG.Core
{

    public static class ApplicationContext
    {
        // Not synchronized because it only needs to be non-null the thread that sets it.
        private static Thread unityThread;

        /// <summary>
        /// Returns true if the main unity thread has been initialized.
        /// May return false negatives on the wrong thread.
        /// </summary>
        public static bool IsUnityThreadInitialized { get { return (unityThread != null); } }

        /// <summary>
        /// Returns true if we are on the main Unity thread (after it has been initialized).
        /// </summary>
        public static bool IsUnityThreadCurrent
        {
            get
            {
                if (unityThread != null && Thread.CurrentThread == unityThread)
                {
                    ThrowUnlessUnityThreadIsCurrent();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Throws unless you're on the main Unity thread.
        /// Optimistic check. Very early on it may return false positives.
        /// </summary>
        public static void ThrowUnlessUnityThreadIsCurrent()
        {
            if (unityThread != null && unityThread != Thread.CurrentThread)
            {
                throw new InvalidOperationException("You must call this method from the main Unity3D thread.");
            }
        }

        /// <summary>
        /// This check should ensure that we are on the main Unity thread.
        /// It should throw if we are not.
        /// </summary>
        private static void VerifyOnMainUnityThread()
        {
            if (!(Application.isPlaying || Application.isEditor))
            {
                // Should be impossible to get here.
                throw new InvalidOperationException("Somehow (Application.isPlaying || Application.isEditor) is not true. You might be on the wrong thread.");
            }
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void CaptureUnityThread()
        {
            using (ProfileUtil.PushSample("ApplicationContext.CaptureUnityThread"))
            {
                VerifyOnMainUnityThread();

                if (unityThread != null && unityThread != Thread.CurrentThread)
                {
                    Debug.LogError(typeof(ApplicationContext) + " The main Unity thread has changed somehow. This is very unexpected.");
                }

                unityThread = Thread.CurrentThread;
            }
        }
    }
}


