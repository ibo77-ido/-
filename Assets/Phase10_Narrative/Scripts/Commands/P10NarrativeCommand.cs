using System;

namespace Phase10_Narrative
{
    [Serializable]
    public sealed class P10NarrativeCommand
    {
        public P10NarrativeCommandType CommandType;
        public string Payload;
        public string TargetId;
        public string NodeId;

        public P10NarrativeCommand()
        {
            CommandType = P10NarrativeCommandType.None;
            Payload = string.Empty;
            TargetId = string.Empty;
            NodeId = string.Empty;
        }

        public P10NarrativeCommand(P10NarrativeCommandType commandType)
            : this()
        {
            CommandType = commandType;
        }
    }
}
