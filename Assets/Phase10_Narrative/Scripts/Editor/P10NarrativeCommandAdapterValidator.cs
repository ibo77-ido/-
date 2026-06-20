using System;
using UnityEditor;
using UnityEngine;

namespace Phase10_Narrative
{
    public static class P10NarrativeCommandAdapterValidator
    {
        public static void RunP10B_03CommandAdapterValidation()
        {
            try
            {
                ValidateSupportedCommandsCreateRequests();
                ValidateRejectsInvalidCommands();
                ValidateCommandBusRemainsPublisherBoundary();
                Debug.Log("P10B-03 Narrative Command Adapter validation passed.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10B-03 Narrative Command Adapter validation failed: " + ex.Message);
                EditorApplication.Exit(1);
            }
        }

        private static void ValidateSupportedCommandsCreateRequests()
        {
            P10NarrativeCommandAdapter adapter = new P10NarrativeCommandAdapter();

            AssertRequest(adapter, P10NarrativeCommandType.NarrativePauseGameplay);
            AssertRequest(adapter, P10NarrativeCommandType.NarrativeResumeGameplay);
            AssertRequest(adapter, P10NarrativeCommandType.NarrativeRequestInputLock);
            AssertRequest(adapter, P10NarrativeCommandType.NarrativeReleaseInputLock);
            AssertRequest(adapter, P10NarrativeCommandType.NarrativeRequestOpenDialogue);
            AssertRequest(adapter, P10NarrativeCommandType.NarrativeFinishedBlockingSegment);

            if (adapter.ReceivedCommands.Count != 6)
            {
                throw new InvalidOperationException("Received command count mismatch.");
            }

            if (adapter.RequestHistory.Count != 6)
            {
                throw new InvalidOperationException("Request history count mismatch.");
            }
        }

        private static void ValidateRejectsInvalidCommands()
        {
            P10NarrativeCommandAdapter adapter = new P10NarrativeCommandAdapter();

            if (adapter.TryCreateRequest(null, out P10NarrativeCommandRequest nullRequest) || nullRequest != null)
            {
                throw new InvalidOperationException("Null command was not rejected.");
            }

            P10NarrativeCommand unsupportedCommand = new P10NarrativeCommand(P10NarrativeCommandType.None);

            if (adapter.TryCreateRequest(unsupportedCommand, out P10NarrativeCommandRequest unsupportedRequest)
                || unsupportedRequest != null)
            {
                throw new InvalidOperationException("Unsupported command was not rejected.");
            }

            adapter.Receive(null);
            adapter.Receive(unsupportedCommand);

            if (adapter.ReceivedCommands.Count != 0 || adapter.RequestHistory.Count != 0)
            {
                throw new InvalidOperationException("Invalid commands were recorded.");
            }

            if (P10NarrativeCommandAdapter.IsSupportedCommandType(P10NarrativeCommandType.None))
            {
                throw new InvalidOperationException("Unsupported command type reported as supported.");
            }
        }

        private static void ValidateCommandBusRemainsPublisherBoundary()
        {
            P10NarrativeCommandBus commandBus = new P10NarrativeCommandBus();
            P10NarrativeCommandAdapter adapter = new P10NarrativeCommandAdapter();
            adapter.Attach(commandBus);

            P10NarrativeCommand command = CreateCommand(P10NarrativeCommandType.NarrativePauseGameplay);
            commandBus.Publish(command);

            if (commandBus.CommandHistory.Count != 1)
            {
                throw new InvalidOperationException("Command bus did not record published command.");
            }

            if (adapter.RequestHistory.Count != 1)
            {
                throw new InvalidOperationException("Adapter did not create request from published command.");
            }

            adapter.Detach(commandBus);
            commandBus.Publish(CreateCommand(P10NarrativeCommandType.NarrativeResumeGameplay));

            if (commandBus.CommandHistory.Count != 2)
            {
                throw new InvalidOperationException("Command bus publish history mismatch after detach.");
            }

            if (adapter.RequestHistory.Count != 1)
            {
                throw new InvalidOperationException("Adapter received command after detach.");
            }
        }

        private static void AssertRequest(
            P10NarrativeCommandAdapter adapter,
            P10NarrativeCommandType commandType)
        {
            P10NarrativeCommand command = CreateCommand(commandType);

            if (!P10NarrativeCommandAdapter.IsSupportedCommandType(commandType))
            {
                throw new InvalidOperationException("Supported command type reported as unsupported: " + commandType);
            }

            if (!adapter.TryCreateRequest(command, out P10NarrativeCommandRequest request))
            {
                throw new InvalidOperationException("Request creation failed: " + commandType);
            }

            AssertRequestFields(request, commandType);
            adapter.Receive(command);
        }

        private static P10NarrativeCommand CreateCommand(P10NarrativeCommandType commandType)
        {
            return new P10NarrativeCommand(commandType)
            {
                Payload = "P10B_03_TEST_PAYLOAD",
                TargetId = "P10B_03_TARGET",
                NodeId = "P10_CH01_NODE_ORDER_001_PASS"
            };
        }

        private static void AssertRequestFields(
            P10NarrativeCommandRequest request,
            P10NarrativeCommandType expectedCommandType)
        {
            if (request == null)
            {
                throw new InvalidOperationException("Request is null.");
            }

            if (request.CommandType != expectedCommandType
                || request.Payload != "P10B_03_TEST_PAYLOAD"
                || request.TargetId != "P10B_03_TARGET"
                || request.NodeId != "P10_CH01_NODE_ORDER_001_PASS")
            {
                throw new InvalidOperationException("Request field copy mismatch.");
            }
        }
    }
}
