using UnityEditor;

[InitializeOnLoad]
public class SdkAddTags
{
    static SdkAddTags()
    {
        string[] tags = new[] {
            "SkillsCanvasTag",
            "pickup",
            "Yellow",
            "GameIcon",
            "RPValueText",
            "TacoValueText",
            "LeaderBoardMoneyType",
            "CreateTournamentMoneySign",
            "CashValueText",
            "AvatarInPanel",
            "ManageTournamentMoneyType",
            "Red"};

        TagManager.AddTags(tags);
    }
}