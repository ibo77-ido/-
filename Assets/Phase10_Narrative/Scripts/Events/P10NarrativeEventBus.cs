using System;
using System.Collections.Generic;

namespace Phase10_Narrative
{
    public sealed class P10NarrativeEventBus
    {
        private readonly List<P10NarrativeEvent> eventHistory = new List<P10NarrativeEvent>();

        public event Action<P10NarrativeEvent> EventPublished;

        public IReadOnlyList<P10NarrativeEvent> EventHistory
        {
            get { return eventHistory; }
        }

        public void Subscribe(Action<P10NarrativeEvent> handler)
        {
            EventPublished += handler;
        }

        public void Unsubscribe(Action<P10NarrativeEvent> handler)
        {
            EventPublished -= handler;
        }

        public void Publish(P10NarrativeEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            eventHistory.Add(evt);
            EventPublished?.Invoke(evt);
        }

        public void Clear()
        {
            eventHistory.Clear();
        }
    }
}
