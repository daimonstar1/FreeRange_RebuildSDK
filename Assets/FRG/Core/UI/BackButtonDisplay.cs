using UnityEngine;
using UnityEngine.UI;

namespace LabyrinthUI
{
    /// <summary>
    /// A UI wrapper so Button UI object gets hooked up to BackButton singleton.
    /// </summary>
    public class BackButtonDisplay : MonoBehaviour
    {
        [SerializeField]
        Button button;

        void OnEnable()
        {
            if (button == null) return;
            button.onClick.AddListener(BackButton.instance.ExecuteBackAction);
        }

        void OnDisable()
        {
            if (button == null) return;
            button.onClick.RemoveListener(BackButton.instance.ExecuteBackAction);
        }
    }
}