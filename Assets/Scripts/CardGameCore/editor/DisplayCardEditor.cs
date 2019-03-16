using UnityEditor;
using UnityEngine;

namespace FRG.Taco
{
    [CustomEditor(typeof(DisplayCard))]
    public class DisplayCardEditor : Editor
    {
        Vector3 animTarget;

        public override void OnInspectorGUI()
        {
            var card = target as DisplayCard;

            animTarget = EditorGUILayout.Vector3Field("Anim target", animTarget);

            if (GUILayout.Button("Flip", EditorStyles.miniButton))
            {
                card.Flip();
            }

            if (GUILayout.Button("Move towards", EditorStyles.miniButton))
            {
                card.cardAnimationController.MoveTowardsAnimated(animTarget, null, 2);
            }

            if (GUILayout.Button("Print animation state", EditorStyles.miniButton))
            {
                Debug.Log($"card is IsAnimationPlaying: {card.cardAnimationController.IsAnimationPlaying()}");
            }

            DrawDefaultInspector();
        }
    }
}