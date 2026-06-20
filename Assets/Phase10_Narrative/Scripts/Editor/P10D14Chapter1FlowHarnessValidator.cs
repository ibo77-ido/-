
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Phase10_Narrative
{
    public static class P10D14Chapter1FlowHarnessValidator
    {
        private const string NodePrologue = "P10_CH01_NODE_PROLOGUE_01";
        private const string NodeTutorial = "P10_CH01_NODE_TUTORIAL_01";
        private const string NodeOrder001Accept = "P10_CH01_NODE_ORDER_001_ACCEPT";
        private const string NodeOrder001Pass = "P10_CH01_NODE_ORDER_001_PASS";
        private const string NodeOrder001Fail = "P10_CH01_NODE_ORDER_001_FAIL";
        private const string NodeOrder003Accept = "P10_CH01_NODE_ORDER_003_ACCEPT";
        private const string NodeOrder003Pass = "P10_CH01_NODE_ORDER_003_PASS";
        private const string NodeOrder003Fail = "P10_CH01_NODE_ORDER_003_FAIL";
        private const string NodeOrder004Accept = "P10_CH01_NODE_ORDER_004_ACCEPT";
        private const string NodeOrder004PassNormal = "P10_CH01_NODE_ORDER_004_PASS_NORMAL";
        private const string NodeOrder004Fail = "P10_CH01_NODE_ORDER_004_FAIL";
        private const string NodeOrder004Climax = "P10_CH01_NODE_ORDER_004_CLIMAX";
        private const string NodeChapterEnding = "P10_CH01_NODE_CHAPTER_ENDING";
        private const string DialogueAssetFolder = "Assets/Phase10_Narrative/ScriptableObjects/Dialogues";
        private const string CharacterAssetFolder = "Assets/Phase10_Narrative/ScriptableObjects/Characters";
        private const string EditorVisibleDialogueUiPrefabPath = "Assets/Phase10_Narrative/Prefabs/UI/P10_CH01_DialogueUI_Placeholder.prefab";
        private const string EditorVisibleNarrativeOverlayScenePath = "Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity";
        private const string EditorVisibleManualSlotRootName = "P10_CH01_DialogueManualSlots";
        private const string EditorVisibleSpeakerSlotPath = "P10_CH01_DialogueManualSlots/P10_DialogueSpeakerText";
        private const string EditorVisibleBodySlotPath = "P10_CH01_DialogueManualSlots/P10_DialogueBodyText";

        public static void RunP10D14Chapter1FlowHarnessValidation()
        {
            RunValidation("P10D-14 Chapter 1 Flow Harness validation passed.", () =>
            {
                ValidateChapterFlow(false);
                ValidateApprovedTriggerCompletionProbe();
                ValidateSnapshotRestoreDoesNotDrift();
            });
        }

        public static void RunP10D19Chapter1ChineseVerticalSliceValidation()
        {
            RunValidation("P10D-19 Chapter 1 Chinese Vertical Slice validation passed.", () =>
            {
                ValidateDialogueAssetSegmentation();
                ValidateChapterFlow(true);
                ValidateApprovedTriggerCompletionProbe();
                ValidateSnapshotRestoreDoesNotDrift();
            });
        }

        public static void RunP10D20R2DialogueSegmentationAndTaskTriggerStabilityValidation()
        {
            RunValidation("P10D-20R2 Dialogue Segmentation and Task Trigger Stability validation passed.", () =>
            {
                ValidateDialogueAssetSegmentation();
                ValidateDialogueLogSplitLines();
                for (int i = 0; i < 5; i++)
                {
                    ValidateChapterFlow(true);
                    ValidateNoUiNormalCompletionCycle();
                    ValidateNoUiClimaxCompletionCycle();
                    ValidateQueuedClimaxCompletionCycle();
                }

                ValidateChapterFlow(true);
                ValidateApprovedTriggerCompletionProbe();
                ValidateSnapshotRestoreDoesNotDrift();
            });
        }

        public static void RunP10D20R3SpeakerDisplayValidation()
        {
            RunValidation("P10D-20R3 Speaker Display validation passed.", () =>
            {
                ValidateRuntimeSpeakerDisplayForAllChapterLines();
                ValidateChapterFlow(true);
                ValidateSnapshotRestoreDoesNotDrift();
            });
        }

        public static void RunP10D20R5RuntimeDialogueSpeakerVisibleDisplayValidation()
        {
            RunValidation("P10D-20R5 Runtime Dialogue Speaker Visible Display validation passed.", () =>
            {
                ValidateRuntimeSpeakerVisibleDisplayRepair();
                ValidateRuntimeSpeakerDisplayForAllChapterLines();
                ValidateChapterFlow(true);
                ValidateSnapshotRestoreDoesNotDrift();
            });
        }

        public static void RunP10D20R6RuntimeDialogueTextCompletenessValidation()
        {
            RunValidation("P10D-20R6 Runtime Dialogue Text Completeness validation passed.", () =>
            {
                ValidateRuntimeDialogueTextCompletenessRepair();
                ValidateRuntimeSpeakerVisibleDisplayRepair();
                ValidateRuntimeSpeakerDisplayForAllChapterLines();
                ValidateChapterFlow(true);
                ValidateSnapshotRestoreDoesNotDrift();
            });
        }

        public static void RunP10D20R7DialogueLogLayoutAndPlayerFacingTermLocalizationValidation()
        {
            RunValidation("P10D-20R7 Dialogue Log Layout and Player-Facing Term Localization validation passed.", () =>
            {
                ValidateDialogueLogLayoutAndPlayerFacingTermLocalizationRepair();
                ValidateRuntimeDialogueTextCompletenessRepair();
                ValidateRuntimeSpeakerVisibleDisplayRepair();
                ValidateRuntimeSpeakerDisplayForAllChapterLines();
                ValidateDialogueAssetSegmentation();
                ValidateChapterFlow(true);
                ValidateApprovedTriggerCompletionProbe();
                ValidateSnapshotRestoreDoesNotDrift();
            });
        }

        public static void RunP10D20R8FixedDialogueTextSlotsAndSpeakerPrefixCleanupValidation()
        {
            RunValidation("P10D-20R8 Fixed Dialogue Text Slots and Speaker Prefix Cleanup validation passed.", () =>
            {
                ValidateFixedDialogueTextSlotsAndSpeakerPrefixCleanupRepair();
                ValidateDialogueLogLayoutAndPlayerFacingTermLocalizationRepair();
                ValidateRuntimeDialogueTextCompletenessRepair();
                ValidateRuntimeSpeakerVisibleDisplayRepair();
                ValidateRuntimeSpeakerDisplayForAllChapterLines();
                ValidateDialogueAssetSegmentation();
                ValidateChapterFlow(true);
                ValidateApprovedTriggerCompletionProbe();
                ValidateSnapshotRestoreDoesNotDrift();
            });
        }

        public static void SetupP10D20R9EditorVisibleFixedDialogueTextSlots()
        {
            RunValidation("P10D-20R9 Editor Visible Fixed Dialogue Text Slot Objects setup passed.", () =>
            {
                EnsureEditorVisibleDialogueUiPrefab();
                EnsureOverlaySceneUsesEditorVisibleDialogueUiPrefab();
            });
        }

        public static void RunP10D20R9EditorVisibleFixedDialogueTextSlotObjectsValidation()
        {
            RunValidation("P10D-20R9 Editor Visible Fixed Dialogue Text Slot Objects validation passed.", () =>
            {
                ValidateEditorVisibleFixedDialogueTextSlotObjectsRepair();
                ValidateFixedDialogueTextSlotsAndSpeakerPrefixCleanupRepair();
                ValidateDialogueLogLayoutAndPlayerFacingTermLocalizationRepair();
                ValidateRuntimeDialogueTextCompletenessRepair();
                ValidateRuntimeSpeakerVisibleDisplayRepair();
                ValidateRuntimeSpeakerDisplayForAllChapterLines();
                ValidateDialogueAssetSegmentation();
                ValidateChapterFlow(true);
                ValidateApprovedTriggerCompletionProbe();
                ValidateSnapshotRestoreDoesNotDrift();
            });
        }

        public static void SetupP10D20R9BClearlyVisibleDialogueTextSlotObjects()
        {
            RunValidation("P10D-20R9B Clearly Visible Dialogue Text Slot Objects setup passed.", () =>
            {
                EnsureEditorVisibleDialogueUiPrefab();
                EnsureOverlaySceneUsesEditorVisibleDialogueUiPrefab();
            });
        }

        public static void RunP10D20R9BClearlyVisibleDialogueTextSlotObjectsValidation()
        {
            RunValidation("P10D-20R9B Clearly Visible Dialogue Text Slot Objects validation passed.", () =>
            {
                ValidateEditorVisibleFixedDialogueTextSlotObjectsRepair();
                ValidateFixedDialogueTextSlotsAndSpeakerPrefixCleanupRepair();
                ValidateDialogueLogLayoutAndPlayerFacingTermLocalizationRepair();
                ValidateRuntimeDialogueTextCompletenessRepair();
                ValidateRuntimeSpeakerVisibleDisplayRepair();
                ValidateRuntimeSpeakerDisplayForAllChapterLines();
                ValidateDialogueAssetSegmentation();
                ValidateChapterFlow(true);
                ValidateApprovedTriggerCompletionProbe();
                ValidateSnapshotRestoreDoesNotDrift();
            });
        }

        public static void RunP10D21CurrentOrderPanelWithCraftingHintsValidation()
        {
            RunValidation("P10D-21 Current Order Panel with Crafting Hints validation passed.", () =>
            {
                EnsureEditorVisibleDialogueUiPrefab();
                EnsureOverlaySceneUsesEditorVisibleDialogueUiPrefab();
                ValidateCurrentOrderPanelWithCraftingHints();
                ValidateEditorVisibleFixedDialogueTextSlotObjectsRepair();
                ValidateFixedDialogueTextSlotsAndSpeakerPrefixCleanupRepair();
                ValidateDialogueLogLayoutAndPlayerFacingTermLocalizationRepair();
                ValidateRuntimeDialogueTextCompletenessRepair();
                ValidateRuntimeSpeakerVisibleDisplayRepair();
                ValidateRuntimeSpeakerDisplayForAllChapterLines();
                ValidateDialogueAssetSegmentation();
                ValidateChapterFlow(true);
                ValidateApprovedTriggerCompletionProbe();
                ValidateSnapshotRestoreDoesNotDrift();
            });
        }

        public static void RunP10D24DialogueBoxClickToAdvanceValidation()
        {
            RunValidation("P10D-24 Dialogue Box Click-to-Advance validation passed.", () =>
            {
                EnsureEditorVisibleDialogueUiPrefab();
                EnsureOverlaySceneUsesEditorVisibleDialogueUiPrefab();
                ValidateDialogueBoxClickToAdvance();
                ValidateEditorVisibleFixedDialogueTextSlotObjectsRepair();
                ValidateCurrentOrderPanelWithCraftingHints();
                ValidateFixedDialogueTextSlotsAndSpeakerPrefixCleanupRepair();
                ValidateDialogueLogLayoutAndPlayerFacingTermLocalizationRepair();
                ValidateRuntimeDialogueTextCompletenessRepair();
                ValidateRuntimeSpeakerVisibleDisplayRepair();
                ValidateRuntimeSpeakerDisplayForAllChapterLines();
                ValidateDialogueAssetSegmentation();
                ValidateChapterFlow(true);
                ValidateApprovedTriggerCompletionProbe();
                ValidateSnapshotRestoreDoesNotDrift();
            });
        }

        public static void RunP10D24RRemoveContinueButtonAndDialogueBoxClickAdvanceValidation()
        {
            RunValidation("P10D-24R Remove Continue Button and Dialogue Box Click Advance validation passed.", () =>
            {
                EnsureEditorVisibleDialogueUiPrefab();
                EnsureOverlaySceneUsesEditorVisibleDialogueUiPrefab();
                ValidateDialogueBoxClickToAdvance();
                ValidateEditorVisibleFixedDialogueTextSlotObjectsRepair();
                ValidateCurrentOrderPanelWithCraftingHints();
                ValidateFixedDialogueTextSlotsAndSpeakerPrefixCleanupRepair();
                ValidateDialogueLogLayoutAndPlayerFacingTermLocalizationRepair();
                ValidateRuntimeDialogueTextCompletenessRepair();
                ValidateRuntimeSpeakerVisibleDisplayRepair();
                ValidateRuntimeSpeakerDisplayForAllChapterLines();
                ValidateDialogueAssetSegmentation();
                ValidateChapterFlow(true);
                ValidateApprovedTriggerCompletionProbe();
                ValidateSnapshotRestoreDoesNotDrift();
            });
        }

        public static void RunP10D24STopRightActionBarAndPanelMutualExclusionValidation()
        {
            RunValidation("P10D-24S TopRight Action Bar and Panel Mutual Exclusion validation passed.", () =>
            {
                EnsureEditorVisibleDialogueUiPrefab();
                EnsureOverlaySceneUsesEditorVisibleDialogueUiPrefab();
                ValidateTopRightActionBarAndPanelMutualExclusion();
                ValidateDialogueBoxClickToAdvance();
                ValidateCurrentOrderPanelWithCraftingHints();
            });
        }

        public static void RunP10D24TScrollbarDragInteractionValidation()
        {
            RunValidation("P10D-24T Scrollbar Drag Interaction validation passed.", () =>
            {
                EnsureEditorVisibleDialogueUiPrefab();
                EnsureOverlaySceneUsesEditorVisibleDialogueUiPrefab();
                ValidateScrollbarDragInteraction();
                ValidateDialogueBoxClickToAdvance();
                ValidateCurrentOrderPanelWithCraftingHints();
                ValidateTopRightActionBarAndPanelMutualExclusion();
            });
        }

        public static void RunP10D24UDialogueTextContainmentAndTopRightButtonLayoutValidation()
        {
            RunValidation("P10D-24U Dialogue Text Containment and TopRight Button Layout validation passed.", () =>
            {
                EnsureEditorVisibleDialogueUiPrefab();
                EnsureOverlaySceneUsesEditorVisibleDialogueUiPrefab();
                ValidateEditorVisibleFixedDialogueTextSlotObjectsRepair();
                ValidateTopRightActionBarAndPanelMutualExclusion();
            });
        }

        private static void RunValidation(string passMessage, Action validation)
        {
            try
            {
                validation();
                ValidateBridgeSurfaceUnchanged();
                Debug.Log(passMessage);
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError(passMessage.Replace(" passed.", " failed: ") + ex);
                EditorApplication.Exit(1);
            }
        }

        private static void ValidateDialogueAssetSegmentation()
        {
            Dictionary<string, P10CharacterDataSO> characters = LoadCharacters();
            List<P10DialogueNodeSO> nodes = LoadDialogueNodes();
            if (nodes.Count != 13)
            {
                throw new InvalidOperationException("Expected exactly 13 Chapter 1 dialogue assets, actual: " + nodes.Count + ".");
            }

            foreach (P10DialogueNodeSO node in nodes)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.NodeId))
                {
                    throw new InvalidOperationException("Dialogue node has empty NodeId.");
                }

                AssertSpeakerResolves(node.NodeId, node.SpeakerCharacterId, characters);
                AssertNoSpeakerPrefix(node.NodeId, node.DialogueText, characters);
                if (node.DialogueLines == null || node.DialogueLines.Count == 0)
                {
                    throw new InvalidOperationException("Node has no DialogueLines: " + node.NodeId);
                }

                for (int i = 0; i < node.DialogueLines.Count; i++)
                {
                    P10DialogueLine line = node.DialogueLines[i];
                    string label = node.NodeId + "#" + i;
                    if (line == null)
                    {
                        throw new InvalidOperationException("Null dialogue line at " + label + ".");
                    }

                    AssertSpeakerResolves(label, line.SpeakerCharacterId, characters);
                    AssertNoSpeakerPrefix(label, line.DialogueText, characters);
                    if (string.IsNullOrWhiteSpace(line.DialogueText) || !ContainsCjk(line.DialogueText))
                    {
                        throw new InvalidOperationException("Dialogue line is empty or not Chinese-localized at " + label + ".");
                    }
                }
            }

            AssertLineCount(nodes, NodePrologue, 3);
            AssertLineCount(nodes, NodeOrder001Pass, 3);
            AssertLineCount(nodes, NodeOrder003Pass, 3);
            AssertLineCount(nodes, NodeOrder004Accept, 3);
            AssertLineCount(nodes, NodeOrder004Climax, 3);
            AssertLineCount(nodes, NodeChapterEnding, 4);
        }

        private static void ValidateDialogueLogSplitLines()
        {
            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject root = new GameObject("P10D20R2_DialogueLogSplitRoot");
            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "DialogueLogSplitManager");
                P10DialogueController controller = CreateDialogueController(manager.transform, manager);
                InjectPhase10AssetDialogueData(controller);
                controller.EnsureRuntimeSurfaceInstance();

                AssertSceneTrigger(manager, "StartPrologue", NodePrologue, true);
                P10DialogueNodeSO prologue = ResolveNode(NodePrologue);
                int expectedLogCount = 0;
                DisplayLine(controller, manager, ref expectedLogCount, NodePrologue, 0);
                AssertLatestEntryDoesNotContain(controller, prologue.DialogueLines[1].DialogueText);
                AssertLatestEntryDoesNotContain(controller, prologue.DialogueLines[2].DialogueText);
                AdvanceLine(controller, manager, ref expectedLogCount, NodePrologue, 1);
                AssertLatestEntryDoesNotContain(controller, prologue.DialogueLines[0].DialogueText);
                AssertLatestEntryDoesNotContain(controller, prologue.DialogueLines[2].DialogueText);
                AdvanceLine(controller, manager, ref expectedLogCount, NodePrologue, 2);
                AssertLatestEntryDoesNotContain(controller, prologue.DialogueLines[0].DialogueText);
                AssertLatestEntryDoesNotContain(controller, prologue.DialogueLines[1].DialogueText);
                AssertRefreshDoesNotMutate(controller, manager, expectedLogCount, NodePrologue);
                AssertDialogueLogPanelDoesNotMutate(controller, manager, expectedLogCount);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyGeneratedEventSystemIfNeeded(existingEventSystem);
            }
        }

        private static void ValidateRuntimeSpeakerDisplayForAllChapterLines()
        {
            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject root = new GameObject("P10D20R3_SpeakerDisplayRoot");
            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "SpeakerDisplayManager");
                P10DialogueController controller = CreateDialogueController(manager.transform, manager);
                InjectPhase10AssetDialogueData(controller);
                controller.EnsureRuntimeSurfaceInstance();

                List<P10DialogueNodeSO> nodes = LoadDialogueNodes();
                int expectedLogCount = 0;
                int observedSpeakerChanges = 0;
                string previousSpeakerName = string.Empty;

                foreach (P10DialogueNodeSO node in nodes)
                {
                    if (node == null || node.DialogueLines == null || node.DialogueLines.Count == 0)
                    {
                        continue;
                    }

                    controller.SetCurrentNode(node.NodeId);
                    expectedLogCount++;
                    AssertRuntimeSurfaceLine(controller, expectedLogCount, node, 0, ref previousSpeakerName, ref observedSpeakerChanges);

                    for (int lineIndex = 1; lineIndex < node.DialogueLines.Count; lineIndex++)
                    {
                        controller.AdvanceDialogue();
                        expectedLogCount++;
                        AssertRuntimeSurfaceLine(controller, expectedLogCount, node, lineIndex, ref previousSpeakerName, ref observedSpeakerChanges);
                    }
                }

                if (observedSpeakerChanges < 5)
                {
                    throw new InvalidOperationException("Speaker display validation did not cover enough speaker label transitions.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyGeneratedEventSystemIfNeeded(existingEventSystem);
            }
        }

        private static void ValidateRuntimeSpeakerVisibleDisplayRepair()
        {
            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject root = new GameObject("P10D20R5_VisibleSpeakerDisplayRoot");
            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "VisibleSpeakerDisplayManager");
                P10DialogueController controller = CreateDialogueController(manager.transform, manager);
                InjectPhase10AssetDialogueData(controller);
                controller.EnsureRuntimeSurfaceInstance();

                int expectedLogCount = 0;
                AssertSceneTrigger(manager, "StartPrologue", NodePrologue, true);
                controller.SetCurrentNode(manager.GetCurrentNode());
                expectedLogCount++;
                AssertVisibleBottomDialogueSpeaker(controller, expectedLogCount, NodePrologue, 0, "Prologue narrator");

                controller.AdvanceDialogue();
                expectedLogCount++;
                AssertVisibleBottomDialogueSpeaker(controller, expectedLogCount, NodePrologue, 1, "Prologue Xu Laobo");

                controller.SetCurrentNode(NodeOrder001Accept);
                expectedLogCount++;
                AssertVisibleBottomDialogueSpeaker(controller, expectedLogCount, NodeOrder001Accept, 0, "ORDER_001 Zhou Zhanggui");

                controller.SetCurrentNode(NodeOrder003Accept);
                expectedLogCount++;
                AssertVisibleBottomDialogueSpeaker(controller, expectedLogCount, NodeOrder003Accept, 0, "ORDER_003 Chen Shuyuan");

                controller.SetCurrentNode(NodeOrder004Accept);
                expectedLogCount++;
                AssertVisibleBottomDialogueSpeaker(controller, expectedLogCount, NodeOrder004Accept, 0, "ORDER_004 Lu Ke");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyGeneratedEventSystemIfNeeded(existingEventSystem);
            }
        }

        private static void ValidateRuntimeDialogueTextCompletenessRepair()
        {
            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject root = new GameObject("P10D20R6_TextCompletenessRoot");
            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "TextCompletenessManager");
                P10DialogueController controller = CreateDialogueController(manager.transform, manager);
                InjectPhase10AssetDialogueData(controller);
                controller.EnsureRuntimeSurfaceInstance();

                int expectedLogCount = 0;
                AssertSceneTrigger(manager, "StartPrologue", NodePrologue, true);
                controller.SetCurrentNode(manager.GetCurrentNode());
                expectedLogCount++;
                AssertCompleteVisibleBottomDialogueLine(controller, expectedLogCount, NodePrologue, 0, "Prologue narrator first line");

                controller.AdvanceDialogue();
                expectedLogCount++;
                AssertCompleteVisibleBottomDialogueLine(controller, expectedLogCount, NodePrologue, 1, "Prologue Xu Laobo full line");

                controller.AdvanceDialogue();
                expectedLogCount++;
                AssertCompleteVisibleBottomDialogueLine(controller, expectedLogCount, NodePrologue, 2, "Prologue system prompt full line");

                AssertCompleteVisibleBottomDialogueLineByDirectNode(controller, ref expectedLogCount, NodeTutorial, 0, "Tutorial Xu Laobo long line");
                AssertCompleteVisibleBottomDialogueLineByDirectNode(controller, ref expectedLogCount, NodeOrder001Accept, 0, "ORDER_001_ACCEPT Zhou long line");
                controller.AdvanceDialogue();
                expectedLogCount++;
                AssertCompleteVisibleBottomDialogueLine(controller, expectedLogCount, NodeOrder001Accept, 1, "ORDER_001_ACCEPT system prompt");
                AssertCompleteVisibleBottomDialogueLineByDirectNode(controller, ref expectedLogCount, NodeOrder003Accept, 0, "ORDER_003_ACCEPT Chen long line");
                controller.AdvanceDialogue();
                expectedLogCount++;
                AssertCompleteVisibleBottomDialogueLine(controller, expectedLogCount, NodeOrder003Accept, 1, "ORDER_003_ACCEPT system prompt");
                AssertCompleteVisibleBottomDialogueLineByDirectNode(controller, ref expectedLogCount, NodeOrder004Accept, 0, "ORDER_004_ACCEPT Lu long line");
                controller.AdvanceDialogue();
                expectedLogCount++;
                AssertCompleteVisibleBottomDialogueLine(controller, expectedLogCount, NodeOrder004Accept, 1, "ORDER_004_ACCEPT Xu Laobo warning");
                controller.AdvanceDialogue();
                expectedLogCount++;
                AssertCompleteVisibleBottomDialogueLine(controller, expectedLogCount, NodeOrder004Accept, 2, "ORDER_004_ACCEPT system prompt");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyGeneratedEventSystemIfNeeded(existingEventSystem);
            }
        }

        private static void ValidateDialogueLogLayoutAndPlayerFacingTermLocalizationRepair()
        {
            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject root = new GameObject("P10D20R7_LogLayoutLocalizationRoot");
            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "LogLayoutLocalizationManager");
                P10DialogueController controller = CreateDialogueController(manager.transform, manager);
                InjectPhase10AssetDialogueData(controller);
                controller.EnsureRuntimeSurfaceInstance();

                int expectedLogCount = 0;
                DisplaySpecificLine(controller, ref expectedLogCount, NodePrologue, 2, "Prologue system prompt");
                DisplaySpecificLine(controller, ref expectedLogCount, NodeOrder001Accept, 1, "ORDER_001 accept system prompt");
                DisplaySpecificLine(controller, ref expectedLogCount, NodeOrder001Pass, 2, "ORDER_001 pass UI result");
                DisplaySpecificLine(controller, ref expectedLogCount, NodeOrder003Accept, 1, "ORDER_003 accept system prompt");
                DisplaySpecificLine(controller, ref expectedLogCount, NodeOrder003Fail, 1, "ORDER_003 fail system prompt");
                DisplaySpecificLine(controller, ref expectedLogCount, NodeOrder003Pass, 2, "ORDER_003 pass UI result");
                DisplaySpecificLine(controller, ref expectedLogCount, NodeOrder004Accept, 2, "ORDER_004 accept system prompt");
                DisplaySpecificLine(controller, ref expectedLogCount, NodeOrder004Fail, 2, "ORDER_004 fail system prompt");
                DisplaySpecificLine(controller, ref expectedLogCount, NodeOrder004PassNormal, 1, "ORDER_004 normal pass UI result");
                DisplaySpecificLine(controller, ref expectedLogCount, NodeChapterEnding, 3, "Chapter ending UI result");

                controller.OpenDialogueLog();
                if (!controller.IsDialogueLogVisible)
                {
                    throw new InvalidOperationException("R7 Dialogue Log panel did not open.");
                }

                Transform surface = FindRuntimeDialogueSurface(controller, "R7 Log layout");
                AssertDialogueLogLayout(surface);
                AssertDialogueLogEntriesAreReadableAndLocalized(controller, surface);
                AssertLogContentStillRecordsSpeakerAndText(controller, surface);
                AssertPlayerFacingTechnicalTermsAbsentFromDialogueAssets();

                controller.CloseDialogueLog();
                if (controller.IsDialogueLogVisible)
                {
                    throw new InvalidOperationException("R7 Dialogue Log panel did not close.");
                }

                AssertLogCount(controller, expectedLogCount, "R7 Dialogue Log open/close changed log count.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyGeneratedEventSystemIfNeeded(existingEventSystem);
            }
        }

        private static void ValidateFixedDialogueTextSlotsAndSpeakerPrefixCleanupRepair()
        {
            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject root = new GameObject("P10D20R8_FixedTextSlotsRoot");
            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "FixedTextSlotsManager");
                P10DialogueController controller = CreateDialogueController(manager.transform, manager);
                InjectPhase10AssetDialogueData(controller);
                controller.EnsureRuntimeSurfaceInstance();

                int expectedLogCount = 0;
                AssertSceneTrigger(manager, "StartPrologue", NodePrologue, true);
                controller.SetCurrentNode(manager.GetCurrentNode());
                expectedLogCount++;
                Text speakerSlot = FindBottomSpeakerText(controller, "R8 prologue narrator slot");
                Text bodySlot = FindBottomBodyText(controller, "R8 prologue narrator slot");
                AssertFixedSlotVisibleText(controller, expectedLogCount, NodePrologue, 0, "R8 prologue narrator");
                AssertFixedSlotTextEquals(controller, "旁白", "父亲走的那年，窑炉最后一次点火。", "R8 prologue exact narrator line");

                controller.AdvanceDialogue();
                expectedLogCount++;
                AssertSameFixedTextSlots(controller, speakerSlot, bodySlot, "R8 prologue Xu slot reuse");
                AssertFixedSlotVisibleText(controller, expectedLogCount, NodePrologue, 1, "R8 prologue Xu Laobo");
                AssertFixedSlotBodyDoesNotContainPrefix(controller, "徐老伯", "R8 prologue Xu body prefix cleanup");

                controller.AdvanceDialogue();
                expectedLogCount++;
                AssertSameFixedTextSlots(controller, speakerSlot, bodySlot, "R8 prologue system prompt slot reuse");
                AssertFixedSlotVisibleText(controller, expectedLogCount, NodePrologue, 2, "R8 prologue system prompt");
                AssertFixedSlotBodyDoesNotContainPrefix(controller, "系统提示", "R8 system prompt body prefix cleanup");

                controller.SetCurrentNode(NodeOrder001Pass);
                expectedLogCount++;
                AssertSameFixedTextSlots(controller, speakerSlot, bodySlot, "R8 order pass line 0 slot reuse");
                AssertFixedSlotVisibleText(controller, expectedLogCount, NodeOrder001Pass, 0, "R8 ORDER_001 pass line 0");

                controller.AdvanceDialogue();
                expectedLogCount++;
                AssertSameFixedTextSlots(controller, speakerSlot, bodySlot, "R8 order pass line 1 slot reuse");
                AssertFixedSlotVisibleText(controller, expectedLogCount, NodeOrder001Pass, 1, "R8 ORDER_001 pass narrator");

                controller.AdvanceDialogue();
                expectedLogCount++;
                AssertSameFixedTextSlots(controller, speakerSlot, bodySlot, "R8 order pass UI result slot reuse");
                AssertFixedSlotVisibleText(controller, expectedLogCount, NodeOrder001Pass, 2, "R8 ORDER_001 pass UI result");
                AssertFixedSlotBodyDoesNotContainPrefix(controller, "UI结果", "R8 UI result body prefix cleanup");

                AssertDialogueLogLatestEntriesRemainStructured(controller);
                AssertAllDialogueAssetsUsePureBodyText();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyGeneratedEventSystemIfNeeded(existingEventSystem);
            }
        }

        private static void ValidateDialogueBoxClickToAdvance()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EditorVisibleDialogueUiPrefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException("P10D-24 dialogue UI prefab missing.");
            }

            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject root = new GameObject("P10D24_DialogueBoxClickToAdvanceRoot");
            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "DialogueBoxClickToAdvanceManager");
                P10DialogueController controller = CreateDialogueController(manager.transform, manager);
                InjectPhase10AssetDialogueData(controller);
                AssignDialogueSurfacePrefab(controller, prefab);
                controller.EnsureRuntimeSurfaceInstance();

                Transform surface = FindRuntimeDialogueSurface(controller, "P10D-24 runtime surface");
                Transform dialoguePanel = surface.Find("DialoguePanel");
                Transform panel = surface.Find("DialoguePanel/Panel");
                Transform closeButton = surface.Find("DialoguePanel/Panel/CloseButton");
                Transform logButton = surface.Find("DialoguePanel/Panel/LogButton");
                Transform persistentOrderButton = FindDescendantByName(surface, "PersistentOrderButton");
                Transform logPanel = surface.Find("DialogueLogPanel");
                Transform logCloseButton = surface.Find("DialogueLogPanel/DialogueLogCloseButton");
                Transform currentOrderPanel = surface.Find("CurrentOrderPanel");
                Transform currentOrderCloseButton = surface.Find("CurrentOrderPanel/CurrentOrderCloseButton");

                AssertNoVisibleContinueButton(prefab.transform, "P10D-24 prefab asset");
                AssertNoVisibleContinueButton(surface, "P10D-24 runtime surface");
                RequirePointerClickTarget(dialoguePanel, "P10D-24 DialoguePanel");
                RequirePointerClickTarget(panel, "P10D-24 dialogue panel background");

                ClickUi(dialoguePanel, "P10D-24 hidden/no-node DialoguePanel click", true);
                AssertLogCount(controller, 0, "P10D-24 hidden/no-node click advanced dialogue.");

                int expectedLogCount = 0;
                AssertSceneTrigger(manager, "StartPrologue", NodePrologue, true);
                controller.SetCurrentNode(manager.GetCurrentNode());
                expectedLogCount++;
                Text speakerSlot = FindBottomSpeakerText(controller, "P10D-24 initial speaker slot");
                Text bodySlot = FindBottomBodyText(controller, "P10D-24 initial body slot");
                RequirePointerClickTarget(speakerSlot.transform, "P10D-24 speaker text");
                RequirePointerClickTarget(bodySlot.transform, "P10D-24 body text");
                AssertFixedSlotTextEquals(controller, "旁白", "父亲走的那年，窑炉最后一次点火。", "P10D-24 prologue line 0");

                ClickUi(dialoguePanel, "P10D-24 DialoguePanel click", true);
                expectedLogCount++;
                AssertSameFixedTextSlots(controller, speakerSlot, bodySlot, "P10D-24 slots after DialoguePanel click");
                AssertFixedSlotVisibleText(controller, expectedLogCount, NodePrologue, 1, "P10D-24 DialoguePanel click advanced to line 1");

                ClickUi(bodySlot.transform, "P10D-24 body text click", true);
                expectedLogCount++;
                AssertSameFixedTextSlots(controller, speakerSlot, bodySlot, "P10D-24 slots after body click");
                AssertFixedSlotVisibleText(controller, expectedLogCount, NodePrologue, 2, "P10D-24 body click advanced to line 2");

                ClickUi(speakerSlot.transform, "P10D-24 speaker text click at final line", true);
                AssertNode(manager, P10NarrativeState.Tutorial, NodeTutorial, "P10D-24 speaker click at final line did not advance to the next node.");
                AssertLogCount(controller, expectedLogCount, "P10D-24 final-line click should not log until controller refresh.");
                controller.SetCurrentNode(manager.GetCurrentNode());
                expectedLogCount++;
                AssertSameFixedTextSlots(controller, speakerSlot, bodySlot, "P10D-24 slots after tutorial refresh");
                AssertFixedSlotVisibleText(controller, expectedLogCount, NodeTutorial, 0, "P10D-24 tutorial after final-line click");

                controller.SetCurrentNode(NodeOrder001Pass);
                expectedLogCount++;
                AssertNoVisibleContinueButton(surface, "P10D-24 runtime surface after order reset");
                AssertFixedSlotVisibleText(controller, expectedLogCount, NodeOrder001Pass, 0, "P10D-24 blank panel baseline");
                ClickUi(panel, "P10D-24 dialogue panel blank area click", true);
                expectedLogCount++;
                AssertSameFixedTextSlots(controller, speakerSlot, bodySlot, "P10D-24 slots after blank panel click");
                AssertFixedSlotVisibleText(controller, expectedLogCount, NodeOrder001Pass, 1, "P10D-24 blank panel click advanced");

                controller.SetCurrentNode(NodeOrder003Accept);
                expectedLogCount++;
                string logShieldBodyText = FindBottomBodyText(controller, "P10D-24 log shield baseline").text;
                ClickUi(logButton, "P10D-24 Log button click", true);
                if (!controller.IsDialogueLogVisible)
                {
                    throw new InvalidOperationException("P10D-24 Log button did not open Dialogue Log.");
                }

                AssertOverlayBlocksRaycasts(logPanel, "P10D-24 Dialogue Log panel");
                AssertPersistentButtonHidden(persistentOrderButton, "P10D-24 Order button while Dialogue Log is open");
                AssertPersistentButtonHidden(FindDescendantByName(surface, "PersistentLogButton"), "P10D-24 persistent Log button while Dialogue Log is open");
                AssertLogCount(controller, expectedLogCount, "P10D-24 Log button advanced dialogue.");
                ClickUi(logPanel, "P10D-24 Dialogue Log panel click", false);
                ClickUi(dialoguePanel, "P10D-24 shielded DialoguePanel click while Log is open", true);
                AssertFixedSlotTextEquals(controller, FindBottomSpeakerText(controller, "P10D-24 log shield speaker").text, logShieldBodyText, "P10D-24 log shield text unchanged");
                AssertLogCount(controller, expectedLogCount, "P10D-24 Log panel click or shielded bottom click advanced dialogue.");
                ClickUi(logCloseButton, "P10D-24 Dialogue Log close button click", true);
                if (controller.IsDialogueLogVisible)
                {
                    throw new InvalidOperationException("P10D-24 Dialogue Log close button did not close the panel.");
                }

                AssertLogCount(controller, expectedLogCount, "P10D-24 Dialogue Log close button advanced dialogue.");
                AssertFixedSlotTextEquals(controller, FindBottomSpeakerText(controller, "P10D-24 log close speaker").text, logShieldBodyText, "P10D-24 log close text unchanged");
                AssertPersistentButtonVisible(persistentOrderButton, "P10D-24 Order button after Dialogue Log close");
                AssertPersistentButtonVisible(FindDescendantByName(surface, "PersistentLogButton"), "P10D-24 persistent Log button after Dialogue Log close");

                controller.SetCurrentNode(NodeOrder004Accept);
                expectedLogCount++;
                string orderShieldBodyText = FindBottomBodyText(controller, "P10D-24 order shield baseline").text;
                ClickUi(persistentOrderButton, "P10D-24 Order button click", true);
                if (!controller.IsCurrentOrderPanelVisible)
                {
                    throw new InvalidOperationException("P10D-24 Order button did not open CurrentOrderPanel.");
                }

                AssertOverlayBlocksRaycasts(currentOrderPanel, "P10D-24 CurrentOrderPanel");
                AssertPersistentButtonHidden(persistentOrderButton, "P10D-24 Order button while CurrentOrderPanel is open");
                AssertPersistentButtonHidden(FindDescendantByName(surface, "PersistentLogButton"), "P10D-24 persistent Log button while CurrentOrderPanel is open");
                AssertLogCount(controller, expectedLogCount, "P10D-24 Order button advanced dialogue.");
                ClickUi(currentOrderPanel, "P10D-24 CurrentOrderPanel click", false);
                ClickUi(dialoguePanel, "P10D-24 shielded DialoguePanel click while CurrentOrderPanel is open", true);
                AssertLogCount(controller, expectedLogCount, "P10D-24 CurrentOrderPanel click or shielded bottom click advanced dialogue.");
                AssertFixedSlotTextEquals(controller, FindBottomSpeakerText(controller, "P10D-24 order shield speaker").text, orderShieldBodyText, "P10D-24 order shield text unchanged");
                ClickUi(currentOrderCloseButton, "P10D-24 CurrentOrderPanel close button click", true);
                if (controller.IsCurrentOrderPanelVisible)
                {
                    throw new InvalidOperationException("P10D-24 CurrentOrderPanel close button did not close the panel.");
                }

                AssertLogCount(controller, expectedLogCount, "P10D-24 CurrentOrderPanel close button advanced dialogue.");
                AssertFixedSlotTextEquals(controller, FindBottomSpeakerText(controller, "P10D-24 order close speaker").text, orderShieldBodyText, "P10D-24 order close text unchanged");
                AssertPersistentButtonVisible(persistentOrderButton, "P10D-24 Order button after CurrentOrderPanel close");
                AssertPersistentButtonVisible(FindDescendantByName(surface, "PersistentLogButton"), "P10D-24 persistent Log button after CurrentOrderPanel close");

                controller.SetCurrentNode(NodeOrder003Pass);
                expectedLogCount++;
                string closeBaselineBodyText = FindBottomBodyText(controller, "P10D-24 close baseline").text;
                ClickUi(closeButton, "P10D-24 close button click", true);
                AssertDialoguePanelHidden(surface, "P10D-24 close button");
                AssertLogCount(controller, expectedLogCount, "P10D-24 close button advanced dialogue.");
                ClickUi(dialoguePanel, "P10D-24 hidden DialoguePanel click after close", true);
                AssertLogCount(controller, expectedLogCount, "P10D-24 hidden dialogue click advanced dialogue.");
                AssertFixedSlotTextEquals(controller, FindBottomSpeakerText(controller, "P10D-24 hidden click speaker").text, closeBaselineBodyText, "P10D-24 hidden click text unchanged");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyGeneratedEventSystemIfNeeded(existingEventSystem);
            }
        }

        private static void ValidateEditorVisibleFixedDialogueTextSlotObjectsRepair()
        {
            EnsureEditorVisibleDialogueUiPrefab();
            EnsureOverlaySceneUsesEditorVisibleDialogueUiPrefab();

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EditorVisibleDialogueUiPrefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException("R9 editor-visible dialogue UI prefab missing: " + EditorVisibleDialogueUiPrefabPath + ".");
            }

            AssertManualSlotRootIsShallow(prefab);
            AssertUniqueSlotNameInPrefab(prefab, "P10_DialogueSpeakerText");
            AssertUniqueSlotNameInPrefab(prefab, "P10_DialogueBodyText");
            AssertEditorVisiblePrefabSlot(prefab, EditorVisibleSpeakerSlotPath, "R9 prefab speaker slot");
            AssertEditorVisiblePrefabSlot(prefab, EditorVisibleBodySlotPath, "R9 prefab body slot");
            Transform dialoguePanel = FindDescendantByName(prefab.transform, "DialoguePanel");
            Transform panel = dialoguePanel != null ? dialoguePanel.Find("Panel") : null;
            AssertDialoguePanelActionButtonsDoNotOverlap(panel);
            AssertPrefabYamlHasClearlyVisibleSlots();

            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject root = new GameObject("P10D20R9_EditorVisibleFixedSlotsRoot");
            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "EditorVisibleFixedSlotsManager");
                P10DialogueController controller = CreateDialogueController(manager.transform, manager);
                InjectPhase10AssetDialogueData(controller);
                AssignDialogueSurfacePrefab(controller, prefab);
                controller.EnsureRuntimeSurfaceInstance();

                if (controller.UsesRuntimeCreatedFixedTextSlotFallback)
                {
                    throw new InvalidOperationException("R9 validator detected runtime-created fallback slots. Prefab/editor-visible slots must be used.");
                }

                Text speakerSlot = FindBottomSpeakerText(controller, "R9 initial speaker slot");
                Text bodySlot = FindBottomBodyText(controller, "R9 initial body slot");
                AssertEditorVisibleRuntimeSlot(speakerSlot, "R9 runtime speaker slot");
                AssertEditorVisibleRuntimeSlot(bodySlot, "R9 runtime body slot");
                AssertBottomDialogueTextSlotsContained(controller, "R9 runtime contained initial slots");

                RectTransform speakerRect = speakerSlot.rectTransform;
                RectTransform bodyRect = bodySlot.rectTransform;
                Vector2 originalSpeakerAnchoredPosition = speakerRect.anchoredPosition;
                Vector2 originalBodyAnchoredPosition = bodyRect.anchoredPosition;
                Vector2 movedSpeakerAnchoredPosition = originalSpeakerAnchoredPosition + new Vector2(4f, -3f);
                Vector2 movedBodyAnchoredPosition = originalBodyAnchoredPosition + new Vector2(-4f, 3f);
                speakerRect.anchoredPosition = movedSpeakerAnchoredPosition;
                bodyRect.anchoredPosition = movedBodyAnchoredPosition;
                AssertBottomDialogueTextSlotsContained(controller, "R9 runtime contained moved slots");

                int expectedLogCount = 0;
                AssertSceneTrigger(manager, "StartPrologue", NodePrologue, true);
                controller.SetCurrentNode(manager.GetCurrentNode());
                expectedLogCount++;
                AssertSameFixedTextSlots(controller, speakerSlot, bodySlot, "R9 prologue fixed slot reuse");
                AssertRectTransformAnchoredPositionUnchanged(speakerRect, movedSpeakerAnchoredPosition, "R9 speaker slot after SetCurrentNode");
                AssertRectTransformAnchoredPositionUnchanged(bodyRect, movedBodyAnchoredPosition, "R9 body slot after SetCurrentNode");
                AssertFixedSlotVisibleText(controller, expectedLogCount, NodePrologue, 0, "R9 prologue narrator");
                AssertFixedSlotTextEquals(controller, "旁白", "父亲走的那年，窑炉最后一次点火。", "R9 prologue exact narrator line");
                AssertBottomDialogueTextSlotsContained(controller, "R9 prologue contained slots");

                controller.AdvanceDialogue();
                expectedLogCount++;
                AssertSameFixedTextSlots(controller, speakerSlot, bodySlot, "R9 prologue next fixed slot reuse");
                AssertRectTransformAnchoredPositionUnchanged(speakerRect, movedSpeakerAnchoredPosition, "R9 speaker slot after Next");
                AssertRectTransformAnchoredPositionUnchanged(bodyRect, movedBodyAnchoredPosition, "R9 body slot after Next");
                AssertFixedSlotVisibleText(controller, expectedLogCount, NodePrologue, 1, "R9 prologue Xu");
                AssertFixedSlotBodyDoesNotContainPrefix(controller, "徐老伯", "R9 Xu body prefix cleanup");

                controller.AdvanceDialogue();
                expectedLogCount++;
                AssertSameFixedTextSlots(controller, speakerSlot, bodySlot, "R9 system prompt fixed slot reuse");
                AssertRectTransformAnchoredPositionUnchanged(speakerRect, movedSpeakerAnchoredPosition, "R9 speaker slot after system prompt");
                AssertRectTransformAnchoredPositionUnchanged(bodyRect, movedBodyAnchoredPosition, "R9 body slot after system prompt");
                AssertFixedSlotVisibleText(controller, expectedLogCount, NodePrologue, 2, "R9 system prompt");
                AssertFixedSlotBodyDoesNotContainPrefix(controller, "系统提示", "R9 system prompt body prefix cleanup");

                controller.SetCurrentNode(NodeOrder001Pass);
                expectedLogCount++;
                controller.AdvanceDialogue();
                expectedLogCount++;
                controller.AdvanceDialogue();
                expectedLogCount++;
                AssertSameFixedTextSlots(controller, speakerSlot, bodySlot, "R9 UI result fixed slot reuse");
                AssertRectTransformAnchoredPositionUnchanged(speakerRect, movedSpeakerAnchoredPosition, "R9 speaker slot after UI result");
                AssertRectTransformAnchoredPositionUnchanged(bodyRect, movedBodyAnchoredPosition, "R9 body slot after UI result");
                AssertFixedSlotVisibleText(controller, expectedLogCount, NodeOrder001Pass, 2, "R9 ORDER_001 UI result");
                AssertFixedSlotBodyDoesNotContainPrefix(controller, "UI结果", "R9 UI result body prefix cleanup");
                AssertDialogueLogLatestEntriesRemainStructured(controller);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyGeneratedEventSystemIfNeeded(existingEventSystem);
            }
        }

        private static void ValidateCurrentOrderPanelWithCraftingHints()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EditorVisibleDialogueUiPrefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException("P10D-21 dialogue UI prefab missing.");
            }

            AssertCurrentOrderPrefabUi(prefab);

            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject root = new GameObject("P10D21_CurrentOrderPanelRoot");
            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "CurrentOrderPanelManager");
                P10DialogueController controller = CreateDialogueController(manager.transform, manager);
                InjectPhase10AssetDialogueData(controller);
                AssignDialogueSurfacePrefab(controller, prefab);
                controller.EnsureRuntimeSurfaceInstance();

                AssertCurrentOrderPanelClosed(controller, "initial closed");
                controller.OpenCurrentOrderPanel();
                AssertCurrentOrderPanelText(controller, "P10D-21 no order", "当前无订单");
                AssertDialogueLogUnaffectedByOrderPanel(controller, manager, 0, P10NarrativeState.None, string.Empty, "no order open");
                controller.CloseCurrentOrderPanel();
                AssertCurrentOrderPanelClosed(controller, "no order close");

                AssertSceneTrigger(manager, "StartPrologue", NodePrologue, true);
                controller.SetCurrentNode(manager.GetCurrentNode());
                controller.OpenCurrentOrderPanel();
                AssertCurrentOrderPanelText(controller, "P10D-21 prologue no order", "当前无订单");
                AssertDialogueLogUnaffectedByOrderPanel(controller, manager, 1, P10NarrativeState.Prologue, NodePrologue, "prologue order panel");

                if (!manager.TryAdvanceDialogueNode(NodePrologue, NodeTutorial) || !manager.TryAdvanceDialogueNode(NodeTutorial, NodeOrder001Accept))
                {
                    throw new InvalidOperationException("P10D-21 could not reach ORDER_001 accept.");
                }

                controller.SetCurrentNode(NodeOrder001Accept);
                controller.OpenCurrentOrderPanel();
                AssertCurrentOrderPanelText(
                    controller,
                    "P10D-21 ORDER_001",
                    "订单名称：甜白釉茶碗",
                    "来源 NPC：周掌柜",
                    "当前目标：制作 3 只甜白釉茶碗",
                    "器型提示：碗口要宽，碗底要稳",
                    "釉色提示：白里带一点暖，不要冷冰冰",
                    "烧成温度：1250°C - 1300°C",
                    "评分要求：器型评分 ≥ 80，釉色评分 ≥ 75",
                    "奖励：50 铜钱，声望 +10",
                    "状态：进行中");
                AssertDialogueLogUnaffectedByOrderPanel(controller, manager, 2, P10NarrativeState.Order001, NodeOrder001Accept, "ORDER_001 order panel");

                PublishOrderCompleted(manager, "ORDER_001");
                AssertNode(manager, P10NarrativeState.Order001, NodeOrder001Pass, "P10D-21 ORDER_001 completion did not enter pass.");
                controller.SetCurrentNode(NodeOrder001Pass);
                controller.OpenCurrentOrderPanel();
                AssertCurrentOrderPanelText(controller, "P10D-21 after ORDER_001", "当前无订单");

                if (!manager.TryAdvanceDialogueNode(NodeOrder001Pass, NodeOrder003Accept))
                {
                    throw new InvalidOperationException("P10D-21 could not reach ORDER_003 accept.");
                }

                controller.SetCurrentNode(NodeOrder003Accept);
                controller.OpenCurrentOrderPanel();
                AssertCurrentOrderPanelText(
                    controller,
                    "P10D-21 ORDER_003",
                    "订单名称：影青釉茶碗",
                    "来源 NPC：陈书院",
                    "当前目标：制作 1 只影青釉茶碗",
                    "器型提示：碗口要正，腹部要收",
                    "釉色提示：半透半不透，像雾里的月",
                    "烧成温度：1250°C - 1280°C",
                    "评分要求：器型评分 ≥ 80，釉色评分 ≥ 75",
                    "奖励：55 铜钱，声望 +10",
                    "状态：进行中");

                PublishOrderCompleted(manager, "ORDER_003");
                AssertNode(manager, P10NarrativeState.Order003, NodeOrder003Pass, "P10D-21 ORDER_003 completion did not enter pass.");
                controller.SetCurrentNode(NodeOrder003Pass);
                controller.OpenCurrentOrderPanel();
                AssertCurrentOrderPanelText(controller, "P10D-21 after ORDER_003", "当前无订单");

                if (!manager.TryAdvanceDialogueNode(NodeOrder003Pass, NodeOrder004Accept))
                {
                    throw new InvalidOperationException("P10D-21 could not reach ORDER_004 accept.");
                }

                controller.SetCurrentNode(NodeOrder004Accept);
                controller.OpenCurrentOrderPanel();
                AssertCurrentOrderPanelText(
                    controller,
                    "P10D-21 ORDER_004",
                    "订单名称：祭红釉香筒",
                    "来源 NPC：卢客",
                    "当前目标：制作 1 件祭红釉直筒香罐",
                    "器型提示：直筒型，高一点，口径约十厘米，高约十三厘米",
                    "釉色提示：正红，不能暗，不能偏紫，不能发黑",
                    "烧成温度：1250°C - 1280°C",
                    "评分要求：烧窑评分 ≥ 70，精品综合评分 ≥ 95",
                    "奖励：70 铜钱，声望 +10；精品可获得额外奖励",
                    "状态：进行中");

                PublishOrderCompleted(manager, "ORDER_004");
                AssertNode(manager, P10NarrativeState.Order004, NodeOrder004PassNormal, "P10D-21 ORDER_004 completion did not enter normal pass.");
                controller.SetCurrentNode(NodeOrder004PassNormal);
                controller.OpenCurrentOrderPanel();
                AssertCurrentOrderPanelText(controller, "P10D-21 after ORDER_004", "当前无订单");

                if (!manager.TryAdvanceDialogueNode(NodeOrder004PassNormal, NodeChapterEnding))
                {
                    throw new InvalidOperationException("P10D-21 could not reach chapter ending.");
                }

                controller.SetCurrentNode(NodeChapterEnding);
                controller.OpenCurrentOrderPanel();
                AssertCurrentOrderPanelTextAny(controller, "P10D-21 completed", "当前无订单", "第一章已完成");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyGeneratedEventSystemIfNeeded(existingEventSystem);
            }
        }

        private static void ValidateTopRightActionBarAndPanelMutualExclusion()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EditorVisibleDialogueUiPrefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException("P10D-24S dialogue UI prefab missing.");
            }

            AssertTopRightActionBarPrefabUi(prefab);

            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject root = new GameObject("P10D24S_TopRightActionBarRoot");
            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "TopRightActionBarManager");
                P10DialogueController controller = CreateDialogueController(manager.transform, manager);
                InjectPhase10AssetDialogueData(controller);
                AssignDialogueSurfacePrefab(controller, prefab);
                controller.EnsureRuntimeSurfaceInstance();

                Transform surface = FindRuntimeDialogueSurface(controller, "P10D-24S runtime surface");
                Transform actionBar = surface.Find("P10_TopRightActionBar");
                Transform persistentLogButton = FindDescendantByName(actionBar, "PersistentLogButton");
                Transform persistentOrderButton = FindDescendantByName(actionBar, "PersistentOrderButton");
                Transform dialoguePanel = surface.Find("DialoguePanel");
                Transform logPanel = surface.Find("DialogueLogPanel");
                Transform logCloseButton = surface.Find("DialogueLogPanel/DialogueLogCloseButton");
                Transform currentOrderPanel = surface.Find("CurrentOrderPanel");
                Transform currentOrderCloseButton = surface.Find("CurrentOrderPanel/CurrentOrderCloseButton");

                AssertActionBarVisible(actionBar, "P10D-24S initial action bar");
                AssertPersistentButtonVisible(persistentLogButton, "P10D-24S initial Log button");
                AssertPersistentButtonVisible(persistentOrderButton, "P10D-24S initial Order button");
                AssertTopRightButtonsDoNotOverlap(actionBar, "P10D-24S initial action bar");

                int expectedLogCount = 0;
                AssertSceneTrigger(manager, "StartPrologue", NodePrologue, true);
                controller.SetCurrentNode(manager.GetCurrentNode());
                expectedLogCount++;

                string logBaselineBodyText = FindBottomBodyText(controller, "P10D-24S Log baseline").text;
                ClickUi(persistentLogButton, "P10D-24S persistent Log button click", true);
                if (!controller.IsDialogueLogVisible)
                {
                    throw new InvalidOperationException("P10D-24S persistent Log button did not open Dialogue Log.");
                }

                AssertActionBarHidden(actionBar, "P10D-24S action bar while Log is open");
                AssertPersistentButtonHidden(persistentLogButton, "P10D-24S Log button while Log is open");
                AssertPersistentButtonHidden(persistentOrderButton, "P10D-24S Order button while Log is open");
                AssertOverlayBlocksRaycasts(logPanel, "P10D-24S Dialogue Log panel");
                ClickUi(logPanel, "P10D-24S Dialogue Log panel click", false);
                ClickUi(dialoguePanel, "P10D-24S shielded DialoguePanel click while Log is open", true);
                AssertLogCount(controller, expectedLogCount, "P10D-24S Log panel click advanced dialogue.");
                AssertFixedSlotTextEquals(controller, FindBottomSpeakerText(controller, "P10D-24S Log shield speaker").text, logBaselineBodyText, "P10D-24S Log shield text changed");

                ClickUi(logCloseButton, "P10D-24S Dialogue Log close button click", true);
                if (controller.IsDialogueLogVisible)
                {
                    throw new InvalidOperationException("P10D-24S Dialogue Log close button did not close panel.");
                }

                AssertLogCount(controller, expectedLogCount, "P10D-24S Dialogue Log close advanced dialogue.");
                AssertActionBarVisible(actionBar, "P10D-24S action bar after Log close");
                AssertTopRightButtonsDoNotOverlap(actionBar, "P10D-24S action bar after Log close");

                controller.SetCurrentNode(NodeOrder004Accept);
                expectedLogCount++;
                string orderBaselineBodyText = FindBottomBodyText(controller, "P10D-24S Order baseline").text;
                ClickUi(persistentOrderButton, "P10D-24S persistent Order button click", true);
                if (!controller.IsCurrentOrderPanelVisible)
                {
                    throw new InvalidOperationException("P10D-24S persistent Order button did not open CurrentOrderPanel.");
                }

                AssertActionBarHidden(actionBar, "P10D-24S action bar while Order is open");
                AssertPersistentButtonHidden(persistentLogButton, "P10D-24S Log button while Order is open");
                AssertPersistentButtonHidden(persistentOrderButton, "P10D-24S Order button while Order is open");
                AssertOverlayBlocksRaycasts(currentOrderPanel, "P10D-24S CurrentOrderPanel");
                ClickUi(currentOrderPanel, "P10D-24S CurrentOrderPanel click", false);
                ClickUi(dialoguePanel, "P10D-24S shielded DialoguePanel click while Order is open", true);
                AssertLogCount(controller, expectedLogCount, "P10D-24S Order panel click advanced dialogue.");
                AssertFixedSlotTextEquals(controller, FindBottomSpeakerText(controller, "P10D-24S Order shield speaker").text, orderBaselineBodyText, "P10D-24S Order shield text changed");

                ClickUi(currentOrderCloseButton, "P10D-24S CurrentOrderPanel close button click", true);
                if (controller.IsCurrentOrderPanelVisible)
                {
                    throw new InvalidOperationException("P10D-24S CurrentOrderPanel close button did not close panel.");
                }

                AssertLogCount(controller, expectedLogCount, "P10D-24S CurrentOrderPanel close advanced dialogue.");
                AssertActionBarVisible(actionBar, "P10D-24S action bar after Order close");
                AssertTopRightButtonsDoNotOverlap(actionBar, "P10D-24S action bar after Order close");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyGeneratedEventSystemIfNeeded(existingEventSystem);
            }
        }

        private static void ValidateScrollbarDragInteraction()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EditorVisibleDialogueUiPrefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException("P10D-24T dialogue UI prefab missing.");
            }

            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject root = new GameObject("P10D24T_ScrollbarDragInteractionRoot");
            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "ScrollbarDragInteractionManager");
                P10DialogueController controller = CreateDialogueController(manager.transform, manager);
                InjectPhase10AssetDialogueData(controller);
                AssignDialogueSurfacePrefab(controller, prefab);
                controller.EnsureRuntimeSurfaceInstance();

                Transform surface = FindRuntimeDialogueSurface(controller, "P10D-24T runtime surface");
                Transform dialoguePanel = surface.Find("DialoguePanel");

                int expectedLogCount = 0;
                AssertSceneTrigger(manager, "StartPrologue", NodePrologue, true);
                controller.SetCurrentNode(manager.GetCurrentNode());
                expectedLogCount++;
                AppendSyntheticDialogueLogEntries(controller, 24);

                controller.OpenDialogueLog();
                if (!controller.IsDialogueLogVisible)
                {
                    throw new InvalidOperationException("P10D-24T Dialogue Log did not open.");
                }

                Transform logScrollTransform = surface.Find("DialogueLogPanel/DialogueLogScroll");
                ScrollRect logScroll = RequireScrollRect(logScrollTransform, "P10D-24T Dialogue Log");
                Scrollbar logScrollbar = RequireScrollbar(logScroll, "P10D-24T Dialogue Log");
                AssertScrollContentExceedsViewport(logScroll, "P10D-24T Dialogue Log");
                AssertScrollbarHandleInsideTrack(logScrollbar, "P10D-24T Dialogue Log");
                AssertScrollbarDragChangesPosition(logScroll, logScrollbar, "P10D-24T Dialogue Log");
                int beforeLogPanelClickCount = controller.GetDialogueLogCount();
                ClickUi(logScrollTransform, "P10D-24T Dialogue Log ScrollView click", false);
                ClickUi(logScrollbar.transform, "P10D-24T Dialogue Log Scrollbar click", false);
                ClickUi(logScrollbar.handleRect, "P10D-24T Dialogue Log Scrollbar handle click", false);
                ClickUi(dialoguePanel, "P10D-24T shielded dialogue click while Log scroll is open", true);
                AssertLogCount(controller, beforeLogPanelClickCount, "P10D-24T Log ScrollView / Scrollbar click advanced dialogue.");
                controller.CloseDialogueLog();

                controller.SetCurrentNode(NodeOrder004Accept);
                expectedLogCount = controller.GetDialogueLogCount();
                controller.OpenCurrentOrderPanel();
                if (!controller.IsCurrentOrderPanelVisible)
                {
                    throw new InvalidOperationException("P10D-24T CurrentOrderPanel did not open.");
                }

                Transform orderScrollTransform = surface.Find("CurrentOrderPanel/CurrentOrderScroll");
                ScrollRect orderScroll = RequireScrollRect(orderScrollTransform, "P10D-24T Current Order");
                Scrollbar orderScrollbar = RequireScrollbar(orderScroll, "P10D-24T Current Order");
                ForceLongCurrentOrderContent(surface);
                Canvas.ForceUpdateCanvases();
                AssertScrollContentExceedsViewport(orderScroll, "P10D-24T Current Order");
                AssertScrollbarHandleInsideTrack(orderScrollbar, "P10D-24T Current Order");
                AssertScrollbarDragChangesPosition(orderScroll, orderScrollbar, "P10D-24T Current Order");
                ClickUi(orderScrollTransform, "P10D-24T Current Order ScrollView click", false);
                ClickUi(orderScrollbar.transform, "P10D-24T Current Order Scrollbar click", false);
                ClickUi(orderScrollbar.handleRect, "P10D-24T Current Order Scrollbar handle click", false);
                ClickUi(dialoguePanel, "P10D-24T shielded dialogue click while Order scroll is open", true);
                AssertLogCount(controller, expectedLogCount, "P10D-24T Order ScrollView / Scrollbar click advanced dialogue.");
                controller.CloseCurrentOrderPanel();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyGeneratedEventSystemIfNeeded(existingEventSystem);
            }
        }

        private static void AssertCurrentOrderPrefabUi(GameObject prefab)
        {
            Transform surface = FindDescendantByName(prefab.transform, "P10_Runtime_DialogueSurface");
            if (surface == null)
            {
                throw new InvalidOperationException("P10D-21 prefab missing runtime dialogue surface.");
            }

            AssertPrefabButton(surface, "PersistentOrderButton", "P10D-21 persistent order button");
            Transform panel = surface.Find("CurrentOrderPanel");
            if (panel == null)
            {
                throw new InvalidOperationException("P10D-21 prefab missing CurrentOrderPanel.");
            }

            AssertEditorVisiblePrefabText(panel, "CurrentOrderTitleText", "当前订单", "P10D-21 order title");
            AssertEditorVisiblePrefabText(panel, "CurrentOrderContentText", "当前无订单", "P10D-21 order content");
            AssertPrefabButton(panel, "CurrentOrderCloseButton", "P10D-21 current order close button");
        }

        private static void AssertTopRightActionBarPrefabUi(GameObject prefab)
        {
            Transform surface = FindDescendantByName(prefab.transform, "P10_Runtime_DialogueSurface");
            if (surface == null)
            {
                throw new InvalidOperationException("P10D-24S prefab missing runtime dialogue surface.");
            }

            Transform actionBar = surface.Find("P10_TopRightActionBar");
            if (actionBar == null)
            {
                throw new InvalidOperationException("P10D-24S prefab missing P10_TopRightActionBar.");
            }

            if (actionBar.parent != surface)
            {
                throw new InvalidOperationException("P10D-24S P10_TopRightActionBar must be a direct child of runtime surface.");
            }

            HorizontalLayoutGroup layout = actionBar.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                throw new InvalidOperationException("P10D-24S P10_TopRightActionBar missing HorizontalLayoutGroup.");
            }

            if (layout.spacing < 12f || layout.spacing > 20f)
            {
                throw new InvalidOperationException("P10D-24S P10_TopRightActionBar spacing must be 12-20 px. Actual: " + layout.spacing + ".");
            }

            AssertPrefabButton(actionBar, "PersistentLogButton", "P10D-24S persistent Log button");
            AssertPrefabButton(actionBar, "PersistentOrderButton", "P10D-24S persistent Order button");
            AssertTopRightButtonsDoNotOverlap(actionBar, "P10D-24S prefab action bar");
        }

        private static void DisplaySpecificLine(
            P10DialogueController controller,
            ref int expectedLogCount,
            string nodeId,
            int lineIndex,
            string label)
        {
            controller.SetCurrentNode(nodeId);
            expectedLogCount++;
            AssertCompleteVisibleBottomDialogueLine(controller, expectedLogCount, nodeId, 0, label + " line 0");
            AssertNoBareTechnicalTerm(FindBottomBodyText(controller, label + " bottom line 0").text, label + " bottom line 0");

            for (int i = 1; i <= lineIndex; i++)
            {
                controller.AdvanceDialogue();
                expectedLogCount++;
                AssertCompleteVisibleBottomDialogueLine(controller, expectedLogCount, nodeId, i, label + " line " + i);
                AssertNoBareTechnicalTerm(FindBottomBodyText(controller, label + " bottom line " + i).text, label + " bottom line " + i);
            }
        }

        private static void AssertDialogueLogLayout(Transform surface)
        {
            if (surface == null)
            {
                throw new InvalidOperationException("R7 runtime dialogue surface is missing.");
            }

            Canvas.ForceUpdateCanvases();

            Transform panel = surface.Find("DialogueLogPanel");
            Transform title = surface.Find("DialogueLogPanel/DialogueLogTitle");
            Transform closeButton = surface.Find("DialogueLogPanel/DialogueLogCloseButton");
            Transform scroll = surface.Find("DialogueLogPanel/DialogueLogScroll");
            Transform viewport = surface.Find("DialogueLogPanel/DialogueLogScroll/Viewport");
            Transform content = surface.Find("DialogueLogPanel/DialogueLogScroll/Viewport/Content");
            if (panel == null || title == null || closeButton == null || scroll == null || viewport == null || content == null)
            {
                throw new InvalidOperationException("R7 Dialogue Log hierarchy is incomplete.");
            }

            CanvasGroup panelGroup = panel.GetComponent<CanvasGroup>();
            if (panelGroup == null || panelGroup.alpha <= 0.5f || !panelGroup.interactable || !panelGroup.blocksRaycasts)
            {
                throw new InvalidOperationException("R7 Dialogue Log panel is not visibly open.");
            }

            RectTransform titleRect = RequireRect(title, "R7 Dialogue Log title");
            RectTransform closeRect = RequireRect(closeButton, "R7 Dialogue Log close button");
            RectTransform scrollRectTransform = RequireRect(scroll, "R7 Dialogue Log scroll area");
            RectTransform viewportRect = RequireRect(viewport, "R7 Dialogue Log viewport");
            RectTransform contentRect = RequireRect(content, "R7 Dialogue Log content");

            if (scrollRectTransform.anchorMin.x < 0.035f ||
                scrollRectTransform.anchorMin.y < 0.06f ||
                scrollRectTransform.anchorMax.x > 0.965f ||
                scrollRectTransform.anchorMax.y > 0.85f)
            {
                throw new InvalidOperationException("R7 Dialogue Log scroll area does not leave enough panel margin.");
            }

            if (scrollRectTransform.anchorMax.y > titleRect.anchorMin.y - 0.02f)
            {
                throw new InvalidOperationException("R7 Dialogue Log title and list do not have enough vertical separation.");
            }

            if (viewportRect.offsetMin.x < 16f ||
                viewportRect.offsetMin.y < 16f ||
                viewportRect.offsetMax.x > -16f ||
                viewportRect.offsetMax.y > -16f)
            {
                throw new InvalidOperationException("R7 Dialogue Log viewport padding is too tight.");
            }

            if (GetWorldRect(closeRect).Overlaps(GetWorldRect(viewportRect)))
            {
                throw new InvalidOperationException("R7 Dialogue Log close button overlaps the content viewport.");
            }

            ScrollRect scrollRect = scroll.GetComponent<ScrollRect>();
            if (scrollRect == null || scrollRect.viewport != viewportRect || scrollRect.content != contentRect || !scrollRect.vertical || scrollRect.horizontal)
            {
                throw new InvalidOperationException("R7 Dialogue Log ScrollRect binding or direction is invalid.");
            }

            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            if (layout == null ||
                layout.padding.left < 10 ||
                layout.padding.right < 10 ||
                layout.padding.top < 8 ||
                layout.padding.bottom < 18 ||
                layout.spacing < 14f ||
                !layout.childControlWidth ||
                !layout.childControlHeight ||
                layout.childForceExpandHeight)
            {
                throw new InvalidOperationException("R7 Dialogue Log content layout lacks readable padding or spacing.");
            }

            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter == null || fitter.verticalFit != ContentSizeFitter.FitMode.PreferredSize)
            {
                throw new InvalidOperationException("R7 Dialogue Log content does not size to its entry content.");
            }

            if (contentRect.anchorMin != new Vector2(0f, 1f) ||
                contentRect.anchorMax != new Vector2(1f, 1f) ||
                contentRect.pivot != new Vector2(0.5f, 1f))
            {
                throw new InvalidOperationException("R7 Dialogue Log content should grow downward from the viewport top.");
            }
        }

        private static void AssertDialogueLogEntriesAreReadableAndLocalized(P10DialogueController controller, Transform surface)
        {
            Transform content = surface.Find("DialogueLogPanel/DialogueLogScroll/Viewport/Content");
            if (content == null)
            {
                throw new InvalidOperationException("R7 Dialogue Log content is missing.");
            }

            Canvas.ForceUpdateCanvases();

            for (int i = 0; i < controller.DialogueLogEntries.Count; i++)
            {
                P10DialogueLogEntry entry = controller.DialogueLogEntries[i];
                Transform item = content.Find("DialogueLogEntry_" + entry.Sequence);
                if (item == null)
                {
                    throw new InvalidOperationException("R7 Dialogue Log entry view is missing for sequence " + entry.Sequence + ".");
                }

                VerticalLayoutGroup itemLayout = item.GetComponent<VerticalLayoutGroup>();
                if (itemLayout == null || itemLayout.spacing < 4f || !itemLayout.childControlWidth || !itemLayout.childControlHeight)
                {
                    throw new InvalidOperationException("R7 Dialogue Log entry layout is too cramped for sequence " + entry.Sequence + ".");
                }

                Text speakerText = item.Find("Speaker")?.GetComponent<Text>();
                Text lineText = item.Find("Line")?.GetComponent<Text>();
                Text metaText = item.Find("Meta")?.GetComponent<Text>();
                AssertReadableLogText(speakerText, "R7 log speaker #" + entry.Sequence);
                AssertReadableLogText(lineText, "R7 log line #" + entry.Sequence);
                AssertReadableLogText(metaText, "R7 log meta #" + entry.Sequence);

                AssertNoBareTechnicalTerm(speakerText.text, "R7 log speaker #" + entry.Sequence);
                AssertNoBareTechnicalTerm(lineText.text, "R7 log line #" + entry.Sequence);
                AssertNoBareTechnicalTerm(metaText.text, "R7 log meta #" + entry.Sequence);

                if (!metaText.text.StartsWith("记录 #", StringComparison.Ordinal) || metaText.text.Contains("节点 "))
                {
                    throw new InvalidOperationException("R7 Dialogue Log meta should be weak, localized, and should not expose node ids.");
                }

                if (metaText.color.a > 0.5f)
                {
                    throw new InvalidOperationException("R7 Dialogue Log meta is too visually prominent.");
                }
            }
        }

        private static void AssertLogContentStillRecordsSpeakerAndText(P10DialogueController controller, Transform surface)
        {
            Transform content = surface.Find("DialogueLogPanel/DialogueLogScroll/Viewport/Content");
            if (content == null)
            {
                throw new InvalidOperationException("R7 Dialogue Log content is missing for speaker/text validation.");
            }

            for (int i = 0; i < controller.DialogueLogEntries.Count; i++)
            {
                P10DialogueLogEntry entry = controller.DialogueLogEntries[i];
                Transform item = content.Find("DialogueLogEntry_" + entry.Sequence);
                Text speakerText = item?.Find("Speaker")?.GetComponent<Text>();
                Text lineText = item?.Find("Line")?.GetComponent<Text>();
                string expectedLineText = "内容：" + NormalizeExpectedBodyText(entry.SpeakerName, entry.DialogueText);

                if (speakerText == null || !string.Equals(speakerText.text, "说话人：" + entry.SpeakerName, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("R7 Dialogue Log no longer records the entry speaker visibly.");
                }

                if (lineText == null || !string.Equals(lineText.text, expectedLineText, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("R7 Dialogue Log no longer records the entry text visibly.");
                }
            }
        }

        private static void AssertPlayerFacingTechnicalTermsAbsentFromDialogueAssets()
        {
            foreach (P10DialogueNodeSO node in LoadDialogueNodes())
            {
                if (node == null)
                {
                    continue;
                }

                AssertNoBareTechnicalTerm(node.DialogueText, node.NodeId + " DialogueText");
                if (node.DialogueLines == null)
                {
                    continue;
                }

                for (int i = 0; i < node.DialogueLines.Count; i++)
                {
                    P10DialogueLine line = node.DialogueLines[i];
                    if (line != null)
                    {
                        AssertNoBareTechnicalTerm(line.DialogueText, node.NodeId + "#" + i + " DialogueText");
                    }
                }
            }
        }

        private static void ValidateChapterFlow(bool climax)
        {
            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject root = new GameObject(climax ? "P10D20R2_ClimaxFlowRoot" : "P10D14_NormalFlowRoot");
            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "FlowManager");
                P10DialogueController controller = CreateDialogueController(manager.transform, manager);
                InjectPhase10AssetDialogueData(controller);
                controller.EnsureRuntimeSurfaceInstance();
                int expectedLogCount = 0;

                AssertSceneTrigger(manager, "StartPrologue", NodePrologue, true);
                DisplayAndDrainNode(controller, manager, ref expectedLogCount, NodePrologue);
                AdvanceToNode(controller, manager, P10NarrativeState.Tutorial, NodeTutorial);
                DisplayAndDrainNode(controller, manager, ref expectedLogCount, NodeTutorial);
                AdvanceToNode(controller, manager, P10NarrativeState.Order001, NodeOrder001Accept);
                DisplayAndDrainNode(controller, manager, ref expectedLogCount, NodeOrder001Accept);
                AssertBlockedDialogueNext(controller, manager, expectedLogCount, P10NarrativeState.Order001, NodeOrder001Accept);

                PublishOrderCompleted(manager, "ORDER_001");
                AssertNode(manager, P10NarrativeState.Order001, NodeOrder001Pass, "ORDER_001 did not enter pass node.");
                DisplayAndDrainNode(controller, manager, ref expectedLogCount, NodeOrder001Pass);
                AdvanceToNode(controller, manager, P10NarrativeState.Order003, NodeOrder003Accept);
                DisplayAndDrainNode(controller, manager, ref expectedLogCount, NodeOrder003Accept);
                AssertBlockedDialogueNext(controller, manager, expectedLogCount, P10NarrativeState.Order003, NodeOrder003Accept);

                PublishOrderCompleted(manager, "ORDER_003");
                AssertNode(manager, P10NarrativeState.Order003, NodeOrder003Pass, "ORDER_003 did not enter pass node.");
                DisplayAndDrainNode(controller, manager, ref expectedLogCount, NodeOrder003Pass);
                AdvanceToNode(controller, manager, P10NarrativeState.Order004, NodeOrder004Accept);
                DisplayAndDrainNode(controller, manager, ref expectedLogCount, NodeOrder004Accept);
                AssertBlockedDialogueNext(controller, manager, expectedLogCount, P10NarrativeState.Order004, NodeOrder004Accept);

                if (climax)
                {
                    PublishScoreThreshold(manager, 95);
                    AssertNode(manager, P10NarrativeState.Order004, NodeOrder004Climax, "ORDER_004 climax did not trigger.");
                    DisplayAndDrainNode(controller, manager, ref expectedLogCount, NodeOrder004Climax);
                    AdvanceToNode(controller, manager, P10NarrativeState.Ending, NodeChapterEnding);
                }
                else
                {
                    PublishOrderCompleted(manager, "ORDER_004");
                    AssertNode(manager, P10NarrativeState.Order004, NodeOrder004PassNormal, "ORDER_004 normal completion did not trigger.");
                    DisplayAndDrainNode(controller, manager, ref expectedLogCount, NodeOrder004PassNormal);
                    AdvanceToNode(controller, manager, P10NarrativeState.Ending, NodeChapterEnding);
                }

                DisplayAndDrainNode(controller, manager, ref expectedLogCount, NodeChapterEnding);
                AssertRefreshDoesNotMutate(controller, manager, expectedLogCount, NodeChapterEnding);
                AssertDialogueLogPanelDoesNotMutate(controller, manager, expectedLogCount);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyGeneratedEventSystemIfNeeded(existingEventSystem);
            }
        }

        private static void ValidateNoUiNormalCompletionCycle()
        {
            GameObject root = new GameObject("P10D20R2_NoUiNormalRoot");
            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "NoUiNormalManager");
                ReachOrder004AcceptWithoutUi(manager);
                PublishOrderCompleted(manager, "ORDER_004");
                AssertNode(manager, P10NarrativeState.Order004, NodeOrder004PassNormal, "ORDER_004 normal completion drifted.");
                PublishOrderCompleted(manager, "ORDER_004");
                AssertNode(manager, P10NarrativeState.Order004, NodeOrder004PassNormal, "Duplicate ORDER_004 completion changed normal result.");
                if (!manager.TryAdvanceDialogueNode(NodeOrder004PassNormal, NodeChapterEnding))
                {
                    throw new InvalidOperationException("Normal ORDER_004 result did not advance to Chapter Ending.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void ValidateNoUiClimaxCompletionCycle()
        {
            GameObject root = new GameObject("P10D20R2_NoUiClimaxRoot");
            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "NoUiClimaxManager");
                ReachOrder004AcceptWithoutUi(manager);
                PublishScoreThreshold(manager, 95);
                AssertNode(manager, P10NarrativeState.Order004, NodeOrder004Climax, "ORDER_004 climax completion drifted.");
                PublishScoreThreshold(manager, 95);
                AssertNode(manager, P10NarrativeState.Order004, NodeOrder004Climax, "Duplicate score threshold changed climax result.");
                if (!manager.TryAdvanceDialogueNode(NodeOrder004Climax, NodeChapterEnding))
                {
                    throw new InvalidOperationException("Climax ORDER_004 result did not advance to Chapter Ending.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void ValidateQueuedClimaxCompletionCycle()
        {
            GameObject root = new GameObject("P10D20R2_QueuedClimaxRoot");
            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "QueuedClimaxManager");
                ReachOrder003PassWithoutUi(manager);
                PublishScoreThreshold(manager, 95);
                AssertNode(manager, P10NarrativeState.Order003, NodeOrder003Pass, "Queued score threshold changed state too early.");
                if (!manager.TryAdvanceDialogueNode(NodeOrder003Pass, NodeOrder004Accept))
                {
                    throw new InvalidOperationException("Queued climax flow could not enter ORDER_004 accept.");
                }

                PublishOrderCompleted(manager, "ORDER_004");
                AssertNode(manager, P10NarrativeState.Order004, NodeOrder004Climax, "Queued climax flag did not approve ORDER_004 climax.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void ValidateApprovedTriggerCompletionProbe()
        {
            GameObject root = new GameObject("P10D_ApprovedTriggerProbeRoot");
            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "ApprovedTriggerManager");
                AssertSceneTrigger(manager, "StartPrologue", NodePrologue, true);
                AssertSceneTrigger(manager, "Tutorial", NodeTutorial, true);
                AssertSceneTrigger(manager, "Order001Accept", NodeOrder001Accept, true);
                AssertSceneTrigger(manager, "Order001Pass", NodeOrder001Pass, true);
                AssertSceneTrigger(manager, "Order003Accept", NodeOrder003Accept, true);
                AssertSceneTrigger(manager, "Order003Pass", NodeOrder003Pass, true);
                AssertSceneTrigger(manager, "Order004Accept", NodeOrder004Accept, true);
                AssertSceneTrigger(manager, "Order004PassNormal", NodeOrder004PassNormal, true);
                AssertSceneTrigger(manager, "ChapterEnding", NodeChapterEnding, true);
                AssertNode(manager, P10NarrativeState.Completed, NodeChapterEnding, "Approved trigger chain did not complete Chapter 1.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void ValidateSnapshotRestoreDoesNotDrift()
        {
            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject root = new GameObject("P10D_SnapshotRoot");
            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "SnapshotManager");
                P10DialogueController controller = CreateDialogueController(manager.transform, manager);
                InjectPhase10AssetDialogueData(controller);
                controller.EnsureRuntimeSurfaceInstance();

                int expectedLogCount = 0;
                AssertSceneTrigger(manager, "StartPrologue", NodePrologue, true);
                DisplayAndDrainNode(controller, manager, ref expectedLogCount, NodePrologue);
                AdvanceToNode(controller, manager, P10NarrativeState.Tutorial, NodeTutorial);
                DisplayAndDrainNode(controller, manager, ref expectedLogCount, NodeTutorial);

                P10NarrativeSnapshot snapshot = manager.SaveSnapshot();
                if (snapshot == null || snapshot.DialogueLogEntries == null || snapshot.DialogueLogEntries.Count != expectedLogCount)
                {
                    throw new InvalidOperationException("Snapshot did not capture expected Dialogue Log entries.");
                }

                GameObject restoredRoot = new GameObject("P10D_RestoredSnapshotRoot");
                try
                {
                    P10NarrativeManager restoredManager = CreateManager(restoredRoot.transform, "RestoredSnapshotManager");
                    P10DialogueController restoredController = CreateDialogueController(restoredManager.transform, restoredManager);
                    InjectPhase10AssetDialogueData(restoredController);
                    restoredController.EnsureRuntimeSurfaceInstance();
                    restoredManager.LoadSnapshot(snapshot);
                    AssertLogCount(restoredController, expectedLogCount, "Restored Dialogue Log count mismatch.");
                    restoredController.SetCurrentNode(restoredManager.GetCurrentNode());
                    AssertLogCount(restoredController, expectedLogCount, "Snapshot current node refresh duplicated Dialogue Log entry.");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(restoredRoot);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyGeneratedEventSystemIfNeeded(existingEventSystem);
            }
        }

        private static void DisplayAndDrainNode(P10DialogueController controller, P10NarrativeManager manager, ref int expectedLogCount, string nodeId)
        {
            P10DialogueNodeSO node = ResolveNode(nodeId);
            DisplayLine(controller, manager, ref expectedLogCount, nodeId, 0);
            for (int i = 1; i < node.DialogueLines.Count; i++)
            {
                AdvanceLine(controller, manager, ref expectedLogCount, nodeId, i);
            }
        }

        private static void DisplayLine(P10DialogueController controller, P10NarrativeManager manager, ref int expectedLogCount, string nodeId, int lineIndex)
        {
            controller.SetCurrentNode(manager.GetCurrentNode());
            expectedLogCount++;
            AssertDisplayedLine(controller, expectedLogCount, nodeId, lineIndex);
        }

        private static void AdvanceLine(P10DialogueController controller, P10NarrativeManager manager, ref int expectedLogCount, string nodeId, int lineIndex)
        {
            P10NarrativeState beforeState = manager.GetCurrentState();
            controller.AdvanceDialogue();
            expectedLogCount++;
            AssertNode(manager, beforeState, nodeId, "Dialogue line advance changed node before all lines were consumed.");
            AssertDisplayedLine(controller, expectedLogCount, nodeId, lineIndex);
        }

        private static void AssertDisplayedLine(P10DialogueController controller, int expectedLogCount, string nodeId, int lineIndex)
        {
            P10DialogueLine line = ResolveNode(nodeId).DialogueLines[lineIndex];
            string expectedSpeaker = ResolveCharacter(line.SpeakerCharacterId).DisplayName;
            AssertLogCount(controller, expectedLogCount, "Dialogue Log count mismatch for " + nodeId + "#" + lineIndex + ".");
            P10DialogueLogEntry latest = controller.DialogueLogEntries[controller.DialogueLogEntries.Count - 1];
            if (latest.Sequence != expectedLogCount || latest.NodeId != nodeId || latest.SpeakerName != expectedSpeaker || latest.DialogueText != line.DialogueText)
            {
                throw new InvalidOperationException("Dialogue Log latest entry mismatch for " + nodeId + "#" + lineIndex + ".");
            }

        }

        private static void AssertRuntimeSurfaceLine(
            P10DialogueController controller,
            int expectedLogCount,
            P10DialogueNodeSO node,
            int lineIndex,
            ref string previousSpeakerName,
            ref int observedSpeakerChanges)
        {
            P10DialogueLine line = node.DialogueLines[lineIndex];
            string label = node.NodeId + "#" + lineIndex;
            P10CharacterDataSO character = ResolveCharacter(line.SpeakerCharacterId);
            string expectedSpeaker = character.DisplayName;
            string expectedDialogue = line.DialogueText;

            Transform surface = FindRuntimeDialogueSurface(controller, label);
            if (surface == null)
            {
                throw new InvalidOperationException("Runtime dialogue surface missing for " + label + ".");
            }

            CanvasGroup dialoguePanel = surface.Find("DialoguePanel")?.GetComponent<CanvasGroup>();
            if (dialoguePanel == null || dialoguePanel.alpha <= 0.5f)
            {
                throw new InvalidOperationException("Dialogue panel is not visible for " + label + ".");
            }

            Text speakerText = FindBottomSpeakerText(controller, label);
            Text bodyText = FindBottomBodyText(controller, label);
            if (speakerText == null || bodyText == null)
            {
                throw new InvalidOperationException("Runtime speaker/dialogue text components missing for " + label + ".");
            }

            AssertBottomDialogueSlotsMatch(speakerText, bodyText, expectedSpeaker, expectedDialogue, label);

            AssertLogCount(controller, expectedLogCount, "Dialogue Log count mismatch for " + label + ".");
            P10DialogueLogEntry latest = controller.DialogueLogEntries[controller.DialogueLogEntries.Count - 1];
            if (latest.Sequence != expectedLogCount ||
                latest.NodeId != node.NodeId ||
                latest.SpeakerName != expectedSpeaker ||
                latest.DialogueText != expectedDialogue)
            {
                throw new InvalidOperationException("Dialogue Log latest entry mismatch for speaker display line " + label + ".");
            }

            for (int i = 0; i < node.DialogueLines.Count; i++)
            {
                if (i == lineIndex)
                {
                    continue;
                }

                P10DialogueLine otherLine = node.DialogueLines[i];
                if (otherLine != null &&
                    !string.IsNullOrWhiteSpace(otherLine.DialogueText) &&
                    !string.Equals(otherLine.DialogueText, expectedDialogue, StringComparison.Ordinal) &&
                    bodyText.text.Contains(otherLine.DialogueText))
                {
                    throw new InvalidOperationException("Bottom dialogue box contains another line at " + label + ".");
                }
            }

            if (!string.IsNullOrWhiteSpace(previousSpeakerName) &&
                !string.Equals(previousSpeakerName, expectedSpeaker, StringComparison.Ordinal))
            {
                observedSpeakerChanges++;
            }

            previousSpeakerName = expectedSpeaker;
        }

        private static void AssertVisibleBottomDialogueSpeaker(
            P10DialogueController controller,
            int expectedLogCount,
            string nodeId,
            int lineIndex,
            string label)
        {
            P10DialogueNodeSO node = ResolveNode(nodeId);
            P10DialogueLine line = node.DialogueLines[lineIndex];
            P10CharacterDataSO character = ResolveCharacter(line.SpeakerCharacterId);
            string expectedSpeaker = character.DisplayName;

            Text speakerText = FindBottomSpeakerText(controller, label);
            Text bodyText = FindBottomBodyText(controller, label);
            AssertBottomDialogueSlotsMatch(speakerText, bodyText, expectedSpeaker, line.DialogueText, label);
            AssertLogCount(controller, expectedLogCount, "Dialogue Log count mismatch for " + label + ".");
        }

        private static void AssertCompleteVisibleBottomDialogueLineByDirectNode(
            P10DialogueController controller,
            ref int expectedLogCount,
            string nodeId,
            int lineIndex,
            string label)
        {
            controller.SetCurrentNode(nodeId);
            expectedLogCount++;
            AssertCompleteVisibleBottomDialogueLine(controller, expectedLogCount, nodeId, 0, label + " line 0 reset");

            for (int i = 1; i <= lineIndex; i++)
            {
                controller.AdvanceDialogue();
                expectedLogCount++;
            }

            if (lineIndex > 0)
            {
                AssertCompleteVisibleBottomDialogueLine(controller, expectedLogCount, nodeId, lineIndex, label);
            }
        }

        private static void AssertCompleteVisibleBottomDialogueLine(
            P10DialogueController controller,
            int expectedLogCount,
            string nodeId,
            int lineIndex,
            string label)
        {
            P10DialogueNodeSO node = ResolveNode(nodeId);
            P10DialogueLine line = node.DialogueLines[lineIndex];
            P10CharacterDataSO character = ResolveCharacter(line.SpeakerCharacterId);
            string expectedSpeaker = character.DisplayName;

            Text speakerText = FindBottomSpeakerText(controller, label);
            Text bodyText = FindBottomBodyText(controller, label);
            AssertBottomDialogueSlotsMatch(speakerText, bodyText, expectedSpeaker, line.DialogueText, label);

            if (bodyText.horizontalOverflow != HorizontalWrapMode.Wrap)
            {
                throw new InvalidOperationException("Visible bottom dialogue horizontal overflow is not Wrap at " + label + ".");
            }

            if (bodyText.verticalOverflow == VerticalWrapMode.Truncate)
            {
                throw new InvalidOperationException("Visible bottom dialogue vertical overflow is Truncate at " + label + ".");
            }

            AssertVisibleDialogueDoesNotContainOtherLines(bodyText.text, node, lineIndex, label);
            AssertLogCount(controller, expectedLogCount, "Dialogue Log count mismatch for " + label + ".");
        }

        private static Text FindBottomSpeakerText(P10DialogueController controller, string label)
        {
            return FindBottomDialogueSlot(controller, "P10_DialogueSpeakerText", label);
        }

        private static Text FindBottomBodyText(P10DialogueController controller, string label)
        {
            return FindBottomDialogueSlot(controller, "P10_DialogueBodyText", label);
        }

        private static void AssertBottomDialogueTextSlotsContained(P10DialogueController controller, string label)
        {
            Transform surface = FindRuntimeDialogueSurface(controller, label);
            RectTransform panelRect = surface.Find("DialoguePanel/Panel")?.GetComponent<RectTransform>();
            Text speakerText = FindBottomSpeakerText(controller, label + " speaker");
            Text bodyText = FindBottomBodyText(controller, label + " body");

            if (panelRect == null)
            {
                throw new InvalidOperationException(label + " missing DialoguePanel/Panel RectTransform.");
            }

            AssertRectParent(speakerText.rectTransform, panelRect.transform, label + " speaker");
            AssertRectParent(bodyText.rectTransform, panelRect.transform, label + " body");

            Canvas.ForceUpdateCanvases();
            Rect panelWorldRect = GetWorldRect(panelRect);
            AssertRectContains(panelWorldRect, GetWorldRect(speakerText.rectTransform), label + " speaker text must stay inside dialogue panel");
            AssertRectContains(panelWorldRect, GetWorldRect(bodyText.rectTransform), label + " body text must stay inside dialogue panel");
        }

        private static void AssertRectParent(RectTransform rectTransform, Transform expectedParent, string label)
        {
            if (rectTransform == null)
            {
                throw new InvalidOperationException(label + " RectTransform missing.");
            }

            if (rectTransform.parent != expectedParent)
            {
                throw new InvalidOperationException(label + " must be parented to DialoguePanel/Panel. Actual parent: " + GetHierarchyPath(rectTransform.parent) + ".");
            }
        }

        private static Text FindBottomDialogueSlot(P10DialogueController controller, string objectName, string label)
        {
            Transform surface = FindRuntimeDialogueSurface(controller, label);
            if (!surface.gameObject.activeInHierarchy)
            {
                throw new InvalidOperationException("Runtime dialogue surface is inactive for " + label + ".");
            }

            Text fixedSlot = objectName == "P10_DialogueSpeakerText"
                ? controller.FixedSpeakerText
                : controller.FixedBodyText;
            if (fixedSlot != null && fixedSlot.name == objectName)
            {
                return fixedSlot;
            }

            Transform manualSlots = FindDescendantByName(controller.transform, EditorVisibleManualSlotRootName);
            Transform slotTransform = manualSlots != null ? manualSlots.Find(objectName) : null;
            if (slotTransform == null)
            {
                slotTransform = surface.Find("DialoguePanel/Panel/" + objectName);
            }

            if (slotTransform == null)
            {
                throw new InvalidOperationException("Bottom dialogue fixed text slot missing for " + label + ": " + objectName + ".");
            }

            return slotTransform.GetComponent<Text>();
        }

        private static Transform FindRuntimeDialogueSurface(P10DialogueController controller, string label)
        {
            if (controller == null)
            {
                throw new InvalidOperationException("Dialogue controller missing for " + label + ".");
            }

            Transform direct = controller.transform.Find("P10_Runtime_DialogueSurface");
            if (direct != null)
            {
                return direct;
            }

            Transform recursive = FindDescendantByName(controller.transform, "P10_Runtime_DialogueSurface");
            if (recursive == null)
            {
                throw new InvalidOperationException("Runtime dialogue surface missing for " + label + ".");
            }

            return recursive;
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

        private static void AssertVisibleTextComponent(Text text, string label)
        {
            if (text == null)
            {
                throw new InvalidOperationException("Visible Text component missing for " + label + ".");
            }

            if (!text.gameObject.activeInHierarchy)
            {
                throw new InvalidOperationException("Visible Text GameObject is inactive for " + label + ".");
            }

            if (text.color.a <= 0f)
            {
                throw new InvalidOperationException("Visible Text alpha is zero for " + label + ".");
            }

            RectTransform rectTransform = text.rectTransform;
            bool hasPreferredSize = rectTransform != null &&
                (LayoutUtility.GetPreferredWidth(rectTransform) > 0f ||
                LayoutUtility.GetPreferredHeight(rectTransform) > 0f);
            if (rectTransform == null || !hasPreferredSize && (rectTransform.rect.width <= 0f || rectTransform.rect.height <= 0f))
            {
                throw new InvalidOperationException("Visible Text RectTransform has no size for " + label + ".");
            }
        }

        private static void AssertBottomDialogueSlotsMatch(Text speakerText, Text bodyText, string expectedSpeaker, string dialogueText, string label)
        {
            AssertVisibleTextComponent(speakerText, label + " speaker slot");
            AssertVisibleTextComponent(bodyText, label + " body slot");

            if (speakerText.name != "P10_DialogueSpeakerText")
            {
                throw new InvalidOperationException("Bottom speaker slot uses unstable object name at " + label + ": " + speakerText.name + ".");
            }

            if (bodyText.name != "P10_DialogueBodyText")
            {
                throw new InvalidOperationException("Bottom body slot uses unstable object name at " + label + ": " + bodyText.name + ".");
            }

            if (!string.Equals(speakerText.text, expectedSpeaker, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Bottom dialogue speaker slot mismatch at " + label +
                    ". Expected: " + expectedSpeaker + ", actual: " + speakerText.text + ".");
            }

            string expectedBody = NormalizeExpectedBodyText(expectedSpeaker, dialogueText);
            if (!string.Equals(bodyText.text, expectedBody, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Bottom dialogue body slot mismatch at " + label +
                    ". Expected: " + expectedBody + ", actual: " + bodyText.text + ".");
            }

            if (bodyText.text.Contains(expectedSpeaker + "\uff1a") || bodyText.text.Contains(expectedSpeaker + ":"))
            {
                throw new InvalidOperationException("Bottom body text still contains current speaker prefix at " + label + ".");
            }

            AssertBodyTextHasNoKnownSpeakerPrefix(bodyText.text, label);
        }

        private static void AssertSameFixedTextSlots(P10DialogueController controller, Text expectedSpeakerSlot, Text expectedBodySlot, string label)
        {
            Text actualSpeakerSlot = FindBottomSpeakerText(controller, label);
            Text actualBodySlot = FindBottomBodyText(controller, label);
            if (!object.ReferenceEquals(actualSpeakerSlot, expectedSpeakerSlot))
            {
                throw new InvalidOperationException("Bottom speaker text slot was recreated instead of reused at " + label + ".");
            }

            if (!object.ReferenceEquals(actualBodySlot, expectedBodySlot))
            {
                throw new InvalidOperationException("Bottom body text slot was recreated instead of reused at " + label + ".");
            }
        }

        private static void AssertFixedSlotVisibleText(
            P10DialogueController controller,
            int expectedLogCount,
            string nodeId,
            int lineIndex,
            string label)
        {
            P10DialogueNodeSO node = ResolveNode(nodeId);
            P10DialogueLine line = node.DialogueLines[lineIndex];
            P10CharacterDataSO character = ResolveCharacter(line.SpeakerCharacterId);
            Text speakerText = FindBottomSpeakerText(controller, label);
            Text bodyText = FindBottomBodyText(controller, label);
            AssertBottomDialogueSlotsMatch(speakerText, bodyText, character.DisplayName, line.DialogueText, label);
            AssertVisibleDialogueDoesNotContainOtherLines(bodyText.text, node, lineIndex, label);
            AssertNoBareTechnicalTerm(bodyText.text, label + " body text");
            AssertLogCount(controller, expectedLogCount, "Dialogue Log count mismatch for " + label + ".");
        }

        private static void AssertFixedSlotTextEquals(P10DialogueController controller, string expectedSpeaker, string expectedBody, string label)
        {
            Text speakerText = FindBottomSpeakerText(controller, label);
            Text bodyText = FindBottomBodyText(controller, label);
            if (!string.Equals(speakerText.text, expectedSpeaker, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("R8 fixed speaker slot exact mismatch at " + label + ".");
            }

            if (!string.Equals(bodyText.text, expectedBody, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("R8 fixed body slot exact mismatch at " + label + ". Expected: " + expectedBody + ", actual: " + bodyText.text + ".");
            }
        }

        private static void AssertFixedSlotBodyDoesNotContainPrefix(P10DialogueController controller, string speakerName, string label)
        {
            Text bodyText = FindBottomBodyText(controller, label);
            if (bodyText.text.Contains(speakerName + "\uff1a") || bodyText.text.Contains(speakerName + ":"))
            {
                throw new InvalidOperationException("R8 fixed body slot contains speaker prefix at " + label + ".");
            }
        }

        private static void AssertDialogueLogLatestEntriesRemainStructured(P10DialogueController controller)
        {
            if (controller.DialogueLogEntries.Count == 0)
            {
                throw new InvalidOperationException("Dialogue Log should contain entries for R8 fixed slot validation.");
            }

            for (int i = 0; i < controller.DialogueLogEntries.Count; i++)
            {
                P10DialogueLogEntry entry = controller.DialogueLogEntries[i];
                if (string.IsNullOrWhiteSpace(entry.SpeakerName))
                {
                    throw new InvalidOperationException("R8 Dialogue Log entry lost structured speaker.");
                }

                if (string.IsNullOrWhiteSpace(entry.DialogueText))
                {
                    throw new InvalidOperationException("R8 Dialogue Log entry lost structured body text.");
                }

                if (entry.DialogueText.StartsWith(entry.SpeakerName + "\uff1a", StringComparison.Ordinal) ||
                    entry.DialogueText.StartsWith(entry.SpeakerName + ":", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("R8 Dialogue Log entry text redundantly includes speaker prefix.");
                }
            }
        }

        private static void AssertAllDialogueAssetsUsePureBodyText()
        {
            foreach (P10DialogueNodeSO node in LoadDialogueNodes())
            {
                if (node == null)
                {
                    continue;
                }

                AssertBodyTextHasNoKnownSpeakerPrefix(node.DialogueText, node.NodeId + " DialogueText");
                if (node.DialogueLines == null)
                {
                    continue;
                }

                for (int i = 0; i < node.DialogueLines.Count; i++)
                {
                    P10DialogueLine line = node.DialogueLines[i];
                    if (line != null)
                    {
                        AssertBodyTextHasNoKnownSpeakerPrefix(line.DialogueText, node.NodeId + "#" + i + " DialogueText");
                    }
                }
            }
        }

        private static void AssertReadableLogText(Text text, string label)
        {
            AssertVisibleTextComponent(text, label);
            if (string.IsNullOrWhiteSpace(text.text))
            {
                throw new InvalidOperationException("Dialogue Log text is empty for " + label + ".");
            }

            if (text.font == null)
            {
                throw new InvalidOperationException("Dialogue Log text font is missing for " + label + ".");
            }

            if (text.horizontalOverflow != HorizontalWrapMode.Wrap)
            {
                throw new InvalidOperationException("Dialogue Log text must wrap long Chinese lines for " + label + ".");
            }

            if (text.verticalOverflow == VerticalWrapMode.Truncate)
            {
                throw new InvalidOperationException("Dialogue Log text must not truncate vertically for " + label + ".");
            }
        }

        private static RectTransform RequireRect(Transform target, string label)
        {
            RectTransform rectTransform = target != null ? target.GetComponent<RectTransform>() : null;
            if (rectTransform == null)
            {
                throw new InvalidOperationException("Missing RectTransform for " + label + ".");
            }

            if (rectTransform.rect.width <= 0f || rectTransform.rect.height <= 0f)
            {
                throw new InvalidOperationException("RectTransform has no readable size for " + label + ".");
            }

            return rectTransform;
        }

        private static void RequirePointerClickTarget(Transform target, string label)
        {
            if (target == null)
            {
                throw new InvalidOperationException(label + " missing.");
            }

            if (!target.gameObject.activeInHierarchy)
            {
                throw new InvalidOperationException(label + " is inactive.");
            }

            RectTransform rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null || rectTransform.rect.width <= 0f || rectTransform.rect.height <= 0f)
            {
                throw new InvalidOperationException(label + " has no clickable RectTransform size.");
            }

            if (!HasPointerClickHandler(target.gameObject))
            {
                throw new InvalidOperationException(label + " has no IPointerClickHandler.");
            }

            Graphic graphic = target.GetComponent<Graphic>();
            if (graphic == null || !graphic.raycastTarget)
            {
                throw new InvalidOperationException(label + " does not expose a raycast target.");
            }
        }

        private static bool HasPointerClickHandler(GameObject target)
        {
            if (target == null)
            {
                return false;
            }

            Component[] components = target.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] is IPointerClickHandler)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ClickUi(Transform target, string label, bool requireHandler)
        {
            if (target == null)
            {
                throw new InvalidOperationException(label + " click target missing.");
            }

            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                eventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            }

            if (eventSystem == null)
            {
                throw new InvalidOperationException(label + " cannot click without an EventSystem.");
            }

            PointerEventData eventData = new PointerEventData(eventSystem)
            {
                button = PointerEventData.InputButton.Left,
                pointerId = -1,
                position = Vector2.zero
            };

            bool handled = ExecuteEvents.Execute(target.gameObject, eventData, ExecuteEvents.pointerClickHandler);
            if (requireHandler && !handled)
            {
                throw new InvalidOperationException(label + " did not handle pointer click.");
            }
        }

        private static void AssertOverlayBlocksRaycasts(Transform panel, string label)
        {
            if (panel == null)
            {
                throw new InvalidOperationException(label + " missing.");
            }

            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            if (group == null || group.alpha <= 0.5f || !group.interactable || !group.blocksRaycasts)
            {
                throw new InvalidOperationException(label + " is not configured to block raycasts while visible.");
            }

            Graphic graphic = panel.GetComponent<Graphic>();
            if (graphic == null || !graphic.raycastTarget || graphic.color.a <= 0f)
            {
                throw new InvalidOperationException(label + " has no visible raycast-blocking Graphic.");
            }
        }

        private static ScrollRect RequireScrollRect(Transform scrollTransform, string label)
        {
            ScrollRect scrollRect = scrollTransform != null ? scrollTransform.GetComponent<ScrollRect>() : null;
            if (scrollRect == null)
            {
                throw new InvalidOperationException(label + " missing ScrollRect.");
            }

            if (scrollRect.viewport == null || scrollRect.content == null || !scrollRect.vertical || scrollRect.horizontal)
            {
                throw new InvalidOperationException(label + " ScrollRect binding or direction is invalid.");
            }

            if (scrollRect.scrollSensitivity <= 0f)
            {
                throw new InvalidOperationException(label + " mouse wheel scroll sensitivity must be enabled.");
            }

            return scrollRect;
        }

        private static Scrollbar RequireScrollbar(ScrollRect scrollRect, string label)
        {
            Scrollbar scrollbar = scrollRect != null ? scrollRect.verticalScrollbar : null;
            if (scrollbar == null)
            {
                throw new InvalidOperationException(label + " missing vertical Scrollbar.");
            }

            if (scrollbar.handleRect == null || scrollbar.direction != Scrollbar.Direction.BottomToTop)
            {
                throw new InvalidOperationException(label + " Scrollbar handle or direction is invalid.");
            }

            Graphic trackGraphic = scrollbar.GetComponent<Graphic>();
            Graphic handleGraphic = scrollbar.handleRect.GetComponent<Graphic>();
            if (trackGraphic == null || !trackGraphic.raycastTarget || handleGraphic == null || !handleGraphic.raycastTarget)
            {
                throw new InvalidOperationException(label + " Scrollbar track/handle must reserve raycastable art slots.");
            }

            return scrollbar;
        }

        private static void AssertScrollContentExceedsViewport(ScrollRect scrollRect, string label)
        {
            Canvas.ForceUpdateCanvases();
            float viewportHeight = scrollRect.viewport.rect.height;
            float contentHeight = LayoutUtility.GetPreferredHeight(scrollRect.content);
            if (contentHeight <= viewportHeight + 8f)
            {
                contentHeight = scrollRect.content.rect.height;
            }

            if (contentHeight <= viewportHeight + 8f)
            {
                throw new InvalidOperationException(label + " content does not exceed viewport. Content: " + contentHeight + ", viewport: " + viewportHeight + ".");
            }
        }

        private static void AssertScrollbarHandleInsideTrack(Scrollbar scrollbar, string label)
        {
            RectTransform track = scrollbar.GetComponent<RectTransform>();
            RectTransform handle = scrollbar.handleRect;
            Rect trackRect = GetWorldRect(track);
            Rect handleRect = GetWorldRect(handle);
            if (handleRect.xMin < trackRect.xMin - 0.1f ||
                handleRect.xMax > trackRect.xMax + 0.1f ||
                handleRect.yMin < trackRect.yMin - 0.1f ||
                handleRect.yMax > trackRect.yMax + 0.1f)
            {
                throw new InvalidOperationException(label + " Scrollbar handle exceeds track bounds.");
            }
        }

        private static void AssertScrollbarDragChangesPosition(ScrollRect scrollRect, Scrollbar scrollbar, string label)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
            Canvas.ForceUpdateCanvases();
            float before = scrollRect.verticalNormalizedPosition;
            scrollbar.value = 0f;
            scrollbar.onValueChanged.Invoke(0f);
            Canvas.ForceUpdateCanvases();
            float after = scrollRect.verticalNormalizedPosition;
            if (Mathf.Abs(before - after) < 0.2f)
            {
                throw new InvalidOperationException(label + " Scrollbar drag simulation did not change verticalNormalizedPosition. Before: " + before + ", after: " + after + ".");
            }
        }

        private static void AppendSyntheticDialogueLogEntries(P10DialogueController controller, int count)
        {
            MethodInfo record = typeof(P10DialogueController).GetMethod("RecordDialogueLogEntry", BindingFlags.Instance | BindingFlags.NonPublic);
            if (record == null)
            {
                throw new InvalidOperationException("P10D-24T could not access RecordDialogueLogEntry for validator fixture setup.");
            }

            for (int i = 0; i < count; i++)
            {
                record.Invoke(controller, new object[]
                {
                    "P10D24T_SYNTHETIC_LOG_" + i,
                    "旁白",
                    "滚动验证日志第 " + i + " 行：这是一段用于撑开对话记录面板内容高度的中文文本。"
                });
            }
        }

        private static void ForceLongCurrentOrderContent(Transform surface)
        {
            Text content = FindDescendantByName(surface, "CurrentOrderContentText")?.GetComponent<Text>();
            if (content == null)
            {
                throw new InvalidOperationException("P10D-24T CurrentOrderContentText missing.");
            }

            content.text =
                content.text + "\n\n" +
                "滚动验证补充说明：\n" +
                "1. 这段文本只在 Editor validator 临时对象中用于撑开内容。\n" +
                "2. 拖动滑块应移动当前订单内容。\n" +
                "3. 点击滚动区域不应推进底层对白。\n" +
                "4. 鼠标滚轮由 ScrollRect scrollSensitivity 保持启用。\n" +
                "5. 美术轨道和滑块对象保留为可替换 Image。\n" +
                "6. 第一段补充文本用于模拟较长的工艺说明。\n" +
                "7. 第二段补充文本用于模拟烧成温度注意事项。\n" +
                "8. 第三段补充文本用于模拟评分要求说明。\n" +
                "9. 第四段补充文本用于模拟奖励和状态说明。\n" +
                "10. 第五段补充文本用于确认 ScrollRect 内容高度超过 viewport。\n" +
                "11. 第六段补充文本用于确认拖动 handle 后 position 会改变。\n" +
                "12. 第七段补充文本用于确认点击 scrollbar 不推进对白。\n" +
                "13. 第八段补充文本用于确认点击 scroll view 不推进对白。\n" +
                "14. 第九段补充文本用于确认 TopRightActionBar 仍按互斥规则隐藏。\n" +
                "15. 第十段补充文本用于确认订单面板关闭后状态恢复。";
        }

        private static void AssertPersistentButtonHidden(Transform button, string label)
        {
            if (button == null)
            {
                throw new InvalidOperationException(label + " missing.");
            }

            if (button.gameObject.activeInHierarchy)
            {
                throw new InvalidOperationException(label + " should be hidden while overlay panel is open.");
            }
        }

        private static void AssertPersistentButtonVisible(Transform button, string label)
        {
            if (button == null)
            {
                throw new InvalidOperationException(label + " missing.");
            }

            if (!button.gameObject.activeInHierarchy)
            {
                throw new InvalidOperationException(label + " should be visible after overlay panel closes.");
            }
        }

        private static void AssertActionBarHidden(Transform actionBar, string label)
        {
            if (actionBar == null)
            {
                throw new InvalidOperationException(label + " missing.");
            }

            if (actionBar.gameObject.activeInHierarchy)
            {
                throw new InvalidOperationException(label + " should be hidden while overlay panel is open.");
            }
        }

        private static void AssertActionBarVisible(Transform actionBar, string label)
        {
            if (actionBar == null)
            {
                throw new InvalidOperationException(label + " missing.");
            }

            if (!actionBar.gameObject.activeInHierarchy)
            {
                throw new InvalidOperationException(label + " should be visible.");
            }
        }

        private static void AssertTopRightButtonsDoNotOverlap(Transform actionBar, string label)
        {
            if (actionBar == null)
            {
                throw new InvalidOperationException(label + " missing.");
            }

            RectTransform logRect = FindDescendantByName(actionBar, "PersistentLogButton")?.GetComponent<RectTransform>();
            RectTransform orderRect = FindDescendantByName(actionBar, "PersistentOrderButton")?.GetComponent<RectTransform>();
            if (logRect == null || orderRect == null)
            {
                throw new InvalidOperationException(label + " missing Log / Order RectTransform.");
            }

            if (!logRect.gameObject.activeSelf || !orderRect.gameObject.activeSelf)
            {
                throw new InvalidOperationException(label + " Log / Order buttons must be active in normal state.");
            }

            Canvas.ForceUpdateCanvases();
            Rect logWorldRect = GetWorldRect(logRect);
            Rect orderWorldRect = GetWorldRect(orderRect);
            if (RectOverlaps(logWorldRect, orderWorldRect))
            {
                throw new InvalidOperationException(label + " Log / Order buttons overlap. Log: " + logWorldRect + ", Order: " + orderWorldRect + ".");
            }

            AssertClickableImageButton(logRect, label + " Log button");
            AssertClickableImageButton(orderRect, label + " Order button");
        }

        private static void AssertClickableImageButton(RectTransform buttonRect, string label)
        {
            Button button = buttonRect != null ? buttonRect.GetComponent<Button>() : null;
            Image image = buttonRect != null ? buttonRect.GetComponent<Image>() : null;
            if (button == null || image == null)
            {
                throw new InvalidOperationException(label + " must keep Button and Image components.");
            }

            if (!button.interactable || !image.raycastTarget)
            {
                throw new InvalidOperationException(label + " must remain clickable.");
            }

            if (buttonRect.rect.width < 40f || buttonRect.rect.height < 32f)
            {
                throw new InvalidOperationException(label + " clickable rect is too small: " + buttonRect.rect + ".");
            }
        }

        private static bool RectOverlaps(Rect a, Rect b)
        {
            return a.xMin < b.xMax && a.xMax > b.xMin && a.yMin < b.yMax && a.yMax > b.yMin;
        }

        private static void AssertRectContains(Rect outer, Rect inner, string label)
        {
            const float tolerance = 1.5f;
            if (inner.xMin < outer.xMin - tolerance ||
                inner.xMax > outer.xMax + tolerance ||
                inner.yMin < outer.yMin - tolerance ||
                inner.yMax > outer.yMax + tolerance)
            {
                throw new InvalidOperationException(label + ". Outer: " + outer + ", Inner: " + inner + ".");
            }
        }

        private static void AssertDialoguePanelHidden(Transform surface, string label)
        {
            CanvasGroup group = surface != null ? surface.Find("DialoguePanel")?.GetComponent<CanvasGroup>() : null;
            if (group == null)
            {
                throw new InvalidOperationException(label + " DialoguePanel CanvasGroup missing.");
            }

            if (group.alpha > 0.5f || group.interactable || group.blocksRaycasts)
            {
                throw new InvalidOperationException(label + " DialoguePanel did not hide or still blocks input.");
            }
        }

        private static Rect GetWorldRect(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            float minX = corners[0].x;
            float minY = corners[0].y;
            float maxX = corners[0].x;
            float maxY = corners[0].y;
            for (int i = 1; i < corners.Length; i++)
            {
                minX = Mathf.Min(minX, corners[i].x);
                minY = Mathf.Min(minY, corners[i].y);
                maxX = Mathf.Max(maxX, corners[i].x);
                maxY = Mathf.Max(maxY, corners[i].y);
            }

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        private static void AssertNoBareTechnicalTerm(string text, string label)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            string[] forbiddenTerms =
            {
                "ShapeScore",
                "GlazeScore",
                "FireScore",
                "ResultCalculator",
                "GLAZE_",
                "CODEX_",
                "SHAPE_",
                "ORDER_",
                "PerfectScore",
                "Difficulty",
                "Reward",
                "Reputation"
            };

            for (int i = 0; i < forbiddenTerms.Length; i++)
            {
                if (text.Contains(forbiddenTerms[i]))
                {
                    throw new InvalidOperationException("Player-facing text exposes technical term " + forbiddenTerms[i] + " at " + label + ": " + text);
                }
            }
        }

        private static string LocalizeExpectedPlayerFacingText(string value)
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

        private static string NormalizeExpectedBodyText(string speakerName, string dialogueText)
        {
            if (string.IsNullOrWhiteSpace(dialogueText))
            {
                return string.Empty;
            }

            string localized = LocalizeExpectedPlayerFacingText(dialogueText);
            return StripExpectedSpeakerPrefix(localized, speakerName);
        }

        private static string StripExpectedSpeakerPrefix(string value, string currentSpeakerName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            List<string> knownSpeakerNames = new List<string>
            {
                currentSpeakerName,
                "旁白",
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

                string fullWidthPrefix = speakerName + "\uff1a";
                if (trimmed.StartsWith(fullWidthPrefix, StringComparison.Ordinal))
                {
                    return trimmed.Substring(fullWidthPrefix.Length).TrimStart();
                }

                string halfWidthPrefix = speakerName + ":";
                if (trimmed.StartsWith(halfWidthPrefix, StringComparison.Ordinal))
                {
                    return trimmed.Substring(halfWidthPrefix.Length).TrimStart();
                }
            }

            return value;
        }

        private static void AssertBodyTextHasNoKnownSpeakerPrefix(string text, string label)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            string[] knownSpeakerNames =
            {
                "旁白",
                "徐老伯",
                "周掌柜",
                "陈书院",
                "卢客",
                "系统提示",
                "UI结果",
                "玩家"
            };

            for (int i = 0; i < knownSpeakerNames.Length; i++)
            {
                string speakerName = knownSpeakerNames[i];
                if (text.StartsWith(speakerName + "\uff1a", StringComparison.Ordinal) ||
                    text.StartsWith(speakerName + ":", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Body text still contains speaker prefix at " + label + ": " + text);
                }
            }
        }

        private static void EnsureEditorVisibleDialogueUiPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(EditorVisibleDialogueUiPrefabPath);
            if (root == null)
            {
                root = new GameObject("P10_CH01_DialogueUIRoot");
            }

            try
            {
                root.name = "P10_CH01_DialogueUIRoot";
                root.transform.localPosition = Vector3.zero;
                root.transform.localRotation = Quaternion.identity;
                root.transform.localScale = Vector3.one;

                P10DialogueController controller = root.GetComponent<P10DialogueController>();
                if (controller == null)
                {
                    controller = root.AddComponent<P10DialogueController>();
                }

                RemoveMissingScriptsRecursively(root);

                GameObject surface = EnsureChild(root.transform, "P10_Runtime_DialogueSurface", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                RectTransform surfaceRect = surface.GetComponent<RectTransform>();
                surfaceRect.anchorMin = Vector2.zero;
                surfaceRect.anchorMax = Vector2.one;
                surfaceRect.offsetMin = Vector2.zero;
                surfaceRect.offsetMax = Vector2.zero;
                surfaceRect.sizeDelta = new Vector2(1280f, 720f);
                surfaceRect.anchoredPosition = Vector2.zero;
                surfaceRect.localScale = Vector3.one;

                Canvas canvas = surface.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = surface.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280f, 720f);

                GameObject dialoguePanel = EnsureChild(surface.transform, "DialoguePanel", typeof(RectTransform), typeof(CanvasGroup));
                RectTransform dialoguePanelRect = dialoguePanel.GetComponent<RectTransform>();
                dialoguePanelRect.anchorMin = new Vector2(0.02f, 0.02f);
                dialoguePanelRect.anchorMax = new Vector2(0.98f, 0.36f);
                dialoguePanelRect.offsetMin = Vector2.zero;
                dialoguePanelRect.offsetMax = Vector2.zero;

                GameObject panel = EnsureChild(dialoguePanel.transform, "Panel", typeof(RectTransform), typeof(Image));
                RectTransform panelRect = panel.GetComponent<RectTransform>();
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = new Vector2(12f, 12f);
                panelRect.offsetMax = new Vector2(-12f, -12f);
                panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);

                RemoveDirectChildIfExists(panel.transform, "P10_DialogueSpeakerText");
                RemoveDirectChildIfExists(panel.transform, "P10_DialogueBodyText");
                RemoveDirectChildIfExists(panel.transform, "NextButton");

                GameObject manualSlots = EnsureChild(root.transform, EditorVisibleManualSlotRootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                RectTransform manualSlotsRect = manualSlots.GetComponent<RectTransform>();
                manualSlotsRect.anchorMin = Vector2.zero;
                manualSlotsRect.anchorMax = Vector2.one;
                manualSlotsRect.offsetMin = Vector2.zero;
                manualSlotsRect.offsetMax = Vector2.zero;
                manualSlotsRect.sizeDelta = new Vector2(1280f, 720f);
                manualSlotsRect.anchoredPosition = Vector2.zero;
                manualSlotsRect.localScale = Vector3.one;

                Canvas manualSlotsCanvas = manualSlots.GetComponent<Canvas>();
                manualSlotsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                manualSlotsCanvas.overrideSorting = true;
                manualSlotsCanvas.sortingOrder = 20;

                CanvasScaler manualSlotsScaler = manualSlots.GetComponent<CanvasScaler>();
                manualSlotsScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                manualSlotsScaler.referenceResolution = new Vector2(1280f, 720f);

                Text speaker = EnsureText(manualSlots.transform, "P10_DialogueSpeakerText", 22, FontStyle.Bold, TextAnchor.UpperLeft);
                RectTransform speakerRect = speaker.rectTransform;
                speakerRect.anchorMin = new Vector2(0.035f, 0.285f);
                speakerRect.anchorMax = new Vector2(0.965f, 0.36f);
                speakerRect.offsetMin = Vector2.zero;
                speakerRect.offsetMax = Vector2.zero;
                speaker.text = "旁白";

                Text body = EnsureText(manualSlots.transform, "P10_DialogueBodyText", 18, FontStyle.Normal, TextAnchor.UpperLeft);
                RectTransform bodyRect = body.rectTransform;
                bodyRect.anchorMin = new Vector2(0.035f, 0.105f);
                bodyRect.anchorMax = new Vector2(0.965f, 0.285f);
                bodyRect.offsetMin = Vector2.zero;
                bodyRect.offsetMax = Vector2.zero;
                body.text = "父亲走的那年，窑炉最后一次点火。";

                EnsureButton(panel.transform, "LogButton", "记录", new Vector2(0.72f, 0.04f), new Vector2(0.84f, 0.22f));
                EnsureButton(panel.transform, "CloseButton", "关闭", new Vector2(0.86f, 0.04f), new Vector2(0.98f, 0.22f));
                AssertDialoguePanelActionButtonsDoNotOverlap(panel.transform);
                Transform actionBar = EnsureTopRightActionBar(surface.transform);
                EnsureActionBarButton(actionBar, "PersistentLogButton", "记录");
                EnsureActionBarButton(actionBar, "PersistentOrderButton", "订单");
                AssertTopRightButtonsDoNotOverlap(actionBar, "R9B prefab action bar");
                EnsureDialogueLogPanel(surface.transform);
                EnsureCurrentOrderPanel(surface.transform);

                SerializedObject serializedController = new SerializedObject(controller);
                serializedController.FindProperty("speakerText").objectReferenceValue = speaker;
                serializedController.FindProperty("bodyText").objectReferenceValue = body;
                serializedController.ApplyModifiedPropertiesWithoutUndo();

                RemoveMissingScriptsRecursively(root);
                AssertNoMissingScriptsRecursive(root, "R9B prefab contents before save");
                AssertManualSlotRootIsShallow(root);
                AssertUniqueSlotNameInPrefab(root, "P10_DialogueSpeakerText");
                AssertUniqueSlotNameInPrefab(root, "P10_DialogueBodyText");

                bool saveSucceeded;
                PrefabUtility.SaveAsPrefabAsset(root, EditorVisibleDialogueUiPrefabPath, out saveSucceeded);
                if (!saveSucceeded)
                {
                    throw new InvalidOperationException("R9B failed to save editor-visible dialogue UI prefab.");
                }

                AssetDatabase.ImportAsset(EditorVisibleDialogueUiPrefabPath);
                GameObject savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EditorVisibleDialogueUiPrefabPath);
                if (savedPrefab == null)
                {
                    throw new InvalidOperationException("R9B saved prefab could not be reloaded: " + EditorVisibleDialogueUiPrefabPath + ".");
                }

                AssertNoMissingScriptsRecursive(savedPrefab, "R9B saved prefab asset");
                AssertManualSlotRootIsShallow(savedPrefab);
                AssertUniqueSlotNameInPrefab(savedPrefab, "P10_DialogueSpeakerText");
                AssertUniqueSlotNameInPrefab(savedPrefab, "P10_DialogueBodyText");
                AssertPrefabYamlHasClearlyVisibleSlots();
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void EnsureOverlaySceneUsesEditorVisibleDialogueUiPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EditorVisibleDialogueUiPrefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException("Cannot update overlay scene because R9 dialogue UI prefab is missing.");
            }

            Scene scene = EditorSceneManager.OpenScene(EditorVisibleNarrativeOverlayScenePath, OpenSceneMode.Single);
            GameObject narrativeRoot = GameObject.Find("P10_CH01_NarrativeRoot");
            if (narrativeRoot == null)
            {
                narrativeRoot = new GameObject("P10_CH01_NarrativeRoot");
            }

            GameObject existingUiRoot = GameObject.Find("P10_CH01_DialogueUIRoot");
            if (existingUiRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(existingUiRoot);
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.name = "P10_CH01_DialogueUIRoot";
            instance.transform.SetParent(narrativeRoot.transform, false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.ImportAsset(EditorVisibleNarrativeOverlayScenePath);
        }

        private static void EnsureDialogueLogPanel(Transform surface)
        {
            GameObject panel = EnsureChild(surface, "DialogueLogPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.06f, 0.18f);
            panelRect.anchorMax = new Vector2(0.94f, 0.96f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.03f, 0.03f, 0.03f, 0.94f);

            Text title = EnsureText(panel.transform, "DialogueLogTitle", 24, FontStyle.Bold, TextAnchor.UpperLeft);
            title.text = "对话记录";
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 0.88f);
            titleRect.anchorMax = new Vector2(0.76f, 1f);
            titleRect.offsetMin = new Vector2(28f, 0f);
            titleRect.offsetMax = new Vector2(-8f, -12f);

            EnsureButton(panel.transform, "DialogueLogCloseButton", "关闭", new Vector2(0.82f, 0.91f), new Vector2(0.96f, 0.98f));

            GameObject scroll = EnsureChild(panel.transform, "DialogueLogScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            RectTransform scrollRect = scroll.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.04f, 0.07f);
            scrollRect.anchorMax = new Vector2(0.96f, 0.84f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            scroll.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.06f);

            GameObject viewport = EnsureChild(scroll.transform, "Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(18f, 18f);
            viewportRect.offsetMax = new Vector2(-18f, -18f);
            viewport.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = EnsureChild(viewport.transform, "Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 16f;
            layout.padding = new RectOffset(12, 12, 10, 22);

            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollComponent = scroll.GetComponent<ScrollRect>();
            Scrollbar scrollbar = EnsureVerticalScrollbar(scroll.transform, "DialogueLogScrollbar");
            scrollComponent.viewport = viewportRect;
            scrollComponent.content = contentRect;
            scrollComponent.horizontal = false;
            scrollComponent.vertical = true;
            scrollComponent.verticalScrollbar = scrollbar;
            scrollComponent.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollComponent.verticalScrollbarSpacing = 8f;
            scrollComponent.scrollSensitivity = 28f;
            scrollComponent.movementType = ScrollRect.MovementType.Clamped;
        }

        private static void EnsureCurrentOrderPanel(Transform surface)
        {
            GameObject panel = EnsureChild(surface, "CurrentOrderPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.60f, 0.30f);
            panelRect.anchorMax = new Vector2(0.96f, 0.88f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.03f, 0.03f, 0.03f, 0.94f);

            Text title = EnsureText(panel.transform, "CurrentOrderTitleText", 24, FontStyle.Bold, TextAnchor.UpperLeft);
            title.text = "当前订单";
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 0.88f);
            titleRect.anchorMax = new Vector2(0.70f, 1f);
            titleRect.offsetMin = new Vector2(24f, 0f);
            titleRect.offsetMax = new Vector2(-8f, -12f);

            EnsureButton(panel.transform, "CurrentOrderCloseButton", "关闭", new Vector2(0.76f, 0.91f), new Vector2(0.96f, 0.98f));

            GameObject scroll = EnsureChild(panel.transform, "CurrentOrderScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            RectTransform scrollRect = scroll.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.04f, 0.06f);
            scrollRect.anchorMax = new Vector2(0.96f, 0.86f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            scroll.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.04f);

            GameObject viewport = EnsureChild(scroll.transform, "Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(18f, 16f);
            viewportRect.offsetMax = new Vector2(-34f, -16f);
            viewport.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject contentRoot = EnsureChild(viewport.transform, "Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            RectTransform contentRootRect = contentRoot.GetComponent<RectTransform>();
            contentRootRect.anchorMin = new Vector2(0f, 1f);
            contentRootRect.anchorMax = new Vector2(1f, 1f);
            contentRootRect.pivot = new Vector2(0.5f, 1f);
            contentRootRect.offsetMin = Vector2.zero;
            contentRootRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = contentRoot.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 0f;
            layout.padding = new RectOffset(6, 10, 6, 14);

            contentRoot.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Transform oldContent = panel.transform.Find("CurrentOrderContentText");
            if (oldContent != null)
            {
                oldContent.SetParent(contentRoot.transform, false);
            }

            Text content = EnsureText(contentRoot.transform, "CurrentOrderContentText", 18, FontStyle.Normal, TextAnchor.UpperLeft);
            content.text = "当前无订单";
            content.horizontalOverflow = HorizontalWrapMode.Wrap;
            content.verticalOverflow = VerticalWrapMode.Overflow;
            ContentSizeFitter contentTextFitter = content.GetComponent<ContentSizeFitter>();
            if (contentTextFitter == null)
            {
                contentTextFitter = content.gameObject.AddComponent<ContentSizeFitter>();
            }

            contentTextFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            RectTransform contentRect = content.rectTransform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            ScrollRect scrollComponent = scroll.GetComponent<ScrollRect>();
            Scrollbar scrollbar = EnsureVerticalScrollbar(scroll.transform, "CurrentOrderScrollbar");
            scrollComponent.viewport = viewportRect;
            scrollComponent.content = contentRootRect;
            scrollComponent.horizontal = false;
            scrollComponent.vertical = true;
            scrollComponent.verticalScrollbar = scrollbar;
            scrollComponent.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollComponent.verticalScrollbarSpacing = 8f;
            scrollComponent.scrollSensitivity = 28f;
            scrollComponent.movementType = ScrollRect.MovementType.Clamped;
        }

        private static Scrollbar EnsureVerticalScrollbar(Transform scrollRoot, string name)
        {
            GameObject scrollbarObject = EnsureChild(scrollRoot, name, typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            RectTransform rect = scrollbarObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(18f, 0f);
            rect.offsetMin = new Vector2(-18f, 8f);
            rect.offsetMax = new Vector2(0f, -8f);

            Image trackImage = scrollbarObject.GetComponent<Image>();
            trackImage.color = new Color(1f, 1f, 1f, 0.12f);
            trackImage.raycastTarget = true;

            GameObject slidingArea = EnsureChild(scrollbarObject.transform, "Sliding Area", typeof(RectTransform));
            RectTransform slidingRect = slidingArea.GetComponent<RectTransform>();
            slidingRect.anchorMin = Vector2.zero;
            slidingRect.anchorMax = Vector2.one;
            slidingRect.offsetMin = new Vector2(3f, 3f);
            slidingRect.offsetMax = new Vector2(-3f, -3f);

            GameObject handle = EnsureChild(slidingArea.transform, "Handle", typeof(RectTransform), typeof(Image));
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.offsetMin = Vector2.zero;
            handleRect.offsetMax = Vector2.zero;

            Image handleImage = handle.GetComponent<Image>();
            handleImage.color = new Color(1f, 1f, 1f, 0.42f);
            handleImage.raycastTarget = true;

            Scrollbar scrollbar = scrollbarObject.GetComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.targetGraphic = handleImage;
            scrollbar.handleRect = handleRect;
            scrollbar.size = 0.35f;
            scrollbar.value = 1f;
            return scrollbar;
        }

        private static GameObject EnsureChild(Transform parent, string name, params Type[] componentTypes)
        {
            Transform existing = parent.Find(name);
            GameObject child = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null)
            {
                child.transform.SetParent(parent, false);
            }

            for (int i = 0; i < componentTypes.Length; i++)
            {
                Type componentType = componentTypes[i];
                if (child.GetComponent(componentType) == null)
                {
                    child.AddComponent(componentType);
                }
            }

            child.hideFlags = HideFlags.None;
            return child;
        }

        private static int RemoveMissingScriptsRecursively(GameObject root)
        {
            if (root == null)
            {
                return 0;
            }

            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
            for (int i = 0; i < root.transform.childCount; i++)
            {
                removed += RemoveMissingScriptsRecursively(root.transform.GetChild(i).gameObject);
            }

            return removed;
        }

        private static void AssertNoMissingScriptsRecursive(GameObject root, string label)
        {
            if (root == null)
            {
                throw new InvalidOperationException(label + " is missing.");
            }

            int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root);
            if (missingCount > 0)
            {
                throw new InvalidOperationException(label + " still has missing script components on " + GetHierarchyPath(root.transform) + ": " + missingCount + ".");
            }

            for (int i = 0; i < root.transform.childCount; i++)
            {
                AssertNoMissingScriptsRecursive(root.transform.GetChild(i).gameObject, label);
            }
        }

        private static void RemoveDirectChildIfExists(Transform parent, string name)
        {
            Transform existing = parent != null ? parent.Find(name) : null;
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }
        }

        private static Text EnsureText(Transform parent, string name, int fontSize, FontStyle fontStyle, TextAnchor alignment)
        {
            GameObject target = EnsureChild(parent, name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            Text text = target.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = true;
            return text;
        }

        private static Button EnsureButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject target = EnsureChild(parent, name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform rect = target.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            target.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.18f);

            Text labelText = EnsureText(target.transform, GetButtonLabelObjectName(name), 18, FontStyle.Bold, TextAnchor.MiddleCenter);
            labelText.text = label;
            labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
            RectTransform labelRect = labelText.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            return target.GetComponent<Button>();
        }

        private static Transform EnsureTopRightActionBar(Transform surface)
        {
            GameObject bar = EnsureChild(surface, "P10_TopRightActionBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            RectTransform rect = bar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-24f, -24f);
            rect.sizeDelta = new Vector2(216f, 48f);
            rect.localScale = Vector3.one;

            HorizontalLayoutGroup layout = bar.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.spacing = 16f;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return bar.transform;
        }

        private static Button EnsureActionBarButton(Transform actionBar, string name, string label)
        {
            Transform existing = FindDescendantByName(actionBar.root, name);
            GameObject target = existing != null
                ? existing.gameObject
                : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            target.transform.SetParent(actionBar, false);
            target.SetActive(true);

            RectTransform rect = target.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = target.AddComponent<RectTransform>();
            }

            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(96f, 42f);
            rect.localScale = Vector3.one;

            Image image = target.GetComponent<Image>();
            if (image == null)
            {
                image = target.AddComponent<Image>();
            }

            image.color = new Color(1f, 1f, 1f, 0.18f);
            image.raycastTarget = true;

            Button button = target.GetComponent<Button>();
            if (button == null)
            {
                button = target.AddComponent<Button>();
            }

            LayoutElement layoutElement = target.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = target.AddComponent<LayoutElement>();
            }

            layoutElement.preferredWidth = 96f;
            layoutElement.preferredHeight = 42f;
            layoutElement.minWidth = 88f;
            layoutElement.minHeight = 38f;

            Text labelText = EnsureText(target.transform, GetButtonLabelObjectName(name), 18, FontStyle.Bold, TextAnchor.MiddleCenter);
            labelText.text = label;
            labelText.enabled = true;
            labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
            RectTransform labelRect = labelText.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            return button;
        }

        private static string GetButtonLabelObjectName(string buttonName)
        {
            if (buttonName == "LogButton" || buttonName == "PersistentLogButton")
            {
                return "LogLabel";
            }

            if (buttonName == "PersistentOrderButton")
            {
                return "OrderLabel";
            }

            if (buttonName == "NextButton")
            {
                return "NextLabel";
            }

            if (buttonName == "CloseButton" || buttonName == "DialogueLogCloseButton" || buttonName == "CurrentOrderCloseButton")
            {
                return "CloseLabel";
            }

            return buttonName + "Label";
        }

        private static void AssignDialogueSurfacePrefab(P10DialogueController controller, GameObject prefab)
        {
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("dialogueSurfacePrefab").objectReferenceValue = prefab;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssertManualSlotRootIsShallow(GameObject prefab)
        {
            Transform manualSlots = prefab.transform.Find(EditorVisibleManualSlotRootName);
            if (manualSlots == null)
            {
                throw new InvalidOperationException("R9B manual slot root is missing at shallow prefab path: " + EditorVisibleManualSlotRootName + ".");
            }

            if (manualSlots.parent != prefab.transform)
            {
                throw new InvalidOperationException("R9B manual slot root is not a direct child of P10_CH01_DialogueUIRoot.");
            }

            if (manualSlots.Find("P10_DialogueSpeakerText") == null || manualSlots.Find("P10_DialogueBodyText") == null)
            {
                throw new InvalidOperationException("R9B manual slot root does not directly contain both dialogue Text objects.");
            }

            Canvas manualSlotsCanvas = manualSlots.GetComponent<Canvas>();
            if (manualSlotsCanvas == null || manualSlotsCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                throw new InvalidOperationException("R9B manual slot root must have its own ScreenSpaceOverlay Canvas so the shallow Text slots are visible.");
            }

            if (manualSlots.localScale.x <= 0f || manualSlots.localScale.y <= 0f || manualSlots.localScale.z <= 0f)
            {
                throw new InvalidOperationException("R9B manual slot root has zero scale and is not clearly visible.");
            }
        }

        private static void AssertUniqueSlotNameInPrefab(GameObject prefab, string slotName)
        {
            int count = CountDescendantsNamed(prefab.transform, slotName);
            if (count != 1)
            {
                throw new InvalidOperationException("R9B prefab must contain exactly one " + slotName + ", actual: " + count + ".");
            }
        }

        private static int CountDescendantsNamed(Transform root, string name)
        {
            if (root == null)
            {
                return 0;
            }

            int count = root.name == name ? 1 : 0;
            for (int i = 0; i < root.childCount; i++)
            {
                count += CountDescendantsNamed(root.GetChild(i), name);
            }

            return count;
        }

        private static void AssertPrefabYamlHasClearlyVisibleSlots()
        {
            string fullPath = Path.GetFullPath(EditorVisibleDialogueUiPrefabPath);
            if (!File.Exists(fullPath))
            {
                throw new InvalidOperationException("R9B prefab YAML missing on disk: " + EditorVisibleDialogueUiPrefabPath + ".");
            }

            string yaml = File.ReadAllText(fullPath);
            if (!yaml.Contains("m_Name: " + EditorVisibleManualSlotRootName))
            {
                throw new InvalidOperationException("R9B prefab YAML does not contain the shallow manual slot root.");
            }

            if (CountOccurrences(yaml, "m_Name: P10_DialogueSpeakerText") != 1)
            {
                throw new InvalidOperationException("R9B prefab YAML must contain exactly one P10_DialogueSpeakerText.");
            }

            if (CountOccurrences(yaml, "m_Name: P10_DialogueBodyText") != 1)
            {
                throw new InvalidOperationException("R9B prefab YAML must contain exactly one P10_DialogueBodyText.");
            }

            if (yaml.Contains("m_Script: {fileID: 0}"))
            {
                throw new InvalidOperationException("R9B prefab YAML still contains a missing script component.");
            }
        }

        private static int CountOccurrences(string text, string pattern)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(pattern))
            {
                return 0;
            }

            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += pattern.Length;
            }

            return count;
        }

        private static string GetHierarchyPath(Transform target)
        {
            if (target == null)
            {
                return string.Empty;
            }

            string path = target.name;
            Transform parent = target.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        private static void AssertEditorVisiblePrefabSlot(GameObject prefab, string slotPath, string label)
        {
            Transform slot = prefab.transform.Find(slotPath);
            if (slot == null)
            {
                throw new InvalidOperationException(label + " missing at prefab path: " + slotPath + ".");
            }

            Text text = slot.GetComponent<Text>();
            if (text == null)
            {
                throw new InvalidOperationException(label + " missing Text component.");
            }

            if (text.gameObject.hideFlags != HideFlags.None)
            {
                throw new InvalidOperationException(label + " is hidden by HideFlags: " + text.gameObject.hideFlags + ".");
            }

            if (!text.gameObject.activeSelf)
            {
                throw new InvalidOperationException(label + " is inactive in prefab asset.");
            }

            RectTransform rectTransform = text.rectTransform;
            if (rectTransform == null || rectTransform.sizeDelta.sqrMagnitude <= 0f && rectTransform.rect.width <= 0f && rectTransform.rect.height <= 0f)
            {
                throw new InvalidOperationException(label + " has no movable RectTransform in prefab asset.");
            }

            if (rectTransform.localScale.x <= 0f || rectTransform.localScale.y <= 0f || rectTransform.localScale.z <= 0f)
            {
                throw new InvalidOperationException(label + " has zero scale in prefab asset.");
            }
        }

        private static void AssertEditorVisibleRuntimeSlot(Text text, string label)
        {
            if (text == null)
            {
                throw new InvalidOperationException(label + " missing Text component.");
            }

            if (text.gameObject.hideFlags != HideFlags.None)
            {
                throw new InvalidOperationException(label + " is hidden by HideFlags: " + text.gameObject.hideFlags + ".");
            }

            if (!text.gameObject.activeInHierarchy)
            {
                throw new InvalidOperationException(label + " is not active in hierarchy.");
            }

            RectTransform rectTransform = text.rectTransform;
            if (rectTransform == null || rectTransform.rect.width <= 0f || rectTransform.rect.height <= 0f)
            {
                throw new InvalidOperationException(label + " has no movable RectTransform size.");
            }

            Vector3 worldScale = rectTransform.lossyScale;
            if (worldScale.x <= 0f || worldScale.y <= 0f || worldScale.z <= 0f)
            {
                throw new InvalidOperationException(label + " has zero world scale and is not clearly visible.");
            }
        }

        private static void AssertPrefabButton(Transform searchRoot, string buttonName, string label)
        {
            Transform buttonTransform = searchRoot != null ? FindDescendantByName(searchRoot, buttonName) : null;
            Button button = buttonTransform != null ? buttonTransform.GetComponent<Button>() : null;
            if (button == null)
            {
                throw new InvalidOperationException(label + " missing Button component.");
            }

            RectTransform rectTransform = button.GetComponent<RectTransform>();
            LayoutElement layoutElement = button.GetComponent<LayoutElement>();
            bool hasLayoutSize = layoutElement != null &&
                (layoutElement.preferredWidth > 0f || layoutElement.minWidth > 0f) &&
                (layoutElement.preferredHeight > 0f || layoutElement.minHeight > 0f);
            if (rectTransform == null || !hasLayoutSize && rectTransform.sizeDelta.sqrMagnitude <= 0f && rectTransform.rect.width <= 0f && rectTransform.rect.height <= 0f)
            {
                throw new InvalidOperationException(label + " has no usable RectTransform.");
            }

            if (button.gameObject.hideFlags != HideFlags.None || !button.gameObject.activeSelf)
            {
                throw new InvalidOperationException(label + " is hidden or inactive in prefab.");
            }
        }

        private static void AssertDialoguePanelActionButtonsDoNotOverlap(Transform panel)
        {
            if (panel == null)
            {
                throw new InvalidOperationException("Dialogue action button panel is missing.");
            }

            RectTransform logRect = panel.Find("LogButton")?.GetComponent<RectTransform>();
            RectTransform closeRect = panel.Find("CloseButton")?.GetComponent<RectTransform>();
            if (logRect == null || closeRect == null)
            {
                throw new InvalidOperationException("Dialogue action buttons are missing LogButton or CloseButton.");
            }

            if (!logRect.gameObject.activeSelf || !closeRect.gameObject.activeSelf)
            {
                throw new InvalidOperationException("Dialogue action buttons must be active in prefab.");
            }

            if (logRect.anchorMax.x > closeRect.anchorMin.x)
            {
                throw new InvalidOperationException("LogButton overlaps CloseButton horizontally; LogButton must stay to the left of CloseButton.");
            }

            const float MinimumAnchorGap = 0.015f;
            float anchorGap = closeRect.anchorMin.x - logRect.anchorMax.x;
            if (anchorGap < MinimumAnchorGap)
            {
                throw new InvalidOperationException("LogButton is too close to CloseButton. Anchor gap: " + anchorGap + ".");
            }

            if (logRect.anchorMax.y > closeRect.anchorMin.y && closeRect.anchorMax.y > logRect.anchorMin.y && logRect.anchorMax.x > closeRect.anchorMin.x)
            {
                throw new InvalidOperationException("LogButton and CloseButton RectTransform anchors overlap.");
            }
        }

        private static void AssertNoVisibleContinueButton(Transform searchRoot, string label)
        {
            if (searchRoot == null)
            {
                throw new InvalidOperationException(label + " search root missing for continue button visibility check.");
            }

            AssertNoVisibleContinueButtonRecursive(searchRoot, label);
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
                    throw new InvalidOperationException(label + " still contains active visible continue object: " + GetHierarchyPath(target) + ".");
                }

                Button button = target.GetComponent<Button>();
                if (button != null && string.Equals(target.name, "NextButton", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(label + " still contains active visible continue Button: " + GetHierarchyPath(target) + ".");
                }

                Text text = target.GetComponent<Text>();
                if (text != null &&
                    text.enabled &&
                    text.color.a > 0f &&
                    string.Equals(text.text, "\u7ee7\u7eed", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(label + " still contains active Text.text == continue label at " + GetHierarchyPath(target) + ".");
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

        private static void AssertEditorVisiblePrefabText(Transform searchRoot, string textName, string expectedText, string label)
        {
            Transform textTransform = searchRoot != null ? FindDescendantByName(searchRoot, textName) : null;
            Text text = textTransform != null ? textTransform.GetComponent<Text>() : null;
            if (text == null)
            {
                throw new InvalidOperationException(label + " missing Text component.");
            }

            if (!string.IsNullOrWhiteSpace(expectedText) && !string.Equals(text.text, expectedText, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(label + " text mismatch. Expected: " + expectedText + ", actual: " + text.text + ".");
            }

            if (text.gameObject.hideFlags != HideFlags.None || !text.gameObject.activeSelf)
            {
                throw new InvalidOperationException(label + " is hidden or inactive in prefab.");
            }

            RectTransform rectTransform = text.rectTransform;
            if (rectTransform == null)
            {
                throw new InvalidOperationException(label + " has no usable RectTransform.");
            }

            if (text.font == null)
            {
                throw new InvalidOperationException(label + " font is missing.");
            }

            if (text.horizontalOverflow != HorizontalWrapMode.Wrap)
            {
                throw new InvalidOperationException(label + " must wrap long Chinese lines.");
            }

            if (text.verticalOverflow == VerticalWrapMode.Truncate)
            {
                throw new InvalidOperationException(label + " must not truncate vertically.");
            }
        }

        private static void AssertCurrentOrderPanelText(P10DialogueController controller, string label, params string[] expectedFragments)
        {
            if (!controller.IsCurrentOrderPanelVisible)
            {
                throw new InvalidOperationException(label + " current order panel is not visible.");
            }

            Text content = FindRuntimeText(controller, "CurrentOrderContentText", label);
            AssertVisibleTextComponent(content, label + " current order content");
            if (content.horizontalOverflow != HorizontalWrapMode.Wrap)
            {
                throw new InvalidOperationException(label + " current order content must wrap.");
            }

            if (content.verticalOverflow == VerticalWrapMode.Truncate)
            {
                throw new InvalidOperationException(label + " current order content must not truncate.");
            }

            for (int i = 0; i < expectedFragments.Length; i++)
            {
                string expectedFragment = expectedFragments[i];
                if (!content.text.Contains(expectedFragment))
                {
                    throw new InvalidOperationException(label + " missing current order text: " + expectedFragment + ". Actual: " + content.text);
                }
            }

            AssertNoBareTechnicalTerm(content.text, label + " current order content");
        }

        private static void AssertCurrentOrderPanelTextAny(P10DialogueController controller, string label, params string[] acceptedFragments)
        {
            if (!controller.IsCurrentOrderPanelVisible)
            {
                throw new InvalidOperationException(label + " current order panel is not visible.");
            }

            Text content = FindRuntimeText(controller, "CurrentOrderContentText", label);
            AssertVisibleTextComponent(content, label + " current order content");
            for (int i = 0; i < acceptedFragments.Length; i++)
            {
                if (content.text.Contains(acceptedFragments[i]))
                {
                    AssertNoBareTechnicalTerm(content.text, label + " current order content");
                    return;
                }
            }

            throw new InvalidOperationException(label + " missing accepted current order completion text. Actual: " + content.text);
        }

        private static void AssertCurrentOrderPanelClosed(P10DialogueController controller, string label)
        {
            if (controller.IsCurrentOrderPanelVisible)
            {
                throw new InvalidOperationException(label + " current order panel should be closed.");
            }
        }

        private static void AssertDialogueLogUnaffectedByOrderPanel(
            P10DialogueController controller,
            P10NarrativeManager manager,
            int expectedLogCount,
            P10NarrativeState expectedState,
            string expectedNode,
            string label)
        {
            controller.CloseCurrentOrderPanel();
            AssertCurrentOrderPanelClosed(controller, label + " close");
            AssertNode(manager, expectedState, expectedNode, label + " changed narrative state.");
            AssertLogCount(controller, expectedLogCount, label + " changed Dialogue Log count.");
            if (controller.IsDialogueLogVisible)
            {
                throw new InvalidOperationException(label + " unexpectedly opened Dialogue Log.");
            }
        }

        private static Text FindRuntimeText(P10DialogueController controller, string objectName, string label)
        {
            Transform match = FindDescendantByName(controller.transform, objectName);
            Text text = match != null ? match.GetComponent<Text>() : null;
            if (text == null)
            {
                throw new InvalidOperationException(label + " missing runtime Text: " + objectName + ".");
            }

            return text;
        }

        private static void AssertRectTransformAnchoredPositionUnchanged(RectTransform rectTransform, Vector2 expected, string label)
        {
            if (rectTransform == null)
            {
                throw new InvalidOperationException(label + " RectTransform missing.");
            }

            if ((rectTransform.anchoredPosition - expected).sqrMagnitude > 0.0001f)
            {
                throw new InvalidOperationException(label + " anchoredPosition was reset. Expected " + expected + ", actual " + rectTransform.anchoredPosition + ".");
            }
        }

        private static void AssertVisibleDialogueDoesNotContainOtherLines(string visibleText, P10DialogueNodeSO node, int currentLineIndex, string label)
        {
            for (int i = 0; i < node.DialogueLines.Count; i++)
            {
                if (i == currentLineIndex)
                {
                    continue;
                }

                P10DialogueLine otherLine = node.DialogueLines[i];
                if (otherLine != null &&
                    !string.IsNullOrWhiteSpace(otherLine.DialogueText) &&
                    !string.Equals(otherLine.DialogueText, node.DialogueLines[currentLineIndex].DialogueText, StringComparison.Ordinal) &&
                    visibleText.Contains(otherLine.DialogueText))
                {
                    throw new InvalidOperationException("Visible bottom dialogue combines multiple DialogueLines at " + label + ".");
                }
            }
        }

        private static void AdvanceToNode(P10DialogueController controller, P10NarrativeManager manager, P10NarrativeState expectedState, string expectedNode)
        {
            controller.AdvanceDialogue();
            AssertNode(manager, expectedState, expectedNode, "Dialogue Next did not enter expected node.");
        }

        private static void AssertBlockedDialogueNext(P10DialogueController controller, P10NarrativeManager manager, int expectedLogCount, P10NarrativeState expectedState, string expectedNodeId)
        {
            controller.AdvanceDialogue();
            AssertNode(manager, expectedState, expectedNodeId, "Blocked Dialogue Next changed state.");
            AssertLogCount(controller, expectedLogCount, "Blocked Dialogue Next appended a Dialogue Log entry.");
        }

        private static void AssertDialogueLogPanelDoesNotMutate(P10DialogueController controller, P10NarrativeManager manager, int expectedLogCount)
        {
            string beforeNode = manager.GetCurrentNode();
            P10NarrativeState beforeState = manager.GetCurrentState();
            controller.OpenDialogueLog();
            if (!controller.IsDialogueLogVisible)
            {
                throw new InvalidOperationException("Dialogue Log panel did not open.");
            }

            controller.CloseDialogueLog();
            if (controller.IsDialogueLogVisible)
            {
                throw new InvalidOperationException("Dialogue Log panel did not close.");
            }

            AssertNode(manager, beforeState, beforeNode, "Dialogue Log open/close changed narrative state.");
            AssertLogCount(controller, expectedLogCount, "Dialogue Log open/close changed log count.");
        }

        private static void AssertRefreshDoesNotMutate(P10DialogueController controller, P10NarrativeManager manager, int expectedLogCount, string expectedNodeId)
        {
            P10NarrativeState beforeState = manager.GetCurrentState();
            string beforeNode = manager.GetCurrentNode();
            controller.SetCurrentNode(expectedNodeId);
            AssertNode(manager, beforeState, beforeNode, "Current node refresh changed narrative state.");
            AssertLogCount(controller, expectedLogCount, "Current node refresh duplicated Dialogue Log entry.");
        }

        private static void AssertLatestEntryDoesNotContain(P10DialogueController controller, string forbiddenFragment)
        {
            if (string.IsNullOrWhiteSpace(forbiddenFragment))
            {
                return;
            }

            P10DialogueLogEntry latest = controller.DialogueLogEntries[controller.DialogueLogEntries.Count - 1];
            if (!string.IsNullOrWhiteSpace(latest.DialogueText) && latest.DialogueText.Contains(forbiddenFragment))
            {
                throw new InvalidOperationException("Split dialogue line contains text from another line.");
            }
        }

        private static void ReachOrder003PassWithoutUi(P10NarrativeManager manager)
        {
            AssertSceneTrigger(manager, "StartPrologue", NodePrologue, true);
            if (!manager.TryAdvanceDialogueNode(NodePrologue, NodeTutorial) || !manager.TryAdvanceDialogueNode(NodeTutorial, NodeOrder001Accept))
            {
                throw new InvalidOperationException("Could not reach ORDER_001 accept without UI.");
            }

            PublishOrderCompleted(manager, "ORDER_001");
            if (!manager.TryAdvanceDialogueNode(NodeOrder001Pass, NodeOrder003Accept))
            {
                throw new InvalidOperationException("Could not reach ORDER_003 accept without UI.");
            }

            PublishOrderCompleted(manager, "ORDER_003");
            AssertNode(manager, P10NarrativeState.Order003, NodeOrder003Pass, "Could not reach ORDER_003 pass without UI.");
        }

        private static void ReachOrder004AcceptWithoutUi(P10NarrativeManager manager)
        {
            ReachOrder003PassWithoutUi(manager);
            if (!manager.TryAdvanceDialogueNode(NodeOrder003Pass, NodeOrder004Accept))
            {
                throw new InvalidOperationException("Could not reach ORDER_004 accept without UI.");
            }
        }

        private static void PublishOrderCompleted(P10NarrativeManager manager, string orderId)
        {
            manager.PublishEvent(new P10NarrativeEvent(P10NarrativeEventType.OrderCompleted)
            {
                OrderId = orderId
            });
        }

        private static void PublishScoreThreshold(P10NarrativeManager manager, int score)
        {
            manager.PublishEvent(new P10NarrativeEvent(P10NarrativeEventType.ScoreThresholdReached)
            {
                Score = score
            });
        }

        private static void AssertSceneTrigger(P10NarrativeManager manager, string triggerId, string nodeId, bool expected)
        {
            bool actual = manager.HandleSceneTrigger(triggerId, nodeId);
            if (actual != expected)
            {
                throw new InvalidOperationException("Unexpected trigger result for " + triggerId + ".");
            }
        }

        private static void AssertNode(P10NarrativeManager manager, P10NarrativeState expectedState, string expectedNode, string message)
        {
            if (manager.GetCurrentState() != expectedState || manager.GetCurrentNode() != expectedNode)
            {
                throw new InvalidOperationException(message + " Actual state/node: " + manager.GetCurrentState() + " / " + manager.GetCurrentNode());
            }
        }

        private static void AssertLogCount(P10DialogueController controller, int expectedCount, string message)
        {
            if (controller.GetDialogueLogCount() != expectedCount)
            {
                throw new InvalidOperationException(message + " Actual count: " + controller.GetDialogueLogCount());
            }
        }

        private static void AssertLineCount(List<P10DialogueNodeSO> nodes, string nodeId, int expectedLineCount)
        {
            P10DialogueNodeSO node = nodes.Find(candidate => candidate != null && candidate.NodeId == nodeId);
            int actual = node != null && node.DialogueLines != null ? node.DialogueLines.Count : 0;
            if (actual != expectedLineCount)
            {
                throw new InvalidOperationException("Unexpected line count for " + nodeId + ". Expected " + expectedLineCount + ", actual " + actual + ".");
            }
        }

        private static void AssertSpeakerResolves(string label, string speakerCharacterId, Dictionary<string, P10CharacterDataSO> characters)
        {
            if (string.IsNullOrWhiteSpace(speakerCharacterId) || characters == null || !characters.ContainsKey(speakerCharacterId))
            {
                throw new InvalidOperationException("Unresolved speaker id at " + label + ": " + speakerCharacterId + ".");
            }
        }

        private static void AssertNoSpeakerPrefix(string label, string dialogueText, Dictionary<string, P10CharacterDataSO> characters)
        {
            if (string.IsNullOrWhiteSpace(dialogueText) || characters == null)
            {
                return;
            }

            foreach (KeyValuePair<string, P10CharacterDataSO> pair in characters)
            {
                P10CharacterDataSO character = pair.Value;
                if (character == null || string.IsNullOrWhiteSpace(character.DisplayName))
                {
                    continue;
                }

                string prefix = character.DisplayName + "\uff1a";
                if (dialogueText.Contains(prefix))
                {
                    throw new InvalidOperationException("Dialogue text must not contain speaker prefix at " + label + ": " + prefix);
                }
            }
        }

        private static void ValidateBridgeSurfaceUnchanged()
        {
            if (!P10NarrativeGameplayEventAdapter.IsSupportedFactType(P10NarrativeGameplayFactType.OrderCompleted) ||
                !P10NarrativeGameplayEventAdapter.IsSupportedFactType(P10NarrativeGameplayFactType.ScoreThresholdReached))
            {
                throw new InvalidOperationException("Bridge fact adapter support changed.");
            }

            if (typeof(P10NarrativeBridgePort).GetMethod("SubmitGameplayFact") == null ||
                typeof(P10NarrativeBridgePort).GetMethod("ReceiveNarrativeCommand") == null ||
                typeof(P10NarrativeBridgePort).GetMethod("RegisterAnchorPosition") == null)
            {
                throw new InvalidOperationException("P10NarrativeBridgePort surface is missing expected members.");
            }
        }

        private static P10NarrativeManager CreateManager(Transform parent, string name)
        {
            GameObject host = new GameObject("P10D_" + name);
            host.transform.SetParent(parent, false);
            P10NarrativeManager manager = host.AddComponent<P10NarrativeManager>();
            InvokeAwake(manager);
            return manager;
        }

        private static P10DialogueController CreateDialogueController(Transform parent, P10NarrativeManager manager)
        {
            GameObject host = new GameObject("P10D_DialogueController");
            host.transform.SetParent(parent, false);
            P10DialogueController controller = host.AddComponent<P10DialogueController>();
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("narrativeManager").objectReferenceValue = manager;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            return controller;
        }

        private static void InjectPhase10AssetDialogueData(P10DialogueController controller)
        {
            SerializedObject serializedController = new SerializedObject(controller);
            SerializedProperty nodes = serializedController.FindProperty("dialogueNodes");
            nodes.ClearArray();
            foreach (P10DialogueNodeSO node in LoadDialogueNodes())
            {
                int index = nodes.arraySize;
                nodes.InsertArrayElementAtIndex(index);
                nodes.GetArrayElementAtIndex(index).objectReferenceValue = node;
            }

            SerializedProperty characters = serializedController.FindProperty("characters");
            characters.ClearArray();
            foreach (P10CharacterDataSO character in LoadCharacters().Values)
            {
                int index = characters.arraySize;
                characters.InsertArrayElementAtIndex(index);
                characters.GetArrayElementAtIndex(index).objectReferenceValue = character;
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }

        private static List<P10DialogueNodeSO> LoadDialogueNodes()
        {
            List<P10DialogueNodeSO> nodes = new List<P10DialogueNodeSO>();
            string[] guids = AssetDatabase.FindAssets("t:P10DialogueNodeSO", new[] { DialogueAssetFolder });
            foreach (string guid in guids)
            {
                P10DialogueNodeSO node = AssetDatabase.LoadAssetAtPath<P10DialogueNodeSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (node != null && !string.IsNullOrWhiteSpace(node.NodeId) && node.NodeId.StartsWith("P10_CH01_NODE_", StringComparison.Ordinal))
                {
                    nodes.Add(node);
                }
            }

            return nodes;
        }

        private static Dictionary<string, P10CharacterDataSO> LoadCharacters()
        {
            Dictionary<string, P10CharacterDataSO> characters = new Dictionary<string, P10CharacterDataSO>();
            string[] guids = AssetDatabase.FindAssets("t:P10CharacterDataSO", new[] { CharacterAssetFolder });
            foreach (string guid in guids)
            {
                P10CharacterDataSO character = AssetDatabase.LoadAssetAtPath<P10CharacterDataSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (character != null && !string.IsNullOrWhiteSpace(character.CharacterId))
                {
                    characters[character.CharacterId] = character;
                }
            }

            return characters;
        }

        private static P10DialogueNodeSO ResolveNode(string nodeId)
        {
            P10DialogueNodeSO node = LoadDialogueNodes().Find(candidate => candidate != null && candidate.NodeId == nodeId);
            if (node == null)
            {
                throw new InvalidOperationException("Missing dialogue node asset: " + nodeId + ".");
            }

            return node;
        }

        private static P10CharacterDataSO ResolveCharacter(string characterId)
        {
            Dictionary<string, P10CharacterDataSO> characters = LoadCharacters();
            if (!characters.TryGetValue(characterId, out P10CharacterDataSO character) || character == null ||
                string.IsNullOrWhiteSpace(character.DisplayName) || !ContainsCjk(character.DisplayName))
            {
                throw new InvalidOperationException("Missing or non-Chinese character asset: " + characterId + ".");
            }

            return character;
        }

        private static bool ContainsCjk(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c >= '\u4e00' && c <= '\u9fff')
                {
                    return true;
                }
            }

            return false;
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
