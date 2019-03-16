using UnityEditor;
using UnityEngine;

namespace FRG.Core
{
    [CustomEditor(typeof(GameViewResolution))]
    public class GameViewResolutionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var gameViewRes = target as GameViewResolution;

            if (GUILayout.Button("Apply All resolutions", EditorStyles.miniButton))
            {
                foreach (GameViewResolution.ResolutionEntry item in gameViewRes.resolutons)
                {
                    if (!GameViewUtils.SizeExists(GameViewSizeGroupType.Standalone, item.name))
                    {
                        Debug.LogError("GameViewUtils doesn't work with Unity 2018 (found online). Need to fix it or replace with another util that works.");
                        //GameViewUtils.AddCustomSize(
                        //    GameViewUtils.GameViewSizeType.FixedResolution,
                        //    GameViewSizeGroupType.Standalone,
                        //    item.width,
                        //    item.height,
                        //    item.name);
                    }
                }
            }

            DrawDefaultInspector();
        }
    }
}
