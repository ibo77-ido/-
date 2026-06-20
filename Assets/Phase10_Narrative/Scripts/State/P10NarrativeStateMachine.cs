using System;
using System.Collections.Generic;

namespace Phase10_Narrative
{
    public sealed class P10NarrativeStateMachine
    {
        private const int CurrentSnapshotVersion = 2;

        private readonly HashSet<string> playedNodeIds = new HashSet<string>();
        private readonly Dictionary<string, bool> narrativeFlags = new Dictionary<string, bool>();

        private string chapterId = string.Empty;
        private string currentNodeId = string.Empty;
        private P10NarrativeState chapterState = P10NarrativeState.None;

        public void StartChapter(string chapterId)
        {
            ResetChapter();
            this.chapterId = chapterId ?? string.Empty;
            AdvanceToState(P10NarrativeState.Prologue);
        }

        public void HandleEvent(P10NarrativeEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            switch (evt.EventType)
            {
                case P10NarrativeEventType.ChapterStartRequested:
                    StartChapter(evt.ChapterId);
                    break;
                case P10NarrativeEventType.StateAdvanceRequested:
                    AdvanceToState(evt.TargetState);
                    break;
                case P10NarrativeEventType.NodeAdvanceRequested:
                    AdvanceToNode(evt.NodeId);
                    break;
                case P10NarrativeEventType.NodeReplayRequested:
                    ReplayNode(evt.NodeId);
                    break;
                case P10NarrativeEventType.FlagChanged:
                    SetFlag(evt.FlagKey, evt.FlagValue);
                    break;
                case P10NarrativeEventType.ChapterResetRequested:
                    ResetChapter();
                    break;
            }
        }

        public bool CanTransition(P10NarrativeState targetState)
        {
            if (targetState == chapterState)
            {
                return true;
            }

            return GetNextState(chapterState) == targetState;
        }

        public bool AdvanceToState(P10NarrativeState targetState)
        {
            if (!CanTransition(targetState))
            {
                return false;
            }

            chapterState = targetState;
            return true;
        }

        public bool AdvanceToNode(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                return false;
            }

            if (currentNodeId == nodeId || playedNodeIds.Contains(nodeId))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(currentNodeId))
            {
                playedNodeIds.Add(currentNodeId);
            }

            currentNodeId = nodeId;
            return true;
        }

        public bool ReplayNode(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId) || !playedNodeIds.Contains(nodeId))
            {
                return false;
            }

            currentNodeId = nodeId;
            return true;
        }

        public void ResetChapter()
        {
            chapterId = string.Empty;
            currentNodeId = string.Empty;
            chapterState = P10NarrativeState.None;
            playedNodeIds.Clear();
            narrativeFlags.Clear();
        }

        public P10NarrativeSnapshot SaveSnapshot()
        {
            return new P10NarrativeSnapshot
            {
                SnapshotVersion = CurrentSnapshotVersion,
                ChapterState = chapterState,
                CurrentNodeId = currentNodeId,
                PlayedNodeIds = new List<string>(playedNodeIds),
                NarrativeFlags = new Dictionary<string, bool>(narrativeFlags),
                DialogueLogEntries = new List<P10DialogueLogSnapshotEntry>()
            };
        }

        public void LoadSnapshot(P10NarrativeSnapshot snapshot)
        {
            ResetChapter();

            if (snapshot == null)
            {
                return;
            }

            chapterState = snapshot.ChapterState;
            currentNodeId = snapshot.CurrentNodeId ?? string.Empty;

            if (snapshot.PlayedNodeIds != null)
            {
                foreach (string nodeId in snapshot.PlayedNodeIds)
                {
                    if (!string.IsNullOrWhiteSpace(nodeId))
                    {
                        playedNodeIds.Add(nodeId);
                    }
                }
            }

            if (snapshot.NarrativeFlags != null)
            {
                foreach (KeyValuePair<string, bool> flag in snapshot.NarrativeFlags)
                {
                    if (!string.IsNullOrWhiteSpace(flag.Key))
                    {
                        narrativeFlags[flag.Key] = flag.Value;
                    }
                }
            }
        }

        public string GetCurrentNode()
        {
            return currentNodeId;
        }

        public P10NarrativeState GetCurrentState()
        {
            return chapterState;
        }

        public bool HasPlayedNode(string nodeId)
        {
            return !string.IsNullOrWhiteSpace(nodeId) && playedNodeIds.Contains(nodeId);
        }

        public string GetChapterId()
        {
            return chapterId;
        }

        public bool TryGetFlag(string flagKey, out bool value)
        {
            if (string.IsNullOrWhiteSpace(flagKey))
            {
                value = false;
                return false;
            }

            return narrativeFlags.TryGetValue(flagKey, out value);
        }

        private void SetFlag(string flagKey, bool value)
        {
            if (string.IsNullOrWhiteSpace(flagKey))
            {
                return;
            }

            narrativeFlags[flagKey] = value;
        }

        private static P10NarrativeState GetNextState(P10NarrativeState state)
        {
            switch (state)
            {
                case P10NarrativeState.None:
                    return P10NarrativeState.Prologue;
                case P10NarrativeState.Prologue:
                    return P10NarrativeState.Tutorial;
                case P10NarrativeState.Tutorial:
                    return P10NarrativeState.Order001;
                case P10NarrativeState.Order001:
                    return P10NarrativeState.Order003;
                case P10NarrativeState.Order003:
                    return P10NarrativeState.Order004;
                case P10NarrativeState.Order004:
                    return P10NarrativeState.Ending;
                case P10NarrativeState.Ending:
                    return P10NarrativeState.Completed;
                default:
                    return P10NarrativeState.Completed;
            }
        }
    }
}
