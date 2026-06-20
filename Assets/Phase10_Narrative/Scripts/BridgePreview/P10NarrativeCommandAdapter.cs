using System.Collections.Generic;

namespace Phase10_Narrative
{
    public sealed class P10NarrativeCommandRequest
    {
        public P10NarrativeCommandType CommandType { get; private set; }
        public string Payload { get; private set; }
        public string TargetId { get; private set; }
        public string NodeId { get; private set; }

        public P10NarrativeCommandRequest(P10NarrativeCommand command)
        {
            CommandType = command.CommandType;
            Payload = command.Payload;
            TargetId = command.TargetId;
            NodeId = command.NodeId;
        }
    }

    public sealed class P10NarrativeCommandAdapter
    {
        private readonly List<P10NarrativeCommand> receivedCommands = new List<P10NarrativeCommand>();
        private readonly List<P10NarrativeCommandRequest> requestHistory = new List<P10NarrativeCommandRequest>();

        public IReadOnlyList<P10NarrativeCommand> ReceivedCommands
        {
            get { return receivedCommands; }
        }

        public IReadOnlyList<P10NarrativeCommandRequest> RequestHistory
        {
            get { return requestHistory; }
        }

        public void Receive(P10NarrativeCommand command)
        {
            if (!TryCreateRequest(command, out P10NarrativeCommandRequest request))
            {
                return;
            }

            receivedCommands.Add(command);
            requestHistory.Add(request);
        }

        public bool TryCreateRequest(P10NarrativeCommand command, out P10NarrativeCommandRequest request)
        {
            request = null;

            if (command == null || !IsSupportedCommandType(command.CommandType))
            {
                return false;
            }

            request = new P10NarrativeCommandRequest(command);
            return true;
        }

        public void Attach(P10NarrativeCommandBus commandBus)
        {
            commandBus?.Subscribe(Receive);
        }

        public void Detach(P10NarrativeCommandBus commandBus)
        {
            commandBus?.Unsubscribe(Receive);
        }

        public void Clear()
        {
            receivedCommands.Clear();
            requestHistory.Clear();
        }

        public static bool IsSupportedCommandType(P10NarrativeCommandType commandType)
        {
            switch (commandType)
            {
                case P10NarrativeCommandType.NarrativePauseGameplay:
                case P10NarrativeCommandType.NarrativeResumeGameplay:
                case P10NarrativeCommandType.NarrativeRequestInputLock:
                case P10NarrativeCommandType.NarrativeReleaseInputLock:
                case P10NarrativeCommandType.NarrativeRequestOpenDialogue:
                case P10NarrativeCommandType.NarrativeFinishedBlockingSegment:
                    return true;
                default:
                    return false;
            }
        }
    }
}
