using System;

namespace Phase10_Narrative
{
    [Serializable]
    public sealed class P10NarrativeEvent
    {
        public P10NarrativeEventType EventType;
        public string ChapterId;
        public P10NarrativeState TargetState;
        public string NodeId;
        public string FlagKey;
        public bool FlagValue;
        public string OrderId;
        public int Score;
        public string Payload;

        public P10NarrativeEvent()
        {
            EventType = P10NarrativeEventType.None;
            ChapterId = string.Empty;
            TargetState = P10NarrativeState.None;
            NodeId = string.Empty;
            FlagKey = string.Empty;
            FlagValue = false;
            OrderId = string.Empty;
            Score = 0;
            Payload = string.Empty;
        }

        public P10NarrativeEvent(P10NarrativeEventType eventType)
            : this()
        {
            EventType = eventType;
        }
    }
}
