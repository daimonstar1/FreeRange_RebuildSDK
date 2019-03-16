using UnityEngine;
using UnityEditor;
using System.Collections;
using FRG.SharedCore;
using System;
using System.Collections.Generic;

namespace FRG.Core {

    public class ClearPlayerPrefs {
        [MenuItem("FRG/Editor/Clear Player Prefs", priority = 0)]
        public static void DeleteAllPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }
        [MenuItem("FRG/Editor/Clear Editor Prefs", priority = 1)]
        public static void DeleteAllEditorPrefs()
        {
            if (EditorUtility.DisplayDialog("Delete all editor preferences?", "Editor preferences are shared among all projects. You cannot undo this action.", "Yes", "No"))
            {
                EditorPrefs.DeleteAll();
            }
        }

#if !UNITY_2017_1_OR_NEWER
        [MenuItem("FRG/Editor/Clear WWW Download Cache", priority = 2)]
		public static void DeleteAllCache()
		{
            Caching.CleanCache();
        }
#endif

        [MenuItem("FRG/Editor/Unload Unused Assets", priority = 3)]
        public static void ClearMemory()
        {
            GC.Collect();
            EditorUtility.UnloadUnusedAssetsImmediate();

            bool hasNullSelect = false;
            foreach (UnityEngine.Object obj in Selection.objects)
            {
                if (obj == null)
                {
                    hasNullSelect = true;
                    break;
                }
            }
            if (hasNullSelect)
            {
                List<UnityEngine.Object> objs = new List<UnityEngine.Object>();
                foreach (UnityEngine.Object obj in Selection.objects)
                {
                    if (obj != null)
                    {
                        objs.Add(obj);
                        break;
                    }
                }
                Selection.objects = objs.ToArray();
            }
        }
    }
}
