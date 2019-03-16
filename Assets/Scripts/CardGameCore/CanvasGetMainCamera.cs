using UnityEngine;

namespace FRG.Taco
{
    public class CanvasGetMainCamera : MonoBehaviour
    {
        [SerializeField]
        public Canvas canvas;

        void Awake()
        {
            if (canvas != null)
            {
                canvas.worldCamera = Camera.main;
            }
        }
    }
}