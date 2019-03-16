
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FRG.Core
{
    public class PrefabMonitor : UnityEditor.AssetModificationProcessor
    {
        static List<IPresaveCleanupHandler> _previewList;
        static List<GameObject> _gameObjects;

        [InitializeOnLoadMethod]
        private static void EditorInitialize()
        {
            using (ProfileUtil.PushSample("PrefabMonitor.EditorInitialize"))
            {
                PrefabUtility.prefabInstanceUpdated += PrefabUpdate;
            }
        }

        private static void PrefabUpdate(GameObject instance)
        {
            GameObject root = (GameObject)PrefabUtility.GetCorrespondingObjectFromSource(instance);
            if (root != null)
            {
                CleanPreviews(root);
                PoolObject.WarnIfInvalid(root);
            }
        }

        public static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (string path in paths)
            {
                if (PathUtil.IsSceneFile(path))
                {
                    ProcessScene(SceneManager.GetSceneByPath(path));
                }
                else if (PathUtil.IsPrefabFile(path))
                {
                    GameObject root = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
                    if (root != null)
                    {
                        CleanPreviews(root);
                        PoolObject.WarnIfInvalid(root);
                    }
                }
            }
            return paths;
        }

        private static void ProcessScene(Scene scene)
        {
            if (scene.IsValid())
            {
                if (_gameObjects == null) { _gameObjects = new List<GameObject>(); }
                _gameObjects.Clear();
                scene.GetRootGameObjects(_gameObjects);

                for (int j = 0; j < _gameObjects.Count; ++j)
                {
                    GameObject root = _gameObjects[j];
                    if (root != null)
                    {
                        CleanPreviews(root);
                        PoolObject.WarnIfInvalid(root);
                    }
                }
            }
        }

        private static void CleanPreviews(GameObject root)
        {
            if (_previewList == null) { _previewList = new List<IPresaveCleanupHandler>(); }
            _previewList.Clear();

            root.GetComponentsInChildren(true, _previewList);
            for (int i = 0; i < _previewList.Count; ++i)
            {
                IPresaveCleanupHandler preview = _previewList[i];
                UnityEngine.Object obj = preview as UnityEngine.Object;
                if (obj != null)
                {
                    preview.OnPresaveCleanup();
                    EditorUtility.SetDirty(obj);
                }
            }
        }
    }
}
