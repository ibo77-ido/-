using System.Collections.Generic;

namespace Phase10_Narrative
{
    public sealed class P10NarrativeGameplayEventAdapter
    {
        private readonly List<P10NarrativeGameplayFact> factHistory = new List<P10NarrativeGameplayFact>();
        private readonly P10NarrativeEventBus eventBus;

        public P10NarrativeGameplayEventAdapter(P10NarrativeEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public IReadOnlyList<P10NarrativeGameplayFact> FactHistory
        {
            get { return factHistory; }
        }

        public bool TryConvert(P10NarrativeGameplayFact fact, out P10NarrativeEvent narrativeEvent)
        {
            narrativeEvent = null;

            if (fact == null)
            {
                return false;
            }

            P10NarrativeEventType eventType = ConvertEventType(fact.FactType);
            if (eventType == P10NarrativeEventType.None)
            {
                return false;
            }

            narrativeEvent = new P10NarrativeEvent(eventType)
            {
                ChapterId = fact.ChapterId,
                OrderId = fact.OrderId,
                NodeId = fact.NodeId,
                Payload = fact.Payload,
                Score = fact.Score
            };

            return true;
        }

        public bool PublishGameplayFact(P10NarrativeGameplayFact fact)
        {
            if (eventBus == null)
            {
                return false;
            }

            if (!TryConvert(fact, out P10NarrativeEvent narrativeEvent))
            {
                return false;
            }

            factHistory.Add(fact);
            eventBus.Publish(narrativeEvent);
            return true;
        }

        public void Clear()
        {
            factHistory.Clear();
        }

        public static bool IsSupportedFactType(P10NarrativeGameplayFactType factType)
        {
            return ConvertEventType(factType) != P10NarrativeEventType.None;
        }

        private static P10NarrativeEventType ConvertEventType(P10NarrativeGameplayFactType factType)
        {
            switch (factType)
            {
                case P10NarrativeGameplayFactType.GameStarted:
                    return P10NarrativeEventType.GameStarted;
                case P10NarrativeGameplayFactType.OrderCompleted:
                    return P10NarrativeEventType.OrderCompleted;
                case P10NarrativeGameplayFactType.ScoreThresholdReached:
                    return P10NarrativeEventType.ScoreThresholdReached;
                case P10NarrativeGameplayFactType.DialogueLineStarted:
                    return P10NarrativeEventType.DialogueLineStarted;
                case P10NarrativeGameplayFactType.NarrativeStateEntered:
                    return P10NarrativeEventType.NarrativeStateEntered;
                case P10NarrativeGameplayFactType.NarrativePropInspected:
                    return P10NarrativeEventType.NarrativePropInspected;
                default:
                    return P10NarrativeEventType.None;
            }
        }
    }
}
