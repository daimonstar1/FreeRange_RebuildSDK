using FRG.SharedCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FRG.Core
{
    public class GuidLookupWindow : EditorWindow
    {
        const int MaxDisplay = 64;
        private static readonly GUILayoutOption[] longlayoutOptions = new[] { GUILayout.Width(240) };
        private static readonly GUILayoutOption[] shortLayoutOptions = new[] { GUILayout.Width(80) };

        [SerializeField]
        private string uuid = "";
        [SerializeField]
        private string permanentIndex = "";
        [SerializeField]
        private string filter = "";
        [SerializeField]
        private Vector2 scrollPosition;

        [SerializeField]
        string savedUuid = "";
        [SerializeField]
        string savedPermanentIndex = "";
        [SerializeField]
        string savedFilter = "";
        [SerializeField]
        int resultCount = 0;
        [SerializeField]
        string[] resultUuids = ArrayUtil.Empty<string>();
        [SerializeField]
        UnityEngine.Object[] resultObjs = ArrayUtil.Empty<UnityEngine.Object>();

        public string Uuid {
            get {
                return uuid;
            }

            set {
                uuid = value;
            }
        }

        [MenuItem("FRG/Editor/Find Asset UUID", priority = 31)]


        private static void OpenUuidLookupWindow()
        {
            GetWindow(typeof(GuidLookupWindow), true, "Find Asset UUIDs", true);
        }

        [MenuItem("FRG/Editor/Print Selected UUIDs", priority = 31)]
        [MenuItem("Assets/FRG/Print Selected UUIDs", priority = 1031)]
        private static void PrintAssetUuids()
        {
            UnityEngine.Object[] selection = Selection.objects;

            StringBuilder builder = new StringBuilder();
            foreach (UnityEngine.Object obj in selection) {
                string uuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
                if (!string.IsNullOrEmpty(uuid)) {
                    builder.Append(obj.name);
                    builder.Append(": ");
                    builder.Append(uuid);
                }
            }
            if (builder.Length > 0) {
                Debug.Log(builder.ToString());
            }
        }

        [MenuItem("FRG/Editor/Print Selected UUIDs", validate = true)]
        [MenuItem("Assets/FRG/Print Selected UUIDs", validate = true)]
        private static bool PrintAssetUuidsValidation()
        {
            return Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets).Length > 0;
        }
        
        //[MenuItem("Assets/FRG/Serialize MER to Clipboard", priority = 1032)]
        //private static void CopySerializedMer()
        //{
        //    if (AssetDatabase.Contains(Selection.activeObject)) {
        //        var entry = AssetDatabase.LoadAssetAtPath<ExposedManagedEntry>(AssetDatabase.GetAssetPath(Selection.activeObject));
        //        if (entry != null) {
        //            string text;
        //            ExposedManagedEntry.SerializeReference(entry, out text);
        //            EditorGUIUtility.systemCopyBuffer = text;
        //        }
        //    }
        //}

        //[MenuItem("Assets/FRG/Serialize MER to Clipboard", validate = true)]
        //private static bool CopySerializedMerValidation()
        //{
        //    return Selection.activeObject != null 
        //        && (Selection.activeObject is ExposedManagedEntry) 
        //        && AssetDatabase.Contains(Selection.activeObject);
        //}

        [MenuItem("Assets/FRG/Copy Selected UUID To Clipboard", priority = 1032)]
        private static void CopyUuid()
        {
            if (AssetDatabase.Contains(Selection.activeObject)) {
                string uuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject));
                EditorGUIUtility.systemCopyBuffer = uuid;
            }
        }

        [MenuItem("Assets/FRG/Copy Selected UUID To Clipboard", validate = true)]
        private static bool CopyUuidValidation()
        {
            if (Selection.activeObject == null) {
                return false;
            }
            return AssetDatabase.Contains(Selection.activeObject);
        }
        private void OnEnable()
        {
            minSize = new Vector2(600, 420);
            maxSize = new Vector2(600, 420);
            position = new Rect(position.min, minSize);
        }

        //public override void DrawInspectorContent(EnhancedObject obj)
        //{
        //    using (EnhancedProperty uuidProp = obj.SpawnProperty("uuid"))
        //    using (EnhancedProperty databaseIdProp = obj.SpawnProperty("permanentIndex"))
        //    using (EnhancedProperty filterProp = obj.SpawnProperty("filter")) {
        //        EnhancedInspectorLayout.PropertyField(uuidProp);
        //        EnhancedInspectorLayout.PropertyField(databaseIdProp);
        //        EnhancedInspectorLayout.PropertyField(filterProp);
        //    }
        //    obj.ApplyModifiedProperties();

        //    CalculateResults();

        //    if (resultCount == 0) {
        //    }
        //    else if (resultUuids.Length == 0) {
        //        GUILayout.Label(resultCount.ToString() + " results (too many to show)");
        //    }
        //    else {
        //        GUILayout.Label(resultCount.ToString() + " results");

        //        using (EnhancedGUILayout.PushScrollView(ref scrollPosition)) {
        //            for (int i = 0; i < resultUuids.Length; ++i) {
        //                using (EnhancedGUILayout.PushHorizontal()) {
        //                    if (resultObjs[i] == null) {
        //                        GUILayout.TextField(AssetDatabase.GUIDToAssetPath(resultUuids[i]), longlayoutOptions);
        //                    }
        //                    else {
        //                        EditorGUILayout.ObjectField(resultObjs[i], typeof(UnityEngine.Object), false, longlayoutOptions);
        //                    }

        //                    Color color = GUI.contentColor;
        //                    int permanentIndex = EntryManagerEditor.FindRawPermanentIndex(BitUtil.ParseUuid(resultUuids[i]));
        //                    if (permanentIndex != PermanentAssetTable.EmptyPermanentIndex) {
        //                        GUI.contentColor = EnhancedInspector.MixedContentColor;
        //                    }
        //                    GUILayout.TextField(resultUuids[i], longlayoutOptions);

        //                    if (permanentIndex != PermanentAssetTable.EmptyPermanentIndex) {
        //                        ManagedEntry entry = resultObjs[i] as ManagedEntry;
        //                        if (entry != null && entry.StableOption == StableOption.ForbidStable) {
        //                            GUI.contentColor = EnhancedInspector.MixedContentColor;
        //                        }
        //                        else {
        //                            GUI.contentColor = color;
        //                        }

        //                        GUILayout.TextField(permanentIndex.ToString(), shortLayoutOptions);
        //                    }
        //                    else {
        //                        GUILayout.Space(80);
        //                    }

        //                    GUI.contentColor = color;
        //                }
        //            }
        //        }
        //    }
        //}

