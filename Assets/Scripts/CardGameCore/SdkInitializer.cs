
using UnityEngine;

namespace FRG.Taco
{
    public class SdkInitializer : MonoBehaviour
    {
        [SerializeField] SdkData configData;

        private void Awake()
        {
            if (configData == null)
            {
                Debug.LogError("SdkData not set. Please set it in inspector on SdkInitializer.");
                return;
            }

            if (GameTaco.TacoSetup.Instance == null)
            {
                GameTaco.TacoSetup tacoConfig = Instantiate(CardGameData.Instance.GameTacoPrefab).GetComponent<GameTaco.TacoSetup>();
                configData.DoSdkSetup(tacoConfig);
                tacoConfig.gameObject.AddComponent<GameTacoDelegate>();
            }
        }
    }
}