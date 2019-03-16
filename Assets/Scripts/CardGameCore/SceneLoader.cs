#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FRG.Core
{
    [ExecuteInEditMode]
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] string sceneName;
        [SerializeField] LoadSceneMode mode = LoadSceneMode.Additive;
        [SerializeField] bool loadAsync = false;
        [SerializeField] bool openInEditor = false;
        [SerializeField] bool setActive = false;

        void Awake()
        {
#if UNITY_EDITOR
            if (openInEditor && !Application.isPlaying)
            {
                OpenSceneInEditMode();
                return;
            }
#endif

            Util.LoadScene(sceneName, mode, loadAsync);
        }

#if UNITY_EDITOR
        Scene lastActiveScene;
        public void OpenSceneInEditMode()
        {
            if (Application.isPlaying) return;
            if (BuildPipeline.isBuildingPlayer) return;
            if (string.IsNullOrEmpty(sceneName)) return;

            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded) return;

            string scenePath = Util.GetScenePath_Editor(sceneName);
            if (scenePath == null)
            {
                Debug.LogError($"SceneLoader couldn't find scene to load by name { sceneName }. Pleae make sure tha scene is in scenes to build in player settings.");
                return;
            }

            lastActiveScene = SceneManager.GetActiveScene();
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            if (setActive)
            {
                // it's active by default I think
                Scene newScene = SceneManager.GetSceneByPath(scenePath);
                if (newScene.isLoaded)
                {
                    SceneManager.SetActiveScene(newScene);
                }
            }
            else
            {
                // at this point "active scene" is not loaded yet since (I assume)
                // we load another scene in Awake so first scene didn't call all
                // awakes yet and doesn't seem to be laoded.
                // So...delay restoring active scene for next update
                EditorApplication.update += UpdateOnce;
            }

        }

        private void UpdateOnce()
        {
            SceneManager.SetActiveScene(lastActiveScene);
            EditorApplication.update -= UpdateOnce;
        }
#endif
    }
}