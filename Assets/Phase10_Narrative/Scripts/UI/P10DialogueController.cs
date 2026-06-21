using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Phase10_Narrative
{
    public sealed class P10DialogueController : MonoBehaviour
    {
        private const int MaxDialogueLogSnapshotEntries = 200;
        private const string NodeOrder001Accept = "P10_CH01_NODE_ORDER_001_ACCEPT";
        private const string NodeOrder003Accept = "P10_CH01_NODE_ORDER_003_ACCEPT";
        private const string NodeOrder004Accept = "P10_CH01_NODE_ORDER_004_ACCEPT";

        [SerializeField] private P10NarrativeManager narrativeManager;
        [SerializeField] private string currentNodeId = string.Empty;
        [SerializeField] private bool showDialogueWindow = true;
        [SerializeField] private List<P10DialogueNodeSO> dialogueNodes = new List<P10DialogueNodeSO>();
        [SerializeField] private List<P10CharacterDataSO> characters = new List<P10CharacterDataSO>();
        [SerializeField] private GameObject dialogueSurfacePrefab;
        [SerializeField] private Text speakerText;
        [SerializeField] private Text bodyText;

        private readonly List<P10DialogueLogEntry> dialogueLogEntries = new List<P10DialogueLogEntry>();
        private P10DialogueRuntimeSurface runtimeSurface;
        private int nextDialogueLogSequence = 1;
        private bool restoreDialogueAfterLogClose;
        private string currentLineNodeId = string.Empty;
        private int currentLineIndex;
#if UNITY_EDITOR
        private const string EditorVisibleDialogueSurfacePrefabPath = "Assets/Phase10_Narrative/Prefabs/UI/P10_CH01_DialogueUI_Placeholder.prefab";
#endif

        public IReadOnlyList<P10DialogueLogEntry> DialogueLogEntries
        {
            get { return dialogueLogEntries; }
        }

        public bool IsDialogueLogVisible
        {
            get { return runtimeSurface != null && runtimeSurface.IsDialogueLogVisible; }
        }

        public bool IsCurrentOrderPanelVisible
        {
            get { return runtimeSurface != null && runtimeSurface.IsCurrentOrderPanelVisible; }
        }

        public bool UsesRuntimeCreatedFixedTextSlotFallback
        {
            get { return runtimeSurface != null && runtimeSurface.UsesRuntimeCreatedFixedTextSlots; }
        }

        public Text FixedSpeakerText
        {
            get { return runtimeSurface != null ? runtimeSurface.SpeakerText : speakerText; }
        }

        public Text FixedBodyText
        {
            get { return runtimeSurface != null ? runtimeSurface.BodyText : bodyText; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallRuntimeSurface()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            EnsureRuntimeSurface();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureRuntimeSurface();
        }

        private static void EnsureRuntimeSurface()
        {
            P10NarrativeManager manager = FindObjectOfType<P10NarrativeManager>();
            if (manager == null)
            {
                return;
            }

            P10DialogueController existingController = FindObjectOfType<P10DialogueController>();
            if (existingController != null)
            {
                if (existingController.narrativeManager == null)
                {
                    existingController.narrativeManager = manager;
                }
                existingController.EnsureRuntimeSurfaceInstance();
                return;
            }

            GameObject controllerObject = new GameObject("P10_Runtime_DialogueController");
            controllerObject.transform.SetParent(manager.transform, false);

            P10DialogueController controller = controllerObject.AddComponent<P10DialogueController>();
            controller.narrativeManager = manager;
            controller.EnsureRuntimeSurfaceInstance();
        }

        private void Awake()
        {
            if (narrativeManager == null)
            {
                narrativeManager = FindObjectOfType<P10NarrativeManager>();
            }

            TryPopulateDialogueData();
            EnsureRuntimeSurfaceInstance();
        }

        private void OnValidate()
        {
            if (dialogueNodes == null)
            {
                dialogueNodes = new List<P10DialogueNodeSO>();
            }

            if (characters == null)
            {
                characters = new List<P10CharacterDataSO>();
            }

            TryPopulateDialogueData();
        }

        private void Update()
        {
            if (narrativeManager == null)
            {
                narrativeManager = FindObjectOfType<P10NarrativeManager>();
            }

            if (narrativeManager == null)
            {
                return;
            }

            string managerNodeId = narrativeManager.GetCurrentNode();
            if (!string.Equals(currentNodeId, managerNodeId, System.StringComparison.Ordinal))
            {
                SetCurrentNode(managerNodeId);
            }
        }

        public void EnsureRuntimeSurfaceInstance()
        {
            if (runtimeSurface != null)
            {
                return;
            }

            runtimeSurface = GetComponentInChildren<P10DialogueRuntimeSurface>();
            if (runtimeSurface == null)
            {
                Transform existingSurface = FindDescendantByName(transform, "P10_Runtime_DialogueSurface");
                if (existingSurface == null)
                {
                    existingSurface = InstantiateConfiguredDialogueSurface();
                }

                if (existingSurface == null)
                {
                    GameObject surfaceObject = new GameObject("P10_Runtime_DialogueSurface", typeof(RectTransform));
                    surfaceObject.transform.SetParent(transform, false);
                    existingSurface = surfaceObject.transform;
                    Debug.LogWarning("P10 dialogue UI is using runtime-created fallback surface. Editor-visible fixed text slots were not found.");
                }

                runtimeSurface = existingSurface.GetComponent<P10DialogueRuntimeSurface>();
                if (runtimeSurface == null)
                {
                    runtimeSurface = existingSurface.gameObject.AddComponent<P10DialogueRuntimeSurface>();
                }
            }

            if (speakerText == null)
            {
                speakerText = FindEditorVisibleDialogueSlot("P10_DialogueSpeakerText");
            }

            if (bodyText == null)
            {
                bodyText = FindEditorVisibleDialogueSlot("P10_DialogueBodyText");
            }

            runtimeSurface.BindFixedSlots(speakerText, bodyText);
            runtimeSurface.Build(this);
            speakerText = runtimeSurface.SpeakerText;
            bodyText = runtimeSurface.BodyText;
        }

        private Transform InstantiateConfiguredDialogueSurface()
        {
            GameObject prefab = dialogueSurfacePrefab;
#if UNITY_EDITOR
            if (prefab == null)
            {
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EditorVisibleDialogueSurfacePrefabPath);
            }
#endif

            if (prefab == null)
            {
                return null;
            }

            GameObject instance = Instantiate(prefab, transform, false);
            instance.name = prefab.name;

            P10DialogueRuntimeSurface prefabSurface = instance.GetComponentInChildren<P10DialogueRuntimeSurface>();
            if (prefabSurface != null)
            {
                return prefabSurface.transform;
            }

            Transform surface = FindDescendantByName(instance.transform, "P10_Runtime_DialogueSurface");
            return surface != null ? surface : instance.transform;
        }

        private Text FindEditorVisibleDialogueSlot(string slotName)
        {
            Transform manualSlots = FindDescendantByName(transform, "P10_CH01_DialogueManualSlots");
            if (manualSlots != null)
            {
                Transform directSlot = manualSlots.Find(slotName);
                Text directText = directSlot != null ? directSlot.GetComponent<Text>() : null;
                if (directText != null)
                {
                    return directText;
                }
            }

            Transform root = transform.Find("P10_CH01_DialogueUIRoot");
            if (root != null)
            {
                Transform directSlot = root.Find(slotName);
                Text directText = directSlot != null ? directSlot.GetComponent<Text>() : null;
                if (directText != null)
                {
                    return directText;
                }
            }

            Transform fallback = FindDescendantByName(transform, slotName);
            return fallback != null ? fallback.GetComponent<Text>() : null;
        }

        private static Transform FindDescendantByName(Transform root, string objectName)
        {
            if (root == null || string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            if (root.name == objectName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                Transform match = FindDescendantByName(child, objectName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        public void SetCurrentNode(string nodeId)
        {
            string resolvedNodeId = nodeId ?? string.Empty;
            if (!string.Equals(currentNodeId, resolvedNodeId, System.StringComparison.Ordinal)
                || !string.Equals(currentLineNodeId, resolvedNodeId, System.StringComparison.Ordinal))
            {
                currentLineNodeId = resolvedNodeId;
                currentLineIndex = ResolveLatestLoggedLineIndex(resolvedNodeId);
            }

            currentNodeId = resolvedNodeId;
            RefreshRuntimeSurface();
            RefreshCurrentOrderPanelIfVisible();
        }

        public void SendDialogueLineStarted()
        {
            if (narrativeManager == null)
            {
                return;
            }

            narrativeManager.PublishEvent(new P10NarrativeEvent(P10NarrativeEventType.DialogueLineStarted)
            {
                NodeId = currentNodeId
            });
        }

        public void AdvanceDialogueFromDialogueBoxClick()
        {
            if (runtimeSurface == null ||
                !runtimeSurface.IsDialogueVisible ||
                runtimeSurface.IsDialogueLogVisible ||
                runtimeSurface.IsCurrentOrderPanelVisible ||
                string.IsNullOrWhiteSpace(currentNodeId))
            {
                return;
            }

            AdvanceDialogue();
        }

        public void AdvanceDialogue()
        {
            if (TryAdvanceDialogueLine())
            {
                return;
            }

            if (TryHandleDialogueCompletedWithBridge(currentNodeId))
            {
                return;
            }

            List<string> nextNodeIds = ResolveNextNodeIds(currentNodeId);
            if (nextNodeIds != null)
            {
                for (int i = 0; i < nextNodeIds.Count; i++)
                {
                    string nextNodeId = nextNodeIds[i];
                    if (string.IsNullOrWhiteSpace(nextNodeId))
                    {
                        continue;
                    }

                    if (narrativeManager != null)
                    {
                        if (narrativeManager.TryAdvanceDialogueNode(currentNodeId, nextNodeId))
                        {
                            return;
                        }

                        break;
                    }
                }
            }

            if (runtimeSurface != null)
            {
                runtimeSurface.SetVisible(false);
            }

        }

        private bool TryHandleDialogueCompletedWithBridge(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                return false;
            }

            Phase9Phase10Bridge bridge = FindObjectOfType<Phase9Phase10Bridge>();
            if (bridge == null)
            {
                return false;
            }

            if (bridge.SubmitDialogueCompleted(nodeId) && runtimeSurface != null)
            {
                runtimeSurface.SetVisible(false);
            }

            return true;
        }

        public void CloseDialogue()
        {
            if (runtimeSurface != null)
            {
                runtimeSurface.SetVisible(false);
            }

        }

        private void RefreshRuntimeSurface()
        {
            if (!showDialogueWindow || runtimeSurface == null)
            {
                return;
            }

            TryPopulateDialogueData();
            P10DialogueNodeSO dialogueNode = ResolveDialogueNode(currentNodeId);
            P10ResolvedDialogueLine resolvedLine = ResolveCurrentDialogueLine(dialogueNode, currentNodeId);
            P10CharacterDataSO characterData = ResolveCharacter(resolvedLine.SpeakerCharacterId);

            string speakerName = characterData != null && !string.IsNullOrWhiteSpace(characterData.DisplayName)
                ? characterData.DisplayName
                : P10DialogueCatalog.GetSpeakerNameForNode(currentNodeId);
            string dialogueLine = resolvedLine.DialogueText;

            RecordDialogueLogEntry(currentNodeId, speakerName, dialogueLine);
            runtimeSurface.SetDialogue(speakerName, dialogueLine, resolvedLine.SpeakerCharacterId);
        }

        private bool TryAdvanceDialogueLine()
        {
            P10DialogueNodeSO dialogueNode = ResolveDialogueNode(currentNodeId);
            int lineCount = GetDialogueLineCount(dialogueNode);
            if (lineCount <= 0 || currentLineIndex >= lineCount - 1)
            {
                return false;
            }

            currentLineIndex++;
            RefreshRuntimeSurface();
            return true;
        }

        private P10ResolvedDialogueLine ResolveCurrentDialogueLine(P10DialogueNodeSO dialogueNode, string nodeId)
        {
            NormalizeDialogueLineCursor(dialogueNode, nodeId);

            if (dialogueNode != null && dialogueNode.DialogueLines != null && dialogueNode.DialogueLines.Count > 0)
            {
                P10DialogueLine line = dialogueNode.DialogueLines[currentLineIndex];
                string speakerCharacterId = line != null && !string.IsNullOrWhiteSpace(line.SpeakerCharacterId)
                    ? line.SpeakerCharacterId
                    : dialogueNode.SpeakerCharacterId;
                string dialogueText = line != null && !string.IsNullOrWhiteSpace(line.DialogueText)
                    ? line.DialogueText
                    : dialogueNode.DialogueText;

                return new P10ResolvedDialogueLine(speakerCharacterId, dialogueText);
            }

            if (dialogueNode != null)
            {
                return new P10ResolvedDialogueLine(dialogueNode.SpeakerCharacterId, dialogueNode.DialogueText);
            }

            return new P10ResolvedDialogueLine(
                P10DialogueCatalog.GetSpeakerCharacterId(nodeId),
                P10DialogueCatalog.GetDialogueText(nodeId));
        }

        private void NormalizeDialogueLineCursor(P10DialogueNodeSO dialogueNode, string nodeId)
        {
            string resolvedNodeId = nodeId ?? string.Empty;
            if (!string.Equals(currentLineNodeId, resolvedNodeId, System.StringComparison.Ordinal))
            {
                currentLineNodeId = resolvedNodeId;
                currentLineIndex = 0;
            }

            int lineCount = GetDialogueLineCount(dialogueNode);
            if (lineCount <= 0)
            {
                currentLineIndex = 0;
                return;
            }

            if (currentLineIndex < 0)
            {
                currentLineIndex = 0;
                return;
            }

            if (currentLineIndex >= lineCount)
            {
                currentLineIndex = lineCount - 1;
            }
        }

        private static int GetDialogueLineCount(P10DialogueNodeSO dialogueNode)
        {
            return dialogueNode != null && dialogueNode.DialogueLines != null
                ? dialogueNode.DialogueLines.Count
                : 0;
        }

        private int ResolveLatestLoggedLineIndex(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId) || dialogueLogEntries.Count == 0)
            {
                return 0;
            }

            P10DialogueNodeSO dialogueNode = ResolveDialogueNode(nodeId);
            int lineCount = GetDialogueLineCount(dialogueNode);
            if (lineCount <= 0)
            {
                return 0;
            }

            for (int entryIndex = dialogueLogEntries.Count - 1; entryIndex >= 0; entryIndex--)
            {
                P10DialogueLogEntry entry = dialogueLogEntries[entryIndex];
                if (!string.Equals(entry.NodeId, nodeId, System.StringComparison.Ordinal))
                {
                    continue;
                }

                for (int lineIndex = lineCount - 1; lineIndex >= 0; lineIndex--)
                {
                    P10DialogueLine line = dialogueNode.DialogueLines[lineIndex];
                    if (line != null && string.Equals(line.DialogueText, entry.DialogueText, System.StringComparison.Ordinal))
                    {
                        return lineIndex;
                    }
                }

                return 0;
            }

            return 0;
        }

        private void RecordDialogueLogEntry(string nodeId, string speakerName, string dialogueLine)
        {
            if (string.IsNullOrWhiteSpace(nodeId) || string.IsNullOrWhiteSpace(dialogueLine))
            {
                return;
            }

            if (dialogueLogEntries.Count > 0)
            {
                P10DialogueLogEntry lastEntry = dialogueLogEntries[dialogueLogEntries.Count - 1];
                if (string.Equals(lastEntry.NodeId, nodeId, System.StringComparison.Ordinal)
                    && string.Equals(lastEntry.DialogueText, dialogueLine, System.StringComparison.Ordinal))
                {
                    return;
                }
            }

            dialogueLogEntries.Add(new P10DialogueLogEntry(
                nextDialogueLogSequence,
                nodeId,
                string.IsNullOrWhiteSpace(speakerName) ? "旁白" : speakerName,
                dialogueLine));
            nextDialogueLogSequence++;
        }

        public int GetDialogueLogCount()
        {
            return dialogueLogEntries.Count;
        }

        public List<P10DialogueLogSnapshotEntry> ExportDialogueLogSnapshotEntries()
        {
            List<P10DialogueLogSnapshotEntry> snapshotEntries = new List<P10DialogueLogSnapshotEntry>();
            int startIndex = dialogueLogEntries.Count > MaxDialogueLogSnapshotEntries
                ? dialogueLogEntries.Count - MaxDialogueLogSnapshotEntries
                : 0;

            for (int i = startIndex; i < dialogueLogEntries.Count; i++)
            {
                P10DialogueLogEntry entry = dialogueLogEntries[i];
                snapshotEntries.Add(new P10DialogueLogSnapshotEntry(
                    entry.Sequence,
                    entry.NodeId,
                    entry.SpeakerName,
                    entry.DialogueText));
            }

            return snapshotEntries;
        }

        public void ImportDialogueLogSnapshotEntries(IReadOnlyList<P10DialogueLogSnapshotEntry> snapshotEntries)
        {
            dialogueLogEntries.Clear();
            nextDialogueLogSequence = 1;

            if (snapshotEntries != null)
            {
                for (int i = 0; i < snapshotEntries.Count; i++)
                {
                    P10DialogueLogSnapshotEntry snapshotEntry = snapshotEntries[i];
                    if (snapshotEntry == null || string.IsNullOrWhiteSpace(snapshotEntry.DialogueText))
                    {
                        continue;
                    }

                    int sequence = dialogueLogEntries.Count + 1;
                    dialogueLogEntries.Add(new P10DialogueLogEntry(
                        sequence,
                        snapshotEntry.NodeId,
                        string.IsNullOrWhiteSpace(snapshotEntry.SpeakerName) ? "旁白" : snapshotEntry.SpeakerName,
                        snapshotEntry.DialogueText));
                }

                nextDialogueLogSequence = dialogueLogEntries.Count + 1;
                if (nextDialogueLogSequence < 1)
                {
                    nextDialogueLogSequence = 1;
                }
            }

            if (runtimeSurface != null && runtimeSurface.IsDialogueLogVisible)
            {
                runtimeSurface.SetDialogueLogVisible(true, dialogueLogEntries);
            }
        }

        public void OpenDialogueLog()
        {
            EnsureRuntimeSurfaceInstance();

            if (runtimeSurface != null)
            {
                restoreDialogueAfterLogClose = runtimeSurface.IsDialogueVisible;
                runtimeSurface.SetDialogueLogVisible(true, dialogueLogEntries);
            }
        }

        public void CloseDialogueLog()
        {
            if (runtimeSurface != null)
            {
                runtimeSurface.SetDialogueLogVisible(false, dialogueLogEntries);
                if (restoreDialogueAfterLogClose)
                {
                    RefreshRuntimeSurface();
                }
            }
        }

        public void OpenCurrentOrderPanel()
        {
            EnsureRuntimeSurfaceInstance();

            if (runtimeSurface != null)
            {
                runtimeSurface.SetCurrentOrderPanelVisible(true, ResolveCurrentOrderDisplayData());
            }
        }

        public void CloseCurrentOrderPanel()
        {
            if (runtimeSurface != null)
            {
                runtimeSurface.SetCurrentOrderPanelVisible(false, ResolveCurrentOrderDisplayData());
            }
        }

        internal P10CurrentOrderDisplayData ResolveCurrentOrderDisplayData()
        {
            string nodeId = narrativeManager != null ? narrativeManager.GetCurrentNode() : currentNodeId;
            P10NarrativeState state = narrativeManager != null ? narrativeManager.GetCurrentState() : P10NarrativeState.None;

            if (string.Equals(nodeId, NodeOrder001Accept, System.StringComparison.Ordinal))
            {
                return P10CurrentOrderDisplayData.Active(
                    "甜白釉茶碗",
                    "周掌柜",
                    "制作 3 只甜白釉茶碗",
                    "碗口要宽，碗底要稳",
                    "白里带一点暖，不要冷冰冰",
                    "1250°C - 1300°C",
                    "器型评分 ≥ 80，釉色评分 ≥ 75",
                    "50 铜钱，声望 +10",
                    "进行中");
            }

            if (string.Equals(nodeId, NodeOrder003Accept, System.StringComparison.Ordinal))
            {
                return P10CurrentOrderDisplayData.Active(
                    "影青釉茶碗",
                    "陈书院",
                    "制作 1 只影青釉茶碗",
                    "碗口要正，腹部要收",
                    "半透半不透，像雾里的月",
                    "1250°C - 1280°C",
                    "器型评分 ≥ 80，釉色评分 ≥ 75",
                    "55 铜钱，声望 +10",
                    "进行中");
            }

            if (string.Equals(nodeId, NodeOrder004Accept, System.StringComparison.Ordinal))
            {
                return P10CurrentOrderDisplayData.Active(
                    "祭红釉香筒",
                    "卢客",
                    "制作 1 件祭红釉直筒香罐",
                    "直筒型，高一点，口径约十厘米，高约十三厘米",
                    "正红，不能暗，不能偏紫，不能发黑",
                    "1250°C - 1280°C",
                    "烧窑评分 ≥ 70，精品综合评分 ≥ 95",
                    "70 铜钱，声望 +10；精品可获得额外奖励",
                    "进行中");
            }

            return state == P10NarrativeState.Completed
                ? P10CurrentOrderDisplayData.Empty("第一章已完成")
                : P10CurrentOrderDisplayData.Empty("当前无订单");
        }

        private void RefreshCurrentOrderPanelIfVisible()
        {
            if (runtimeSurface != null && runtimeSurface.IsCurrentOrderPanelVisible)
            {
                runtimeSurface.RefreshCurrentOrderPanel(ResolveCurrentOrderDisplayData());
            }
        }

        private P10DialogueNodeSO ResolveDialogueNode(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                return null;
            }

            TryPopulateDialogueData();
            for (int i = 0; i < dialogueNodes.Count; i++)
            {
                P10DialogueNodeSO node = dialogueNodes[i];
                if (node != null && string.Equals(node.NodeId, nodeId, System.StringComparison.Ordinal))
                {
                    return node;
                }
            }

            return null;
        }

        private List<string> ResolveNextNodeIds(string nodeId)
        {
            P10DialogueNodeSO currentNode = ResolveDialogueNode(nodeId);
            if (currentNode != null)
            {
                return currentNode.NextNodeIds;
            }

            return P10DialogueCatalog.GetNextNodeIds(nodeId);
        }

        private P10CharacterDataSO ResolveCharacter(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            TryPopulateDialogueData();
            for (int i = 0; i < characters.Count; i++)
            {
                P10CharacterDataSO character = characters[i];
                if (character != null && string.Equals(character.CharacterId, characterId, System.StringComparison.Ordinal))
                {
                    return character;
                }
            }

            return null;
        }

        private void TryPopulateDialogueData()
        {
#if UNITY_EDITOR
            if (dialogueNodes.Count == 0)
            {
                string[] nodeGuids = AssetDatabase.FindAssets("t:P10DialogueNodeSO", new[] { "Assets/Phase10_Narrative/ScriptableObjects/Dialogues" });
                for (int i = 0; i < nodeGuids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(nodeGuids[i]);
                    P10DialogueNodeSO node = AssetDatabase.LoadAssetAtPath<P10DialogueNodeSO>(path);
                    if (node != null)
                    {
                        dialogueNodes.Add(node);
                    }
                }
            }

            if (characters.Count == 0)
            {
                string[] characterGuids = AssetDatabase.FindAssets("t:P10CharacterDataSO", new[] { "Assets/Phase10_Narrative/ScriptableObjects/Characters" });
                for (int i = 0; i < characterGuids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(characterGuids[i]);
                    P10CharacterDataSO character = AssetDatabase.LoadAssetAtPath<P10CharacterDataSO>(path);
                    if (character != null)
                    {
                        characters.Add(character);
                    }
                }
            }
#endif
        }
    }

    public readonly struct P10DialogueLogEntry
    {
        public P10DialogueLogEntry(int sequence, string nodeId, string speakerName, string dialogueText)
        {
            Sequence = sequence;
            NodeId = nodeId ?? string.Empty;
            SpeakerName = speakerName ?? string.Empty;
            DialogueText = dialogueText ?? string.Empty;
        }

        public int Sequence { get; }
        public string NodeId { get; }
        public string SpeakerName { get; }
        public string DialogueText { get; }
    }

    internal readonly struct P10ResolvedDialogueLine
    {
        public P10ResolvedDialogueLine(string speakerCharacterId, string dialogueText)
        {
            SpeakerCharacterId = speakerCharacterId ?? string.Empty;
            DialogueText = dialogueText ?? string.Empty;
        }

        public string SpeakerCharacterId { get; }
        public string DialogueText { get; }
    }

    internal readonly struct P10CurrentOrderDisplayData
    {
        private P10CurrentOrderDisplayData(
            bool hasOrder,
            string emptyMessage,
            string orderName,
            string sourceNpc,
            string currentGoal,
            string shapeHint,
            string glazeHint,
            string firingTemperature,
            string scoreRequirement,
            string reward,
            string status)
        {
            HasOrder = hasOrder;
            EmptyMessage = emptyMessage ?? string.Empty;
            OrderName = orderName ?? string.Empty;
            SourceNpc = sourceNpc ?? string.Empty;
            CurrentGoal = currentGoal ?? string.Empty;
            ShapeHint = shapeHint ?? string.Empty;
            GlazeHint = glazeHint ?? string.Empty;
            FiringTemperature = firingTemperature ?? string.Empty;
            ScoreRequirement = scoreRequirement ?? string.Empty;
            Reward = reward ?? string.Empty;
            Status = status ?? string.Empty;
        }

        public bool HasOrder { get; }
        public string EmptyMessage { get; }
        public string OrderName { get; }
        public string SourceNpc { get; }
        public string CurrentGoal { get; }
        public string ShapeHint { get; }
        public string GlazeHint { get; }
        public string FiringTemperature { get; }
        public string ScoreRequirement { get; }
        public string Reward { get; }
        public string Status { get; }

        public static P10CurrentOrderDisplayData Empty(string emptyMessage)
        {
            return new P10CurrentOrderDisplayData(false, emptyMessage, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        public static P10CurrentOrderDisplayData Active(
            string orderName,
            string sourceNpc,
            string currentGoal,
            string shapeHint,
            string glazeHint,
            string firingTemperature,
            string scoreRequirement,
            string reward,
            string status)
        {
            return new P10CurrentOrderDisplayData(true, string.Empty, orderName, sourceNpc, currentGoal, shapeHint, glazeHint, firingTemperature, scoreRequirement, reward, status);
        }
    }

    public static class P10DialogueCatalog
    {
        private static readonly Dictionary<string, string> SpeakerNames = new Dictionary<string, string>
        {
            { "P10_CH01_NPC_001_XuLaoBo_Placeholder", "徐老伯" },
            { "P10_CH01_NPC_002_ZhouZhangGui_Placeholder", "周掌柜" },
            { "P10_CH01_NPC_003_ChenShuYuan_Placeholder", "陈书院" },
            { "P10_CH01_NPC_004_LuKe_Placeholder", "卢客" }
        };

        private static readonly Dictionary<string, string> SpeakerByNodeId = new Dictionary<string, string>
        {
            { "P10_CH01_NODE_PROLOGUE_01", "P10_CH01_NPC_001_XuLaoBo_Placeholder" },
            { "P10_CH01_NODE_TUTORIAL_01", "P10_CH01_NPC_001_XuLaoBo_Placeholder" },
            { "P10_CH01_NODE_CHAPTER_ENDING", "P10_CH01_NPC_001_XuLaoBo_Placeholder" },
            { "P10_CH01_NODE_ORDER_001_ACCEPT", "P10_CH01_NPC_002_ZhouZhangGui_Placeholder" },
            { "P10_CH01_NODE_ORDER_001_PASS", "P10_CH01_NPC_002_ZhouZhangGui_Placeholder" },
            { "P10_CH01_NODE_ORDER_001_FAIL", "P10_CH01_NPC_002_ZhouZhangGui_Placeholder" },
            { "P10_CH01_NODE_ORDER_003_ACCEPT", "P10_CH01_NPC_003_ChenShuYuan_Placeholder" },
            { "P10_CH01_NODE_ORDER_003_PASS", "P10_CH01_NPC_003_ChenShuYuan_Placeholder" },
            { "P10_CH01_NODE_ORDER_003_FAIL", "P10_CH01_NPC_003_ChenShuYuan_Placeholder" },
            { "P10_CH01_NODE_ORDER_004_ACCEPT", "P10_CH01_NPC_004_LuKe_Placeholder" },
            { "P10_CH01_NODE_ORDER_004_PASS_NORMAL", "P10_CH01_NPC_004_LuKe_Placeholder" },
            { "P10_CH01_NODE_ORDER_004_FAIL", "P10_CH01_NPC_004_LuKe_Placeholder" },
            { "P10_CH01_NODE_ORDER_004_CLIMAX", "P10_CH01_NPC_004_LuKe_Placeholder" }
        };

        private static readonly Dictionary<string, string> DialogueByNodeId = new Dictionary<string, string>
        {
            { "P10_CH01_NODE_PROLOGUE_01", "窑炉安静着，但这个姓氏还没有彻底熄灭。" },
            { "P10_CH01_NODE_TUTORIAL_01", "先把作坊的节奏找回来，再去接外头的信任。" },
            { "P10_CH01_NODE_ORDER_001_ACCEPT", "第一笔小订单上门，考的是重开窑后的第一份信用。" },
            { "P10_CH01_NODE_ORDER_001_PASS", "第一批交付让周掌柜确认，这座窑还接得住承诺。" },
            { "P10_CH01_NODE_ORDER_001_FAIL", "失手不是停火的理由，它先变成一堂要补上的课。" },
            { "P10_CH01_NODE_ORDER_003_ACCEPT", "书院先生要的是细致，不能只靠寻常买卖的手法应付。" },
            { "P10_CH01_NODE_ORDER_003_PASS", "陈先生从器物里看见了规矩，也终于肯多说一句认可。" },
            { "P10_CH01_NODE_ORDER_003_FAIL", "陈先生的失望提醒你，品质这件事没有侥幸。" },
            { "P10_CH01_NODE_ORDER_004_ACCEPT", "第一章最后的订单问的不是熟练，而是这座窑能不能找回心气。" },
            { "P10_CH01_NODE_ORDER_004_PASS_NORMAL", "订单稳稳交出去，窑厂挣到一个朴素但真实的新开始。" },
            { "P10_CH01_NODE_ORDER_004_FAIL", "最后一单没站稳，但作坊已经有了继续下去的底气。" },
            { "P10_CH01_NODE_ORDER_004_CLIMAX", "一次亮眼的开窑，让重燃的家业真正有了抬头的可能。" },
            { "P10_CH01_NODE_CHAPTER_ENDING", "第一章收束在重新点起的窑火里，门外还有下一重担子。" }
        };

        private static readonly Dictionary<string, List<string>> NextNodeIdsByNodeId = new Dictionary<string, List<string>>
        {
            { "P10_CH01_NODE_PROLOGUE_01", new List<string> { "P10_CH01_NODE_TUTORIAL_01" } },
            { "P10_CH01_NODE_TUTORIAL_01", new List<string> { "P10_CH01_NODE_ORDER_001_ACCEPT" } },
            { "P10_CH01_NODE_ORDER_001_ACCEPT", new List<string> { "P10_CH01_NODE_ORDER_001_PASS" } },
            { "P10_CH01_NODE_ORDER_001_PASS", new List<string> { "P10_CH01_NODE_ORDER_003_ACCEPT" } },
            { "P10_CH01_NODE_ORDER_003_ACCEPT", new List<string> { "P10_CH01_NODE_ORDER_003_PASS" } },
            { "P10_CH01_NODE_ORDER_003_PASS", new List<string> { "P10_CH01_NODE_ORDER_004_ACCEPT" } },
            { "P10_CH01_NODE_ORDER_004_ACCEPT", new List<string> { "P10_CH01_NODE_ORDER_004_PASS_NORMAL" } },
            { "P10_CH01_NODE_ORDER_004_PASS_NORMAL", new List<string> { "P10_CH01_NODE_CHAPTER_ENDING" } },
            { "P10_CH01_NODE_ORDER_004_CLIMAX", new List<string> { "P10_CH01_NODE_CHAPTER_ENDING" } }
        };

        public static string GetSpeakerNameForNode(string nodeId)
        {
            if (!string.IsNullOrWhiteSpace(nodeId) && SpeakerByNodeId.TryGetValue(nodeId, out string speakerId))
            {
                if (SpeakerNames.TryGetValue(speakerId, out string speakerName))
                {
                    return speakerName;
                }
            }

            return "旁白";
        }

        public static string GetSpeakerCharacterId(string nodeId)
        {
            if (!string.IsNullOrWhiteSpace(nodeId) && SpeakerByNodeId.TryGetValue(nodeId, out string speakerId))
            {
                return speakerId;
            }

            return string.Empty;
        }

        public static string GetDialogueText(string nodeId)
        {
            if (!string.IsNullOrWhiteSpace(nodeId) && DialogueByNodeId.TryGetValue(nodeId, out string dialogueText))
            {
                return dialogueText;
            }

            return string.IsNullOrWhiteSpace(nodeId) ? string.Empty : "剧情节点已到达。";
        }

        public static List<string> GetNextNodeIds(string nodeId)
        {
            if (!string.IsNullOrWhiteSpace(nodeId) && NextNodeIdsByNodeId.TryGetValue(nodeId, out List<string> nextNodeIds))
            {
                return nextNodeIds;
            }

            return null;
        }
    }

    internal sealed class P10DialogueRuntimeSurface : MonoBehaviour
    {
        private Canvas canvas;
        private Text speakerText;
        private Text bodyText;
        private Button nextButton;
        private Button closeButton;
        private Button logButton;
        private RectTransform topRightActionBar;
        private Button persistentLogButton;
        private Button persistentOrderButton;
        private CanvasGroup dialoguePanelGroup;
        private CanvasGroup logPanelGroup;
        private CanvasGroup currentOrderPanelGroup;
        private RectTransform logContentRoot;
        private RectTransform currentOrderContentRoot;
        private Text currentOrderContentText;
        private Image speakerPortraitImage;
        private bool isBuilt;
        private bool usesRuntimeCreatedFixedTextSlots;
        private static Font resolvedBuiltinFont;
        private static bool fontWarningLogged;
        private static readonly Dictionary<string, Sprite> artSpriteCache = new Dictionary<string, Sprite>();
        private static readonly Color ReadableTextColor = Color.black;
        private static readonly Color MutedReadableTextColor = new Color(0f, 0f, 0f, 0.62f);
        private const float TopRightButtonWidth = 126f;
        private const float TopRightButtonHeight = 54f;
        private const string DefaultSpeakerName = "\u65c1\u767d";
        private const string SpeakerSeparator = "\uff1a";

        public Text SpeakerText
        {
            get { return speakerText; }
        }

        public Text BodyText
        {
            get { return bodyText; }
        }

        public bool UsesRuntimeCreatedFixedTextSlots
        {
            get { return usesRuntimeCreatedFixedTextSlots; }
        }

        public void BindFixedSlots(Text boundSpeakerText, Text boundBodyText)
        {
            if (boundSpeakerText != null)
            {
                speakerText = boundSpeakerText;
            }

            if (boundBodyText != null)
            {
                bodyText = boundBodyText;
            }
        }

        public void Build(P10DialogueController controller)
        {
            if (isBuilt)
            {
                return;
            }

            EnsureEventSystem();

            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            RectTransform rootRect = GetComponent<RectTransform>();
            if (rootRect == null)
            {
                rootRect = gameObject.AddComponent<RectTransform>();
            }

            Transform dialoguePanelTransform = transform.Find("DialoguePanel");
            GameObject dialoguePanelObject = dialoguePanelTransform != null
                ? dialoguePanelTransform.gameObject
                : new GameObject("DialoguePanel", typeof(RectTransform));
            if (dialoguePanelTransform == null)
            {
                dialoguePanelObject.transform.SetParent(transform, false);
            }

            RectTransform dialoguePanelRect = dialoguePanelObject.GetComponent<RectTransform>();
            dialoguePanelRect.anchorMin = new Vector2(0.02f, 0.02f);
            dialoguePanelRect.anchorMax = new Vector2(0.98f, 0.36f);
            dialoguePanelRect.offsetMin = Vector2.zero;
            dialoguePanelRect.offsetMax = Vector2.zero;

            dialoguePanelGroup = dialoguePanelObject.GetComponent<CanvasGroup>();
            if (dialoguePanelGroup == null)
            {
                dialoguePanelGroup = dialoguePanelObject.AddComponent<CanvasGroup>();
            }

            Image panelImage = FindOrCreatePanel(dialoguePanelObject.transform);
            ApplyArtSprite(panelImage, "P10Art/Dialog/dialogue_bg", new Color(0f, 0f, 0f, 0.72f), Image.Type.Simple);
            Transform panel = panelImage.transform;
            EnsureDialoguePanelClickTarget(dialoguePanelObject, controller);
            EnsureDialoguePanelClickTarget(panel.gameObject, controller);

            speakerPortraitImage = EnsureImage(panel, "P10_SpeakerPortraitImage", new Vector2(0.02f, 0.23f), new Vector2(0.18f, 0.86f));
            speakerPortraitImage.preserveAspect = true;
            speakerPortraitImage.raycastTarget = false;
            speakerPortraitImage.color = new Color(1f, 1f, 1f, 0.92f);

            Image speakerPlateImage = EnsureImage(panel, "P10_DialogueSpeakerNameplate", new Vector2(0.20f, 0.77f), new Vector2(0.58f, 0.96f));
            ApplyArtSprite(speakerPlateImage, "P10Art/Dialog/nameplate", new Color(1f, 1f, 1f, 0.12f), Image.Type.Simple);
            speakerPlateImage.raycastTarget = false;

            Image bodyTextureImage = EnsureImage(panel, "P10_DialogueBodyTexture", new Vector2(0.20f, 0.23f), new Vector2(0.98f, 0.77f));
            ApplyArtSprite(bodyTextureImage, "P10Art/Dialog/body_texture", new Color(1f, 1f, 1f, 0.06f), Image.Type.Simple);
            bodyTextureImage.raycastTarget = false;

            speakerText = FindTextSlot(panel, "P10_DialogueSpeakerText", speakerText);
            if (speakerText == null)
            {
                speakerText = CreateText("P10_DialogueSpeakerText", panel, 22, FontStyle.Bold);
                RectTransform speakerRect = speakerText.rectTransform;
                speakerRect.anchorMin = new Vector2(0f, 0.78f);
                speakerRect.anchorMax = new Vector2(1f, 1f);
                speakerRect.offsetMin = new Vector2(18f, 0f);
                speakerRect.offsetMax = new Vector2(-18f, -12f);
                usesRuntimeCreatedFixedTextSlots = true;
                Debug.LogWarning("P10 dialogue UI created fallback P10_DialogueSpeakerText. Edit the Phase10 prefab to make this slot persistent.");
            }
            ConfigureDialogueTextSlot(
                speakerText,
                panel,
                new Vector2(0.21f, 0.78f),
                new Vector2(0.58f, 0.97f),
                new Vector2(18f, 2f),
                new Vector2(-12f, -2f),
                26,
                FontStyle.Bold,
                TextAnchor.MiddleLeft);

            bodyText = FindTextSlot(panel, "P10_DialogueBodyText", bodyText);
            if (bodyText == null)
            {
                bodyText = CreateText("P10_DialogueBodyText", panel, 18, FontStyle.Normal);
                RectTransform bodyRect = bodyText.rectTransform;
                bodyRect.anchorMin = new Vector2(0f, 0.24f);
                bodyRect.anchorMax = new Vector2(1f, 0.80f);
                bodyRect.offsetMin = new Vector2(18f, 0f);
                bodyRect.offsetMax = new Vector2(-18f, 0f);
                usesRuntimeCreatedFixedTextSlots = true;
                Debug.LogWarning("P10 dialogue UI created fallback P10_DialogueBodyText. Edit the Phase10 prefab to make this slot persistent.");
            }
            ConfigureDialogueTextSlot(
                bodyText,
                panel,
                new Vector2(0.29f, 0.34f),
                new Vector2(0.95f, 0.69f),
                new Vector2(10f, 4f),
                new Vector2(-20f, -2f),
                20,
                FontStyle.Normal,
                TextAnchor.UpperLeft);

            speakerPortraitImage.transform.SetSiblingIndex(1);
            speakerPlateImage.transform.SetSiblingIndex(2);
            bodyTextureImage.transform.SetSiblingIndex(3);
            speakerText.transform.SetAsLastSibling();
            bodyText.transform.SetAsLastSibling();

            EnsureDialogueTextClickTarget(speakerText, controller);
            EnsureDialogueTextClickTarget(bodyText, controller);

            logButton = FindButton(panel, "LogButton");
            if (logButton == null)
            {
                logButton = CreateButton("LogButton", panel, "记录", new Vector2(0.80f, 0.035f), new Vector2(0.985f, 0.24f));
            }
            ConfigureImageOnlyButton(logButton, new Vector2(0.80f, 0.035f), new Vector2(0.985f, 0.24f));

            ApplyButtonArt(logButton, "P10Art/Dialog/button_log");

            nextButton = FindButton(panel, "NextButton");
            ApplyButtonArt(nextButton, "P10Art/Dialog/button_continue");

            closeButton = FindButton(panel, "CloseButton");
            if (closeButton == null)
            {
                closeButton = CreateButton("CloseButton", panel, "关闭", new Vector2(0.86f, 0.035f), new Vector2(0.99f, 0.24f));
            }
            DisableAndHideButton(closeButton, controller);
            logButton.transform.SetAsLastSibling();

            BuildDialogueLogPanel(controller);
            topRightActionBar = EnsureTopRightActionBar();
            persistentLogButton = FindButton(topRightActionBar, "PersistentLogButton");
            if (persistentLogButton == null)
            {
                persistentLogButton = FindButton(transform, "PersistentLogButton");
                if (persistentLogButton == null)
                {
                    persistentLogButton = CreateButton("PersistentLogButton", topRightActionBar, "记录", Vector2.zero, Vector2.one);
                }
            }
            ConfigureActionBarButton(persistentLogButton, "记录");

            ApplyButtonArt(persistentLogButton, "P10Art/Dialog/button_log");

            BuildCurrentOrderPanel(controller);
            persistentOrderButton = FindButton(topRightActionBar, "PersistentOrderButton");
            if (persistentOrderButton == null)
            {
                persistentOrderButton = FindButton(transform, "PersistentOrderButton");
                if (persistentOrderButton == null)
                {
                    persistentOrderButton = CreateButton("PersistentOrderButton", topRightActionBar, "订单", Vector2.zero, Vector2.one);
                }
            }
            ConfigureActionBarButton(persistentOrderButton, "订单");

            ApplyButtonArt(persistentOrderButton, "P10Art/Dialog/button_order");

            logButton.onClick.RemoveListener(controller.OpenDialogueLog);
            logButton.onClick.AddListener(controller.OpenDialogueLog);
            persistentLogButton.onClick.RemoveListener(controller.OpenDialogueLog);
            persistentLogButton.onClick.AddListener(controller.OpenDialogueLog);
            persistentOrderButton.onClick.RemoveListener(controller.OpenCurrentOrderPanel);
            persistentOrderButton.onClick.AddListener(controller.OpenCurrentOrderPanel);
            DisableContinueButton(nextButton, controller);
            DisableAndHideButton(closeButton, controller);
            SetDialogueLogVisible(false, controller.DialogueLogEntries);
            SetCurrentOrderPanelVisible(false, controller.ResolveCurrentOrderDisplayData());
            BringActionBarToFront();
            SetVisible(false);
            isBuilt = true;
        }

        private static void DisableContinueButton(Button button, P10DialogueController controller)
        {
            if (button == null)
            {
                return;
            }

            if (controller != null)
            {
                button.onClick.RemoveListener(controller.AdvanceDialogue);
                button.onClick.RemoveListener(controller.AdvanceDialogueFromDialogueBoxClick);
            }

            button.interactable = false;
            button.gameObject.SetActive(false);
        }

        private static void DisableAndHideButton(Button button, P10DialogueController controller)
        {
            if (button == null)
            {
                return;
            }

            if (controller != null)
            {
                button.onClick.RemoveListener(controller.CloseDialogue);
                button.onClick.RemoveListener(controller.AdvanceDialogue);
                button.onClick.RemoveListener(controller.AdvanceDialogueFromDialogueBoxClick);
            }

            button.interactable = false;

            Graphic[] graphics = button.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].raycastTarget = false;
            }

            button.gameObject.SetActive(false);
        }

        private static void EnsureDialoguePanelClickTarget(GameObject target, P10DialogueController controller)
        {
            if (target == null)
            {
                return;
            }

            Image image = target.GetComponent<Image>();
            if (image == null)
            {
                image = target.AddComponent<Image>();
                image.color = new Color(0f, 0f, 0f, 0f);
            }

            image.raycastTarget = true;

            P10DialogueClickAdvanceTarget clickTarget = target.GetComponent<P10DialogueClickAdvanceTarget>();
            if (clickTarget == null)
            {
                clickTarget = target.AddComponent<P10DialogueClickAdvanceTarget>();
            }

            clickTarget.Bind(controller);
        }

        private static void EnsureDialogueTextClickTarget(Text text, P10DialogueController controller)
        {
            if (text == null)
            {
                return;
            }

            text.raycastTarget = true;

            P10DialogueClickAdvanceTarget clickTarget = text.GetComponent<P10DialogueClickAdvanceTarget>();
            if (clickTarget == null)
            {
                clickTarget = text.gameObject.AddComponent<P10DialogueClickAdvanceTarget>();
            }

            clickTarget.Bind(controller);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("P10_Runtime_EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private void RefreshSpeakerPortrait(string speakerCharacterId, string speakerName)
        {
            if (speakerPortraitImage == null)
            {
                return;
            }

            string portraitPath = ResolveSpeakerPortraitPath(speakerCharacterId, speakerName);
            ApplyArtSprite(speakerPortraitImage, portraitPath, new Color(1f, 1f, 1f, 0.92f), Image.Type.Simple);
            speakerPortraitImage.enabled = speakerPortraitImage.sprite != null;
        }

        private static string ResolveSpeakerPortraitPath(string speakerCharacterId, string speakerName)
        {
            string source = ((speakerCharacterId ?? string.Empty) + " " + (speakerName ?? string.Empty)).ToLowerInvariant();
            if (source.Contains("xulaobo") || source.Contains("xu") || source.Contains("001"))
            {
                return "P10Art/NPC/xulaobo_avatar";
            }

            if (source.Contains("zhouzhanggui") || source.Contains("zhou") || source.Contains("002"))
            {
                return "P10Art/NPC/zhouzhanggui_avatar";
            }

            if (source.Contains("chenshuyuan") || source.Contains("chen") || source.Contains("003"))
            {
                return "P10Art/NPC/chenshuyuan_avatar";
            }

            if (source.Contains("luke") || source.Contains("lu") || source.Contains("004"))
            {
                return "P10Art/NPC/luke_avatar";
            }

            return "P10Art/NPC/system_avatar";
        }

        public void SetDialogue(string speakerName, string dialogueLine, string speakerCharacterId)
        {
            string resolvedSpeakerName = string.IsNullOrWhiteSpace(speakerName) ? DefaultSpeakerName : speakerName;
            if (speakerText != null)
            {
                speakerText.text = resolvedSpeakerName;
            }

            if (bodyText != null)
            {
                bodyText.text = NormalizeBodyDialogueLine(resolvedSpeakerName, dialogueLine);
            }

            RefreshSpeakerPortrait(speakerCharacterId, resolvedSpeakerName);
            SetVisible(!string.IsNullOrWhiteSpace(dialogueLine));
        }

        public void SetVisible(bool isVisible)
        {
            if (canvas != null)
            {
                canvas.enabled = true;
            }

            if (dialoguePanelGroup != null)
            {
                dialoguePanelGroup.alpha = isVisible ? 1f : 0f;
                dialoguePanelGroup.interactable = isVisible;
                dialoguePanelGroup.blocksRaycasts = isVisible;
            }

            if (speakerText != null)
            {
                speakerText.enabled = isVisible;
            }

            if (bodyText != null)
            {
                bodyText.enabled = isVisible;
            }
        }

        public bool IsDialogueVisible
        {
            get { return dialoguePanelGroup != null && dialoguePanelGroup.alpha > 0.5f; }
        }

        public void SetDialogueLogVisible(bool isVisible, IReadOnlyList<P10DialogueLogEntry> entries)
        {
            if (logPanelGroup == null)
            {
                return;
            }

            if (isVisible)
            {
                RebuildDialogueLogContent(entries);
            }

            logPanelGroup.alpha = isVisible ? 1f : 0f;
            logPanelGroup.interactable = isVisible;
            logPanelGroup.blocksRaycasts = isVisible;

            RefreshPersistentActionButtons();
        }

        public bool IsDialogueLogVisible
        {
            get { return logPanelGroup != null && logPanelGroup.alpha > 0.5f; }
        }

        public bool IsCurrentOrderPanelVisible
        {
            get { return currentOrderPanelGroup != null && currentOrderPanelGroup.alpha > 0.5f; }
        }

        public void SetCurrentOrderPanelVisible(bool isVisible, P10CurrentOrderDisplayData orderData)
        {
            if (currentOrderPanelGroup == null)
            {
                return;
            }

            if (isVisible)
            {
                RefreshCurrentOrderPanel(orderData);
            }

            currentOrderPanelGroup.alpha = isVisible ? 1f : 0f;
            currentOrderPanelGroup.interactable = isVisible;
            currentOrderPanelGroup.blocksRaycasts = isVisible;

            RefreshPersistentActionButtons();
        }

        private void RefreshPersistentActionButtons()
        {
            bool dialogueLogVisible = IsDialogueLogVisible;
            bool currentOrderPanelVisible = IsCurrentOrderPanelVisible;
            bool actionBarVisible = !dialogueLogVisible && !currentOrderPanelVisible;

            if (topRightActionBar != null)
            {
                topRightActionBar.gameObject.SetActive(actionBarVisible);
            }

            if (persistentOrderButton != null)
            {
                persistentOrderButton.gameObject.SetActive(actionBarVisible);
            }

            if (persistentLogButton != null)
            {
                persistentLogButton.gameObject.SetActive(actionBarVisible);
            }

            if (actionBarVisible)
            {
                BringActionBarToFront();
            }
        }

        private void BringActionBarToFront()
        {
            if (topRightActionBar == null)
            {
                return;
            }

            topRightActionBar.SetAsLastSibling();

            if (persistentLogButton != null)
            {
                persistentLogButton.interactable = true;
                persistentLogButton.transform.SetAsLastSibling();
            }

            if (persistentOrderButton != null)
            {
                persistentOrderButton.interactable = true;
                persistentOrderButton.transform.SetAsLastSibling();
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(topRightActionBar);
        }

        private RectTransform EnsureTopRightActionBar()
        {
            Transform existing = transform.Find("P10_TopRightActionBar");
            GameObject barObject = existing != null
                ? existing.gameObject
                : new GameObject("P10_TopRightActionBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            if (existing == null)
            {
                barObject.transform.SetParent(transform, false);
            }

            RectTransform rect = barObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-34f, -26f);
            rect.sizeDelta = new Vector2(TopRightButtonWidth * 2f + 22f, TopRightButtonHeight);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;

            HorizontalLayoutGroup layout = barObject.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = barObject.AddComponent<HorizontalLayoutGroup>();
            }

            layout.childAlignment = TextAnchor.MiddleRight;
            layout.spacing = 22f;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return rect;
        }

        private static void ConfigureActionBarButton(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            if (button.transform.parent == null || button.transform.parent.name != "P10_TopRightActionBar")
            {
                Transform surface = button.transform.root;
                Transform bar = FindDescendant(surface, "P10_TopRightActionBar");
                if (bar != null)
                {
                    button.transform.SetParent(bar, false);
                }
            }

            if (button.name == "PersistentLogButton")
            {
                button.transform.SetSiblingIndex(0);
            }
            else if (button.name == "PersistentOrderButton")
            {
                button.transform.SetSiblingIndex(1);
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(TopRightButtonWidth, TopRightButtonHeight);
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
            }

            LayoutElement layoutElement = button.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = button.gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.preferredWidth = TopRightButtonWidth;
            layoutElement.preferredHeight = TopRightButtonHeight;
            layoutElement.minWidth = TopRightButtonWidth;
            layoutElement.minHeight = TopRightButtonHeight;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;

            Text[] labelTexts = button.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labelTexts.Length; i++)
            {
                labelTexts[i].text = string.Empty;
                labelTexts[i].enabled = false;
            }

            Image actionImage = button.GetComponent<Image>();
            if (actionImage != null)
            {
                actionImage.preserveAspect = true;
                actionImage.raycastTarget = true;
            }
        }

        private static void ConfigureDialogueTextSlot(
            Text text,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax,
            int fontSize,
            FontStyle fontStyle,
            TextAnchor alignment)
        {
            if (text == null)
            {
                return;
            }

            if (parent != null && text.transform.parent != parent)
            {
                text.transform.SetParent(parent, false);
            }

            text.font = ResolveBuiltinFont();
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = ReadableTextColor;
            text.raycastTarget = true;

            RectTransform rect = text.rectTransform;
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.SetAsLastSibling();
        }

        private static Image EnsureImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            Transform existing = parent != null ? parent.Find(name) : null;
            bool created = existing == null;
            GameObject imageObject = existing != null
                ? existing.gameObject
                : new GameObject(name, typeof(RectTransform), typeof(Image));
            if (created && parent != null)
            {
                imageObject.transform.SetParent(parent, false);
            }

            RectTransform rect = imageObject.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = imageObject.AddComponent<RectTransform>();
            }

            if (created)
            {
                rect.anchorMin = anchorMin;
                rect.anchorMax = anchorMax;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            Image image = imageObject.GetComponent<Image>();
            if (image == null)
            {
                image = imageObject.AddComponent<Image>();
            }

            return image;
        }

        private static void ApplyButtonArt(Button button, string resourcePath)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image == null)
            {
                image = button.gameObject.AddComponent<Image>();
            }

            ApplyArtSprite(image, resourcePath, new Color(1f, 1f, 1f, 0.18f), Image.Type.Simple);
        }

        private static void ConfigurePanelCloseButton(Button button, Vector2 anchorMin, Vector2 anchorMax)
        {
            ConfigureImageOnlyButton(button, anchorMin, anchorMax);
        }

        private static void ConfigureImageOnlyButton(Button button, Vector2 anchorMin, Vector2 anchorMax)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = anchorMin;
                rect.anchorMax = anchorMax;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
            }

            Text[] labelTexts = button.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labelTexts.Length; i++)
            {
                labelTexts[i].text = string.Empty;
                labelTexts[i].enabled = false;
            }

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.preserveAspect = true;
                image.raycastTarget = true;
            }
        }

        private static void ApplyArtSprite(Image image, string resourcePath, Color fallbackColor, Image.Type imageType)
        {
            if (image == null)
            {
                return;
            }

            Sprite sprite = LoadP10ArtSprite(resourcePath);
            if (sprite == null)
            {
                image.sprite = null;
                image.color = fallbackColor;
                image.type = Image.Type.Simple;
                return;
            }

            image.sprite = sprite;
            image.color = Color.white;
            image.type = imageType;
        }

        private static Sprite LoadP10ArtSprite(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return null;
            }

            string cacheKey = resourcePath;
            Rect? spriteRect = ResolveSpriteRect(resourcePath);
            if (spriteRect.HasValue)
            {
                Rect rect = spriteRect.Value;
                cacheKey = resourcePath + "|" + rect.x + "," + rect.y + "," + rect.width + "," + rect.height;
            }

            if (artSpriteCache.TryGetValue(cacheKey, out Sprite cachedSprite))
            {
                return cachedSprite;
            }

            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null)
            {
                return null;
            }

            Rect sourceRect = spriteRect ?? new Rect(0f, 0f, texture.width, texture.height);
            Sprite sprite = Sprite.Create(
                texture,
                sourceRect,
                new Vector2(0.5f, 0.5f),
                100f);
            artSpriteCache[cacheKey] = sprite;
            return sprite;
        }

        private static Rect? ResolveSpriteRect(string resourcePath)
        {
            if (string.Equals(resourcePath, "P10Art/Dialog/button_log", System.StringComparison.Ordinal))
            {
                return new Rect(54f, 238f, 548f, 156f);
            }

            if (string.Equals(resourcePath, "P10Art/Dialog/button_order", System.StringComparison.Ordinal))
            {
                return new Rect(28f, 164f, 566f, 272f);
            }

            if (string.Equals(resourcePath, "P10Art/Dialog/button_close", System.StringComparison.Ordinal))
            {
                return new Rect(58f, 232f, 558f, 180f);
            }

            return null;
        }

        public void RefreshCurrentOrderPanel(P10CurrentOrderDisplayData orderData)
        {
            if (currentOrderContentText != null)
            {
                currentOrderContentText.text = FormatCurrentOrderPanelText(orderData);
            }
        }

        private void BuildDialogueLogPanel(P10DialogueController controller)
        {
            Transform existingPanel = transform.Find("DialogueLogPanel");
            GameObject panelObject = existingPanel != null
                ? existingPanel.gameObject
                : new GameObject("DialogueLogPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            if (existingPanel == null)
            {
                panelObject.transform.SetParent(transform, false);
            }

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            if (existingPanel == null)
            {
                panelRect.anchorMin = new Vector2(0.06f, 0.18f);
                panelRect.anchorMax = new Vector2(0.94f, 0.96f);
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
            }

            Image panelImage = panelObject.GetComponent<Image>();
            if (panelImage == null)
            {
                panelImage = panelObject.AddComponent<Image>();
            }

            ApplyArtSprite(panelImage, "P10Art/Log/log_bg", new Color(0.03f, 0.03f, 0.03f, 0.94f), Image.Type.Simple);

            logPanelGroup = panelObject.GetComponent<CanvasGroup>();
            if (logPanelGroup == null)
            {
                logPanelGroup = panelObject.AddComponent<CanvasGroup>();
            }

            Text titleText = FindTextSlot(panelObject.transform, "DialogueLogTitle", null);
            if (titleText == null)
            {
                titleText = CreateText("DialogueLogTitle", panelObject.transform, 24, FontStyle.Bold);
                RectTransform titleRect = titleText.rectTransform;
                titleRect.anchorMin = new Vector2(0f, 0.88f);
                titleRect.anchorMax = new Vector2(0.76f, 1f);
                titleRect.offsetMin = new Vector2(28f, 0f);
                titleRect.offsetMax = new Vector2(-8f, -12f);
            }

            titleText.text = "对话记录";
            titleText.color = ReadableTextColor;

            Button closeLogButton = FindButton(panelObject.transform, "DialogueLogCloseButton");
            if (closeLogButton == null)
            {
                closeLogButton = CreateButton("DialogueLogCloseButton", panelObject.transform, "关闭", new Vector2(0.74f, 0.875f), new Vector2(0.985f, 0.995f));
            }
            ConfigurePanelCloseButton(closeLogButton, new Vector2(0.74f, 0.875f), new Vector2(0.985f, 0.995f));
            ApplyButtonArt(closeLogButton, "P10Art/Dialog/button_close");

            closeLogButton.onClick.RemoveListener(controller.CloseDialogueLog);
            closeLogButton.onClick.AddListener(controller.CloseDialogueLog);

            Transform existingScroll = panelObject.transform.Find("DialogueLogScroll");
            GameObject scrollObject = existingScroll != null
                ? existingScroll.gameObject
                : new GameObject("DialogueLogScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            if (existingScroll == null)
            {
                scrollObject.transform.SetParent(panelObject.transform, false);
            }

            RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
            if (existingScroll == null)
            {
                scrollRectTransform.anchorMin = new Vector2(0.04f, 0.07f);
                scrollRectTransform.anchorMax = new Vector2(0.96f, 0.84f);
                scrollRectTransform.offsetMin = Vector2.zero;
                scrollRectTransform.offsetMax = Vector2.zero;
            }

            Image scrollImage = scrollObject.GetComponent<Image>();
            if (scrollImage == null)
            {
                scrollImage = scrollObject.AddComponent<Image>();
            }

            ApplyArtSprite(scrollImage, "P10Art/Dialog/body_texture", new Color(1f, 1f, 1f, 0.06f), Image.Type.Simple);

            Transform existingViewport = scrollObject.transform.Find("Viewport");
            GameObject viewportObject = existingViewport != null
                ? existingViewport.gameObject
                : new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            if (existingViewport == null)
            {
                viewportObject.transform.SetParent(scrollObject.transform, false);
            }

            RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
            if (existingViewport == null)
            {
                viewportRect.anchorMin = Vector2.zero;
                viewportRect.anchorMax = Vector2.one;
                viewportRect.offsetMin = new Vector2(18f, 18f);
                viewportRect.offsetMax = new Vector2(-18f, -18f);
            }

            Image viewportImage = viewportObject.GetComponent<Image>();
            if (viewportImage == null)
            {
                viewportImage = viewportObject.AddComponent<Image>();
            }

            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
            Mask viewportMask = viewportObject.GetComponent<Mask>();
            if (viewportMask == null)
            {
                viewportMask = viewportObject.AddComponent<Mask>();
            }

            viewportMask.showMaskGraphic = false;

            Transform existingContent = viewportObject.transform.Find("Content");
            GameObject contentObject = existingContent != null
                ? existingContent.gameObject
                : new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            if (existingContent == null)
            {
                contentObject.transform.SetParent(viewportObject.transform, false);
            }

            logContentRoot = contentObject.GetComponent<RectTransform>();
            if (existingContent == null)
            {
                logContentRoot.anchorMin = new Vector2(0f, 1f);
                logContentRoot.anchorMax = new Vector2(1f, 1f);
                logContentRoot.pivot = new Vector2(0.5f, 1f);
                logContentRoot.offsetMin = Vector2.zero;
                logContentRoot.offsetMax = Vector2.zero;
            }

            VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = contentObject.AddComponent<VerticalLayoutGroup>();
            }

            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 16f;
            layout.padding = new RectOffset(12, 12, 10, 22);

            ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = contentObject.AddComponent<ContentSizeFitter>();
            }

            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                scrollRect = scrollObject.AddComponent<ScrollRect>();
            }

            Scrollbar logScrollbar = EnsureVerticalScrollbar(scrollObject.transform, "DialogueLogScrollbar");
            ApplyScrollbarArt(logScrollbar);

            scrollRect.viewport = viewportRect;
            scrollRect.content = logContentRoot;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.verticalScrollbar = logScrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.verticalScrollbarSpacing = 8f;
            scrollRect.scrollSensitivity = 28f;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
        }

        private void BuildCurrentOrderPanel(P10DialogueController controller)
        {
            Transform existingPanel = transform.Find("CurrentOrderPanel");
            GameObject panelObject = existingPanel != null
                ? existingPanel.gameObject
                : new GameObject("CurrentOrderPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            if (existingPanel == null)
            {
                panelObject.transform.SetParent(transform, false);
            }

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            if (existingPanel == null)
            {
                panelRect.anchorMin = new Vector2(0.60f, 0.30f);
                panelRect.anchorMax = new Vector2(0.96f, 0.88f);
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
            }

            Image panelImage = panelObject.GetComponent<Image>();
            if (panelImage == null)
            {
                panelImage = panelObject.AddComponent<Image>();
            }

            ApplyArtSprite(panelImage, "P10Art/Dialog/dialogue_bg", new Color(0.03f, 0.03f, 0.03f, 0.94f), Image.Type.Simple);

            currentOrderPanelGroup = panelObject.GetComponent<CanvasGroup>();
            if (currentOrderPanelGroup == null)
            {
                currentOrderPanelGroup = panelObject.AddComponent<CanvasGroup>();
            }

            Text titleText = FindTextSlot(panelObject.transform, "CurrentOrderTitleText", null);
            if (titleText == null)
            {
                titleText = CreateText("CurrentOrderTitleText", panelObject.transform, 24, FontStyle.Bold);
                RectTransform titleRect = titleText.rectTransform;
                titleRect.anchorMin = new Vector2(0f, 0.88f);
                titleRect.anchorMax = new Vector2(0.70f, 1f);
                titleRect.offsetMin = new Vector2(24f, 0f);
                titleRect.offsetMax = new Vector2(-8f, -12f);
            }

            titleText.text = "当前订单";
            titleText.color = ReadableTextColor;

            Button closeOrderButton = FindButton(panelObject.transform, "CurrentOrderCloseButton");
            if (closeOrderButton == null)
            {
                closeOrderButton = CreateButton("CurrentOrderCloseButton", panelObject.transform, "关闭", new Vector2(0.72f, 0.875f), new Vector2(0.985f, 0.995f));
            }
            ConfigurePanelCloseButton(closeOrderButton, new Vector2(0.72f, 0.875f), new Vector2(0.985f, 0.995f));
            ApplyButtonArt(closeOrderButton, "P10Art/Dialog/button_close");

            closeOrderButton.onClick.RemoveListener(controller.CloseCurrentOrderPanel);
            closeOrderButton.onClick.AddListener(controller.CloseCurrentOrderPanel);

            Transform existingScroll = panelObject.transform.Find("CurrentOrderScroll");
            GameObject scrollObject = existingScroll != null
                ? existingScroll.gameObject
                : new GameObject("CurrentOrderScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            if (existingScroll == null)
            {
                scrollObject.transform.SetParent(panelObject.transform, false);
            }

            RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
            if (existingScroll == null)
            {
                scrollRectTransform.anchorMin = new Vector2(0.04f, 0.06f);
                scrollRectTransform.anchorMax = new Vector2(0.96f, 0.86f);
                scrollRectTransform.offsetMin = Vector2.zero;
                scrollRectTransform.offsetMax = Vector2.zero;
            }

            Image scrollImage = scrollObject.GetComponent<Image>();
            if (scrollImage == null)
            {
                scrollImage = scrollObject.AddComponent<Image>();
            }

            ApplyArtSprite(scrollImage, "P10Art/Dialog/body_texture", new Color(1f, 1f, 1f, 0.04f), Image.Type.Simple);

            Transform existingViewport = scrollObject.transform.Find("Viewport");
            GameObject viewportObject = existingViewport != null
                ? existingViewport.gameObject
                : new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            if (existingViewport == null)
            {
                viewportObject.transform.SetParent(scrollObject.transform, false);
            }

            RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
            if (existingViewport == null)
            {
                viewportRect.anchorMin = Vector2.zero;
                viewportRect.anchorMax = Vector2.one;
                viewportRect.offsetMin = new Vector2(18f, 16f);
                viewportRect.offsetMax = new Vector2(-34f, -16f);
            }

            Image viewportImage = viewportObject.GetComponent<Image>();
            if (viewportImage == null)
            {
                viewportImage = viewportObject.AddComponent<Image>();
            }

            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
            Mask viewportMask = viewportObject.GetComponent<Mask>();
            if (viewportMask == null)
            {
                viewportMask = viewportObject.AddComponent<Mask>();
            }

            viewportMask.showMaskGraphic = false;

            Transform existingContent = viewportObject.transform.Find("Content");
            GameObject contentObject = existingContent != null
                ? existingContent.gameObject
                : new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            if (existingContent == null)
            {
                contentObject.transform.SetParent(viewportObject.transform, false);
            }

            currentOrderContentRoot = contentObject.GetComponent<RectTransform>();
            if (existingContent == null)
            {
                currentOrderContentRoot.anchorMin = new Vector2(0f, 1f);
                currentOrderContentRoot.anchorMax = new Vector2(1f, 1f);
                currentOrderContentRoot.pivot = new Vector2(0.5f, 1f);
                currentOrderContentRoot.offsetMin = Vector2.zero;
                currentOrderContentRoot.offsetMax = Vector2.zero;
            }

            VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = contentObject.AddComponent<VerticalLayoutGroup>();
            }

            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 0f;
            layout.padding = new RectOffset(6, 10, 6, 14);

            ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = contentObject.AddComponent<ContentSizeFitter>();
            }

            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            currentOrderContentText = FindTextSlot(contentObject.transform, "CurrentOrderContentText", null);
            if (currentOrderContentText == null)
            {
                Transform oldContentText = panelObject.transform.Find("CurrentOrderContentText");
                if (oldContentText != null)
                {
                    currentOrderContentText = oldContentText.GetComponent<Text>();
                    if (currentOrderContentText != null)
                    {
                        currentOrderContentText.transform.SetParent(contentObject.transform, false);
                    }
                }
            }

            if (currentOrderContentText == null)
            {
                currentOrderContentText = CreateText("CurrentOrderContentText", contentObject.transform, 18, FontStyle.Normal);
            }

            EnsureCurrentOrderPropStrip(contentObject.transform);

            RectTransform contentTextRect = currentOrderContentText.rectTransform;
            contentTextRect.anchorMin = new Vector2(0f, 1f);
            contentTextRect.anchorMax = new Vector2(1f, 1f);
            contentTextRect.pivot = new Vector2(0.5f, 1f);
            contentTextRect.offsetMin = Vector2.zero;
            contentTextRect.offsetMax = Vector2.zero;
            currentOrderContentText.horizontalOverflow = HorizontalWrapMode.Wrap;
            currentOrderContentText.verticalOverflow = VerticalWrapMode.Overflow;
            currentOrderContentText.color = ReadableTextColor;
            ContentSizeFitter contentTextFitter = currentOrderContentText.GetComponent<ContentSizeFitter>();
            if (contentTextFitter == null)
            {
                contentTextFitter = currentOrderContentText.gameObject.AddComponent<ContentSizeFitter>();
            }

            contentTextFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            currentOrderContentText.text = FormatCurrentOrderPanelText(controller.ResolveCurrentOrderDisplayData());

            ScrollRect orderScrollRect = scrollObject.GetComponent<ScrollRect>();
            if (orderScrollRect == null)
            {
                orderScrollRect = scrollObject.AddComponent<ScrollRect>();
            }

            Scrollbar orderScrollbar = EnsureVerticalScrollbar(scrollObject.transform, "CurrentOrderScrollbar");
            ApplyScrollbarArt(orderScrollbar);
            orderScrollRect.viewport = viewportRect;
            orderScrollRect.content = currentOrderContentRoot;
            orderScrollRect.horizontal = false;
            orderScrollRect.vertical = true;
            orderScrollRect.verticalScrollbar = orderScrollbar;
            orderScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            orderScrollRect.verticalScrollbarSpacing = 8f;
            orderScrollRect.scrollSensitivity = 28f;
            orderScrollRect.movementType = ScrollRect.MovementType.Clamped;
        }

        private static void EnsureCurrentOrderPropStrip(Transform parent)
        {
            Transform existing = parent.Find("P10_CurrentOrderPropStrip");
            GameObject stripObject = existing != null
                ? existing.gameObject
                : new GameObject("P10_CurrentOrderPropStrip", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            if (existing == null)
            {
                stripObject.transform.SetParent(parent, false);
            }

            RectTransform stripRect = stripObject.GetComponent<RectTransform>();
            stripRect.anchorMin = new Vector2(0f, 1f);
            stripRect.anchorMax = new Vector2(1f, 1f);
            stripRect.pivot = new Vector2(0.5f, 1f);
            stripRect.offsetMin = Vector2.zero;
            stripRect.offsetMax = Vector2.zero;

            LayoutElement stripLayout = stripObject.GetComponent<LayoutElement>();
            stripLayout.minHeight = 54f;
            stripLayout.preferredHeight = 54f;

            HorizontalLayoutGroup layout = stripObject.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 8f;
            layout.padding = new RectOffset(0, 0, 0, 8);
            stripObject.transform.SetAsFirstSibling();

            EnsurePropIcon(stripObject.transform, "P10_PropIconReward", "P10Art/Props/reward_icon");
            EnsurePropIcon(stripObject.transform, "P10_PropIconLedger", "P10Art/Props/father_ledger");
            EnsurePropIcon(stripObject.transform, "P10_PropIconKilnTool", "P10Art/Props/old_kiln_tool");
            EnsurePropIcon(stripObject.transform, "P10_PropIconOrder", "P10Art/Props/ancient_order");
        }

        private static void EnsurePropIcon(Transform parent, string name, string resourcePath)
        {
            Image image = EnsureImage(parent, name, Vector2.zero, Vector2.one);
            image.preserveAspect = true;
            image.raycastTarget = false;
            ApplyArtSprite(image, resourcePath, new Color(1f, 1f, 1f, 0.16f), Image.Type.Simple);

            RectTransform rect = image.rectTransform;
            rect.sizeDelta = new Vector2(46f, 46f);

            LayoutElement layout = image.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = image.gameObject.AddComponent<LayoutElement>();
            }

            layout.minWidth = 46f;
            layout.minHeight = 46f;
            layout.preferredWidth = 46f;
            layout.preferredHeight = 46f;
        }

        private static Scrollbar EnsureVerticalScrollbar(Transform scrollRoot, string name)
        {
            Transform existing = scrollRoot.Find(name);
            GameObject scrollbarObject = existing != null
                ? existing.gameObject
                : new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            if (existing == null)
            {
                scrollbarObject.transform.SetParent(scrollRoot, false);
            }

            RectTransform rect = scrollbarObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(18f, 0f);
            rect.offsetMin = new Vector2(-18f, 8f);
            rect.offsetMax = new Vector2(0f, -8f);

            Image trackImage = scrollbarObject.GetComponent<Image>();
            if (trackImage == null)
            {
                trackImage = scrollbarObject.AddComponent<Image>();
            }

            trackImage.color = new Color(1f, 1f, 1f, 0.12f);
            trackImage.raycastTarget = true;

            Transform slidingArea = scrollbarObject.transform.Find("Sliding Area");
            GameObject slidingAreaObject = slidingArea != null
                ? slidingArea.gameObject
                : new GameObject("Sliding Area", typeof(RectTransform));
            if (slidingArea == null)
            {
                slidingAreaObject.transform.SetParent(scrollbarObject.transform, false);
            }

            RectTransform slidingRect = slidingAreaObject.GetComponent<RectTransform>();
            slidingRect.anchorMin = Vector2.zero;
            slidingRect.anchorMax = Vector2.one;
            slidingRect.offsetMin = new Vector2(3f, 3f);
            slidingRect.offsetMax = new Vector2(-3f, -3f);

            Transform handle = slidingAreaObject.transform.Find("Handle");
            GameObject handleObject = handle != null
                ? handle.gameObject
                : new GameObject("Handle", typeof(RectTransform), typeof(Image));
            if (handle == null)
            {
                handleObject.transform.SetParent(slidingAreaObject.transform, false);
            }

            RectTransform handleRect = handleObject.GetComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.offsetMin = Vector2.zero;
            handleRect.offsetMax = Vector2.zero;

            Image handleImage = handleObject.GetComponent<Image>();
            if (handleImage == null)
            {
                handleImage = handleObject.AddComponent<Image>();
            }

            handleImage.color = new Color(1f, 1f, 1f, 0.42f);
            handleImage.raycastTarget = true;

            Scrollbar scrollbar = scrollbarObject.GetComponent<Scrollbar>();
            if (scrollbar == null)
            {
                scrollbar = scrollbarObject.AddComponent<Scrollbar>();
            }

            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.targetGraphic = handleImage;
            scrollbar.handleRect = handleRect;
            scrollbar.size = 0.35f;
            scrollbar.value = 1f;
            return scrollbar;
        }

        private static void ApplyScrollbarArt(Scrollbar scrollbar)
        {
            if (scrollbar == null)
            {
                return;
            }

            Image trackImage = scrollbar.GetComponent<Image>();
            ApplyArtSprite(trackImage, "P10Art/Log/scrollbar_track", new Color(1f, 1f, 1f, 0.12f), Image.Type.Simple);

            Image handleImage = scrollbar.handleRect != null
                ? scrollbar.handleRect.GetComponent<Image>()
                : null;
            ApplyArtSprite(handleImage, "P10Art/Log/scrollbar_handle", new Color(1f, 1f, 1f, 0.42f), Image.Type.Simple);
        }

        private void RebuildDialogueLogContent(IReadOnlyList<P10DialogueLogEntry> entries)
        {
            if (logContentRoot == null)
            {
                return;
            }

            for (int i = logContentRoot.childCount - 1; i >= 0; i--)
            {
                DestroyUiObject(logContentRoot.GetChild(i).gameObject);
            }

            if (entries == null || entries.Count == 0)
            {
                Text emptyText = CreateText("EmptyDialogueLog", logContentRoot, 18, FontStyle.Italic);
                emptyText.color = ReadableTextColor;
                emptyText.text = "暂无对话记录";
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                CreateDialogueLogEntryView(entries[i], logContentRoot);
            }
        }

        private static void CreateDialogueLogEntryView(P10DialogueLogEntry entry, Transform parent)
        {
            GameObject itemObject = new GameObject("DialogueLogEntry_" + entry.Sequence, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            itemObject.transform.SetParent(parent, false);

            Image itemImage = itemObject.GetComponent<Image>();
            ApplyArtSprite(itemImage, "P10Art/Log/log_item_bg", new Color(1f, 1f, 1f, 0.08f), Image.Type.Simple);
            itemImage.raycastTarget = false;

            VerticalLayoutGroup layout = itemObject.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 7f;
            layout.padding = new RectOffset(18, 18, 12, 14);

            ContentSizeFitter fitter = itemObject.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Text speaker = CreateText("Speaker", itemObject.transform, 20, FontStyle.Bold);
            string resolvedSpeakerName = string.IsNullOrWhiteSpace(entry.SpeakerName) ? "旁白" : entry.SpeakerName;
            speaker.color = ReadableTextColor;
            speaker.text = resolvedSpeakerName + "：";
            speaker.alignment = TextAnchor.UpperLeft;
            speaker.horizontalOverflow = HorizontalWrapMode.Wrap;

            Text line = CreateText("Line", itemObject.transform, 19, FontStyle.Normal);
            line.color = ReadableTextColor;
            line.text = NormalizeBodyDialogueLine(resolvedSpeakerName, entry.DialogueText);
            line.alignment = TextAnchor.UpperLeft;

            Text meta = CreateText("Meta", itemObject.transform, 12, FontStyle.Normal);
            meta.color = MutedReadableTextColor;
            meta.text = "记录 #" + entry.Sequence;
        }

        private static string NormalizeBodyDialogueLine(string speakerName, string dialogueLine)
        {
            if (string.IsNullOrWhiteSpace(dialogueLine))
            {
                return string.Empty;
            }

            string localized = LocalizePlayerFacingTerms(dialogueLine);
            return StripKnownSpeakerPrefix(localized, speakerName);
        }

        private static string StripKnownSpeakerPrefix(string value, string currentSpeakerName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            List<string> knownSpeakerNames = new List<string>
            {
                currentSpeakerName,
                DefaultSpeakerName,
                "徐老伯",
                "周掌柜",
                "陈书院",
                "卢客",
                "系统提示",
                "UI结果",
                "玩家"
            };

            string trimmed = value.TrimStart();
            for (int i = 0; i < knownSpeakerNames.Count; i++)
            {
                string speakerName = knownSpeakerNames[i];
                if (string.IsNullOrWhiteSpace(speakerName))
                {
                    continue;
                }

                string fullWidthPrefix = speakerName + SpeakerSeparator;
                if (trimmed.StartsWith(fullWidthPrefix, System.StringComparison.Ordinal))
                {
                    return trimmed.Substring(fullWidthPrefix.Length).TrimStart();
                }

                string halfWidthPrefix = speakerName + ":";
                if (trimmed.StartsWith(halfWidthPrefix, System.StringComparison.Ordinal))
                {
                    return trimmed.Substring(halfWidthPrefix.Length).TrimStart();
                }
            }

            return value;
        }

        private static string LocalizePlayerFacingTerms(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string localized = value;
            localized = localized.Replace("ShapeScore", "器型评分");
            localized = localized.Replace("GlazeScore", "釉色评分");
            localized = localized.Replace("FireScore", "烧窑评分");
            localized = localized.Replace("ResultCalculator", "综合评分");
            localized = localized.Replace("PerfectScore", "精品评分线");
            localized = localized.Replace("Difficulty", "难度");
            localized = localized.Replace("Reward", "奖励");
            localized = localized.Replace("Reputation", "声望");
            localized = localized.Replace("GLAZE_001", "影青釉");
            localized = localized.Replace("GLAZE_002", "甜白釉");
            localized = localized.Replace("GLAZE_004", "祭红釉");
            localized = localized.Replace("CODEX_006", "影青釉图鉴");
            localized = localized.Replace("CODEX_007", "甜白釉图鉴");
            localized = localized.Replace("CODEX_009", "祭红釉图鉴");
            localized = localized.Replace("SHAPE_001", "碗");
            localized = localized.Replace("SHAPE_002", "盘");
            localized = localized.Replace("SHAPE_005", "罐");
            localized = localized.Replace("ORDER_001", "甜白釉茶碗订单");
            localized = localized.Replace("ORDER_003", "影青釉茶碗订单");
            localized = localized.Replace("ORDER_004", "祭红釉香筒订单");
            return localized;
        }

        private static string FormatCurrentOrderPanelText(P10CurrentOrderDisplayData orderData)
        {
            if (!orderData.HasOrder)
            {
                return string.IsNullOrWhiteSpace(orderData.EmptyMessage) ? "当前无订单" : orderData.EmptyMessage;
            }

            return
                "订单名称：" + orderData.OrderName + "\n" +
                "来源 NPC：" + orderData.SourceNpc + "\n" +
                "当前目标：" + orderData.CurrentGoal + "\n" +
                "器型提示：" + orderData.ShapeHint + "\n" +
                "釉色提示：" + orderData.GlazeHint + "\n" +
                "烧成温度：" + orderData.FiringTemperature + "\n" +
                "评分要求：" + orderData.ScoreRequirement + "\n" +
                "奖励：" + orderData.Reward + "\n" +
                "状态：" + orderData.Status;
        }

        private static void DestroyUiObject(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private static Image CreatePanel(Transform parent)
        {
            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(12f, 12f);
            rect.offsetMax = new Vector2(-12f, -12f);

            Image image = panel.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.72f);
            return image;
        }

        private static Image FindOrCreatePanel(Transform parent)
        {
            Transform panelTransform = parent.Find("Panel");
            if (panelTransform != null)
            {
                Image existingImage = panelTransform.GetComponent<Image>();
                if (existingImage != null)
                {
                    return existingImage;
                }

                return panelTransform.gameObject.AddComponent<Image>();
            }

            return CreatePanel(parent);
        }

        private static Text FindTextSlot(Transform searchRoot, string name, Text boundText)
        {
            if (boundText != null && boundText.name == name)
            {
                return boundText;
            }

            Transform match = FindDescendant(searchRoot, name);
            return match != null ? match.GetComponent<Text>() : null;
        }

        private static Button FindButton(Transform searchRoot, string name)
        {
            Transform match = FindDescendant(searchRoot, name);
            return match != null ? match.GetComponent<Button>() : null;
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            if (root == null || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (root.name == name)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                Transform match = FindDescendant(child, name);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static Text CreateText(string name, Transform parent, int fontSize, FontStyle fontStyle)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            Text text = textObject.GetComponent<Text>();
            text.font = ResolveBuiltinFont();
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.color = ReadableTextColor;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.18f);

            Text labelText = CreateText(GetButtonLabelObjectName(name, label), buttonObject.transform, 18, FontStyle.Bold);
            labelText.text = label;
            labelText.font = ResolveBuiltinFont();
            labelText.fontSize = 18;
            labelText.fontStyle = FontStyle.Bold;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
            labelText.verticalOverflow = VerticalWrapMode.Overflow;
            RectTransform labelRect = labelText.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            return buttonObject.GetComponent<Button>();
        }

        private static string GetButtonLabelObjectName(string buttonName, string label)
        {
            if (buttonName == "LogButton" || buttonName == "PersistentLogButton")
            {
                return "LogLabel";
            }

            if (buttonName == "NextButton")
            {
                return "NextLabel";
            }

            if (buttonName == "CloseButton" || buttonName == "DialogueLogCloseButton")
            {
                return "CloseLabel";
            }

            return label + "Label";
        }

        private static Font ResolveBuiltinFont()
        {
            if (resolvedBuiltinFont != null)
            {
                return resolvedBuiltinFont;
            }

            resolvedBuiltinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (resolvedBuiltinFont == null)
            {
                resolvedBuiltinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            if (resolvedBuiltinFont == null && !fontWarningLogged)
            {
                Debug.LogWarning("P10 dialogue runtime UI could not resolve built-in font LegacyRuntime.ttf or Arial.ttf.");
                fontWarningLogged = true;
            }

            return resolvedBuiltinFont;
        }
    }

    internal sealed class P10DialogueClickAdvanceTarget : MonoBehaviour, IPointerClickHandler
    {
        private P10DialogueController controller;

        public void Bind(P10DialogueController owner)
        {
            controller = owner;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData != null && eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (controller != null)
            {
                controller.AdvanceDialogueFromDialogueBoxClick();
            }
        }
    }
}
