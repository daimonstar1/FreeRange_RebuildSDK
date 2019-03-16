
using UnityEngine;

namespace FRG.Core
{
    [AddComponentMenu(""), DisallowMultipleComponent]
    public sealed class FocusHandler : MonoBehaviour
    {
        private static bool isFocused = false;
        private static bool isShuttingDown = false;
        private static bool isQutting = false;
        private static FocusHandler savedInstance = null;

#if UNITY_EDITOR
        private static bool isTrackingPlayMode = false;
        private static bool wasPlayingOnLastChange = false;
        private static bool wasPausedOnLastChange = false;
#endif

        public static bool IsFocused
        {
            get
            {
                Prime();
                return isFocused;
            }
        }

        public static bool IsShuttingDown
        {
            get
            {
#if UNITY_EDITOR
                if (ApplicationContext.IsUnityThreadCurrent)
                {
                    PrimePlayModeTracking();
                    if (!Application.isPlaying) return true;
                }
                else
                {
                    if (!wasPlayingOnLastChange) return true;
                }
#endif
                if (isShuttingDown) return true;
                if (isQutting) return true;
                return false;
            }
        }

        private void Awake()
        {
            // Don't reset on destroy
            savedInstance = this;
        }

        private void OnDisable()
        {
            if (!IsShuttingDown)
            {
                isShuttingDown = true;

                UnityEngine.Debug.Log("Shutting down game.");
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            if (Application.isEditor)
            {
                isFocused = true;
            }
            else
            {
                isFocused = focus;
                //if (muteAudioListener) AudioListener.pause= !isFocused;
            }

            SyncTime.RequiresSync = true;
        }

        private void OnApplicationQuit()
        {
            isQutting = true;

            //UnityEngine.Debug.Log("Quitting game.");
        }

        //public static void CancelQuit()
        //{
        //    isQutting = false;
        //    Application.CancelQuit();
        //    UnityEngine.Debug.Log("Canceled game quit.");
        //}

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void Prime()
        {
            //LoggingConfigurator.Configure();

            using (ProfileUtil.PushSample("FocusHandler.Prime"))
            {
                if (savedInstance == null)
                {
                    savedInstance = ServiceLocator.ResolveRuntime<FocusHandler>();
                }

#if UNITY_EDITOR
                PrimePlayModeTracking();
#endif
            }
        }

#if UNITY_EDITOR
        private static void PrimePlayModeTracking()
        {
            if (!isTrackingPlayMode)
            {
                UnityEditor.EditorApplication.playModeStateChanged += PlayModeChanged;
                isTrackingPlayMode = true;
            }
        }

        private static void PlayModeChanged(UnityEditor.PlayModeStateChange pmsc)
        {
            bool isPlaying = Application.isPlaying;
            bool isPaused = UnityEditor.EditorApplication.isPaused;

            if (isPlaying && wasPlayingOnLastChange && !isPaused && !wasPausedOnLastChange)
            {
                if (!isShuttingDown)
                {
                    //UnityEngine.Debug.Log("Stopping play mode.");

                    isShuttingDown = true;
                }
            }
            if (isShuttingDown)
            {
                UnityEditor.EditorApplication.playModeStateChanged -= PlayModeChanged;
            }

            wasPlayingOnLastChange = isPlaying;
            wasPausedOnLastChange = isPaused;
        }

        [UnityEditor.InitializeOnLoadMethod]
        private static void Initialize()
        {
            //LoggingConfigurator.Configure();

            using (ProfileUtil.PushSample("FocusHandler.Initialize"))
            {
                OnScriptsReloaded();
                PrimePlayModeTracking();
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts(-900)]
        private static void OnScriptsReloaded()
        {
            if (UnityEditor.EditorApplication.isPlaying)
            {
                if (!isShuttingDown)
                {
                    UnityEngine.Debug.Log("Scripts reloaded; exiting PlayMode.");
                    isShuttingDown = true;
                }

                UnityEditor.EditorApplication.isPlaying = false;

                //DarkTonic.MasterAudio.MasterAudio instance = DarkTonic.MasterAudio.MasterAudio.Instance;
                //if (ReferenceEquals(instance, null)) { instance = (DarkTonic.MasterAudio.MasterAudio)GameObject.FindObjectOfType(typeof(DarkTonic.MasterAudio.MasterAudio)); }
                //if (!ReferenceEquals(instance, null)) { instance.disableLogging = true; }
            }
        }
#endif
    }
}