//        private void CalculateResults()
//        {
//            if (string.Equals(Uuid, savedUuid, StringComparison.Ordinal) &&
//                permanentIndex == savedPermanentIndex &&
//                string.Equals(filter, savedFilter, StringComparison.Ordinal)) {
//                return;
//            }

//            savedUuid = Uuid;
//            savedPermanentIndex = permanentIndex;
//            savedFilter = filter;

//            string uuidText = Uuid.Trim();
//            string filterText = filter.Trim();
//            int indexCompare;
//            int.TryParse(permanentIndex.Trim(), out indexCompare);

//            int indexModulo = 1;
//            if (indexCompare <= 0 || indexCompare > EntryManagerEditor.PermanentAssetTable.PermanentExposedAssetList.Length + PermanentAssetTable.ReservedPermanentIndexCount) {
//                indexCompare = 0;
//            }
//            else {
//                while (indexCompare > indexModulo) {
//                    indexModulo *= 10;
//                }
//            }

//            resultCount = 0;
//            resultUuids = ArrayUtil.Empty<string>();
//            resultObjs = ArrayUtil.Empty<UnityEngine.Object>();

//            if ((string.IsNullOrEmpty(uuidText) || uuidText.Length > 32) && string.IsNullOrEmpty(filterText) && indexCompare == 0) {
//                return;
//            }

//            if (indexCompare != 0) {
//                filterText = filterText + " t:ManagedEntry";
//            }

//            string[] uuids;
//            if (string.IsNullOrEmpty(filterText)) {
//                uuids = AssetDatabase.GetAllAssetPaths();
//                for (int i = 0; i < uuids.Length; ++i) {
//#if UNITY_5_5
//                    // Fix stupid warning
//                    if (System.IO.Path.IsPathRooted(uuids[i])) {
//                        continue;
//                    }
//#endif
//                    uuids[i] = AssetDatabase.AssetPathToGUID(uuids[i]);
//                }
//            }
//            else {
//                uuids = PathUtil.FindRawAssetUuids(filterText);
//            }

//            if (string.IsNullOrEmpty(uuidText) && indexCompare == 0) {
//                resultCount = uuids.Length;
//                if (resultCount <= MaxDisplay) {
//                    resultUuids = uuids;
//                }
//            }
//            else {
//                List<string> list = new List<string>(MaxDisplay);
//                foreach (string iter in uuids) {
//                    if (string.IsNullOrEmpty(iter)) { continue; }
//                    if (!string.IsNullOrEmpty(uuidText) && !iter.Contains(uuidText)) { continue; }

//                    Guid parsed;
//                    if (!BitUtil.TryParseUuid(iter, out parsed)) { continue; }

//                    if (indexCompare != 0) {
//                        int iterPermanentIndex = EntryManagerEditor.FindRawPermanentIndex(parsed);
//                        while (iterPermanentIndex > indexModulo) {
//                            if (iterPermanentIndex % indexModulo == indexCompare) {
//                                break;
//                            }
//                            iterPermanentIndex /= 10;
//                        }
//                        if (iterPermanentIndex % indexModulo != indexCompare) {
//                            continue;
//                        }
//                    }

//                    resultCount += 1;
//                    if (resultCount <= MaxDisplay) {
//                        list.Add(iter);
//                    }
//                }

//                if (resultCount <= MaxDisplay) {
//                    resultUuids = list.ToArray();
//                }
//            }

//            if (resultUuids.Length > 0) {
//                string[] paths = new string[resultUuids.Length];
//                for (int i = 0; i < resultUuids.Length; ++i) {
//                    string id = resultUuids[i];
//                    paths[i] = AssetDatabase.GUIDToAssetPath(id);
//                }

//                Array.Sort(paths, resultUuids);

//                resultObjs = new UnityEngine.Object[paths.Length];
//                for (int i = 0; i < paths.Length; ++i) {
//                    string path = paths[i];
//                    resultObjs[i] = AssetDatabase.LoadMainAssetAtPath(path);
//                }
//            }
//        }
    }
}
