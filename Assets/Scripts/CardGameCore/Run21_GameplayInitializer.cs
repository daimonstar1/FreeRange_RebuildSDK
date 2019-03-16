namespace FRG.Taco
{
    using UnityEngine;

    public class Run21_GameplayInitializer : MonoBehaviour
    {
        private void Start()
        {
            GameTaco.TacoSetup.Instance.ToggleTacoHeaderFooter(false);
        }
    }
}