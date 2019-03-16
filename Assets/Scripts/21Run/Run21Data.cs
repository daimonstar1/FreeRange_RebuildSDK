using FRG.Core;
using UnityEngine;

namespace FRG.Taco.Run21
{
    public class Run21Data : ScriptableObject
    {
        public static Run21Data Instance
        {
            get { return ServiceLocator.ResolveAsset<Run21Data>(); }
        }

        [SerializeField] public Run21Score.Scoring scoringData;
        [SerializeField] public Run21Score.ComboPoints comboPoints;

        [SerializeField] public PopupFactory popupFactory;
        
        [SerializeField] public AnimationConfig21Run animationConfig;

        [SerializeField] public Gameplay.Durations durations;
        [SerializeField] public float laneScoreScale;

        [SerializeField] public float outlinePulseDuration = 1f;
        [SerializeField] public Color bustLaneOutline;
        [SerializeField] public Color wildcardClearOutline;

    }
}