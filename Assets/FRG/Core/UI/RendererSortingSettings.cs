using UnityEngine;

public class RendererSortingSettings : MonoBehaviour
{
    [SerializeField] string sortingLayerName = "Default";
    [SerializeField] int sortingOrder = 0;

	void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            if (!string.IsNullOrEmpty(sortingLayerName))
                renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = sortingOrder;
        }
    }
}
