using System;
using UnityEditor;
using UnityEngine;

namespace Phase10_Narrative
{
    public static class P10NarrativeGameplayEventAdapterValidator
    {
        public static void RunP10B_02GameplayFactAdapterValidation()
        {
            try
            {
                ValidateRequiredFactConversions();
                ValidateRejectsInvalidFacts();
                ValidatePublishRequiresEventBus();
                Debug.Log("P10B-02 Gameplay Fact Adapter validation passed.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10B-02 Gameplay Fact Adapter validation failed: " + ex.Message);
                EditorApplication.Exit(1);
            }
        }

        private static void ValidateRequiredFactConversions()
        {
            P10NarrativeEventBus eventBus = new P10NarrativeEventBus();
            P10NarrativeGameplayEventAdapter adapter = new P10NarrativeGameplayEventAdapter(eventBus);

            AssertConversion(adapter, P10NarrativeGameplayFactType.GameStarted, P10NarrativeEventType.GameStarted);
            AssertConversion(adapter, P10NarrativeGameplayFactType.OrderCompleted, P10NarrativeEventType.OrderCompleted);
            AssertConversion(adapter, P10NarrativeGameplayFactType.ScoreThresholdReached, P10NarrativeEventType.ScoreThresholdReached);
            AssertConversion(adapter, P10NarrativeGameplayFactType.DialogueLineStarted, P10NarrativeEventType.DialogueLineStarted);
            AssertConversion(adapter, P10NarrativeGameplayFactType.NarrativeStateEntered, P10NarrativeEventType.NarrativeStateEntered);
            AssertConversion(adapter, P10NarrativeGameplayFactType.NarrativePropInspected, P10NarrativeEventType.NarrativePropInspected);

            if (eventBus.EventHistory.Count != 6)
            {
                throw new InvalidOperationException("Published event count mismatch.");
            }

            if (adapter.FactHistory.Count != 6)
            {
                throw new InvalidOperationException("Fact history count mismatch.");
            }
        }

        private static void ValidateRejectsInvalidFacts()
        {
            P10NarrativeEventBus eventBus = new P10NarrativeEventBus();
            P10NarrativeGameplayEventAdapter adapter = new P10NarrativeGameplayEventAdapter(eventBus);

            if (adapter.TryConvert(null, out P10NarrativeEvent nullEvent) || nullEvent != null)
            {
                throw new InvalidOperationException("Null fact was not rejected.");
            }

            P10NarrativeGameplayFact unsupportedFact = new P10NarrativeGameplayFact
            {
                FactType = P10NarrativeGameplayFactType.None
            };

            if (adapter.TryConvert(unsupportedFact, out P10NarrativeEvent unsupportedEvent) || unsupportedEvent != null)
            {
                throw new InvalidOperationException("Unsupported fact was not rejected.");
            }

            if (adapter.PublishGameplayFact(null))
            {
                throw new InvalidOperationException("Null fact publish was not rejected.");
            }

            if (adapter.PublishGameplayFact(unsupportedFact))
            {
                throw new InvalidOperationException("Unsupported fact publish was not rejected.");
            }

            if (P10NarrativeGameplayEventAdapter.IsSupportedFactType(P10NarrativeGameplayFactType.None))
            {
                throw new InvalidOperationException("Unsupported fact type reported as supported.");
            }
        }

        private static void ValidatePublishRequiresEventBus()
        {
            P10NarrativeGameplayEventAdapter adapter = new P10NarrativeGameplayEventAdapter(null);

            if (adapter.PublishGameplayFact(CreateFact(P10NarrativeGameplayFactType.GameStarted)))
            {
                throw new InvalidOperationException("Publish succeeded without an event bus.");
            }

            if (adapter.FactHistory.Count != 0)
            {
                throw new InvalidOperationException("Fact history changed without an event bus.");
            }
        }

        private static void AssertConversion(
            P10NarrativeGameplayEventAdapter adapter,
            P10NarrativeGameplayFactType factType,
            P10NarrativeEventType expectedEventType)
        {
            P10NarrativeGameplayFact fact = CreateFact(factType);

            if (!P10NarrativeGameplayEventAdapter.IsSupportedFactType(factType))
            {
                throw new InvalidOperationException("Supported fact type reported as unsupported: " + factType);
            }

            if (!adapter.TryConvert(fact, out P10NarrativeEvent narrativeEvent))
            {
                throw new InvalidOperationException("Fact conversion failed: " + factType);
            }

            AssertEvent(narrativeEvent, expectedEventType);

            if (!adapter.PublishGameplayFact(fact))
            {
                throw new InvalidOperationException("Fact publish failed: " + factType);
            }
        }

        private static P10NarrativeGameplayFact CreateFact(P10NarrativeGameplayFactType factType)
        {
            return new P10NarrativeGameplayFact
            {
                FactType = factType,
                ChapterId = "P10_CH01",
                OrderId = "ORDER_001",
                NodeId = "P10_CH01_NODE_ORDER_001_PASS",
                TargetId = "IGNORED_TARGET",
                Payload = "P10B_02_TEST_PAYLOAD",
                Score = 95
            };
        }

        private static void AssertEvent(P10NarrativeEvent evt, P10NarrativeEventType expectedEventType)
        {
            if (evt == null)
            {
                throw new InvalidOperationException("Converted event is null.");
            }

            if (evt.EventType != expectedEventType)
            {
                throw new InvalidOperationException("Unexpected event type.");
            }

            if (evt.ChapterId != "P10_CH01"
                || evt.OrderId != "ORDER_001"
                || evt.NodeId != "P10_CH01_NODE_ORDER_001_PASS"
                || evt.Payload != "P10B_02_TEST_PAYLOAD"
                || evt.Score != 95)
            {
                throw new InvalidOperationException("Allowed field copy mismatch.");
            }

            if (evt.TargetState != P10NarrativeState.None
                || !string.IsNullOrEmpty(evt.FlagKey)
                || evt.FlagValue)
            {
                throw new InvalidOperationException("Disallowed fields were populated.");
            }
        }
    }
}
