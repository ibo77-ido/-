using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Phase10_Narrative
{
    public static class P10E02InteractionTestHarnessValidator
    {
        private const string PassMessage = "P10E-02 Phase10-only Interaction Test Harness validation passed.";

        public static void RunP10E02InteractionTestHarnessValidation()
        {
            try
            {
                ValidateInteractionGateHarness();
                Debug.Log(PassMessage);
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10E-02 Phase10-only Interaction Test Harness validation failed: " + ex);
                EditorApplication.Exit(1);
            }
        }

        private static void ValidateInteractionGateHarness()
        {
            List<GameObject> createdObjects = new List<GameObject>();
            P10NarrativeStateMachine stateMachine = new P10NarrativeStateMachine();
            P10NarrativeSnapshot beforeSnapshot = stateMachine.SaveSnapshot();

            int dialogueUiCountBefore = UnityEngine.Object.FindObjectsOfType<P10DialogueController>(true).Length;

            try
            {
                Transform player = CreateTestTransform("TestPlayerTransform", Vector3.zero, createdObjects);
                Dictionary<string, Transform> interactPoints = new Dictionary<string, Transform>
                {
                    { P10NpcInteractionGateEvaluator.NpcXuLaoBo, CreateTestTransform("TestNPC_XuLaoBo_InteractPoint", new Vector3(P10NpcInteractionGateEvaluator.DefaultInteractionDistance, 0f, 0f), createdObjects) },
                    { P10NpcInteractionGateEvaluator.NpcZhouZhangGui, CreateTestTransform("TestNPC_ZhouZhangGui_InteractPoint", new Vector3(P10NpcInteractionGateEvaluator.DefaultInteractionDistance, 0f, 0f), createdObjects) },
                    { P10NpcInteractionGateEvaluator.NpcChenShuYuan, CreateTestTransform("TestNPC_ChenShuYuan_InteractPoint", new Vector3(P10NpcInteractionGateEvaluator.DefaultInteractionDistance, 0f, 0f), createdObjects) },
                    { P10NpcInteractionGateEvaluator.NpcLuKe, CreateTestTransform("TestNPC_LuKe_InteractPoint", new Vector3(P10NpcInteractionGateEvaluator.DefaultInteractionDistance, 0f, 0f), createdObjects) }
                };

                ValidateMainlineMapping(player, interactPoints);
                ValidateClickResultPriority(player, interactPoints);
                ValidateInvalidNpc(player);

                P10NarrativeSnapshot afterSnapshot = stateMachine.SaveSnapshot();
                AssertSnapshotUnchanged(beforeSnapshot, afterSnapshot);
                AssertNoDialogueUiOpened(dialogueUiCountBefore);
            }
            finally
            {
                for (int i = createdObjects.Count - 1; i >= 0; i--)
                {
                    if (createdObjects[i] != null)
                    {
                        UnityEngine.Object.DestroyImmediate(createdObjects[i]);
                    }
                }
            }

            AssertNoDialogueUiOpened(dialogueUiCountBefore);
        }

        private static void ValidateMainlineMapping(Transform player, Dictionary<string, Transform> interactPoints)
        {
            AssertGate(
                player.position,
                interactPoints[P10NpcInteractionGateEvaluator.NpcXuLaoBo].position,
                P10NarrativeState.Prologue,
                P10NpcInteractionGateEvaluator.NpcXuLaoBo,
                P10NpcInteractionGateResult.AllowDialogue,
                "Prologue -> XuLaoBo");

            AssertGate(
                player.position,
                interactPoints[P10NpcInteractionGateEvaluator.NpcZhouZhangGui].position,
                P10NarrativeState.Order001,
                P10NpcInteractionGateEvaluator.NpcZhouZhangGui,
                P10NpcInteractionGateResult.AllowDialogue,
                "Order001 -> ZhouZhangGui");

            AssertGate(
                player.position,
                interactPoints[P10NpcInteractionGateEvaluator.NpcChenShuYuan].position,
                P10NarrativeState.Order003,
                P10NpcInteractionGateEvaluator.NpcChenShuYuan,
                P10NpcInteractionGateResult.AllowDialogue,
                "Order003 -> ChenShuYuan");

            AssertGate(
                player.position,
                interactPoints[P10NpcInteractionGateEvaluator.NpcLuKe].position,
                P10NarrativeState.Order004,
                P10NpcInteractionGateEvaluator.NpcLuKe,
                P10NpcInteractionGateResult.AllowDialogue,
                "Order004 -> LuKe");

            AssertGate(
                player.position,
                interactPoints[P10NpcInteractionGateEvaluator.NpcXuLaoBo].position,
                P10NarrativeState.Ending,
                P10NpcInteractionGateEvaluator.NpcXuLaoBo,
                P10NpcInteractionGateResult.AllowDialogue,
                "Ending -> XuLaoBo");
        }

        private static void ValidateClickResultPriority(Transform player, Dictionary<string, Transform> interactPoints)
        {
            Vector3 inRange = player.position + new Vector3(P10NpcInteractionGateEvaluator.DefaultInteractionDistance, 0f, 0f);
            Vector3 outOfRange = player.position + new Vector3(P10NpcInteractionGateEvaluator.DefaultInteractionDistance + 0.01f, 0f, 0f);

            interactPoints[P10NpcInteractionGateEvaluator.NpcXuLaoBo].position = inRange;
            AssertGate(player.position, inRange, P10NarrativeState.Prologue, P10NpcInteractionGateEvaluator.NpcXuLaoBo, P10NpcInteractionGateResult.AllowDialogue, "current NPC in range");

            interactPoints[P10NpcInteractionGateEvaluator.NpcXuLaoBo].position = outOfRange;
            AssertGate(player.position, outOfRange, P10NarrativeState.Prologue, P10NpcInteractionGateEvaluator.NpcXuLaoBo, P10NpcInteractionGateResult.TooFar, "current NPC out of range");

            interactPoints[P10NpcInteractionGateEvaluator.NpcZhouZhangGui].position = inRange;
            AssertGate(player.position, inRange, P10NarrativeState.Prologue, P10NpcInteractionGateEvaluator.NpcZhouZhangGui, P10NpcInteractionGateResult.QuestNotAvailableYet, "non-current NPC in range");

            interactPoints[P10NpcInteractionGateEvaluator.NpcZhouZhangGui].position = outOfRange;
            AssertGate(player.position, outOfRange, P10NarrativeState.Prologue, P10NpcInteractionGateEvaluator.NpcZhouZhangGui, P10NpcInteractionGateResult.TooFar, "non-current NPC out of range");
        }

        private static void ValidateInvalidNpc(Transform player)
        {
            AssertGate(
                player.position,
                player.position,
                P10NarrativeState.Prologue,
                "InvalidNpc",
                P10NpcInteractionGateResult.InvalidNpc,
                "invalid NPC");
        }

        private static Transform CreateTestTransform(string objectName, Vector3 position, List<GameObject> createdObjects)
        {
            GameObject gameObject = new GameObject(objectName);
            gameObject.transform.position = position;
            createdObjects.Add(gameObject);
            return gameObject.transform;
        }

        private static void AssertGate(
            Vector3 playerPosition,
            Vector3 interactPointPosition,
            P10NarrativeState currentState,
            string npcId,
            P10NpcInteractionGateResult expected,
            string label)
        {
            P10NpcInteractionGateResult actual = P10NpcInteractionGateEvaluator.Evaluate(
                playerPosition,
                interactPointPosition,
                currentState,
                npcId,
                P10NpcInteractionGateEvaluator.DefaultInteractionDistance);

            if (actual != expected)
            {
                throw new InvalidOperationException(label + " expected " + expected + " but got " + actual + ".");
            }
        }

        private static void AssertSnapshotUnchanged(P10NarrativeSnapshot before, P10NarrativeSnapshot after)
        {
            if (before == null || after == null)
            {
                throw new InvalidOperationException("Narrative state snapshot missing.");
            }

            if (before.ChapterState != after.ChapterState ||
                !string.Equals(before.CurrentNodeId, after.CurrentNodeId, StringComparison.Ordinal) ||
                before.PlayedNodeIds.Count != after.PlayedNodeIds.Count ||
                before.NarrativeFlags.Count != after.NarrativeFlags.Count ||
                before.DialogueLogEntries.Count != after.DialogueLogEntries.Count)
            {
                throw new InvalidOperationException("Interaction test harness mutated narrative state.");
            }
        }

        private static void AssertNoDialogueUiOpened(int dialogueUiCountBefore)
        {
            int dialogueUiCountAfter = UnityEngine.Object.FindObjectsOfType<P10DialogueController>(true).Length;
            if (dialogueUiCountAfter != dialogueUiCountBefore)
            {
                throw new InvalidOperationException("Interaction test harness opened or created Dialogue UI.");
            }
        }
    }
}
