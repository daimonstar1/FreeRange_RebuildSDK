using UnityEngine;

[ExecuteInEditMode]
public class ColliderScaler : MonoBehaviour
{
    [SerializeField] BoxCollider collider;
    [SerializeField] RectTransform rectTransform;

    Resolution resolution;

    void Start()
    {
        resolution = Screen.currentResolution;
        ScaleCollider();
    }

#if UNITY_EDITOR
    void Update()
    {
        if (resolution.height != Screen.height || resolution.width != Screen.width)
        {
            ScaleCollider();
            resolution = Screen.currentResolution;
        }
    }
#endif

    void ScaleCollider()
    {
        collider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 10f);
    }
}
