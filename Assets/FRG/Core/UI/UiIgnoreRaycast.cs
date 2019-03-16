using UnityEngine;

namespace FRG.Core.UI
{

    public class UiIgnoreRaycast : MonoBehaviour, ICanvasRaycastFilter
    {
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            return false;
        }
    }
}
