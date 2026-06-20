using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Phase10_Narrative
{
    public static class P10CH01MainFlowValidationPlanValidator
    {
        public static void RunMainFlowValidationPlan()
        {
            GameObject root = null;

            try
            {
                root = new GameObject("P10_CH01_MainFlowValidationPlan_Root");
                root.hideFlags = HideFlags.HideAndDontSave;

                ValidateOpeningAndNpcGates(root.transform);
                ValidateOrderGates(root.transform);
                ValidateAdapterNeutralFactForwarding(root.transform);
                ValidateFullHappyPath(root.transform);
                ValidateDuplicateRewardAndEndingGate(root.transform);
                ValidateNoDirectPhase3Phase6References();

                Debug.Log("P10_CH01 MainFlowValidationPlan validation passed.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10_CH01 MainFlowValidationPlan validation failed: " + ex.Message);
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

        private static void ValidateOpeningAndNpcGates(Transform parent)
        {
            Phase9Phase10Bridge bridge = CreateBridge(parent, "OpeningAndNpcGates");
            Phase9Phase10NpcForwarder xu;
            Phase9Phase10NpcForwarder zhou;
            Phase9Phase10NpcForwarder chen;
            Phase9Phase10NpcForwarder lu;
            CreateNpcForwarders(parent, bridge, out xu, out zhou, out chen, out lu);

            AssertTrue(bridge.SubmitGameStarted(), "TC01 GameStarted should enter opening narration.");
            AssertStep(bridge, P10CH01FlowStep.OpeningNarration, "TC01 OpeningNarration");
            AssertFalse(xu.RequestInteraction(), "TC01 Xu must wait for prologue completion.");
            AssertFalse(zhou.RequestInteraction(), "TC01 Zhou must be blocked during opening.");
            AssertFalse(chen.RequestInteraction(), "TC01 Chen must be blocked during opening.");
            AssertFalse(lu.RequestInteraction(), "TC01 Lu must be blocked during opening.");

            AssertTrue(bridge.SubmitDialogueCompleted(P10CH01FlowController.NodePrologue), "TC01 prologue completion should unlock Xu.");
            AssertStep(bridge, P10CH01FlowStep.XuTutorialDialogue, "TC01 XuTutorialDialogue");
            AssertTrue(xu.RequestInteraction(), "TC02 Xu interaction should be accepted after prologue.");
            AssertFalse(zhou.RequestInteraction(), "TC02 Zhou still blocked before tutorial craft.");
            AssertFalse(chen.RequestInteraction(), "TC02 Chen still blocked before ORDER_003.");
            AssertFalse(lu.RequestInteraction(), "TC02 Lu still blocked before ORDER_004.");
        }

        private static void ValidateOrderGates(Transform parent)
        {
            Phase9Phase10Bridge bridge = CreateBridge(parent, "OrderGates");
            Phase9Phase10NpcForwarder xu;
            Phase9Phase10NpcForwarder zhou;
            Phase9Phase10NpcForwarder chen;
            Phase9Phase10NpcForwarder lu;
            CreateNpcForwarders(parent, bridge, out xu, out zhou, out chen, out lu);

            ReachTutorialCrafting(bridge, xu);
            AssertFalse(zhou.RequestInteraction(), "TC03 ORDER_001 NPC must be blocked before TutorialCraftCompleted.");
            AssertFalse(bridge.SubmitOrderCompleted(P10CH01FlowController.Order001, 80, "Pass"), "TC03 ORDER_001 completion must be blocked before crafting.");

            AssertTrue(bridge.SubmitTutorialCraftCompleted(), "TC03 TutorialCraftCompleted should unlock ORDER_001.");
            AssertStep(bridge, P10CH01FlowStep.Order001Accept, "TC03 ORDER_001 accept");
            AssertTrue(zhou.RequestInteraction(), "TC03 Zhou should be accepted at ORDER_001 accept.");
            AssertFalse(chen.RequestInteraction(), "TC05 Chen must be blocked before ORDER_001 reward dialogue completion.");
            AssertFalse(lu.RequestInteraction(), "TC05 Lu must be blocked before ORDER_004.");

            AssertTrue(bridge.SubmitDialogueCompleted(P10CH01FlowController.NodeOrder001Accept), "TC04 ORDER_001 accept dialogue completion.");
            AssertStep(bridge, P10CH01FlowStep.Order001Crafting, "TC04 ORDER_001 crafting");
            AssertFalse(bridge.SubmitOrderCompleted(P10CH01FlowController.Order003, 80, "Pass"), "TC05 ORDER_003 cannot complete during ORDER_001 crafting.");
            AssertTrue(bridge.SubmitOrderCompleted(P10CH01FlowController.Order001, 80, "Pass"), "TC04 ORDER_001 completion.");
            AssertStep(bridge, P10CH01FlowStep.Order001Reward, "TC04 ORDER_001 reward");
            AssertFalse(chen.RequestInteraction(), "TC05 Chen must wait for ORDER_001 pass dialogue completion.");

            AssertTrue(bridge.SubmitRewardGranted(P10CH01FlowController.Order001), "TC04 ORDER_001 reward grant.");
            AssertTrue(bridge.SubmitDialogueCompleted(P10CH01FlowController.NodeOrder001Pass), "TC05 ORDER_001 pass dialogue completion.");
            AssertStep(bridge, P10CH01FlowStep.Order003Accept, "TC05 ORDER_003 accept");
            AssertTrue(chen.RequestInteraction(), "TC05 Chen should be accepted at ORDER_003 accept.");
            AssertFalse(lu.RequestInteraction(), "TC07 Lu must be blocked before ORDER_003 pass.");
        }

        private static void ValidateAdapterNeutralFactForwarding(Transform parent)
        {
            Phase9Phase10Bridge bridge = CreateBridge(parent, "AdapterFacts");
            Phase9Phase10GameplayFactAdapter adapter = bridge.gameObject.AddComponent<Phase9Phase10GameplayFactAdapter>();
            adapter.Bind(bridge);

            Phase9Phase10NpcForwarder xu;
            Phase9Phase10NpcForwarder zhou;
            Phase9Phase10NpcForwarder chen;
            Phase9Phase10NpcForwarder lu;
            CreateNpcForwarders(parent, bridge, out xu, out zhou, out chen, out lu);

            ReachTutorialCrafting(bridge, xu);
            AssertTrue(adapter.SubmitObservedGameplayResult(70, "Tutorial"), "Adapter should forward tutorial craft completion.");
            AssertStep(bridge, P10CH01FlowStep.Order001Accept, "Adapter tutorial fact");

            AssertTrue(zhou.RequestInteraction(), "Adapter setup ORDER_001 interaction.");
            AssertTrue(bridge.SubmitDialogueCompleted(P10CH01FlowController.NodeOrder001Accept), "Adapter setup ORDER_001 crafting.");
            AssertTrue(adapter.SubmitObservedGameplayResult(88, "Pass"), "Adapter should forward ORDER_001 completion.");
            AssertStep(bridge, P10CH01FlowStep.Order001Reward, "Adapter ORDER_001 reward");
            AssertTrue(adapter.SubmitObservedRewardExit(), "Adapter should forward pending ORDER_001 reward.");
            AssertFalse(adapter.SubmitObservedRewardExit(), "Adapter should not duplicate reward forwarding.");
        }

        private static void ValidateFullHappyPath(Transform parent)
        {
            Phase9Phase10Bridge bridge = CreateBridge(parent, "FullHappyPath");
            Phase9Phase10NpcForwarder xu;
            Phase9Phase10NpcForwarder zhou;
            Phase9Phase10NpcForwarder chen;
            Phase9Phase10NpcForwarder lu;
            CreateNpcForwarders(parent, bridge, out xu, out zhou, out chen, out lu);

            ReachTutorialCrafting(bridge, xu);
            AssertTrue(bridge.SubmitTutorialCraftCompleted(), "Happy path tutorial craft.");

            CompleteOrder(bridge, zhou, P10CH01FlowController.NodeOrder001Accept, P10CH01FlowController.Order001, 80, P10CH01FlowController.NodeOrder001Pass);
            CompleteOrder(bridge, chen, P10CH01FlowController.NodeOrder003Accept, P10CH01FlowController.Order003, 82, P10CH01FlowController.NodeOrder003Pass);
            CompleteOrder(bridge, lu, P10CH01FlowController.NodeOrder004Accept, P10CH01FlowController.Order004, 96, P10CH01FlowController.NodeOrder004Climax);

            AssertStep(bridge, P10CH01FlowStep.ChapterEnding, "TC09 ChapterEnding step after ORDER_004 pass.");
            AssertTrue(bridge.SubmitChapterEndingRequested(), "TC09 Chapter ending request.");
            AssertTrue(bridge.SubmitDialogueCompleted(P10CH01FlowController.NodeChapterEnding), "TC10 Chapter ending dialogue completion.");
            AssertStep(bridge, P10CH01FlowStep.Completed, "TC10 Completed");
        }

        private static void ValidateDuplicateRewardAndEndingGate(Transform parent)
        {
            Phase9Phase10Bridge bridge = CreateBridge(parent, "DuplicateRewardAndEndingGate");
            Phase9Phase10NpcForwarder xu;
            Phase9Phase10NpcForwarder zhou;
            Phase9Phase10NpcForwarder chen;
            Phase9Phase10NpcForwarder lu;
            CreateNpcForwarders(parent, bridge, out xu, out zhou, out chen, out lu);

            AssertTrue(bridge.SubmitGameStarted(), "Ending gate setup.");
            AssertFalse(bridge.SubmitChapterEndingRequested(), "TC09 ChapterEndingRequested must be blocked before ChapterEnding.");

            ReachTutorialCraftingAfterStart(bridge, xu);
            AssertTrue(bridge.SubmitTutorialCraftCompleted(), "Duplicate reward setup tutorial.");
            AssertTrue(zhou.RequestInteraction(), "Duplicate reward setup Zhou.");
            AssertTrue(bridge.SubmitDialogueCompleted(P10CH01FlowController.NodeOrder001Accept), "Duplicate reward setup ORDER_001 crafting.");
            AssertTrue(bridge.SubmitOrderCompleted(P10CH01FlowController.Order001, 80, "Pass"), "Duplicate reward setup ORDER_001 complete.");
            AssertTrue(bridge.SubmitRewardGranted(P10CH01FlowController.Order001), "TC11 first ORDER_001 reward.");
            AssertFalse(bridge.SubmitRewardGranted(P10CH01FlowController.Order001), "TC11 duplicate ORDER_001 reward must be blocked.");
        }

        private static void ValidateNoDirectPhase3Phase6References()
        {
            Type[] types =
            {
                typeof(P10CH01FlowController),
                typeof(P10CH01GameplayFact),
                typeof(Phase9Phase10Bridge),
                typeof(Phase9Phase10NpcForwarder),
                typeof(Phase9Phase10GameplayFactAdapter)
            };

            for (int i = 0; i < types.Length; i++)
            {
                AssertTypeHasNoDirectPhase3Phase6Reference(types[i]);
            }
        }

        private static void AssertTypeHasNoDirectPhase3Phase6Reference(Type type)
        {
            if (ContainsPhase3OrPhase6(type.BaseType))
            {
                throw new InvalidOperationException(type.Name + " has a Phase3/Phase6 base type.");
            }

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            for (int i = 0; i < fields.Length; i++)
            {
                if (ContainsPhase3OrPhase6(fields[i].FieldType))
                {
                    throw new InvalidOperationException(type.Name + " field directly references Phase3/Phase6: " + fields[i].Name);
                }
            }

            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            for (int i = 0; i < methods.Length; i++)
            {
                if (ContainsPhase3OrPhase6(methods[i].ReturnType))
                {
                    throw new InvalidOperationException(type.Name + " method return directly references Phase3/Phase6: " + methods[i].Name);
                }

                ParameterInfo[] parameters = methods[i].GetParameters();
                for (int p = 0; p < parameters.Length; p++)
                {
                    if (ContainsPhase3OrPhase6(parameters[p].ParameterType))
                    {
                        throw new InvalidOperationException(type.Name + " method parameter directly references Phase3/Phase6: " + methods[i].Name);
                    }
                }
            }
        }

        private static bool ContainsPhase3OrPhase6(Type type)
        {
            string fullName = type != null ? type.FullName : string.Empty;
            return fullName.Contains("Phase3") || fullName.Contains("Phase6");
        }

        private static Phase9Phase10Bridge CreateBridge(Transform parent, string name)
        {
            GameObject host = CreateChild(parent, "Bridge_" + name);
            Phase9Phase10Bridge bridge = host.AddComponent<Phase9Phase10Bridge>();
            DisableRuntimeBootstrap(bridge);
            return bridge;
        }

        private static void DisableRuntimeBootstrap(Phase9Phase10Bridge bridge)
        {
            SerializedObject serializedBridge = new SerializedObject(bridge);
            serializedBridge.FindProperty("autoStartChapterOnSceneReady").boolValue = false;
            serializedBridge.FindProperty("loadOverlaySceneAdditively").boolValue = false;
            serializedBridge.FindProperty("autoInstallGameplayFactAdapter").boolValue = false;
            serializedBridge.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateNpcForwarders(
            Transform parent,
            Phase9Phase10Bridge bridge,
            out Phase9Phase10NpcForwarder xu,
            out Phase9Phase10NpcForwarder zhou,
            out Phase9Phase10NpcForwarder chen,
            out Phase9Phase10NpcForwarder lu)
        {
            xu = CreateNpc(parent, "Xu", bridge, P10CH01FlowController.NpcXuLaoBo);
            zhou = CreateNpc(parent, "Zhou", bridge, P10CH01FlowController.NpcZhouZhangGui);
            chen = CreateNpc(parent, "Chen", bridge, P10CH01FlowController.NpcChenShuYuan);
            lu = CreateNpc(parent, "Lu", bridge, P10CH01FlowController.NpcLuKe);
        }

        private static Phase9Phase10NpcForwarder CreateNpc(Transform parent, string name, Phase9Phase10Bridge bridge, string npcId)
        {
            GameObject npc = CreateChild(parent, "NPC_" + name + "_" + Guid.NewGuid().ToString("N"));
            Phase9Phase10NpcForwarder forwarder = npc.AddComponent<Phase9Phase10NpcForwarder>();
            forwarder.Bind(bridge, npcId);
            return forwarder;
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            GameObject child = new GameObject(name);
            child.hideFlags = HideFlags.HideAndDontSave;
            child.transform.SetParent(parent, false);
            return child;
        }

        private static void ReachTutorialCrafting(Phase9Phase10Bridge bridge, Phase9Phase10NpcForwarder xu)
        {
            AssertTrue(bridge.SubmitGameStarted(), "Setup GameStarted.");
            ReachTutorialCraftingAfterStart(bridge, xu);
        }

        private static void ReachTutorialCraftingAfterStart(Phase9Phase10Bridge bridge, Phase9Phase10NpcForwarder xu)
        {
            AssertTrue(bridge.SubmitDialogueCompleted(P10CH01FlowController.NodePrologue), "Setup prologue completion.");
            AssertTrue(xu.RequestInteraction(), "Setup Xu interaction.");
            AssertTrue(bridge.SubmitDialogueCompleted(P10CH01FlowController.NodeTutorial), "Setup tutorial dialogue completion.");
            AssertStep(bridge, P10CH01FlowStep.TutorialCrafting, "Setup tutorial crafting.");
        }

        private static void CompleteOrder(
            Phase9Phase10Bridge bridge,
            Phase9Phase10NpcForwarder npc,
            string acceptNodeId,
            string orderId,
            int score,
            string passNodeId)
        {
            AssertTrue(npc.RequestInteraction(), "Order interaction " + orderId);
            AssertTrue(bridge.SubmitDialogueCompleted(acceptNodeId), "Order accept dialogue " + orderId);
            AssertTrue(bridge.SubmitOrderCompleted(orderId, score, "Pass"), "Order completion " + orderId);
            AssertTrue(bridge.SubmitRewardGranted(orderId), "Order reward " + orderId);
            AssertTrue(bridge.SubmitDialogueCompleted(passNodeId), "Order pass dialogue " + orderId);
        }

        private static void AssertStep(Phase9Phase10Bridge bridge, P10CH01FlowStep expected, string label)
        {
            if (bridge.CurrentStep != expected)
            {
                throw new InvalidOperationException(label + ": expected " + expected + " but got " + bridge.CurrentStep);
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
    }
}
