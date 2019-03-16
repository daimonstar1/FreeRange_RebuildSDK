using FRG.Core;
using GameTaco;
using UnityEngine;

namespace FRG.Taco
{
    [CreateAssetMenu]
    public class SdkData : ScriptableObject
    {
        [Tooltip("Game name for Game Taco SDK Config")]
        public string Name = "Friendly Wager 21 Run";
        [Tooltip("Game Icon for Game Taco SDK Config")]
        public Sprite Icon;
        [Tooltip("SiteID for Game Taco SDK Config (supplied by Game Taco)")]
        public string SiteId = "friendly_wager_21_run";
        [Tooltip("Game Id we got from Game Taco. (supplied by Game Taco)")]
        public string idGameName = "FW_21_RUN";
        public string versionOfGame = "1.0.0";

        public void DoSdkSetup(TacoSetup tacoConfig)
        {
            tacoConfig.gameScene = null;
            tacoConfig.gameName = Name;
            tacoConfig.GameIcon = Icon;
            tacoConfig.SiteId = SiteId;
            tacoConfig.idGameName = idGameName;
            tacoConfig.versionOfGame = versionOfGame;
        }
    }
}
