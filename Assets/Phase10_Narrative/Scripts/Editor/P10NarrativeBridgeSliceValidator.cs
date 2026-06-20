using System;
using UnityEditor;
using UnityEngine;

namespace Phase10_Narrative
{
    public static class P10NarrativeBridgeSliceValidator
    {
        private const string ChapterId = "P10_CH01";
        private const string PrologueNodeId = "P10_CH01_NODE_PROLOGUE_01";
        private const string DialogueTargetId = "P10_CH01_DialogueUIRoot";

        public static void RunP10B_05BridgeSliceValidation()
        {
            try
            {
                ValidateGameStartedToPrologueSlice();
                Debug.Log("P10B-05 End-to-End Bridge Slice validation passed.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10B-05 End-to-End Bridge Slice validation failed: " + ex.Message);
                EditorApplication.Exit(1);
            }
        }

        private static void ValidateGameStartedToPrologueSlice()
        {
            P10NarrativeEventBus eventBus = new P10NarrativeEventBus();
            P10NarrativeCommandBus commandBus = new P10NarrativeCommandBus();
            P10NarrativeStateMachine stateMachine = new P10NarrativeStateMachine();
            P10NarrativeGameplayEventAdapter gameplayEventAdapter = new P10NarrativeGameplayEventAdapter(eventBus);
            P10NarrativeCommandAdapter commandAdapter = new P10NarrativeCommandAdapter();

            eventBus.Subscribe(stateMachine.HandleEvent);
            eventBus.Subscribe(evt => RelayGameStartedToStateMachineEvents(evt, eventBus));
            commandAdapter.Attach(commandBus);

            P10NarrativeGameplayFact fact = new P10NarrativeGameplayFact
            {
                FactType = P10NarrativeGameplayFactType.GameStarted,
                ChapterId = ChapterId,
                NodeId = PrologueNodeId,
                Payload = "P10B_05_GAME_STARTED"
            };

            if (!gameplayEventAdapter.PublishGameplayFact(fact))
            {
                throw new InvalidOperationException("Gameplay fact did not enter Phase10 through adapter.");
            }

            if (gameplayEventAdapter.FactHistory.Count != 1)
            {
                throw new InvalidOperationException("Gameplay fact adapter history mismatch.");
            }

            if (eventBus.EventHistory.Count != 3)
            {
                throw new InvalidOperationException("Event bus history should contain GameStarted plus two state-machine request events.");
            }

            AssertEvent(eventBus.EventHistory[0], P10NarrativeEventType.GameStarted, ChapterId, PrologueNodeId);
            AssertEvent(eventBus.EventHistory[1], P10NarrativeEventType.ChapterStartRequested, ChapterId, string.Empty);
            AssertEvent(eventBus.EventHistory[2], P10NarrativeEventType.NodeAdvanceRequested, string.Empty, PrologueNodeId);

            if (stateMachine.GetCurrentState() != P10NarrativeState.Prologue)
            {
                throw new InvalidOperationException("State machine did not reach Prologue.");
            }

            if (stateMachine.GetCurrentNode() != PrologueNodeId)
            {
                throw new InvalidOperationException("State machine did not reach expected prologue node.");
            }

            P10NarrativeCommand command = new P10NarrativeCommand(P10NarrativeCommandType.NarrativeRequestOpenDialogue)
            {
                TargetId = DialogueTargetId,
                NodeId = PrologueNodeId,
                Payload = "P10B_05_OPEN_PROLOGUE_DIALOGUE"
            };

            commandBus.Publish(command);

            if (commandBus.CommandHistory.Count != 1)
            {
                throw new InvalidOperationException("Command bus did not record narrative command.");
            }

            if (commandAdapter.RequestHistory.Count != 1)
            {
                throw new InvalidOperationException("Command adapter did not create adapter-mediated request.");
            }

            P10NarrativeCommandRequest request = commandAdapter.RequestHistory[0];
            if (request.CommandType != P10NarrativeCommandType.NarrativeRequestOpenDialogue
                || request.TargetId != DialogueTargetId
                || request.NodeId != PrologueNodeId
                || request.Payload != "P10B_05_OPEN_PROLOGUE_DIALOGUE")
            {
                throw new InvalidOperationException("Command request field mismatch.");
            }
        }

        private static void RelayGameStartedToStateMachineEvents(P10NarrativeEvent evt, P10NarrativeEventBus eventBus)
        {
            if (evt == null || evt.EventType != P10NarrativeEventType.GameStarted)
            {
                return;
            }

            eventBus.Publish(new P10NarrativeEvent(P10NarrativeEventType.ChapterStartRequested)
            {
                ChapterId = evt.ChapterId
            });

            eventBus.Publish(new P10NarrativeEvent(P10NarrativeEventType.NodeAdvanceRequested)
            {
                NodeId = evt.NodeId
            });
        }

        private static void AssertEvent(
            P10NarrativeEvent evt,
            P10NarrativeEventType expectedType,
            string expectedChapterId,
            string expectedNodeId)
        {
            if (evt == null)
            {
                throw new InvalidOperationException("Expected event is null.");
            }

            if (evt.EventType != expectedType)
            {
                throw new InvalidOperationException("Unexpected event type.");
            }

            if (evt.ChapterId != expectedChapterId)
            {
                throw new InvalidOperationException("Unexpected event chapter id.");
            }

            if (evt.NodeId != expectedNodeId)
            {
                throw new InvalidOperationException("Unexpected event node id.");
            }
        }
    }
}
