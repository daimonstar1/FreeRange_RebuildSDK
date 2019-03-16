using FRG.Core;
using UnityEngine;

namespace FRG.Taco
{
    public class CardGameData : ScriptableObject
    {
        public static CardGameData Instance
        {
            get { return ServiceLocator.ResolveAsset<CardGameData>(); }
        } 

        public DisplayCardFactory DisplayCardFactory;

        public DisplayDeckFactory DisplayDeckFactory;
        
        public GameObject GameTacoPrefab;

        [Tooltip(
            "Height of the plane cards will be moved to before animating drawing from deck to another deck. " + 
            "Needed to solve being occluded from other cards in same deck if going down, or occluded in destination deck if coming from down.")]
        public float CardDrawHeight = -100f;
    }
}