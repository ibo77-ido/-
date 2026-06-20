using System;
using UnityEngine;

namespace Phase10_Narrative
{
    public enum P10NarrativeGameplayFactType
    {
        None,
        GameStarted,
        OrderCompleted,
        ScoreThresholdReached,
        DialogueLineStarted,
        NarrativeStateEntered,
        NarrativePropInspected
    }

    [Serializable]
    public sealed class P10NarrativeGameplayFact
    {
        public P10NarrativeGameplayFactType FactType;
        public string ChapterId;
        public string OrderId;
        public string NodeId;
        public string TargetId;
        public string Payload;
        public int Score;

        public P10NarrativeGameplayFact()
        {
            FactType = P10NarrativeGameplayFactType.None;
            ChapterId = string.Empty;
            OrderId = string.Empty;
            NodeId = string.Empty;
            TargetId = string.Empty;
            Payload = string.Empty;
            Score = 0;
        }
    }

    [Serializable]
    public sealed class P10NarrativeAnchorMapping
    {
        public string AnchorId;
        public Vector3 Position;

        public P10NarrativeAnchorMapping()
        {
            AnchorId = string.Empty;
            Position = Vector3.zero;
        }

        public P10NarrativeAnchorMapping(string anchorId, Vector3 position)
        {
            AnchorId = anchorId ?? string.Empty;
            Position = position;
        }
    }

    public interface P10NarrativeBridgePort
    {
        void SubmitGameplayFact(P10NarrativeGameplayFact fact);
        void ReceiveNarrativeCommand(P10NarrativeCommand command);
        void RegisterAnchorPosition(string anchorId, Vector3 position);
    }
}
