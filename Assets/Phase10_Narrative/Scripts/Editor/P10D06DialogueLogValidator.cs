using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Phase10_Narrative
{
    public static class P10D06DialogueLogValidator
    {
        private const string NodePrologue = "P10_CH01_NODE_PROLOGUE_01";
        private const string NodeTutorial = "P10_CH01_NODE_TUTORIAL_01";
        private const string ExpectedPrologueSpeaker = "旁白";
        private const string ExpectedTutorialSpeaker = "徐老伯";

        public static void RunP10D06DialogueLogValidation()
        {
            try
            {
                ValidateDialogueLogRecordingAndPanel();
                Debug.Log("P10D-06 Dialogue Log validation passed.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10D-06 Dialogue Log validation failed: " + ex.Message);
                EditorApplication.Exit(1);
            }
        }

        public static void RunP10D10DialogueLogSnapshotValidation()
        {
            try
            {
                ValidateDialogueLogSnapshotRoundTrip();
                Debug.Log("P10D-10 Dialogue Log Snapshot Extension validation passed.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10D-10 Dialogue Log Snapshot Extension validation failed: " + ex.Message);
                EditorApplication.Exit(1);
            }
        }

        public static void RunP10D18RuntimeUILabelLocalizationValidation()
        {
            try
            {
                ValidateDialogueLogRecordingAndPanel();
                ValidateDialogueLogSnapshotRoundTrip();
                ValidateRuntimeChineseLabelsDoNotAdvanceNarrative();
                Debug.Log("P10D-18 Runtime UI Chinese Label Localization validation passed.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10D-18 Runtime UI Chinese Label Localization validation failed: " + ex.Message);
                EditorApplication.Exit(1);
            }
        }

        private static void ValidateDialogueLogRecordingAndPanel()
        {
            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject root = new GameObject("P10D06_DialogueLogRoot");

            try
            {
                P10NarrativeManager manager = CreateManager(root.transform);
                P10DialogueController controller = CreateDialogueController(root.transform, manager);
                controller.EnsureRuntimeSurfaceInstance();
                AssertRuntimeSurfaceLayout(controller);
                AssertLogCount(controller, 0, "Dialogue log should start empty.");
                controller.OpenDialogueLog();
                if (!controller.IsDialogueLogVisible)
                {
                    throw new InvalidOperationException("Persistent dialogue log did not open while no dialogue was active.");
                }

                AssertEmptyDialogueLogView(controller);
                controller.CloseDialogueLog();
                if (controller.IsDialogueLogVisible)
                {
                    throw new InvalidOperationException("Empty dialogue log panel did not close.");
                }

                AssertLogCount(controller, 0, "Opening an empty dialogue log should not create log entries.");

                if (!manager.HandleSceneTrigger("StartPrologue", NodePrologue))
                {
                    throw new InvalidOperationException("Could not enter prologue.");
                }

                controller.SetCurrentNode(manager.GetCurrentNode());
                AssertLogCount(controller, 1, "Prologue did not record one dialogue log entry.");
                AssertLatestEntry(controller, 1, NodePrologue, ExpectedPrologueSpeaker);
                AssertDialogueLogEntryViews(controller);
                controller.CloseDialogueLog();

                controller.SetCurrentNode(manager.GetCurrentNode());
                AssertLogCount(controller, 1, "Repeated SetCurrentNode duplicated the prologue log entry.");
                controller.CloseDialogue();
                AssertCurrentDialogueUiHidden(controller);
                controller.OpenDialogueLog();
                if (!controller.IsDialogueLogVisible)
                {
                    throw new InvalidOperationException("Persistent dialogue log did not open after the dialogue UI was closed.");
                }

                AssertLogCount(controller, 1, "Opening log while dialogue UI is closed created a duplicate entry.");
                controller.CloseDialogueLog();
                AssertCurrentDialogueUiHidden(controller);
                AssertLogCount(controller, 1, "Closing log while dialogue UI is closed changed log count.");

                AdvanceDialogueUntilNode(controller, manager, NodeTutorial);
                if (manager.GetCurrentNode() != NodeTutorial)
                {
                    throw new InvalidOperationException("Dialogue did not advance to tutorial.");
                }

                controller.SetCurrentNode(manager.GetCurrentNode());
                AssertLatestEntry(controller, controller.GetDialogueLogCount(), NodeTutorial, ExpectedTutorialSpeaker);
                AssertAllEntriesHaveSpeaker(controller);

                controller.OpenDialogueLog();
                if (!controller.IsDialogueLogVisible)
                {
                    throw new InvalidOperationException("Dialogue log panel did not open.");
                }
                AssertDialogueLogEntryViews(controller);

                controller.CloseDialogueLog();
                if (controller.IsDialogueLogVisible)
                {
                    throw new InvalidOperationException("Dialogue log panel did not close.");
                }

                AssertCurrentDialogueUiVisible(controller, NodeTutorial);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyGeneratedEventSystemIfNeeded(existingEventSystem);
            }
        }

        private static void ValidateDialogueLogSnapshotRoundTrip()
        {
            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject root = new GameObject("P10D10_DialogueLogSnapshotRoot");

            try
            {
                P10NarrativeManager manager = CreateManager(root.transform);
                P10DialogueController controller = CreateDialogueController(manager.transform, manager);
                controller.EnsureRuntimeSurfaceInstance();

                if (!manager.HandleSceneTrigger("StartPrologue", NodePrologue))
                {
                    throw new InvalidOperationException("Could not enter prologue for snapshot save.");
                }

                controller.SetCurrentNode(manager.GetCurrentNode());
                P10NarrativeSnapshot savedSnapshot = manager.SaveSnapshot();

                if (savedSnapshot.SnapshotVersion != 2)
                {
                    throw new InvalidOperationException("Snapshot version must be 2.");
                }

                if (savedSnapshot.DialogueLogEntries == null || savedSnapshot.DialogueLogEntries.Count != 1)
                {
                    throw new InvalidOperationException("SaveSnapshot did not include dialogue log entries.");
                }

                if (savedSnapshot.DialogueLogEntries[0].Sequence != 1
                    || savedSnapshot.DialogueLogEntries[0].NodeId != NodePrologue
                    || savedSnapshot.DialogueLogEntries[0].SpeakerName != controller.DialogueLogEntries[0].SpeakerName
                    || string.IsNullOrWhiteSpace(savedSnapshot.DialogueLogEntries[0].DialogueText))
                {
                    throw new InvalidOperationException("Saved dialogue log entry did not match runtime log data.");
                }

                ValidateMaxDialogueLogSnapshotEntries(root.transform);
                ValidateDialogueLogSnapshotLoad(root.transform, savedSnapshot);
                ValidateV1SnapshotCompatibility(root.transform);
                ValidateDuplicateBaselineAfterRestore(root.transform);
                ValidateImportedSequenceNormalization(root.transform);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyGeneratedEventSystemIfNeeded(existingEventSystem);
            }
        }

        private static void ValidateMaxDialogueLogSnapshotEntries(Transform parent)
        {
            P10NarrativeManager manager = CreateManager(parent);
            P10DialogueController controller = CreateDialogueController(manager.transform, manager);

            List<P10DialogueLogSnapshotEntry> seedEntries = new List<P10DialogueLogSnapshotEntry>();
            for (int i = 1; i <= 205; i++)
            {
                seedEntries.Add(new P10DialogueLogSnapshotEntry(
                    i,
                    "P10_TEST_NODE_" + i,
                    "Narrative",
                    "Line " + i));
            }

            controller.ImportDialogueLogSnapshotEntries(seedEntries);
            P10NarrativeSnapshot snapshot = manager.SaveSnapshot();
            if (snapshot.DialogueLogEntries == null || snapshot.DialogueLogEntries.Count != 200)
            {
                throw new InvalidOperationException("Dialogue log snapshot did not cap saved entries at 200.");
            }

            if (snapshot.DialogueLogEntries[0].Sequence != 6 || snapshot.DialogueLogEntries[199].Sequence != 205)
            {
                throw new InvalidOperationException("Dialogue log snapshot must keep newest entries when over capacity.");
            }
        }

        private static void ValidateDialogueLogSnapshotLoad(Transform parent, P10NarrativeSnapshot sourceSnapshot)
        {
            P10NarrativeManager restoredManager = CreateManager(parent);
            P10DialogueController restoredController = CreateDialogueController(restoredManager.transform, restoredManager);
            restoredController.EnsureRuntimeSurfaceInstance();

            restoredManager.LoadSnapshot(sourceSnapshot);

            if (restoredManager.GetCurrentState() != sourceSnapshot.ChapterState
                || restoredManager.GetCurrentNode() != sourceSnapshot.CurrentNodeId)
            {
                throw new InvalidOperationException("LoadSnapshot changed narrative state incorrectly.");
            }

            AssertLogCount(restoredController, 1, "LoadSnapshot did not restore dialogue log entries.");
            AssertLatestEntry(restoredController, 1, NodePrologue, sourceSnapshot.DialogueLogEntries[0].SpeakerName);

            restoredController.OpenDialogueLog();
            AssertDialogueLogEntryViews(restoredController);
            restoredController.CloseDialogueLog();
        }

        private static void ValidateV1SnapshotCompatibility(Transform parent)
        {
            P10NarrativeManager manager = CreateManager(parent);
            P10DialogueController controller = CreateDialogueController(manager.transform, manager);
            controller.ImportDialogueLogSnapshotEntries(new List<P10DialogueLogSnapshotEntry>
            {
                new P10DialogueLogSnapshotEntry(10, NodePrologue, "Xu Lao Bo", P10DialogueCatalog.GetDialogueText(NodePrologue))
            });

            P10NarrativeSnapshot v1Snapshot = new P10NarrativeSnapshot
            {
                SnapshotVersion = 1,
                ChapterState = P10NarrativeState.Prologue,
                CurrentNodeId = NodePrologue,
                PlayedNodeIds = new List<string>(),
                NarrativeFlags = new Dictionary<string, bool>(),
                DialogueLogEntries = null
            };

            manager.LoadSnapshot(v1Snapshot);

            if (manager.GetCurrentState() != P10NarrativeState.Prologue || manager.GetCurrentNode() != NodePrologue)
            {
                throw new InvalidOperationException("v1 snapshot did not restore narrative state.");
            }

            AssertLogCount(controller, 0, "v1 snapshot load must restore dialogue log as empty.");
            controller.OpenDialogueLog();
            AssertEmptyDialogueLogView(controller);
            controller.CloseDialogueLog();
        }

        private static void ValidateDuplicateBaselineAfterRestore(Transform parent)
        {
            P10NarrativeManager manager = CreateManager(parent);
            P10DialogueController controller = CreateDialogueController(manager.transform, manager);
            controller.EnsureRuntimeSurfaceInstance();

            if (!manager.HandleSceneTrigger("StartPrologue", NodePrologue))
            {
                throw new InvalidOperationException("Could not enter prologue for duplicate baseline validation.");
            }

            controller.SetCurrentNode(manager.GetCurrentNode());
            AssertLogCount(controller, 1, "Duplicate baseline seed should record one prologue entry.");
            P10DialogueLogEntry baselineEntry = controller.DialogueLogEntries[0];

            P10NarrativeSnapshot snapshot = new P10NarrativeSnapshot
            {
                SnapshotVersion = 2,
                ChapterState = P10NarrativeState.Prologue,
                CurrentNodeId = NodePrologue,
                PlayedNodeIds = new List<string>(),
                NarrativeFlags = new Dictionary<string, bool>(),
                DialogueLogEntries = new List<P10DialogueLogSnapshotEntry>
                {
                    new P10DialogueLogSnapshotEntry(7, baselineEntry.NodeId, baselineEntry.SpeakerName, baselineEntry.DialogueText)
                }
            };

            manager.LoadSnapshot(snapshot);
            AssertLogCount(controller, 1, "Snapshot restore should import exactly one dialogue log entry.");

            controller.SetCurrentNode(manager.GetCurrentNode());
            AssertLogCount(controller, 1, "Restored current node refresh duplicated the last dialogue log entry.");

            if (!manager.TryAdvanceDialogueNode(NodePrologue, NodeTutorial))
            {
                throw new InvalidOperationException("Could not advance restored manager to tutorial.");
            }

            controller.SetCurrentNode(manager.GetCurrentNode());
            AssertLogCount(controller, 2, "New post-restore dialogue line was not recorded.");
            AssertLatestEntry(controller, 2, NodeTutorial, ExpectedTutorialSpeaker);
        }

        private static void ValidateImportedSequenceNormalization(Transform parent)
        {
            P10NarrativeManager manager = CreateManager(parent);
            P10DialogueController controller = CreateDialogueController(manager.transform, manager);
            controller.EnsureRuntimeSurfaceInstance();

            controller.ImportDialogueLogSnapshotEntries(new List<P10DialogueLogSnapshotEntry>
            {
                new P10DialogueLogSnapshotEntry(10, "P10_TEST_NODE_A", "Narrative", "Line A"),
                new P10DialogueLogSnapshotEntry(10, "P10_TEST_NODE_B", "Narrative", "Line B"),
                new P10DialogueLogSnapshotEntry(0, "P10_TEST_NODE_C", "Narrative", "Line C")
            });

            AssertLogCount(controller, 3, "Sequence normalization should keep all valid imported entries.");
            AssertEntrySequence(controller, 0, 1);
            AssertEntrySequence(controller, 1, 2);
            AssertEntrySequence(controller, 2, 3);

            P10NarrativeSnapshot snapshot = new P10NarrativeSnapshot
            {
                SnapshotVersion = 2,
                ChapterState = P10NarrativeState.Prologue,
                CurrentNodeId = NodePrologue,
                PlayedNodeIds = new List<string>(),
                NarrativeFlags = new Dictionary<string, bool>(),
                DialogueLogEntries = controller.ExportDialogueLogSnapshotEntries()
            };

            manager.LoadSnapshot(snapshot);
            controller.SetCurrentNode(manager.GetCurrentNode());
            AssertLatestEntry(controller, 4, NodePrologue, ExpectedPrologueSpeaker);
        }

        private static P10NarrativeManager CreateManager(Transform parent)
        {
            GameObject host = new GameObject("P10D06_Manager");
            host.transform.SetParent(parent, false);
            P10NarrativeManager manager = host.AddComponent<P10NarrativeManager>();
            InvokeAwake(manager);
            return manager;
        }

        private static P10DialogueController CreateDialogueController(Transform parent, P10NarrativeManager manager)
        {
            GameObject host = new GameObject("P10D06_DialogueController");
            host.transform.SetParent(parent, false);

            P10DialogueController controller = host.AddComponent<P10DialogueController>();
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("narrativeManager").objectReferenceValue = manager;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            return controller;
        }

        private static void AssertLogCount(P10DialogueController controller, int expectedCount, string message)
        {
            if (controller.GetDialogueLogCount() != expectedCount)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void AdvanceDialogueUntilNode(P10DialogueController controller, P10NarrativeManager manager, string expectedNodeId)
        {
            for (int i = 0; i < 8; i++)
            {
                controller.AdvanceDialogue();
                if (manager.GetCurrentNode() == expectedNodeId)
                {
                    return;
                }
            }
        }

        private static void AssertRuntimeSurfaceLayout(P10DialogueController controller)
        {
            Transform surface = FindRuntimeDialogueSurface(controller, "P10D-06 layout");

            RectTransform surfaceRect = surface.GetComponent<RectTransform>();
            AssertRectTransformHasSize(surfaceRect, "Runtime dialogue surface must have a usable RectTransform.");

            Transform dialoguePanel = surface.Find("DialoguePanel");
            if (dialoguePanel == null)
            {
                throw new InvalidOperationException("DialoguePanel child was not created.");
            }

            RectTransform dialoguePanelRect = dialoguePanel.GetComponent<RectTransform>();
            AssertRectTransformHasSize(dialoguePanelRect, "DialoguePanel must have a usable RectTransform.");

            Transform logPanel = surface.Find("DialogueLogPanel");
            if (logPanel == null)
            {
                throw new InvalidOperationException("DialogueLogPanel child was not created under the full-screen surface.");
            }

            if (logPanel.parent == dialoguePanel)
            {
                throw new InvalidOperationException("DialogueLogPanel must not be parented under the bottom DialoguePanel.");
            }

            RectTransform logPanelRect = logPanel.GetComponent<RectTransform>();
            AssertRectTransformHasSize(logPanelRect, "DialogueLogPanel must have a usable RectTransform.");

            AssertButtonLabel(surface, "DialoguePanel/Panel/LogButton/LogLabel", "记录");
            AssertButtonLabel(surface, "PersistentLogButton/LogLabel", "记录");
            AssertNoVisibleContinueButton(surface, "P10D-06 layout");
            AssertButtonLabel(surface, "DialoguePanel/Panel/CloseButton/CloseLabel", "关闭");
            AssertButtonLabel(surface, "DialogueLogPanel/DialogueLogCloseButton/CloseLabel", "关闭");
            AssertText(surface, "DialogueLogPanel/DialogueLogTitle", "对话记录");

            Transform logScroll = logPanel.Find("DialogueLogScroll");
            if (logScroll == null)
            {
                throw new InvalidOperationException("DialogueLogScroll was not created under DialogueLogPanel.");
            }

            ScrollRect scrollRect = logScroll.GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                throw new InvalidOperationException("DialogueLogScroll is missing ScrollRect.");
            }

            if (!scrollRect.vertical || scrollRect.horizontal)
            {
                throw new InvalidOperationException("Dialogue log history must scroll vertically only.");
            }

            if (scrollRect.viewport == null || scrollRect.content == null)
            {
                throw new InvalidOperationException("Dialogue log ScrollRect must bind viewport and content.");
            }

            if (scrollRect.viewport.name != "Viewport" || scrollRect.content.name != "Content")
            {
                throw new InvalidOperationException("Dialogue log ScrollRect viewport/content hierarchy is invalid.");
            }
        }

        private static void AssertButtonLabel(Transform surface, string labelPath, string expectedText)
        {
            Transform labelTransform = surface.Find(labelPath);
            if (labelTransform == null)
            {
                throw new InvalidOperationException("Button label was not found: " + labelPath);
            }

            Text labelText = labelTransform.GetComponent<Text>();
            if (labelText == null)
            {
                throw new InvalidOperationException("Button label is missing Text component: " + labelPath);
            }

            if (!string.Equals(labelText.text, expectedText, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Button label text mismatch: " + labelPath);
            }

            if (labelText.font == null)
            {
                throw new InvalidOperationException("Button label font is missing: " + labelPath);
            }

            if (labelText.color.a <= 0f)
            {
                throw new InvalidOperationException("Button label color is transparent: " + labelPath);
            }

            if (labelText.alignment != TextAnchor.MiddleCenter)
            {
                throw new InvalidOperationException("Button label must be centered: " + labelPath);
            }

            if (labelText.horizontalOverflow == HorizontalWrapMode.Wrap || labelText.verticalOverflow == VerticalWrapMode.Truncate)
            {
                throw new InvalidOperationException("Button label overflow settings can truncate text: " + labelPath);
            }

            RectTransform labelRect = labelText.rectTransform;
            AssertAnchors(labelRect, Vector2.zero, Vector2.one, "Button label must use full stretch anchors: " + labelPath);
        }

        private static void AssertCurrentDialogueUiVisible(P10DialogueController controller, string expectedNodeId)
        {
            Transform surface = FindRuntimeDialogueSurface(controller, "current dialogue visible");

            Canvas canvas = surface.GetComponent<Canvas>();
            if (canvas == null || !canvas.enabled)
            {
                throw new InvalidOperationException("Current dialogue UI was not visible after closing log.");
            }

            Text dialogueText = controller.FixedBodyText;
            if (dialogueText == null || dialogueText.name != "P10_DialogueBodyText")
            {
                Transform dialogueTextTransform = FindDescendantByName(surface, "P10_DialogueBodyText");
                dialogueText = dialogueTextTransform != null ? dialogueTextTransform.GetComponent<Text>() : null;
            }

            string expectedDialogue = ResolveLatestDialogueText(controller, expectedNodeId);
            if (dialogueText == null || string.IsNullOrWhiteSpace(expectedDialogue) || !string.Equals(dialogueText.text, expectedDialogue, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Current dialogue text was not restored after closing log.");
            }
        }

        private static string ResolveLatestDialogueText(P10DialogueController controller, string expectedNodeId)
        {
            for (int i = controller.DialogueLogEntries.Count - 1; i >= 0; i--)
            {
                P10DialogueLogEntry entry = controller.DialogueLogEntries[i];
                if (entry.NodeId == expectedNodeId)
                {
                    return entry.DialogueText;
                }
            }

            return string.Empty;
        }

        private static void AssertCurrentDialogueUiHidden(P10DialogueController controller)
        {
            Transform surface = FindRuntimeDialogueSurface(controller, "current dialogue hidden");

            Canvas canvas = surface.GetComponent<Canvas>();
            if (canvas == null || !canvas.enabled)
            {
                throw new InvalidOperationException("Persistent dialogue surface canvas must stay enabled.");
            }

            CanvasGroup dialoguePanelGroup = surface.Find("DialoguePanel")?.GetComponent<CanvasGroup>();
            if (dialoguePanelGroup == null || dialoguePanelGroup.alpha > 0.5f)
            {
                throw new InvalidOperationException("Bottom dialogue UI should remain hidden.");
            }
        }

        private static void AssertEmptyDialogueLogView(P10DialogueController controller)
        {
            Transform surface = FindRuntimeDialogueSurface(controller, "empty dialogue log");
            Text emptyText = surface?.Find("DialogueLogPanel/DialogueLogScroll/Viewport/Content/EmptyDialogueLog")?.GetComponent<Text>();
            if (emptyText == null || !string.Equals(emptyText.text, "暂无对话记录", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Empty dialogue log state was not displayed.");
            }
        }

        private static void AssertAllEntriesHaveSpeaker(P10DialogueController controller)
        {
            for (int i = 0; i < controller.DialogueLogEntries.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(controller.DialogueLogEntries[i].SpeakerName))
                {
                    throw new InvalidOperationException("Dialogue log entry has empty speakerName.");
                }
            }
        }

        private static void AssertDialogueLogEntryViews(P10DialogueController controller)
        {
            controller.OpenDialogueLog();

            Transform surface = FindRuntimeDialogueSurface(controller, "dialogue log entries");
            Transform content = surface?.Find("DialogueLogPanel/DialogueLogScroll/Viewport/Content");
            if (content == null)
            {
                throw new InvalidOperationException("Dialogue log content was not found.");
            }

            for (int i = 0; i < controller.DialogueLogEntries.Count; i++)
            {
                P10DialogueLogEntry entry = controller.DialogueLogEntries[i];
                Transform item = content.Find("DialogueLogEntry_" + entry.Sequence);
                if (item == null)
                {
                    throw new InvalidOperationException("Dialogue log entry view was not created.");
                }

                Text speakerText = item.Find("Speaker")?.GetComponent<Text>();
                Text lineText = item.Find("Line")?.GetComponent<Text>();
                Text metaText = item.Find("Meta")?.GetComponent<Text>();

                if (speakerText == null || string.IsNullOrWhiteSpace(speakerText.text))
                {
                    throw new InvalidOperationException("Dialogue log entry view speaker is empty.");
                }

                if (!string.Equals(speakerText.text, "说话人：" + entry.SpeakerName, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Dialogue log entry view speaker does not match entry speakerName.");
                }

                if (lineText == null || string.IsNullOrWhiteSpace(lineText.text))
                {
                    throw new InvalidOperationException("Dialogue log entry view line is empty.");
                }

                if (!lineText.text.StartsWith("内容：", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Dialogue log entry view line label is not localized.");
                }

                if (metaText == null || string.IsNullOrWhiteSpace(metaText.text) || !metaText.text.Contains(entry.Sequence.ToString()))
                {
                    throw new InvalidOperationException("Dialogue log entry view meta is missing sequence.");
                }

                if (!metaText.text.StartsWith("记录 #", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Dialogue log entry view meta label is not localized.");
                }
            }
        }

        private static void ValidateRuntimeChineseLabelsDoNotAdvanceNarrative()
        {
            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject root = new GameObject("P10D18_RuntimeUILabelRoot");

            try
            {
                P10NarrativeManager manager = CreateManager(root.transform);
                P10DialogueController controller = CreateDialogueController(root.transform, manager);
                controller.EnsureRuntimeSurfaceInstance();

                Transform surface = FindRuntimeDialogueSurface(controller, "runtime Chinese labels");
                AssertRuntimeChineseLabels(surface);

                string beforeNode = manager.GetCurrentNode();
                P10NarrativeState beforeState = manager.GetCurrentState();

                controller.OpenDialogueLog();
                AssertRuntimeChineseLabels(surface);
                AssertEmptyDialogueLogView(controller);
                if (manager.GetCurrentNode() != beforeNode || manager.GetCurrentState() != beforeState)
                {
                    throw new InvalidOperationException("Opening localized dialogue log advanced narrative state.");
                }

                controller.CloseDialogueLog();
                if (manager.GetCurrentNode() != beforeNode || manager.GetCurrentState() != beforeState)
                {
                    throw new InvalidOperationException("Closing localized dialogue log advanced narrative state.");
                }

                if (!manager.HandleSceneTrigger("StartPrologue", NodePrologue))
                {
                    throw new InvalidOperationException("Could not enter prologue for localized UI validation.");
                }

                controller.SetCurrentNode(manager.GetCurrentNode());
                beforeNode = manager.GetCurrentNode();
                beforeState = manager.GetCurrentState();

                controller.OpenDialogueLog();
                AssertDialogueLogEntryViews(controller);
                AssertRuntimeChineseLabels(surface);

                if (manager.GetCurrentNode() != beforeNode || manager.GetCurrentState() != beforeState)
                {
                    throw new InvalidOperationException("Opening populated localized dialogue log advanced narrative state.");
                }

                controller.CloseDialogueLog();
                if (manager.GetCurrentNode() != beforeNode || manager.GetCurrentState() != beforeState)
                {
                    throw new InvalidOperationException("Closing populated localized dialogue log advanced narrative state.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyGeneratedEventSystemIfNeeded(existingEventSystem);
            }
        }

        private static void AssertRuntimeChineseLabels(Transform surface)
        {
            if (surface == null)
            {
                throw new InvalidOperationException("Runtime dialogue surface was not created.");
            }

            AssertButtonLabel(surface, "DialoguePanel/Panel/LogButton/LogLabel", "记录");
            AssertButtonLabel(surface, "PersistentLogButton/LogLabel", "记录");
            AssertNoVisibleContinueButton(surface, "runtime Chinese labels");
            AssertButtonLabel(surface, "DialoguePanel/Panel/CloseButton/CloseLabel", "关闭");
            AssertButtonLabel(surface, "DialogueLogPanel/DialogueLogCloseButton/CloseLabel", "关闭");
            AssertText(surface, "DialogueLogPanel/DialogueLogTitle", "对话记录");
        }

        private static void AssertNoVisibleContinueButton(Transform surface, string label)
        {
            if (surface == null)
            {
                throw new InvalidOperationException(label + " surface missing for continue button visibility check.");
            }

            AssertNoVisibleContinueButtonRecursive(surface, label);
        }

        private static void AssertNoVisibleContinueButtonRecursive(Transform target, string label)
        {
            if (target == null)
            {
                return;
            }

            bool active = IsActiveForPrefabOrHierarchy(target);
            if (active)
            {
                if (string.Equals(target.name, "NextButton", StringComparison.Ordinal) ||
                    string.Equals(target.name, "NextLabel", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(label + " still contains active visible continue object.");
                }

                Text text = target.GetComponent<Text>();
                if (text != null &&
                    text.enabled &&
                    text.color.a > 0f &&
                    string.Equals(text.text, "\u7ee7\u7eed", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(label + " still contains active visible continue text.");
                }
            }

            for (int i = 0; i < target.childCount; i++)
            {
                AssertNoVisibleContinueButtonRecursive(target.GetChild(i), label);
            }
        }

        private static bool IsActiveForPrefabOrHierarchy(Transform target)
        {
            if (target == null)
            {
                return false;
            }

            if (target.gameObject.activeInHierarchy)
            {
                return true;
            }

            Transform current = target;
            while (current != null)
            {
                if (!current.gameObject.activeSelf)
                {
                    return false;
                }

                current = current.parent;
            }

            return true;
        }

        private static void AssertText(Transform surface, string labelPath, string expectedText)
        {
            Transform labelTransform = surface.Find(labelPath);
            if (labelTransform == null)
            {
                throw new InvalidOperationException("Text was not found: " + labelPath);
            }

            Text labelText = labelTransform.GetComponent<Text>();
            if (labelText == null)
            {
                throw new InvalidOperationException("Text component was not found: " + labelPath);
            }

            if (!string.Equals(labelText.text, expectedText, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Localized text mismatch: " + labelPath);
            }

            if (labelText.font == null)
            {
                throw new InvalidOperationException("Localized text font is missing: " + labelPath);
            }
        }

        private static void AssertAnchors(RectTransform rect, Vector2 expectedMin, Vector2 expectedMax, string message)
        {
            if (rect == null)
            {
                throw new InvalidOperationException(message + " Missing RectTransform.");
            }

            if (!Approximately(rect.anchorMin, expectedMin) || !Approximately(rect.anchorMax, expectedMax))
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void AssertRectTransformHasSize(RectTransform rect, string message)
        {
            if (rect == null)
            {
                throw new InvalidOperationException(message + " Missing RectTransform.");
            }

            Rect resolvedRect = rect.rect;
            bool hasResolvedSize = resolvedRect.width > 0f && resolvedRect.height > 0f;
            bool hasAnchorArea = rect.anchorMax.x > rect.anchorMin.x && rect.anchorMax.y > rect.anchorMin.y;
            if (!hasResolvedSize && !hasAnchorArea)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static bool Approximately(Vector2 actual, Vector2 expected)
        {
            const float Tolerance = 0.0001f;
            return Mathf.Abs(actual.x - expected.x) <= Tolerance
                && Mathf.Abs(actual.y - expected.y) <= Tolerance;
        }

        private static Transform FindRuntimeDialogueSurface(P10DialogueController controller, string label)
        {
            if (controller == null)
            {
                throw new InvalidOperationException("Dialogue controller was not created for " + label + ".");
            }

            Transform direct = controller.transform.Find("P10_Runtime_DialogueSurface");
            if (direct != null)
            {
                return direct;
            }

            Transform recursive = FindDescendantByName(controller.transform, "P10_Runtime_DialogueSurface");
            if (recursive != null)
            {
                return recursive;
            }

            Text bodySlot = controller.FixedBodyText;
            if (bodySlot != null)
            {
                Transform parent = bodySlot.transform;
                while (parent != null && parent != controller.transform)
                {
                    if (parent.Find("DialogueLogPanel") != null && parent.Find("DialoguePanel") != null)
                    {
                        return parent;
                    }

                    parent = parent.parent;
                }
            }

            throw new InvalidOperationException("Runtime dialogue surface was not created for " + label + ".");
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

        private static void AssertLatestEntry(
            P10DialogueController controller,
            int expectedSequence,
            string expectedNodeId,
            string expectedSpeaker)
        {
            if (controller.DialogueLogEntries.Count == 0)
            {
                throw new InvalidOperationException("Dialogue log is empty.");
            }

            P10DialogueLogEntry entry = controller.DialogueLogEntries[controller.DialogueLogEntries.Count - 1];
            if (entry.Sequence != expectedSequence)
            {
                throw new InvalidOperationException("Dialogue log sequence mismatch.");
            }

            if (entry.NodeId != expectedNodeId)
            {
                throw new InvalidOperationException("Dialogue log node mismatch.");
            }

            if (entry.SpeakerName != expectedSpeaker)
            {
                throw new InvalidOperationException("Dialogue log speaker mismatch.");
            }

            if (string.IsNullOrWhiteSpace(entry.DialogueText))
            {
                throw new InvalidOperationException("Dialogue log text is empty.");
            }
        }

        private static void AssertEntrySequence(P10DialogueController controller, int index, int expectedSequence)
        {
            if (controller.DialogueLogEntries[index].Sequence != expectedSequence)
            {
                throw new InvalidOperationException("Dialogue log imported sequence was not normalized.");
            }
        }

        private static void DestroyGeneratedEventSystemIfNeeded(EventSystem existingEventSystem)
        {
            if (existingEventSystem != null)
            {
                return;
            }

            EventSystem generatedEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            if (generatedEventSystem != null && generatedEventSystem.name == "P10_Runtime_EventSystem")
            {
                UnityEngine.Object.DestroyImmediate(generatedEventSystem.gameObject);
            }
        }

        private static void InvokeAwake(MonoBehaviour behaviour)
        {
            MethodInfo awake = behaviour.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            if (awake != null)
            {
                awake.Invoke(behaviour, null);
            }
        }
    }
}
