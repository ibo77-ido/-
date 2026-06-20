using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Phase10_Narrative
{
    [DisallowMultipleComponent]
    public sealed class Phase9Phase10Bridge : MonoBehaviour
    {
        public const string DefaultBridgeRootName = "_BridgeRoot";
        public const string OverlaySceneName = "P10_CH01_NarrativeOverlay";

        private static readonly NpcBinding[] DefaultNpcBindings =
        {
            new NpcBinding("徐老伯", P10CH01FlowController.NpcXuLaoBo),
            new NpcBinding("周掌柜", P10CH01FlowController.NpcZhouZhangGui),
            new NpcBinding("陈书院", P10CH01FlowController.NpcChenShuYuan),
            new NpcBinding("卢客", P10CH01FlowController.NpcLuKe)
        };

        [Header("Phase10")]
        [SerializeField] private bool autoStartChapterOnSceneReady = true;
        [SerializeField] private bool loadOverlaySceneAdditively = true;
        [SerializeField] private bool autoInstallGameplayFactAdapter = true;
        [SerializeField] private string overlaySceneName = OverlaySceneName;
        [SerializeField] private P10CH01FlowController flowController;
        [SerializeField] private P10NarrativeManager narrativeManager;
        [SerializeField] private P10DialogueController dialogueController;

        [Header("Phase9 NPC Names")]
        [SerializeField] private string xuLaoBoSceneName = "徐老伯";
        [SerializeField] private string zhouZhangGuiSceneName = "周掌柜";
        [SerializeField] private string chenShuYuanSceneName = "陈书院";
        [SerializeField] private string luKeSceneName = "卢客";

        [Header("Auto Dialogue Navigation")]
        [SerializeField] private bool autoMoveToNextDialogueNpc = true;
        [SerializeField] private bool autoRequestInteractionOnArrival = true;
        [SerializeField, Min(0.05f)] private float autoMoveArrivalDistance = 0.8f;
        [SerializeField, Min(0.1f)] private float autoMoveRetryInterval = 0.5f;
        [SerializeField, Min(0.1f)] private float autoMoveDestinationSampleDistance = 5f;

        private readonly Dictionary<string, string> npcIdBySceneName = new Dictionary<string, string>(StringComparer.Ordinal);
        private bool chapterStartRequested;
        private bool bindingAttempted;
        private bool bootstrapStarted;
        private bool autoStartWatcherStarted;
        private float nextAutoStartAttemptTime;
        private P10CH01FlowStep observedAutoMoveStep = P10CH01FlowStep.None;
        private string pendingAutoMoveNpcId = string.Empty;
        private string pendingAutoMoveNpcSceneName = string.Empty;
        private Transform pendingAutoMoveNpcTransform;
        private Vector3 pendingAutoMoveDestination;
        private bool hasPendingAutoMoveDestination;
        private bool autoMoveCommandIssued;
        private float nextAutoMoveAttemptTime;
        private MonoBehaviour cachedPhase9InteractionBridge;
        private MonoBehaviour cachedPlayerCharacter;
        private MonoBehaviour cachedMovementController;

        public P10CH01FlowController FlowController
        {
            get { return flowController; }
        }

        public P10CH01FlowStep CurrentStep
        {
            get { return flowController != null ? flowController.CurrentStep : P10CH01FlowStep.None; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallForLoadedScene()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            EnsureBridgeInstance();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureBridgeInstance();
        }

        private static Phase9Phase10Bridge EnsureBridgeInstance()
        {
            Phase9Phase10Bridge existingBridge = FindObjectOfType<Phase9Phase10Bridge>();
            if (existingBridge != null)
            {
                return existingBridge;
            }

            GameObject bridgeRoot = GameObject.Find(DefaultBridgeRootName);
            if (bridgeRoot == null && !HasAnyKnownPhase9Npc())
            {
                return null;
            }

            if (bridgeRoot == null)
            {
                bridgeRoot = new GameObject(DefaultBridgeRootName);
            }

            return bridgeRoot.AddComponent<Phase9Phase10Bridge>();
        }

        private void Awake()
        {
            BuildNpcBindingMap();
            ResolvePhase10References();
        }

        private void Start()
        {
            EnsureRuntimeCoroutinesStarted();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnUnitySceneLoaded;
            EnsureRuntimeCoroutinesStarted();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnUnitySceneLoaded;
        }

        private void Update()
        {
            TryAutoStartChapterWhenReady();
            TickAutoDialogueNavigation();
        }

        private void EnsureRuntimeCoroutinesStarted()
        {
            if (!Application.isPlaying || !isActiveAndEnabled)
            {
                return;
            }

            if (!bootstrapStarted)
            {
                bootstrapStarted = true;
                StartCoroutine(BootstrapBridge());
            }

            if (!autoStartWatcherStarted)
            {
                autoStartWatcherStarted = true;
                StartCoroutine(AutoStartChapterWhenReady());
            }
        }

        public bool RequestNpcInteraction(string npcId)
        {
            return EnsureFlowController() != null && flowController.RequestNpcInteraction(npcId);
        }

        public bool SubmitGameStarted()
        {
            if (chapterStartRequested && IsChapterStartNodeActive())
            {
                return false;
            }

            bool accepted = SubmitFact(P10CH01GameplayFactType.GameStarted, string.Empty, string.Empty, 0, string.Empty);
            if (accepted)
            {
                chapterStartRequested = true;
            }

            return accepted;
        }

        public bool SubmitDialogueCompleted(string nodeId)
        {
            bool accepted = SubmitFact(P10CH01GameplayFactType.DialogueCompleted, string.Empty, nodeId, 0, string.Empty);
            if (accepted)
            {
                CloseDialogueAfterAcceptedCompletion();
            }

            return accepted;
        }

        public bool SubmitTutorialCraftCompleted()
        {
            return SubmitFact(P10CH01GameplayFactType.TutorialCraftCompleted, string.Empty, string.Empty, 0, string.Empty);
        }

        public bool SubmitOrderAccepted(string orderId)
        {
            return SubmitFact(P10CH01GameplayFactType.OrderAccepted, orderId, string.Empty, 0, string.Empty);
        }

        public bool SubmitOrderSubmitted(string orderId)
        {
            return SubmitFact(P10CH01GameplayFactType.OrderSubmitted, orderId, string.Empty, 0, string.Empty);
        }

        public bool SubmitOrderCompleted(string orderId, int score, string result)
        {
            return SubmitFact(P10CH01GameplayFactType.OrderCompleted, orderId, string.Empty, score, result);
        }

        public bool SubmitRewardGranted(string orderId)
        {
            return SubmitFact(P10CH01GameplayFactType.RewardGranted, orderId, string.Empty, 0, string.Empty);
        }

        public bool SubmitChapterEndingRequested()
        {
            return SubmitFact(P10CH01GameplayFactType.ChapterEndingRequested, string.Empty, string.Empty, 0, string.Empty);
        }

        public bool SubmitFact(P10CH01GameplayFact fact)
        {
            if (EnsureFlowController() == null || !flowController.ApplyGameplayFact(fact))
            {
                return false;
            }

            if (ShouldSyncDialogueForFact(fact))
            {
                SyncDialogueControllerToNarrativeNode();
            }

            return true;
        }

        public void RebindSceneNpcs()
        {
            bindingAttempted = false;
            BuildNpcBindingMap();
            BindSceneNpcs();
        }

        private bool SubmitFact(P10CH01GameplayFactType type, string orderId, string nodeId, int score, string result)
        {
            return SubmitFact(new P10CH01GameplayFact
            {
                FactType = type,
                ChapterId = P10CH01FlowController.ChapterId,
                OrderId = orderId ?? string.Empty,
                NodeId = nodeId ?? string.Empty,
                Score = score,
                Result = result ?? string.Empty
            });
        }

        private static bool ShouldSyncDialogueForFact(P10CH01GameplayFact fact)
        {
            if (fact == null)
            {
                return false;
            }

            return fact.FactType == P10CH01GameplayFactType.GameStarted
                || fact.FactType == P10CH01GameplayFactType.OrderCompleted
                || fact.FactType == P10CH01GameplayFactType.ChapterEndingRequested;
        }

        private void CloseDialogueAfterAcceptedCompletion()
        {
            ResolvePhase10References();
            if (dialogueController != null)
            {
                dialogueController.CloseDialogue();
            }

            if (flowController == null)
            {
                return;
            }

            string nextNpcId = ResolveAutoDialogueNpcId(flowController.CurrentStep);
            if (!string.IsNullOrWhiteSpace(nextNpcId))
            {
                BeginAutoMoveToNpc(nextNpcId);
            }
        }

        private void OnUnitySceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResolvePhase10References();
            RebindSceneNpcs();
            ClearPendingAutoMove();
        }

        private IEnumerator AutoStartChapterWhenReady()
        {
            WaitForSeconds retryDelay = new WaitForSeconds(0.5f);
            while (Application.isPlaying && autoStartChapterOnSceneReady && !IsChapterStartNodeActive())
            {
                if (!IsPhase10RuntimeReady())
                {
                    yield return retryDelay;
                    continue;
                }

                EnsureFlowController();
                EnsureGameplayFactAdapter();
                BindSceneNpcs();

                if (TryRecoverOpeningNarrationNode())
                {
                    chapterStartRequested = true;
                    yield return retryDelay;
                    continue;
                }

                if (flowController != null && flowController.CurrentStep == P10CH01FlowStep.None)
                {
                    SubmitGameStarted();
                }

                yield return retryDelay;
            }
        }

        private void TryAutoStartChapterWhenReady()
        {
            if (!Application.isPlaying
                || !autoStartChapterOnSceneReady
                || Time.realtimeSinceStartup < nextAutoStartAttemptTime)
            {
                return;
            }

            nextAutoStartAttemptTime = Time.realtimeSinceStartup + 0.5f;
            if (!IsPhase10RuntimeReady())
            {
                return;
            }

            EnsureFlowController();
            EnsureGameplayFactAdapter();
            BindSceneNpcs();

            if (TryRecoverOpeningNarrationNode())
            {
                chapterStartRequested = true;
                return;
            }

            if (chapterStartRequested && IsChapterStartNodeActive())
            {
                return;
            }

            if (flowController != null && flowController.CurrentStep == P10CH01FlowStep.None)
            {
                SubmitGameStarted();
            }
        }

        private void BuildNpcBindingMap()
        {
            npcIdBySceneName.Clear();
            AddNpcBinding(xuLaoBoSceneName, P10CH01FlowController.NpcXuLaoBo);
            AddNpcBinding(zhouZhangGuiSceneName, P10CH01FlowController.NpcZhouZhangGui);
            AddNpcBinding(chenShuYuanSceneName, P10CH01FlowController.NpcChenShuYuan);
            AddNpcBinding(luKeSceneName, P10CH01FlowController.NpcLuKe);

            for (int i = 0; i < DefaultNpcBindings.Length; i++)
            {
                AddNpcBinding(DefaultNpcBindings[i].SceneName, DefaultNpcBindings[i].NpcId);
            }
        }

        private void AddNpcBinding(string sceneName, string npcId)
        {
            if (string.IsNullOrWhiteSpace(sceneName) || string.IsNullOrWhiteSpace(npcId))
            {
                return;
            }

            npcIdBySceneName[sceneName] = npcId;
        }

        private void BindSceneNpcs()
        {
            if (bindingAttempted)
            {
                return;
            }

            bindingAttempted = true;

            foreach (KeyValuePair<string, string> binding in npcIdBySceneName)
            {
                GameObject npc = GameObject.Find(binding.Key);
                if (npc == null)
                {
                    continue;
                }

                Phase9Phase10NpcForwarder forwarder = npc.GetComponent<Phase9Phase10NpcForwarder>();
                if (forwarder == null)
                {
                    forwarder = npc.AddComponent<Phase9Phase10NpcForwarder>();
                }

                forwarder.Bind(this, binding.Value);
            }
        }

        private void TickAutoDialogueNavigation()
        {
            if (!Application.isPlaying || !autoMoveToNextDialogueNpc)
            {
                return;
            }

            P10CH01FlowController controller = EnsureFlowController();
            if (controller == null)
            {
                return;
            }

            P10CH01FlowStep step = controller.CurrentStep;
            if (step != observedAutoMoveStep)
            {
                observedAutoMoveStep = step;
                ClearPendingAutoMove();

                string nextNpcId = ResolveAutoDialogueNpcId(step);
                if (!string.IsNullOrWhiteSpace(nextNpcId))
                {
                    if (IsExpectedDialogueNodeActive(step) && IsDialogueUiVisible() && RewindPrematureAutoDialogueNode(step))
                    {
                        return;
                    }

                    BeginAutoMoveToNpc(nextNpcId);
                }
            }
            else
            {
                RecoverMissingDialogueAutoMove(step);
            }

            if (string.IsNullOrWhiteSpace(pendingAutoMoveNpcId))
            {
                return;
            }

            if (IsDialogueUiVisible())
            {
                return;
            }

            if (!CanMoveInWorldMode())
            {
                return;
            }

            if (pendingAutoMoveNpcTransform == null)
            {
                pendingAutoMoveNpcTransform = FindNpcTransform(pendingAutoMoveNpcSceneName);
                if (pendingAutoMoveNpcTransform == null)
                {
                    return;
                }
            }

            if (!hasPendingAutoMoveDestination && !TryResolveAutoMoveDestination(pendingAutoMoveNpcTransform.position, out pendingAutoMoveDestination))
            {
                return;
            }

            Transform player = ResolvePlayerTransform();
            if (player == null)
            {
                return;
            }

            float arrivalDistance = Mathf.Max(0.05f, autoMoveArrivalDistance);
            float sqrArrivalDistance = arrivalDistance * arrivalDistance;
            Vector3 arrivalPoint = hasPendingAutoMoveDestination ? pendingAutoMoveDestination : pendingAutoMoveNpcTransform.position;
            if ((player.position - arrivalPoint).sqrMagnitude <= sqrArrivalDistance
                || (autoMoveCommandIssued && !IsPlayerMoving()))
            {
                StopPlayerAutoMove();
                TryRestoreNarrativePrerequisiteForStep(step);
                if (!autoRequestInteractionOnArrival || TryRequestNpcInteraction(pendingAutoMoveNpcId))
                {
                    ClearPendingAutoMove();
                }

                return;
            }

            if (autoMoveCommandIssued && IsPlayerMoving())
            {
                return;
            }

            if (Time.realtimeSinceStartup < nextAutoMoveAttemptTime)
            {
                return;
            }

            nextAutoMoveAttemptTime = Time.realtimeSinceStartup + Mathf.Max(0.1f, autoMoveRetryInterval);
            autoMoveCommandIssued = TryMovePlayerTo(pendingAutoMoveDestination);
        }

        private static string ResolveAutoDialogueNpcId(P10CH01FlowStep step)
        {
            switch (step)
            {
                case P10CH01FlowStep.XuTutorialDialogue:
                    return P10CH01FlowController.NpcXuLaoBo;
                case P10CH01FlowStep.Order001Accept:
                    return P10CH01FlowController.NpcZhouZhangGui;
                case P10CH01FlowStep.Order003Accept:
                    return P10CH01FlowController.NpcChenShuYuan;
                case P10CH01FlowStep.Order004Accept:
                    return P10CH01FlowController.NpcLuKe;
                default:
                    return string.Empty;
            }
        }

        private bool TryRequestNpcInteraction(string npcId)
        {
            if (!RequestNpcInteraction(npcId))
            {
                return false;
            }

            SyncDialogueControllerToNarrativeNode();
            return true;
        }

        private void SyncDialogueControllerToNarrativeNode()
        {
            ResolvePhase10References();
            if (dialogueController == null || narrativeManager == null)
            {
                return;
            }

            string nodeId = narrativeManager.GetCurrentNode();
            if (!string.IsNullOrWhiteSpace(nodeId))
            {
                dialogueController.SetCurrentNode(nodeId);
            }
        }

        private void RecoverMissingDialogueAutoMove(P10CH01FlowStep step)
        {
            if (!string.IsNullOrWhiteSpace(pendingAutoMoveNpcId) || IsDialogueUiVisible())
            {
                return;
            }

            string nextNpcId = ResolveAutoDialogueNpcId(step);
            if (string.IsNullOrWhiteSpace(nextNpcId))
            {
                return;
            }

            if (IsExpectedDialogueNodeActive(step))
            {
                RewindPrematureAutoDialogueNode(step);
                return;
            }

            BeginAutoMoveToNpc(nextNpcId);
        }

        private bool IsExpectedDialogueNodeActive(P10CH01FlowStep step)
        {
            ResolvePhase10References();
            if (narrativeManager == null)
            {
                return false;
            }

            string expectedNodeId = ResolveAutoDialogueNodeId(step);
            return !string.IsNullOrWhiteSpace(expectedNodeId)
                && string.Equals(narrativeManager.GetCurrentNode(), expectedNodeId, StringComparison.Ordinal);
        }

        private bool RewindPrematureAutoDialogueNode(P10CH01FlowStep step)
        {
            ResolvePhase10References();
            if (narrativeManager == null || dialogueController == null)
            {
                return false;
            }

            string expectedNodeId = ResolveAutoDialogueNodeId(step);
            string prerequisiteNodeId = ResolvePrerequisiteDialogueNodeId(step);
            P10NarrativeState prerequisiteState = ResolvePrerequisiteNarrativeState(step);
            if (string.IsNullOrWhiteSpace(expectedNodeId)
                || string.IsNullOrWhiteSpace(prerequisiteNodeId)
                || prerequisiteState == P10NarrativeState.None
                || !string.Equals(narrativeManager.GetCurrentNode(), expectedNodeId, StringComparison.Ordinal))
            {
                return false;
            }

            narrativeManager.LoadSnapshot(new P10NarrativeSnapshot
            {
                SnapshotVersion = 2,
                ChapterState = prerequisiteState,
                CurrentNodeId = prerequisiteNodeId,
                PlayedNodeIds = new List<string>(),
                NarrativeFlags = new Dictionary<string, bool>(),
                DialogueLogEntries = new List<P10DialogueLogSnapshotEntry>()
            });

            dialogueController.SetCurrentNode(narrativeManager.GetCurrentNode());
            dialogueController.CloseDialogue();
            BeginAutoMoveToNpc(ResolveAutoDialogueNpcId(step));
            return true;
        }

        private bool TryRestoreNarrativePrerequisiteForStep(P10CH01FlowStep step)
        {
            ResolvePhase10References();
            if (narrativeManager == null || !string.IsNullOrWhiteSpace(narrativeManager.GetCurrentNode()))
            {
                return false;
            }

            string prerequisiteNodeId = ResolvePrerequisiteDialogueNodeId(step);
            P10NarrativeState prerequisiteState = ResolvePrerequisiteNarrativeState(step);
            if (string.IsNullOrWhiteSpace(prerequisiteNodeId) || prerequisiteState == P10NarrativeState.None)
            {
                return false;
            }

            narrativeManager.LoadSnapshot(new P10NarrativeSnapshot
            {
                SnapshotVersion = 2,
                ChapterState = prerequisiteState,
                CurrentNodeId = prerequisiteNodeId,
                PlayedNodeIds = new List<string>(),
                NarrativeFlags = new Dictionary<string, bool>(),
                DialogueLogEntries = new List<P10DialogueLogSnapshotEntry>()
            });

            return string.Equals(narrativeManager.GetCurrentNode(), prerequisiteNodeId, StringComparison.Ordinal);
        }

        private static string ResolveAutoDialogueNodeId(P10CH01FlowStep step)
        {
            switch (step)
            {
                case P10CH01FlowStep.XuTutorialDialogue:
                    return P10CH01FlowController.NodeTutorial;
                case P10CH01FlowStep.Order001Accept:
                    return P10CH01FlowController.NodeOrder001Accept;
                case P10CH01FlowStep.Order003Accept:
                    return P10CH01FlowController.NodeOrder003Accept;
                case P10CH01FlowStep.Order004Accept:
                    return P10CH01FlowController.NodeOrder004Accept;
                default:
                    return string.Empty;
            }
        }

        private static string ResolvePrerequisiteDialogueNodeId(P10CH01FlowStep step)
        {
            switch (step)
            {
                case P10CH01FlowStep.XuTutorialDialogue:
                    return P10CH01FlowController.NodePrologue;
                case P10CH01FlowStep.Order001Accept:
                    return P10CH01FlowController.NodeTutorial;
                case P10CH01FlowStep.Order003Accept:
                    return P10CH01FlowController.NodeOrder001Pass;
                case P10CH01FlowStep.Order004Accept:
                    return P10CH01FlowController.NodeOrder003Pass;
                default:
                    return string.Empty;
            }
        }

        private static P10NarrativeState ResolvePrerequisiteNarrativeState(P10CH01FlowStep step)
        {
            switch (step)
            {
                case P10CH01FlowStep.XuTutorialDialogue:
                    return P10NarrativeState.Prologue;
                case P10CH01FlowStep.Order001Accept:
                    return P10NarrativeState.Tutorial;
                case P10CH01FlowStep.Order003Accept:
                    return P10NarrativeState.Order001;
                case P10CH01FlowStep.Order004Accept:
                    return P10NarrativeState.Order003;
                default:
                    return P10NarrativeState.None;
            }
        }

        private void BeginAutoMoveToNpc(string npcId)
        {
            BuildNpcBindingMap();
            pendingAutoMoveNpcId = npcId ?? string.Empty;
            pendingAutoMoveNpcSceneName = ResolveSceneNameForNpcId(pendingAutoMoveNpcId);
            pendingAutoMoveNpcTransform = FindNpcTransform(pendingAutoMoveNpcSceneName);
            autoMoveCommandIssued = false;
            nextAutoMoveAttemptTime = 0f;
        }

        private string ResolveSceneNameForNpcId(string npcId)
        {
            if (string.IsNullOrWhiteSpace(npcId))
            {
                return string.Empty;
            }

            foreach (KeyValuePair<string, string> binding in npcIdBySceneName)
            {
                if (string.Equals(binding.Value, npcId, StringComparison.Ordinal))
                {
                    return binding.Key;
                }
            }

            return string.Empty;
        }

        private static Transform FindNpcTransform(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return null;
            }

            GameObject npc = GameObject.Find(sceneName);
            return npc != null ? npc.transform : null;
        }

        private bool TryResolveAutoMoveDestination(Vector3 npcPosition, out Vector3 destination)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(npcPosition, out hit, Mathf.Max(0.1f, autoMoveDestinationSampleDistance), NavMesh.AllAreas))
            {
                destination = hit.position;
                hasPendingAutoMoveDestination = true;
                return true;
            }

            destination = npcPosition;
            return false;
        }

        private void ClearPendingAutoMove()
        {
            pendingAutoMoveNpcId = string.Empty;
            pendingAutoMoveNpcSceneName = string.Empty;
            pendingAutoMoveNpcTransform = null;
            pendingAutoMoveDestination = Vector3.zero;
            hasPendingAutoMoveDestination = false;
            autoMoveCommandIssued = false;
            nextAutoMoveAttemptTime = 0f;
        }

        private bool CanMoveInWorldMode()
        {
            MonoBehaviour bridge = ResolveCachedMonoBehaviour(ref cachedPhase9InteractionBridge, "Phase9InteractionBridge");
            if (bridge == null)
            {
                return true;
            }

            FieldInfo runtimeModeField = bridge.GetType().GetField("currentRuntimeMode", BindingFlags.Instance | BindingFlags.NonPublic);
            object runtimeMode = runtimeModeField != null ? runtimeModeField.GetValue(bridge) : null;
            string runtimeModeName = runtimeMode != null ? runtimeMode.ToString() : string.Empty;
            return !string.Equals(runtimeModeName, "GameplayMode", StringComparison.Ordinal);
        }

        private bool IsDialogueUiVisible()
        {
            ResolvePhase10References();
            if (dialogueController == null)
            {
                return false;
            }

            if (dialogueController.IsDialogueLogVisible || dialogueController.IsCurrentOrderPanelVisible)
            {
                return true;
            }

            FieldInfo runtimeSurfaceField = typeof(P10DialogueController).GetField("runtimeSurface", BindingFlags.Instance | BindingFlags.NonPublic);
            object runtimeSurface = runtimeSurfaceField != null ? runtimeSurfaceField.GetValue(dialogueController) : null;
            if (runtimeSurface == null)
            {
                return false;
            }

            PropertyInfo visibleProperty = runtimeSurface.GetType().GetProperty("IsDialogueVisible", BindingFlags.Instance | BindingFlags.Public);
            object value = visibleProperty != null ? visibleProperty.GetValue(runtimeSurface, null) : null;
            return value is bool && (bool)value;
        }

        private Transform ResolvePlayerTransform()
        {
            MonoBehaviour playerCharacter = ResolveCachedMonoBehaviour(ref cachedPlayerCharacter, "PlayerCharacter");
            if (playerCharacter != null)
            {
                PropertyInfo transformProperty = playerCharacter.GetType().GetProperty("Transform", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                object value = transformProperty != null ? transformProperty.GetValue(playerCharacter, null) : null;
                Transform resolvedTransform = value as Transform;
                return resolvedTransform != null ? resolvedTransform : playerCharacter.transform;
            }

            MonoBehaviour movementController = ResolveCachedMonoBehaviour(ref cachedMovementController, "MovementController");
            return movementController != null ? movementController.transform : null;
        }

        private bool IsPlayerMoving()
        {
            MonoBehaviour movementController = ResolveCachedMonoBehaviour(ref cachedMovementController, "MovementController");
            if (movementController == null)
            {
                return false;
            }

            MethodInfo method = movementController.GetType().GetMethod("IsMoving", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object result = method != null ? method.Invoke(movementController, null) : null;
            return result is bool && (bool)result;
        }

        private bool TryMovePlayerTo(Vector3 destination)
        {
            MonoBehaviour playerCharacter = ResolveCachedMonoBehaviour(ref cachedPlayerCharacter, "PlayerCharacter");
            if (TryInvokeVector3Move(playerCharacter, "TrySetDestination", destination))
            {
                return true;
            }

            if (TryInvokeVector3Move(playerCharacter, "SetDestination", destination))
            {
                return true;
            }

            MonoBehaviour movementController = ResolveCachedMonoBehaviour(ref cachedMovementController, "MovementController");
            if (TryInvokeVector3Move(movementController, "TrySetDestination", destination))
            {
                return true;
            }

            return TryInvokeVector3Move(movementController, "SetDestination", destination);
        }

        private void StopPlayerAutoMove()
        {
            MonoBehaviour playerCharacter = ResolveCachedMonoBehaviour(ref cachedPlayerCharacter, "PlayerCharacter");
            if (TryInvokeNoArg(playerCharacter, "StopMoving"))
            {
                return;
            }

            MonoBehaviour movementController = ResolveCachedMonoBehaviour(ref cachedMovementController, "MovementController");
            TryInvokeNoArg(movementController, "Stop");
        }

        private static MonoBehaviour ResolveCachedMonoBehaviour(ref MonoBehaviour cachedBehaviour, string typeName)
        {
            if (cachedBehaviour != null)
            {
                return cachedBehaviour;
            }

            cachedBehaviour = FindMonoBehaviourByTypeName(typeName);
            return cachedBehaviour;
        }

        private static bool TryInvokeVector3Move(MonoBehaviour target, string methodName, Vector3 destination)
        {
            if (target == null)
            {
                return false;
            }

            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(Vector3) }, null);
            if (method == null)
            {
                return false;
            }

            object result = method.Invoke(target, new object[] { destination });
            return !(result is bool) || (bool)result;
        }

        private static bool TryInvokeNoArg(MonoBehaviour target, string methodName)
        {
            if (target == null)
            {
                return false;
            }

            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (method == null)
            {
                return false;
            }

            method.Invoke(target, null);
            return true;
        }

        private static MonoBehaviour FindMonoBehaviourByTypeName(string typeName)
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

        private P10CH01FlowController EnsureFlowController()
        {
            ResolvePhase10References();
            if (flowController != null)
            {
                return flowController;
            }

            GameObject host = GameObject.Find("P10_CH01_FlowController");
            if (host == null)
            {
                host = new GameObject("P10_CH01_FlowController");
            }

            flowController = host.GetComponent<P10CH01FlowController>();
            if (flowController == null)
            {
                flowController = host.AddComponent<P10CH01FlowController>();
            }

            if (narrativeManager != null)
            {
                flowController.BindNarrativeManager(narrativeManager);
            }

            return flowController;
        }

        private void ResolvePhase10References()
        {
            if (narrativeManager == null)
            {
                narrativeManager = FindObjectOfType<P10NarrativeManager>();
            }

            if (dialogueController == null)
            {
                dialogueController = FindObjectOfType<P10DialogueController>();
            }

            if (flowController == null)
            {
                flowController = FindObjectOfType<P10CH01FlowController>();
            }

            if (flowController != null && narrativeManager != null)
            {
                flowController.BindNarrativeManager(narrativeManager);
            }
        }

        private void EnsureGameplayFactAdapter()
        {
            if (!autoInstallGameplayFactAdapter)
            {
                return;
            }

            Type adapterType = Type.GetType("Phase10_Narrative.Phase9Phase10GameplayFactAdapter");
            if (adapterType == null)
            {
                return;
            }

            Component adapter = GetComponent(adapterType);
            if (adapter == null)
            {
                adapter = gameObject.AddComponent(adapterType);
            }

            adapterType.GetMethod("Bind", new[] { typeof(Phase9Phase10Bridge) })?.Invoke(adapter, new object[] { this });
        }

        private static bool HasAnyKnownPhase9Npc()
        {
            for (int i = 0; i < DefaultNpcBindings.Length; i++)
            {
                if (GameObject.Find(DefaultNpcBindings[i].SceneName) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsPhase10RuntimeReady()
        {
            ResolvePhase10References();
            return narrativeManager != null && dialogueController != null;
        }

        private bool IsChapterStartNodeActive()
        {
            ResolvePhase10References();
            return narrativeManager != null
                && string.Equals(narrativeManager.GetCurrentNode(), P10CH01FlowController.NodePrologue, StringComparison.Ordinal);
        }

        private bool TryRecoverOpeningNarrationNode()
        {
            ResolvePhase10References();
            if (flowController == null
                || narrativeManager == null
                || flowController.CurrentStep != P10CH01FlowStep.OpeningNarration
                || !string.IsNullOrWhiteSpace(narrativeManager.GetCurrentNode()))
            {
                return false;
            }

            return narrativeManager.HandleSceneTrigger("StartPrologue", P10CH01FlowController.NodePrologue);
        }

        private IEnumerator BootstrapBridge()
        {
            if (loadOverlaySceneAdditively)
            {
                yield return LoadOverlaySceneIfNeeded();
            }

            ResolvePhase10References();
            EnsureFlowController();
            EnsureGameplayFactAdapter();
            BindSceneNpcs();

            if (autoStartChapterOnSceneReady)
            {
                yield return WaitForPhase10RuntimeReady();
                if (!TryRecoverOpeningNarrationNode())
                {
                    SubmitGameStarted();
                }
            }
        }

        private IEnumerator WaitForPhase10RuntimeReady()
        {
            while (!IsPhase10RuntimeReady())
            {
                yield return null;
            }
        }

        private IEnumerator LoadOverlaySceneIfNeeded()
        {
            if (string.IsNullOrWhiteSpace(overlaySceneName))
            {
                yield break;
            }

            Scene overlayScene = SceneManager.GetSceneByName(overlaySceneName);
            if (overlayScene.isLoaded)
            {
                yield break;
            }

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(overlaySceneName, LoadSceneMode.Additive);
            if (loadOperation == null)
            {
                yield break;
            }

            while (!loadOperation.isDone)
            {
                yield return null;
            }

            ResolvePhase10References();
        }

        private readonly struct NpcBinding
        {
            public NpcBinding(string sceneName, string npcId)
            {
                SceneName = sceneName;
                NpcId = npcId;
            }

            public string SceneName { get; }
            public string NpcId { get; }
        }
    }
}
