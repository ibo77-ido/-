using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Phase10_Narrative
{
    [InitializeOnLoad]
    public static class P10NarrativePlayModeValidator
    {
        private const string ScenePath = "Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity";
        private const string PendingSessionKey = "P10_04_PlayModeValidationPending";
        private const string P10D20RPendingSessionKey = "P10D20R_RuntimeDisplayRepairValidationPending";
        private const string NodePrologue = "P10_CH01_NODE_PROLOGUE_01";

        static P10NarrativePlayModeValidator()
        {
            if (SessionState.GetBool(PendingSessionKey, false))
            {
                EditorApplication.update += RunWhenPlayModeReady;
            }

            if (SessionState.GetBool(P10D20RPendingSessionKey, false))
            {
                EditorApplication.update += RunP10D20RWhenPlayModeReady;
            }

        }

        public static void RunP10_04PlayModeValidation()
        {
            SessionState.SetBool(PendingSessionKey, true);
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.isPlaying = true;
            EditorApplication.update += RunWhenPlayModeReady;
        }

        public static void RunP10D20RChineseDialogueRuntimeDisplayRepairValidation()
        {
            SessionState.SetBool(P10D20RPendingSessionKey, true);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.isPlaying = true;
            EditorApplication.update += RunP10D20RWhenPlayModeReady;
        }

        private static void RunWhenPlayModeReady()
        {
            if (!SessionState.GetBool(PendingSessionKey, false) || !EditorApplication.isPlaying)
            {
                return;
            }

            EditorApplication.update -= RunWhenPlayModeReady;
            EditorApplication.delayCall += RunValidationInPlayMode;
        }

        private static void RunP10D20RWhenPlayModeReady()
        {
            if (!SessionState.GetBool(P10D20RPendingSessionKey, false) || !EditorApplication.isPlaying)
            {
                return;
            }

            EditorApplication.update -= RunP10D20RWhenPlayModeReady;
            EditorApplication.delayCall += RunP10D20RValidationInPlayMode;
        }

        private static void RunValidationInPlayMode()
        {
            P10NarrativeManager manager = UnityEngine.Object.FindObjectOfType<P10NarrativeManager>();

            if (manager == null)
            {
                Debug.LogError("P10-04 PlayMode validation failed: manager missing.");
                Exit(1);
                return;
            }

            manager.PublishEvent(new P10NarrativeEvent(P10NarrativeEventType.GameStarted));
            manager.PublishEvent(new P10NarrativeEvent(P10NarrativeEventType.OrderCompleted)
            {
                OrderId = "ORDER_001"
            });
            manager.PublishEvent(new P10NarrativeEvent(P10NarrativeEventType.OrderCompleted)
            {
                OrderId = "ORDER_003"
            });
            manager.PublishEvent(new P10NarrativeEvent(P10NarrativeEventType.ScoreThresholdReached)
            {
                Score = 95
            });
            manager.PublishCommand(new P10NarrativeCommand(P10NarrativeCommandType.NarrativePauseGameplay)
            {
                Payload = "P10 play mode validation",
                NodeId = manager.GetCurrentNode()
            });
            manager.PublishCommand(new P10NarrativeCommand(P10NarrativeCommandType.NarrativeResumeGameplay)
            {
                Payload = "P10 play mode validation",
                NodeId = manager.GetCurrentNode()
            });
            manager.DebugReplayCurrentNode();

            bool valid = manager.GetCurrentState() == P10NarrativeState.Completed
                && manager.GetCurrentNode() == "P10_CH01_NODE_CHAPTER_ENDING";

            Debug.Log(valid
                ? "P10-04 PlayMode validation passed."
                : "P10-04 PlayMode validation failed: unexpected state or node.");

            Exit(valid ? 0 : 1);
        }

        private static void RunP10D20RValidationInPlayMode()
        {
            try
            {
                ValidateP10D20RRuntimeDialogueDisplayUsesAssets();
                Debug.Log("P10D-20R Chinese Dialogue Runtime Display Repair validation passed.");
                ExitP10D20R(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10D-20R Chinese Dialogue Runtime Display Repair validation failed: " + ex);
                ExitP10D20R(1);
            }
        }

        private static void ValidateP10D20RRuntimeDialogueDisplayUsesAssets()
        {
            GameObject root = new GameObject("P10D20R_RuntimeDisplayRepairRoot");

            try
            {
                GameObject managerObject = new GameObject("P10D20R_NarrativeManager");
                managerObject.transform.SetParent(root.transform, false);
                P10NarrativeManager manager = managerObject.AddComponent<P10NarrativeManager>();

                GameObject controllerObject = new GameObject("P10D20R_DialogueController");
                controllerObject.transform.SetParent(managerObject.transform, false);
                P10DialogueController controller = controllerObject.AddComponent<P10DialogueController>();
                controller.EnsureRuntimeSurfaceInstance();

                if (!manager.HandleSceneTrigger("StartPrologue", NodePrologue))
                {
                    throw new InvalidOperationException("P10D20R could not enter Chapter 1 prologue.");
                }

                controller.SetCurrentNode(manager.GetCurrentNode());

                P10DialogueNodeSO prologueNode = LoadDialogueNode(NodePrologue);
                P10CharacterDataSO speaker = LoadCharacter(prologueNode.SpeakerCharacterId);

                AssertChineseAssetRuntimeDisplay(controller, prologueNode, speaker);
                AssertDialogueLogUsesChineseAsset(controller, prologueNode, speaker);
                AssertRuntimeDisplayNotCatalogFallback(controller, prologueNode);
            }
            finally
            {
                if (root != null)
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        private static P10DialogueNodeSO LoadDialogueNode(string nodeId)
        {
            string path = "Assets/Phase10_Narrative/ScriptableObjects/Dialogues/" + nodeId + ".asset";
            P10DialogueNodeSO node = AssetDatabase.LoadAssetAtPath<P10DialogueNodeSO>(path);
            if (node == null)
            {
                throw new InvalidOperationException("P10D20R missing dialogue node asset: " + path);
            }

            if (string.IsNullOrWhiteSpace(node.DialogueText) || !ContainsCjk(node.DialogueText))
            {
                throw new InvalidOperationException("P10D20R dialogue node asset is not Chinese-localized: " + nodeId);
            }

            return node;
        }

        private static P10CharacterDataSO LoadCharacter(string characterId)
        {
            string[] guids = AssetDatabase.FindAssets("t:P10CharacterDataSO", new[] { "Assets/Phase10_Narrative/ScriptableObjects/Characters" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                P10CharacterDataSO character = AssetDatabase.LoadAssetAtPath<P10CharacterDataSO>(path);
                if (character != null && string.Equals(character.CharacterId, characterId, StringComparison.Ordinal))
                {
                    if (string.IsNullOrWhiteSpace(character.DisplayName) || !ContainsCjk(character.DisplayName))
                    {
                        throw new InvalidOperationException("P10D20R character asset is not Chinese-localized: " + characterId);
                    }

                    return character;
                }
            }

            throw new InvalidOperationException("P10D20R missing character asset: " + characterId);
        }

        private static void AssertChineseAssetRuntimeDisplay(P10DialogueController controller, P10DialogueNodeSO node, P10CharacterDataSO speaker)
        {
            Transform surface = controller.transform.Find("P10_Runtime_DialogueSurface");
            if (surface == null)
            {
                throw new InvalidOperationException("P10D20R runtime dialogue surface was not created.");
            }

            CanvasGroup dialoguePanel = surface.Find("DialoguePanel")?.GetComponent<CanvasGroup>();
            if (dialoguePanel == null || dialoguePanel.alpha <= 0.5f)
            {
                throw new InvalidOperationException("P10D20R dialogue panel is not visible.");
            }

            Text speakerText = surface.Find("DialoguePanel/Panel/P10_DialogueSpeakerText")?.GetComponent<Text>();
            Text dialogueText = surface.Find("DialoguePanel/Panel/P10_DialogueBodyText")?.GetComponent<Text>();
            if (speakerText == null || dialogueText == null)
            {
                throw new InvalidOperationException("P10D20R runtime text components are missing.");
            }

            if (!string.Equals(speakerText.text, speaker.DisplayName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("P10D20R runtime speaker does not match Character asset.");
            }

            if (!string.Equals(dialogueText.text, node.DialogueText, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("P10D20R runtime dialogue does not match DialogueNode asset.");
            }

            if (!ContainsCjk(speakerText.text) || !ContainsCjk(dialogueText.text))
            {
                throw new InvalidOperationException("P10D20R runtime display text is not Chinese.");
            }

            if (speakerText.font == null || dialogueText.font == null)
            {
                throw new InvalidOperationException("P10D20R runtime text font is missing.");
            }

            if (speakerText.color.a <= 0f || dialogueText.color.a <= 0f)
            {
                throw new InvalidOperationException("P10D20R runtime text is transparent.");
            }
        }

        private static void AssertDialogueLogUsesChineseAsset(P10DialogueController controller, P10DialogueNodeSO node, P10CharacterDataSO speaker)
        {
            if (controller.DialogueLogEntries.Count != 1)
            {
                throw new InvalidOperationException("P10D20R expected exactly one dialogue log entry.");
            }

            P10DialogueLogEntry entry = controller.DialogueLogEntries[0];
            if (entry.NodeId != node.NodeId
                || entry.SpeakerName != speaker.DisplayName
                || entry.DialogueText != node.DialogueText)
            {
                throw new InvalidOperationException("P10D20R Dialogue Log entry does not match Chinese assets.");
            }
        }

        private static void AssertRuntimeDisplayNotCatalogFallback(P10DialogueController controller, P10DialogueNodeSO node)
        {
            P10DialogueLogEntry entry = controller.DialogueLogEntries[0];
            if (string.Equals(entry.DialogueText, P10DialogueCatalog.GetDialogueText(node.NodeId), StringComparison.Ordinal))
            {
                throw new InvalidOperationException("P10D20R runtime dialogue is still using P10DialogueCatalog fallback text.");
            }

            if (string.Equals(entry.SpeakerName, P10DialogueCatalog.GetSpeakerNameForNode(node.NodeId), StringComparison.Ordinal))
            {
                throw new InvalidOperationException("P10D20R runtime speaker is still using P10DialogueCatalog fallback speaker.");
            }
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

        private static void Exit(int code)
        {
            SessionState.SetBool(PendingSessionKey, false);
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(code);
        }

        private static void ExitP10D20R(int code)
        {
            SessionState.SetBool(P10D20RPendingSessionKey, false);
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(code);
        }

    }
}
