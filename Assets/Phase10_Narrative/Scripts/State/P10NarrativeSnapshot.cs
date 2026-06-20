using System;
using System.Collections.Generic;

namespace Phase10_Narrative
{
    [Serializable]
    public sealed class P10NarrativeSnapshot
    {
        public int SnapshotVersion;
        public P10NarrativeState ChapterState;
        public string CurrentNodeId;
        public List<string> PlayedNodeIds;
        public Dictionary<string, bool> NarrativeFlags;
        public List<P10DialogueLogSnapshotEntry> DialogueLogEntries;

        public P10NarrativeSnapshot()
        {
            SnapshotVersion = 2;
            ChapterState = P10NarrativeState.None;
            CurrentNodeId = string.Empty;
            PlayedNodeIds = new List<string>();
            NarrativeFlags = new Dictionary<string, bool>();
            DialogueLogEntries = new List<P10DialogueLogSnapshotEntry>();
        }
    }

    [Serializable]
    public sealed class P10DialogueLogSnapshotEntry
    {
        public int Sequence;
        public string NodeId;
        public string SpeakerName;
        public string DialogueText;

        public P10DialogueLogSnapshotEntry()
        {
            Sequence = 0;
            NodeId = string.Empty;
            SpeakerName = string.Empty;
            DialogueText = string.Empty;
        }

        public P10DialogueLogSnapshotEntry(int sequence, string nodeId, string speakerName, string dialogueText)
        {
            Sequence = sequence;
            NodeId = nodeId ?? string.Empty;
            SpeakerName = speakerName ?? string.Empty;
            DialogueText = dialogueText ?? string.Empty;
        }
    }
}
