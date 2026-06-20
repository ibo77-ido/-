using System;

namespace Phase10_Narrative
{
    public enum P10CH01GameplayFactType
    {
        None,
        GameStarted,
        DialogueCompleted,
        TutorialCraftCompleted,
        OrderAccepted,
        OrderSubmitted,
        OrderCompleted,
        RewardGranted,
        ChapterEndingRequested
    }

    [Serializable]
    public sealed class P10CH01GameplayFact
    {
        public P10CH01GameplayFactType FactType;
        public string ChapterId;
        public string OrderId;
        public string NodeId;
        public string Result;
        public int Score;

        public P10CH01GameplayFact()
        {
            FactType = P10CH01GameplayFactType.None;
            ChapterId = string.Empty;
            OrderId = string.Empty;
            NodeId = string.Empty;
            Result = string.Empty;
            Score = 0;
        }
    }
}
