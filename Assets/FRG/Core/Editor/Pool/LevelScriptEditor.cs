
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FRG.Core {
    [InitializeOnLoad]
    [UnityEditor.CustomEditor(typeof(PoolObjectSpawner))]
    public class LevelScriptEditor : Editor {
        static bool prefsLoaded = false;
        static bool _spawnersInHierarchy = false;

        public static bool SpawnersInHierarchy {
            get {
                return _spawnersInHierarchy;
            }
            set
            {
                if( value != _spawnersInHierarchy ) {
                    _spawnersInHierarchy = value;
                    if( _spawnersInHierarchy ) {
                        EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemOnGUI;
                    } else {
                        EditorApplication.hierarchyWindowItemOnGUI -= HierarchyItemOnGUI;
                    }
                    EditorPrefs.SetBool( "SpawnersInHierarchy", _spawnersInHierarchy );
                }
            }
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            PoolObjectSpawner tar = target as PoolObjectSpawner;
            
            if(tar != null && tar.ElementPrefab != null) {
                var tree = GetSpawnerTree(tar, null, 0);
                if(tree != null) {
                    EditorGUILayout.BeginVertical();
                    DoInspectorGuiOnSpawnerNode(tree);
                    EditorGUILayout.EndVertical();
                }
            }
        }

        static Dictionary<int, bool> hasPoolObjectSpawner;

        /// <summary>
        /// draw spawn/despawn buttons in hierarchy for ease of use
        /// </summary>
        static LevelScriptEditor() {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemOnGUI;
            hasPoolObjectSpawner = new Dictionary<int, bool>();
        }

        static void HierarchyItemOnGUI( int instanceID, Rect selectionRect ) {
            if( !prefsLoaded ) {
                SpawnersInHierarchy = EditorPrefs.GetBool( "SpawnersInHierarchy", false );
            }

            GameObject go;
            bool hasSpawner;
            PoolObjectSpawner spawner;
            if( hasPoolObjectSpawner.ContainsKey( instanceID ) ) {
                hasSpawner = hasPoolObjectSpawner[instanceID];
                if( !hasSpawner ) return;
                go = EditorUtility.InstanceIDToObject( instanceID ) as GameObject;
                if( go == null ) return;
                spawner = go.GetComponent<PoolObjectSpawner>();
            } else {
                go = EditorUtility.InstanceIDToObject( instanceID ) as GameObject;
                if( go == null ) return;
                spawner = go.GetComponent<PoolObjectSpawner>();
                hasSpawner = (spawner != null);
                hasPoolObjectSpawner[instanceID] = hasSpawner;
                if( !hasSpawner ) return;
            }
            if( spawner != null && spawner.ElementPrefab != null ) {
                Rect buttonRect = new Rect( selectionRect );
                buttonRect.xMin = buttonRect.xMax - 20;
                if( spawner.IsSpawned ) {
                    if( GUI.Button( buttonRect, "x" ) ) {
                        spawner.ButtonDespawnPreviewWithoutSaving();
                    }
                    buttonRect.xMin -= 20;
                    buttonRect.xMax -= 20;
                    if( GUI.Button( buttonRect, "s" ) ) {
                        spawner.ButtonSaveAndDespawnPreviewRecursive();
                    }
                } else {
                    if( GUI.Button( buttonRect, "+" ) ) {
                        spawner.ButtonSpawnPreview();
                        // select child's child to open it up
                        if( spawner.Child != null && spawner.Child.gameObject != null ) {
                            Selection.activeGameObject = spawner.Child.gameObject;
                            if( spawner.Child.transform.childCount > 0 ) {
                                Selection.activeGameObject = spawner.Child.transform.GetChild( 0 ).gameObject;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Inspector draw function for a spawner tree structure generated from GetSpawnerTree()
        /// </summary>
        /// <param name="node">Top-most node of the spawner tree</param>
        public void DoInspectorGuiOnSpawnerNode(SpawnerNode node) {
            var oldBgColor = GUI.backgroundColor;

            try {
                GUI.backgroundColor = node.spawner == null || !node.spawner.IsSpawned ? new Color(0.85f, 0.85f, 0.85f, 1f) : new Color(0.65f, 0.65f, 0.65f, 1f);

                EditorGUILayout.BeginVertical();                                      //begin vertical main group
                
                if(node.spawner.IsSpawned) GUILayout.Label(node.spawner.name, EditorStyles.boldLabel);
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                buttonStyle.alignment = TextAnchor.MiddleLeft;
                labelStyle.alignment = TextAnchor.MiddleLeft;

                EditorGUILayout.BeginHorizontal();                                    //begin horizontal buttons group
                if(node.spawner != null && node.spawner.Child != null) {
                    if(GUILayout.Button("DESPAWN " + node.shorthandName, buttonStyle)) {
                        node.spawner.ButtonDespawnPreviewWithoutSaving();
                    }
                    if(GUILayout.Button("SAVE & Despawn")) {
                        node.spawner.ButtonSaveAndDespawnPreviewRecursive(); ///TODO: confirm that this saves
                    }
                } else if(!(node.spawner.ElementPrefab != null)) {
                    GUILayout.Label("Spawner " + node.shorthandName + " has no prefab set!", labelStyle);
                }
                else if (GUILayout.Button("SPAWN " + node.shorthandName, buttonStyle)) {
                    if(node.spawner != null) {
                        node.spawner.ButtonSpawnPreview();
                        if(node.spawner.Child != null && node.spawner.Child.gameObject != null)
                            EditorGUIUtility.PingObject(node.spawner.Child.gameObject);
                    }else{
                        Debug.LogWarning("No prefab to spawn...");
                    }
                }
                if(GUILayout.Button("Select", GUILayout.MaxWidth(100f))) {
                    Selection.objects = new UnityEngine.Object[] { node.spawner.gameObject };
                    EditorGUIUtility.PingObject(node.spawner.IsSpawned ? node.spawner.Child.gameObject : node.spawner.gameObject);
                }
                EditorGUILayout.EndHorizontal();                                       //end horizontal buttons group

                EditorGUILayout.BeginVertical();                                       //begin vertical children group
                foreach(var child in node.children) {                
                    EditorGUILayout.BeginHorizontal();                                 //begin horizontal child indent
                    GUILayout.Space((float)(20));
                    DoInspectorGuiOnSpawnerNode(child);
                    EditorGUILayout.EndHorizontal();                                   //end horizontal child indent
                }
                EditorGUILayout.EndVertical();                                         //end vertical children group

                if(node.spawner != null && node.spawner.IsSpawned) GUILayout.Space(8f);
                EditorGUILayout.EndVertical();                                         //end vertical main group
            } catch {
            } finally {
                GUI.backgroundColor = oldBgColor;
            }
        }

        //goes through children recursively, only to the depth of one spawner
        public List<PoolObjectSpawner> GetDirectChildSpawners(Transform trans) {

            List<PoolObjectSpawner> spawners = new List<PoolObjectSpawner>();
            for(int i=0;i<trans.childCount;i++) {
                Transform child = trans.GetChild(i);
                if(child == trans) continue;
                PoolObjectSpawner spawner = child.gameObject.GetComponent<PoolObjectSpawner>();
                if(spawner != null) {
                    spawners.Add(spawner);
                    continue;
                }
                List<PoolObjectSpawner> spawnersInChild = GetDirectChildSpawners(child);
                if(spawnersInChild == null || spawnersInChild.Count == 0) continue;
                spawners.AddRange(spawnersInChild);
            }

            return spawners;
        }
            

        /// <summary>
        /// Returns a tree structure representing spawner relationships starting from the specified spawner; Depth-first Recursive
        /// </summary>
        /// <param name="spawner">Spawner/leaf to start with</param>
        /// <param name="parent">Parent that owns this spawner; may be null</param>
        /// <param name="depth">Current depth of this spawner; 0 is the top-most parent spawner</param>
        public SpawnerNode GetSpawnerTree(PoolObjectSpawner spawner, SpawnerNode parent, int depth) {
            
            var ret = new SpawnerNode();
            ret.spawner = spawner;
            ret.depth = depth;

            // NOTE: BREAKING THIS BECAUSE NAME IS GOING AWAY
            ret.shorthandName = spawner.gameObject.name.Replace("Spawner_", "");
            //if(parent != null && ret.spawner.ElementPrefab != parent.spawner.ElementPrefab) {
            //    ret.shorthandName = (spawner.ElementPrefab.Name ?? "NULL").Replace(parent.spawner.ElementPrefab.Name, "...");
            //}else{
            //    ret.shorthandName = spawner.ElementPrefab.Name ?? "NULL";
            //}

            //find all child spawners for this spawner
            List<PoolObjectSpawner> subSpawners = GetDirectChildSpawners(spawner.transform);
            List<SpawnerNode> children = new List<SpawnerNode>();
            for(int i=0; i<subSpawners.Count; ++i) {
                children.Add(GetSpawnerTree(subSpawners[i], ret, depth + 1));
            }

            ret.children = children;

            return ret;
        }

        /// <summary>
        /// Node class representing a spawner and all spawners within the PoolObject that it spawns
        /// </summary>
        [Serializable]
        public class SpawnerNode {
            [SerializeField] public string shorthandName = "";
            [SerializeField] public int depth = 0;
            [SerializeField] public PoolObjectSpawner spawner = null;
            [SerializeField] public List<SpawnerNode> children = null;
        }
    }
}