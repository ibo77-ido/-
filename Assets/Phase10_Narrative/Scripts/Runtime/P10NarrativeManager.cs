using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Phase10_Narrative
{
    public sealed class P10NarrativeManager : MonoBehaviour
    {
        private const string ChapterId = "P10_CH01";
        private const string NodePrologue = "P10_CH01_NODE_PROLOGUE_01";
        private const string NodeTutorial = "P10_CH01_NODE_TUTORIAL_01";
        private const string NodeOrder001Accept = "P10_CH01_NODE_ORDER_001_ACCEPT";
        private const string NodeOrder001Pass = "P10_CH01_NODE_ORDER_001_PASS";
        private const string NodeOrder003Accept = "P10_CH01_NODE_ORDER_003_ACCEPT";
        private const string NodeOrder003Pass = "P10_CH01_NODE_ORDER_003_PASS";
        private const string NodeOrder004Accept = "P10_CH01_NODE_ORDER_004_ACCEPT";
        private const string NodeOrder004PassNormal = "P10_CH01_NODE_ORDER_004_PASS_NORMAL";
        private const string NodeOrder004Climax = "P10_CH01_NODE_ORDER_004_CLIMAX";
        private const string NodeChapterEnding = "P10_CH01_NODE_CHAPTER_ENDING";
        private const string Order004ClimaxReadyFlag = "ORDER_004_CLIMAX_READY";

        private readonly P10NarrativeStateMachine stateMachine = new P10NarrativeStateMachine();
        private readonly P10NarrativeEventBus eventBus = new P10NarrativeEventBus();
        private readonly P10NarrativeCommandBus commandBus = new P10NarrativeCommandBus();
        private readonly List<string> debugLog = new List<string>();

        public IReadOnlyList<string> DebugLog
        {
            get { return debugLog; }
        }

        private void Awake()
        {
            eventBus.Subscribe(HandleNarrativeEvent);
            commandBus.Subscribe(HandleNarrativeCommand);
        }

        private void OnDestroy()
        {
            eventBus.Unsubscribe(HandleNarrativeEvent);
            commandBus.Unsubscribe(HandleNarrativeCommand);
        }

        public void PublishEvent(P10NarrativeEvent evt)
        {
            eventBus.Publish(evt);
        }

        public void PublishCommand(P10NarrativeCommand command)
        {
            commandBus.Publish(command);
        }

        public string GetCurrentNode()
        {
            return stateMachine.GetCurrentNode();
        }

        public P10NarrativeState GetCurrentState()
        {
            return stateMachine.GetCurrentState();
        }

        public P10NarrativeSnapshot SaveSnapshot()
        {
            P10NarrativeSnapshot snapshot = stateMachine.SaveSnapshot();
            P10DialogueController dialogueController = ResolveDialogueController();
            snapshot.DialogueLogEntries = dialogueController != null
                ? dialogueController.ExportDialogueLogSnapshotEntries()
                : new List<P10DialogueLogSnapshotEntry>();
            return snapshot;
        }

        public void LoadSnapshot(P10NarrativeSnapshot snapshot)
        {
            stateMachine.LoadSnapshot(snapshot);

            P10DialogueController dialogueController = ResolveDialogueController();
            if (dialogueController == null)
            {
                return;
            }

            if (snapshot != null && snapshot.SnapshotVersion >= 2 && snapshot.DialogueLogEntries != null)
            {
                dialogueController.ImportDialogueLogSnapshotEntries(snapshot.DialogueLogEntries);
                return;
            }

            dialogueController.ImportDialogueLogSnapshotEntries(null);
        }

        public bool HandleSceneTrigger(string triggerId, string targetNodeId)
        {
            if (!IsApprovedFirstPassTrigger(triggerId, targetNodeId))
            {
                AddLog("SceneTrigger blocked: unsupported trigger pair " + triggerId + " -> " + targetNodeId);
                return false;
            }

            if (string.IsNullOrWhiteSpace(targetNodeId))
            {
                AddLog("SceneTrigger blocked: empty targetNodeId");
                return false;
            }

            if (stateMachine.HasPlayedNode(targetNodeId) || string.Equals(stateMachine.GetCurrentNode(), targetNodeId, StringComparison.Ordinal))
            {
                AddLog("SceneTrigger ignored: duplicate node " + targetNodeId);
                return false;
            }

            if (!IsNextFirstPassNode(targetNodeId))
            {
                AddLog("SceneTrigger blocked: out of order " + triggerId + " -> " + targetNodeId + " after " + stateMachine.GetCurrentNode());
                return false;
            }

            bool advanced = false;

            switch (targetNodeId)
            {
                case NodePrologue:
                    advanced = MoveToState(P10NarrativeState.Prologue) && TryAdvanceNode(NodePrologue);
                    break;
                case NodeTutorial:
                    advanced = MoveToState(P10NarrativeState.Tutorial) && TryAdvanceNode(NodeTutorial);
                    break;
                case NodeOrder001Accept:
                    advanced = MoveToState(P10NarrativeState.Order001) && TryAdvanceNode(NodeOrder001Accept);
                    break;
                case NodeOrder001Pass:
                    advanced = MoveToState(P10NarrativeState.Order001) && TryAdvanceNode(NodeOrder001Pass);
                    break;
                case NodeOrder003Accept:
                    advanced = MoveToState(P10NarrativeState.Order003) && TryAdvanceNode(NodeOrder003Accept);
                    break;
                case NodeOrder003Pass:
                    advanced = MoveToState(P10NarrativeState.Order003) && TryAdvanceNode(NodeOrder003Pass);
                    break;
                case NodeOrder004Accept:
                    advanced = MoveToState(P10NarrativeState.Order004) && TryAdvanceNode(NodeOrder004Accept);
                    break;
                case NodeOrder004PassNormal:
                    advanced = MoveToState(P10NarrativeState.Order004) && TryAdvanceNode(NodeOrder004PassNormal);
                    break;
                case NodeOrder004Climax:
                    advanced = MoveToState(P10NarrativeState.Order004) && TryAdvanceNode(NodeOrder004Climax);
                    break;
                case NodeChapterEnding:
                    advanced = MoveToState(P10NarrativeState.Ending) && TryAdvanceNode(NodeChapterEnding);
                    if (advanced)
                    {
                        advanced = MoveToState(P10NarrativeState.Completed);
                    }
                    break;
                default:
                    AddLog("SceneTrigger blocked: unsupported targetNodeId " + targetNodeId);
                    return false;
            }

            AddLog(advanced
                ? "SceneTrigger accepted: " + triggerId + " -> " + targetNodeId
                : "SceneTrigger blocked: " + triggerId + " -> " + targetNodeId);
            return advanced;
        }

        private bool IsApprovedFirstPassTrigger(string triggerId, string targetNodeId)
        {
            if (string.IsNullOrWhiteSpace(triggerId))
            {
                return false;
            }

            switch (triggerId)
            {
                case "StartPrologue":
                    return targetNodeId == NodePrologue;
                case "Tutorial":
                    return targetNodeId == NodeTutorial;
                case "Order001Accept":
                    return targetNodeId == NodeOrder001Accept;
                case "Order001Pass":
                    return targetNodeId == NodeOrder001Pass;
                case "Order003Accept":
                    return targetNodeId == NodeOrder003Accept;
                case "Order003Pass":
                    return targetNodeId == NodeOrder003Pass;
                case "Order004Accept":
                    return targetNodeId == NodeOrder004Accept;
                case "Order004PassNormal":
                    return targetNodeId == NodeOrder004PassNormal;
                case "Order004Climax":
                    return targetNodeId == NodeOrder004Climax;
                case "ChapterEnding":
                    return targetNodeId == NodeChapterEnding;
                default:
                    return false;
            }
        }

        private bool IsNextFirstPassNode(string targetNodeId)
        {
            string currentNode = stateMachine.GetCurrentNode();

            switch (targetNodeId)
            {
                case NodePrologue:
                    return stateMachine.GetCurrentState() == P10NarrativeState.None && string.IsNullOrEmpty(currentNode);
                case NodeTutorial:
                    return currentNode == NodePrologue;
                case NodeOrder001Accept:
                    return currentNode == NodeTutorial;
                case NodeOrder001Pass:
                    return currentNode == NodeOrder001Accept;
                case NodeOrder003Accept:
                    return currentNode == NodeOrder001Pass;
                case NodeOrder003Pass:
                    return currentNode == NodeOrder003Accept;
                case NodeOrder004Accept:
                    return currentNode == NodeOrder003Pass;
                case NodeOrder004PassNormal:
                    return currentNode == NodeOrder004Accept;
                case NodeOrder004Climax:
                    return currentNode == NodeOrder004Accept;
                case NodeChapterEnding:
                    return currentNode == NodeOrder004PassNormal || currentNode == NodeOrder004Climax;
                default:
                    return false;
            }
        }

        public bool DebugReplayCurrentNode()
        {
            string nodeId = stateMachine.GetCurrentNode();

            if (!stateMachine.HasPlayedNode(nodeId))
            {
                AddLog("ReplayNode blocked: " + nodeId);
                return false;
            }

            stateMachine.HandleEvent(new P10NarrativeEvent(P10NarrativeEventType.NodeReplayRequested)
            {
                NodeId = nodeId
            });

            bool replayed = stateMachine.GetCurrentNode() == nodeId;
            AddLog(replayed ? "ReplayNode: " + nodeId : "ReplayNode blocked");
            return replayed;
        }

        public bool TryAdvanceDialogueNode(string currentNodeId, string targetNodeId)
        {
            if (string.IsNullOrWhiteSpace(currentNodeId) || string.IsNullOrWhiteSpace(targetNodeId))
            {
                AddLog("DialogueAdvance blocked: empty node id");
                return false;
            }

            if (!string.Equals(stateMachine.GetCurrentNode(), currentNodeId, StringComparison.Ordinal))
            {
                AddLog("DialogueAdvance blocked: stale current node " + currentNodeId);
                return false;
            }

            if (!CanAdvanceDialogueNode(currentNodeId, targetNodeId))
            {
                AddLog("DialogueAdvance blocked: " + currentNodeId + " -> " + targetNodeId);
                return false;
            }

            if (!MoveToDialogueNodeState(targetNodeId))
            {
                AddLog("DialogueAdvance blocked: state for " + targetNodeId);
                return false;
            }

            bool advanced = TryAdvanceNode(targetNodeId);
            AddLog(advanced
                ? "DialogueAdvance accepted: " + currentNodeId + " -> " + targetNodeId
                : "DialogueAdvance blocked: " + currentNodeId + " -> " + targetNodeId);
            return advanced;
        }
        private void HandleNarrativeEvent(P10NarrativeEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            AddLog("Event: " + evt.EventType);

            switch (evt.EventType)
            {
                case P10NarrativeEventType.GameStarted:
                    StartDebugChapter();
                    break;
                case P10NarrativeEventType.OrderCompleted:
                    HandleOrderCompleted(evt);
                    break;
                case P10NarrativeEventType.ScoreThresholdReached:
                    HandleScoreThreshold(evt.Score);
                    break;
                case P10NarrativeEventType.DialogueLineStarted:
                    AddLog("DialogueLineStarted: " + evt.NodeId);
                    break;
                case P10NarrativeEventType.NarrativeStateEntered:
                    AddLog("NarrativeStateEntered: " + evt.TargetState);
                    break;
                case P10NarrativeEventType.NarrativePropInspected:
                    AddLog("NarrativePropInspected: " + evt.Payload);
                    break;
                case P10NarrativeEventType.NodeAdvanceRequested:
                    if (!TryAdvanceDialogueNode(stateMachine.GetCurrentNode(), evt.NodeId))
                    {
                        AddLog("NodeAdvanceRequested blocked: " + evt.NodeId);
                    }
                    break;
                default:
                    stateMachine.HandleEvent(evt);
                    break;
            }
        }

        private void HandleNarrativeCommand(P10NarrativeCommand command)
        {
            if (command == null)
            {
                return;
            }

            AddLog("Command: " + command.CommandType);
        }

        private void StartDebugChapter()
        {
            stateMachine.HandleEvent(new P10NarrativeEvent(P10NarrativeEventType.ChapterStartRequested)
            {
                ChapterId = ChapterId
            });
            TryAdvanceNode(NodePrologue);
        }

        private void HandleOrderCompleted(P10NarrativeEvent evt)
        {
            string orderId = evt != null ? evt.OrderId : string.Empty;

            if (string.Equals(orderId, "ORDER_001", StringComparison.Ordinal))
            {
                if (stateMachine.GetCurrentState() == P10NarrativeState.Order001
                    && string.Equals(stateMachine.GetCurrentNode(), NodeOrder001Accept, StringComparison.Ordinal))
                {
                    TryAdvanceNode(NodeOrder001Pass);
                }
                else
                {
                    AddLog("OrderCompleted blocked: ORDER_001 requires accept node");
                }
                return;
            }

            if (string.Equals(orderId, "ORDER_003", StringComparison.Ordinal))
            {
                if (stateMachine.GetCurrentState() == P10NarrativeState.Order003
                    && string.Equals(stateMachine.GetCurrentNode(), NodeOrder003Accept, StringComparison.Ordinal))
                {
                    TryAdvanceNode(NodeOrder003Pass);
                }
                else
                {
                    AddLog("OrderCompleted blocked: ORDER_003 requires accept node");
                }
            }

            if (string.Equals(orderId, "ORDER_004", StringComparison.Ordinal))
            {
                if (stateMachine.GetCurrentState() != P10NarrativeState.Order004
                    || !string.Equals(stateMachine.GetCurrentNode(), NodeOrder004Accept, StringComparison.Ordinal))
                {
                    AddLog("OrderCompleted blocked: ORDER_004 requires accept node");
                    return;
                }

                TryAdvanceNode(IsOrder004ClimaxApproved(evt) ? NodeOrder004Climax : NodeOrder004PassNormal);
                return;
            }
        }

        private void HandleScoreThreshold(int score)
        {
            if (score < 95)
            {
                AddLog("ScoreThresholdReached ignored: " + score);
                return;
            }

            SetNarrativeFlag(Order004ClimaxReadyFlag, true);

            if (stateMachine.GetCurrentState() != P10NarrativeState.Order004
                || !string.Equals(stateMachine.GetCurrentNode(), NodeOrder004Accept, StringComparison.Ordinal))
            {
                AddLog("ScoreThresholdReached queued: ORDER_004 climax ready");
                return;
            }

            TryAdvanceNode(NodeOrder004Climax);
        }

        private bool MoveToState(P10NarrativeState targetState)
        {
            P10NarrativeState before = stateMachine.GetCurrentState();
            if (before == targetState)
            {
                AddLog("State already: " + targetState);
                return true;
            }

            if (!stateMachine.CanTransition(targetState))
            {
                AddLog("State blocked: " + targetState);
                return false;
            }

            stateMachine.HandleEvent(new P10NarrativeEvent(P10NarrativeEventType.StateAdvanceRequested)
            {
                TargetState = targetState
            });

            bool accepted = stateMachine.GetCurrentState() == targetState;
            AddLog(accepted
                ? "State entered: " + stateMachine.GetCurrentState()
                : "State blocked: " + targetState);
            return accepted;
        }

        private bool TryAdvanceNode(string nodeId)
        {
            string before = stateMachine.GetCurrentNode();
            stateMachine.HandleEvent(new P10NarrativeEvent(P10NarrativeEventType.NodeAdvanceRequested)
            {
                NodeId = nodeId
            });

            bool accepted = before != stateMachine.GetCurrentNode();
            AddLog(accepted
                ? "Node entered: " + stateMachine.GetCurrentNode()
                : "Node blocked: " + nodeId);
            return accepted;
        }

        private bool CanAdvanceDialogueNode(string currentNodeId, string targetNodeId)
        {
            if (string.IsNullOrWhiteSpace(currentNodeId) || string.IsNullOrWhiteSpace(targetNodeId))
            {
                return false;
            }

            switch (currentNodeId)
            {
                case NodePrologue:
                    return stateMachine.GetCurrentState() == P10NarrativeState.Prologue
                        && targetNodeId == NodeTutorial;
                case NodeTutorial:
                    return stateMachine.GetCurrentState() == P10NarrativeState.Tutorial
                        && targetNodeId == NodeOrder001Accept;
                case NodeOrder001Accept:
                    return false;
                case NodeOrder001Pass:
                    return stateMachine.GetCurrentState() == P10NarrativeState.Order001
                        && targetNodeId == NodeOrder003Accept;
                case NodeOrder003Accept:
                    return false;
                case NodeOrder003Pass:
                    return stateMachine.GetCurrentState() == P10NarrativeState.Order003
                        && targetNodeId == NodeOrder004Accept;
                case NodeOrder004Accept:
                    return false;
                case NodeOrder004PassNormal:
                case NodeOrder004Climax:
                    return stateMachine.GetCurrentState() == P10NarrativeState.Order004
                        && targetNodeId == NodeChapterEnding;
                default:
                    return false;
            }
        }

        private bool MoveToDialogueNodeState(string targetNodeId)
        {
            switch (targetNodeId)
            {
                case NodePrologue:
                    return MoveToState(P10NarrativeState.Prologue);
                case NodeTutorial:
                    return MoveToState(P10NarrativeState.Tutorial);
                case NodeOrder001Accept:
                case NodeOrder001Pass:
                    return MoveToState(P10NarrativeState.Order001);
                case NodeOrder003Accept:
                case NodeOrder003Pass:
                    return MoveToState(P10NarrativeState.Order003);
                case NodeOrder004Accept:
                case NodeOrder004PassNormal:
                case NodeOrder004Climax:
                    return MoveToState(P10NarrativeState.Order004);
                case NodeChapterEnding:
                    return MoveToState(P10NarrativeState.Ending);
                default:
                    return false;
            }
        }
        private void AddLog(string line)
        {
            debugLog.Add(line);

            if (debugLog.Count > 40)
            {
                debugLog.RemoveAt(0);
            }
        }

        private bool IsOrder004ClimaxApproved(P10NarrativeEvent evt)
        {
            if (evt != null && evt.Score >= 95)
            {
                return true;
            }

            return stateMachine.TryGetFlag(Order004ClimaxReadyFlag, out bool isReady) && isReady;
        }

        private void SetNarrativeFlag(string flagKey, bool value)
        {
            stateMachine.HandleEvent(new P10NarrativeEvent(P10NarrativeEventType.FlagChanged)
            {
                FlagKey = flagKey,
                FlagValue = value
            });
        }

        private P10DialogueController ResolveDialogueController()
        {
            P10DialogueController controller = GetComponentInChildren<P10DialogueController>();
            if (controller != null)
            {
                return controller;
            }

            return FindObjectOfType<P10DialogueController>();
        }
    }
}
