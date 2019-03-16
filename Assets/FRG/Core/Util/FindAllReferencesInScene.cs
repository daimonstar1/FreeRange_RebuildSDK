using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace FRG.Core
{
    public static class FindAllReferencesInScene
    {

        class FoundItem
        {
            public GameObject referencingObject;
            public SerializedProperty referencingProperty;
        }

        [MenuItem("GameObject/FRG/Find References In Scene", false, 0)]
        //Finds all objects in active scenes that reference the currently selected object. They are printed to the log line by line so you can click on them and find the object.
        static void FindReferencesInScene()
        {
            List<FoundItem> referencingObjects = null;
            try
            {
                referencingObjects = Find(Selection.activeGameObject, true);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            if (referencingObjects == null || referencingObjects.Count == 0)
            {
                Debug.Log("No referencing objects found for " + Selection.activeGameObject.name, Selection.activeGameObject);
                return;
            }

            Debug.Log("Found " + referencingObjects.Count + " objects that reference " + Selection.activeGameObject.name, Selection.activeGameObject);
            for (int i = 0; i < referencingObjects.Count; i++)
            {
                Debug.Log((i + 1) + ". " + referencingObjects[i].referencingObject.name + " (" + referencingObjects[i].referencingProperty.serializedObject.targetObject.GetType().Name + " : " + referencingObjects[i].referencingProperty.displayName + ")" , referencingObjects[i].referencingObject);
            }

        }

        static List<FoundItem> Find(GameObject referencedObject, bool ShowProgressBar = false)
        {
            if (referencedObject == null)
                return null;

            EditorUtility.DisplayCancelableProgressBar("Searching for " + referencedObject.name + " references", "", 0);

            List<FoundItem> foundObjects = new List<FoundItem>();

            var scene = referencedObject.scene;
            var rootObjects = scene.GetRootGameObjects();
            for (int j = 0; j < rootObjects.Length; j++)
            {
                if (ShowProgressBar)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Searching for " + referencedObject.name + " references in " + scene.name, "", j / (float)rootObjects.Length))
                        return foundObjects;
                }
                FindReferencesInHierarchy(rootObjects[j], referencedObject, foundObjects);
            }
            return foundObjects;
        }

        static void FindReferencesInHierarchy(GameObject root, GameObject referencedObject, List<FoundItem> foundObjects)
        {
            var components = root.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                    continue;
                SerializedObject so = new SerializedObject(components[i]);
                var it = so.GetIterator();
                while (it.NextVisible(true))
                {
                    if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue != null)
                    {
                        
                        GameObject go = it.objectReferenceValue as GameObject;
                        if (go == null && it.objectReferenceValue is Component)
                            go = (it.objectReferenceValue as Component).gameObject;
                        if(go == null)
                            continue;
                        if (go == referencedObject)
                        {
                            foundObjects.Add(new FoundItem
                            {
                                referencingObject = root,
                                referencingProperty = it
                            });
                            //We only need to mark this object once, doesn't matter if it's referenced multiple times
                            goto searchChildObjects; //Can't break out of double loop, using GOTO
                        }
                    }
                }
            }
            searchChildObjects:
            int count = root.transform.childCount;
            for (int i = 0; i < count; i++)
            {
                var t = root.transform.GetChild(i);
                if (t == null || t.gameObject == null) //Check if object is being deleted
                    continue;
                //Recurse over the children
                FindReferencesInHierarchy(root.transform.GetChild(i).gameObject, referencedObject, foundObjects);
            }
        }
    }
}
#endif