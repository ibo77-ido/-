using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace Phase10_Narrative
{
    [DisallowMultipleComponent]
    public sealed class Phase9Phase10GameplayFactAdapter : MonoBehaviour
    {
        [SerializeField] private Phase9Phase10Bridge bridge;
        [SerializeField, Min(0.05f)] private float pollIntervalSeconds = 0.25f;
        [SerializeField] private string phase3GameManagerTypeName = "GameManager";
        [SerializeField] private string phase3ResultPanelTypeName = "ResultPanelController";
        [SerializeField] private string phase9InteractionBridgeTypeName = "Phase9InteractionBridge";

        private MonoBehaviour observedPhase9InteractionBridge;
        private MonoBehaviour observedGameManager;
        private Component observedResultPanel;
        private UnityEvent observedExitEvent;
        private UnityAction resultExitListener;
        private string lastObservedState = string.Empty;
        private float nextPollTime;
        private bool submittedResultForCurrentState;
        private bool lastOrderDone;
        private bool lastShapeDone;
        private bool lastGlazeDone;
        private bool lastKilnDone;
        private P10CH01FlowStep submittedSingleModuleStep = P10CH01FlowStep.None;
        private string rewardPendingOrderId = string.Empty;

        public void Bind(Phase9Phase10Bridge owner)
        {
            bridge = owner;
        }

        private void Awake()
        {
            if (bridge == null)
            {
                bridge = GetComponent<Phase9Phase10Bridge>();
            }

            if (bridge == null)
            {
                bridge = FindObjectOfType<Phase9Phase10Bridge>();
            }

            resultExitListener = HandleResultExitRequested;
        }

        private void OnDestroy()
        {
            UnsubscribeResultExitEvent();
        }

        private void Update()
        {
            ObservePhase9SingleModuleCompletion();

            if (Time.unscaledTime < nextPollTime)
            {
                return;
            }

            nextPollTime = Time.unscaledTime + pollIntervalSeconds;
            ObservePhase3State();
        }

        public bool SubmitObservedGameplayResult(int score, string result)
        {
            if (bridge == null)
            {
                bridge = FindObjectOfType<Phase9Phase10Bridge>();
            }

            if (bridge == null)
            {
                return false;
            }

            switch (bridge.CurrentStep)
            {
                case P10CH01FlowStep.TutorialCrafting:
                    return bridge.SubmitTutorialCraftCompleted();
                case P10CH01FlowStep.Order001Crafting:
                    return SubmitOrderCompleted(P10CH01FlowController.Order001, score, result);
                case P10CH01FlowStep.Order003Crafting:
                    return SubmitOrderCompleted(P10CH01FlowController.Order003, score, result);
                case P10CH01FlowStep.Order004Crafting:
                    return SubmitOrderCompleted(P10CH01FlowController.Order004, score, result);
                default:
                    return false;
            }
        }

        public bool SubmitObservedRewardExit()
        {
            if (bridge == null || string.IsNullOrWhiteSpace(rewardPendingOrderId))
            {
                return false;
            }

            string orderId = rewardPendingOrderId;
            rewardPendingOrderId = string.Empty;
            return bridge.SubmitRewardGranted(orderId);
        }

        private bool SubmitOrderCompleted(string orderId, int score, string result)
        {
            bool accepted = bridge.SubmitOrderCompleted(orderId, score, result);
            if (accepted)
            {
                rewardPendingOrderId = orderId;
            }

            return accepted;
        }

        private void ObservePhase3State()
        {
            ResolveObservedObjects();

            if (observedGameManager == null)
            {
                return;
            }

            string currentState = ReadPropertyString(observedGameManager, "CurrentState");
            if (!string.Equals(currentState, lastObservedState, StringComparison.Ordinal))
            {
                submittedResultForCurrentState = false;
                lastObservedState = currentState;
            }

            if (!string.Equals(currentState, "Result", StringComparison.Ordinal) || submittedResultForCurrentState)
            {
                return;
            }

            submittedResultForCurrentState = true;
            ObservedResult observedResult = ReadObservedResult();
            SubmitObservedGameplayResult(observedResult.Score, observedResult.Result);
        }

        private void ResolveObservedObjects()
        {
            if (observedPhase9InteractionBridge == null)
            {
                observedPhase9InteractionBridge = FindMonoBehaviourByTypeName(phase9InteractionBridgeTypeName);
            }

            if (observedGameManager == null)
            {
                observedGameManager = FindMonoBehaviourByTypeName(phase3GameManagerTypeName);
            }

            if (observedResultPanel == null)
            {
                observedResultPanel = FindComponentByTypeName(phase3ResultPanelTypeName);
                SubscribeResultExitEvent();
            }
        }

        private void ObservePhase9SingleModuleCompletion()
        {
            ResolveObservedObjects();

            if (bridge == null)
            {
                bridge = FindObjectOfType<Phase9Phase10Bridge>();
            }

            if (bridge == null || observedPhase9InteractionBridge == null)
            {
                return;
            }

            P10CH01FlowStep currentStep = bridge.CurrentStep;
            if (!IsCraftingStep(currentStep))
            {
                submittedSingleModuleStep = P10CH01FlowStep.None;
                CapturePhase9ModuleFlags();
                return;
            }

            bool orderDone = ReadBoolField(observedPhase9InteractionBridge, "orderDone");
            bool shapeDone = ReadBoolField(observedPhase9InteractionBridge, "shapeDone");
            bool glazeDone = ReadBoolField(observedPhase9InteractionBridge, "glazeDone");
            bool kilnDone = ReadBoolField(observedPhase9InteractionBridge, "kilnDone");
            bool newlyCompletedModule = (orderDone && !lastOrderDone)
                || (shapeDone && !lastShapeDone)
                || (glazeDone && !lastGlazeDone)
                || (kilnDone && !lastKilnDone);

            lastOrderDone = orderDone;
            lastShapeDone = shapeDone;
            lastGlazeDone = glazeDone;
            lastKilnDone = kilnDone;

            if (!newlyCompletedModule || submittedSingleModuleStep == currentStep)
            {
                return;
            }

            if (SubmitObservedGameplayResult(100, "SingleModuleCompleted"))
            {
                submittedSingleModuleStep = currentStep;
            }
        }

        private void CapturePhase9ModuleFlags()
        {
            if (observedPhase9InteractionBridge == null)
            {
                lastOrderDone = false;
                lastShapeDone = false;
                lastGlazeDone = false;
                lastKilnDone = false;
                return;
            }

            lastOrderDone = ReadBoolField(observedPhase9InteractionBridge, "orderDone");
            lastShapeDone = ReadBoolField(observedPhase9InteractionBridge, "shapeDone");
            lastGlazeDone = ReadBoolField(observedPhase9InteractionBridge, "glazeDone");
            lastKilnDone = ReadBoolField(observedPhase9InteractionBridge, "kilnDone");
        }

        private static bool IsCraftingStep(P10CH01FlowStep step)
        {
            return step == P10CH01FlowStep.TutorialCrafting
                || step == P10CH01FlowStep.Order001Crafting
                || step == P10CH01FlowStep.Order003Crafting
                || step == P10CH01FlowStep.Order004Crafting;
        }

        private void SubscribeResultExitEvent()
        {
            if (observedResultPanel == null || observedExitEvent != null)
            {
                return;
            }

            observedExitEvent = ReadPropertyValue(observedResultPanel, "OnExitGameplayEvent") as UnityEvent;
            if (observedExitEvent != null && resultExitListener != null)
            {
                observedExitEvent.AddListener(resultExitListener);
            }
        }

        private void UnsubscribeResultExitEvent()
        {
            if (observedExitEvent != null && resultExitListener != null)
            {
                observedExitEvent.RemoveListener(resultExitListener);
            }

            observedExitEvent = null;
        }

        private void HandleResultExitRequested()
        {
            SubmitObservedRewardExit();
        }

        private ObservedResult ReadObservedResult()
        {
            object pendingResultData = ReadFieldValue(observedResultPanel, "pendingResultData");
            if (pendingResultData == null)
            {
                return new ObservedResult(0, string.Empty);
            }

            float shapeScore = ReadFloatField(pendingResultData, "shapeScore");
            float glazeScore = ReadFloatField(pendingResultData, "glazeScore");
            float fireScore = ReadFloatField(pendingResultData, "fireScore");
            int score = Mathf.RoundToInt((shapeScore + glazeScore + fireScore) / 3f);

            string result = ReadStringField(pendingResultData, "orderResult");
            if (string.IsNullOrWhiteSpace(result))
            {
                result = ReadStringField(pendingResultData, "grade");
            }

            return new ObservedResult(score, result);
        }

        private static MonoBehaviour FindMonoBehaviourByTypeName(string typeName)
        {
            return FindComponentByTypeName(typeName) as MonoBehaviour;
        }

        private static Component FindComponentByTypeName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return null;
            }

            MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                Type type = behaviour.GetType();
                if (string.Equals(type.Name, typeName, StringComparison.Ordinal)
                    || string.Equals(type.FullName, typeName, StringComparison.Ordinal))
                {
                    return behaviour;
                }
            }

            return null;
        }

        private static string ReadPropertyString(object target, string propertyName)
        {
            object value = ReadPropertyValue(target, propertyName);
            return value != null ? value.ToString() : string.Empty;
        }

        private static object ReadPropertyValue(object target, string propertyName)
        {
            if (target == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return null;
            }

            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return property != null ? property.GetValue(target, null) : null;
        }

        private static object ReadFieldValue(object target, string fieldName)
        {
            if (target == null || string.IsNullOrWhiteSpace(fieldName))
            {
                return null;
            }

            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return field != null ? field.GetValue(target) : null;
        }

        private static float ReadFloatField(object target, string fieldName)
        {
            object value = ReadFieldValue(target, fieldName);
            if (value is float floatValue)
            {
                return floatValue;
            }

            if (value is double doubleValue)
            {
                return (float)doubleValue;
            }

            if (value is int intValue)
            {
                return intValue;
            }

            return 0f;
        }

        private static bool ReadBoolField(object target, string fieldName)
        {
            object value = ReadFieldValue(target, fieldName);
            return value is bool && (bool)value;
        }

        private static string ReadStringField(object target, string fieldName)
        {
            object value = ReadFieldValue(target, fieldName);
            return value != null ? value.ToString() : string.Empty;
        }

        private readonly struct ObservedResult
        {
            public ObservedResult(int score, string result)
            {
                Score = score;
                Result = result ?? string.Empty;
            }

            public int Score { get; }
            public string Result { get; }
        }
    }
}
