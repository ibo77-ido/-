using System;
using UnityEditor;
using UnityEngine;

namespace Phase10_Narrative
{
    public static class P10CH01Phase9BridgeValidator
    {
        public static void RunP10B_02Phase9BridgeValidation()
        {
            GameObject root = null;

            try
            {
                root = new GameObject("P10B_02_Phase9BridgeValidator_Root");
                root.hideFlags = HideFlags.HideAndDontSave;

                GameObject bridgeHost = CreateChild(root.transform, Phase9Phase10Bridge.DefaultBridgeRootName);
                Phase9Phase10Bridge bridge = bridgeHost.AddComponent<Phase9Phase10Bridge>();
                DisableRuntimeBootstrap(bridge);

                GameObject xu = CreateChild(root.transform, "徐老伯");
                GameObject zhou = CreateChild(root.transform, "周掌柜");
                GameObject chen = CreateChild(root.transform, "陈书院");
                GameObject lu = CreateChild(root.transform, "卢客");

                bridge.RebindSceneNpcs();

                AssertNpcForwarder(xu, P10CH01FlowController.NpcXuLaoBo);
                AssertNpcForwarder(zhou, P10CH01FlowController.NpcZhouZhangGui);
                AssertNpcForwarder(chen, P10CH01FlowController.NpcChenShuYuan);
                AssertNpcForwarder(lu, P10CH01FlowController.NpcLuKe);

                AssertTrue(bridge.SubmitGameStarted(), "GameStarted should start opening narration.");
                AssertStep(bridge, P10CH01FlowStep.OpeningNarration);
                AssertFalse(zhou.GetComponent<Phase9Phase10NpcForwarder>().RequestInteraction(), "Zhou should be blocked before Xu tutorial gate.");
                AssertStep(bridge, P10CH01FlowStep.OpeningNarration);

                AssertTrue(bridge.SubmitDialogueCompleted(P10CH01FlowController.NodePrologue), "Prologue completion should unlock Xu.");
                AssertStep(bridge, P10CH01FlowStep.XuTutorialDialogue);
                AssertTrue(xu.GetComponent<Phase9Phase10NpcForwarder>().RequestInteraction(), "Xu click should forward to FlowController.");
                AssertTrue(bridge.SubmitDialogueCompleted(P10CH01FlowController.NodeTutorial), "Tutorial dialogue completion should enter tutorial crafting.");
                AssertStep(bridge, P10CH01FlowStep.TutorialCrafting);

                AssertFalse(zhou.GetComponent<Phase9Phase10NpcForwarder>().RequestInteraction(), "Zhou should be blocked until TutorialCraftCompleted.");
                AssertTrue(bridge.SubmitTutorialCraftCompleted(), "TutorialCraftCompleted should unlock ORDER_001 accept.");
                AssertStep(bridge, P10CH01FlowStep.Order001Accept);
                AssertTrue(zhou.GetComponent<Phase9Phase10NpcForwarder>().RequestInteraction(), "Zhou should be available at ORDER_001 accept.");

                ValidateBridgeHasNoDirectPhase3Phase6References();

                Debug.Log("P10B-02 Phase9->Phase10 bridge validation passed.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10B-02 Phase9->Phase10 bridge validation failed: " + ex.Message);
                EditorApplication.Exit(1);
            }
            finally
            {
                if (root != null)
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            GameObject child = new GameObject(name);
            child.hideFlags = HideFlags.HideAndDontSave;
            child.transform.SetParent(parent, false);
            return child;
        }

        private static void DisableRuntimeBootstrap(Phase9Phase10Bridge bridge)
        {
            SerializedObject serializedBridge = new SerializedObject(bridge);
            serializedBridge.FindProperty("autoStartChapterOnSceneReady").boolValue = false;
            serializedBridge.FindProperty("loadOverlaySceneAdditively").boolValue = false;
            serializedBridge.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssertNpcForwarder(GameObject npc, string expectedNpcId)
        {
            Phase9Phase10NpcForwarder forwarder = npc.GetComponent<Phase9Phase10NpcForwarder>();
            if (forwarder == null)
            {
                throw new InvalidOperationException("Missing NPC forwarder on " + npc.name);
            }

            if (!string.Equals(forwarder.NpcId, expectedNpcId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("NPC " + npc.name + " expected " + expectedNpcId + " but got " + forwarder.NpcId);
            }
        }

        private static void AssertStep(Phase9Phase10Bridge bridge, P10CH01FlowStep expected)
        {
            if (bridge.CurrentStep != expected)
            {
                throw new InvalidOperationException("Expected step " + expected + " but got " + bridge.CurrentStep);
            }
        }

        private static void AssertTrue(bool value, string label)
        {
            if (!value)
            {
                throw new InvalidOperationException("Expected true: " + label);
            }
        }

        private static void AssertFalse(bool value, string label)
        {
            if (value)
            {
                throw new InvalidOperationException("Expected false: " + label);
            }
        }

        private static void ValidateBridgeHasNoDirectPhase3Phase6References()
        {
            Type[] types =
            {
                typeof(Phase9Phase10Bridge),
                typeof(Phase9Phase10NpcForwarder),
                typeof(P10CH01GameplayFact)
            };

            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (type.FullName != null && (type.FullName.Contains("Phase3") || type.FullName.Contains("Phase6")))
                {
                    throw new InvalidOperationException("Unexpected direct Phase3/Phase6 reference on " + type.FullName);
                }
            }
        }
    }
}
