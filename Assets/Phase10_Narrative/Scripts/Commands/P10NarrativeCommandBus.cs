using System;
using System.Collections.Generic;

namespace Phase10_Narrative
{
    public sealed class P10NarrativeCommandBus
    {
        private readonly List<P10NarrativeCommand> commandHistory = new List<P10NarrativeCommand>();

        public event Action<P10NarrativeCommand> CommandPublished;

        public IReadOnlyList<P10NarrativeCommand> CommandHistory
        {
            get { return commandHistory; }
        }

        public void Subscribe(Action<P10NarrativeCommand> handler)
        {
            CommandPublished += handler;
        }

        public void Unsubscribe(Action<P10NarrativeCommand> handler)
        {
            CommandPublished -= handler;
        }

        public void Publish(P10NarrativeCommand command)
        {
            if (command == null)
            {
                return;
            }

            commandHistory.Add(command);
            CommandPublished?.Invoke(command);
        }

        public void Clear()
        {
            commandHistory.Clear();
        }
    }
}
