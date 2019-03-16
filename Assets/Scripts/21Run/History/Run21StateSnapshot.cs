using System;
using FRG.Taco.Run21;

/// <summary>
/// Model used by <see cref="Run21StateSnapshotManager"/> to store deck/score info for given <see cref="Run21"/> instance.
/// </summary>
namespace FRG.Taco
{
    public class Run21StateSnapshot : ICloneable
    {
        public Run21Score Score { get; set; }
        public Deck DrawDeck { get; set; }
        public Deck Lane1 { get; set; }
        public Deck Lane2 { get; set; }
        public Deck Lane3 { get; set; }
        public Deck Lane4 { get; set; }
        public Deck ActiveDeck { get; set; }
        public int ScoredStreak { get; set; }
        public int RemainingCards { get; set; }
        public int BustedCardCount { get; set; }

        public int PlayedLaneIndex { get; set; }

        /// <summary>
        /// Factory method used to create <see cref="Run21StateSnapshot"/> from <see cref="Run21"/> game instance.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static Run21StateSnapshot From(Run21.Run21 game)
        {
            Run21StateSnapshot snapshot = new Run21StateSnapshot();

            snapshot.DrawDeck = game.DrawDeck.Clone();
            snapshot.ActiveDeck = game.ActiveCardDeck.Clone();
            snapshot.Lane1 = game.FirstLane.Clone();
            snapshot.Lane2 = game.SecondLane.Clone();
            snapshot.Lane3 = game.ThirdLane.Clone();
            snapshot.Lane4 = game.FourthLane.Clone();
            snapshot.Score = game.Score.Clone();
            snapshot.ScoredStreak = game.ScoredStreak;
            snapshot.RemainingCards = game.RemainingCards;
            snapshot.BustedCardCount = game.BustedCardCount;

            if (Gameplay.instance != null)
            {
                snapshot.PlayedLaneIndex = Gameplay.instance.laneToDealTo;
            }

            return snapshot;
        }

        /// <summary>
        /// Clones the <see cref="Run21StateSnapshot"/> instance it was called upon.
        /// </summary>
        public object Clone()
        {
            Run21StateSnapshot snapshotClone = new Run21StateSnapshot();

            snapshotClone.DrawDeck = DrawDeck.Clone();
            snapshotClone.ActiveDeck = ActiveDeck.Clone();
            snapshotClone.Lane1 = Lane1.Clone();
            snapshotClone.Lane2 = Lane2.Clone();
            snapshotClone.Lane3 = Lane3.Clone();
            snapshotClone.Lane4 = Lane4.Clone();
            snapshotClone.Score = Score.Clone();
            snapshotClone.ScoredStreak = ScoredStreak;
            snapshotClone.RemainingCards = RemainingCards;
            snapshotClone.BustedCardCount = BustedCardCount;
            snapshotClone.PlayedLaneIndex = PlayedLaneIndex;

            return snapshotClone;
        }

        public Deck GetLaneDeckByIndex(int laneIndex)
        {
            if (laneIndex == 0)
            {
                return Lane1;
            }

            if (laneIndex == 1)
            {
                return Lane2;
            }

            if (laneIndex == 2)
            {
                return Lane3;
            }

            if (laneIndex == 3)
            {
                return Lane4;
            }

            throw new ArgumentException($"Cannot resolve lane deck for index :{laneIndex}");
        }
    }
}