using System;
using UnityEditor;
using UnityEngine;

namespace Phase10_Narrative
{
    public static class P10CH01FlowControllerValidator
    {
        public static void RunP10B_01FlowControllerValidation()
        {
            try
            {
                ValidateHappyPath();
                ValidateEarlyNpcBlocks();
                ValidateEarlyOrderBlocks();
                ValidateDuplicateRewardBlocks();
                ValidateChapterEndingGate();
                ValidateNoDirectPhase3Phase6References();
                Debug.Log("P10B-01 CH01 FlowController validation passed.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10B-01 CH01 FlowController validation failed: " + ex.Message);
                EditorApplication.Exit(1);
            }
        }

        private static void ValidateHappyPath()
        {
            P10CH01FlowController flow = CreateFlow();

            AssertTrue(flow.ApplyGameplayFact(Fact(P10CH01GameplayFactType.GameStarted)), "GameStarted");
            AssertStep(flow, P10CH01FlowStep.OpeningNarration);
            AssertTrue(flow.NotifyDialogueCompleted(P10CH01FlowController.NodePrologue), "Prologue complete");
            AssertStep(flow, P10CH01FlowStep.XuTutorialDialogue);
            AssertTrue(flow.RequestNpcInteraction(P10CH01FlowController.NpcXuLaoBo), "Xu interaction");
            AssertTrue(flow.NotifyDialogueCompleted(P10CH01FlowController.NodeTutorial), "Tutorial dialogue complete");
            AssertTrue(flow.ApplyGameplayFact(Fact(P10CH01GameplayFactType.TutorialCraftCompleted)), "Tutorial craft complete");
            AssertStep(flow, P10CH01FlowStep.Order001Accept);
            AssertTrue(flow.RequestNpcInteraction(P10CH01FlowController.NpcZhouZhangGui), "Zhou interaction");
            AssertTrue(flow.NotifyDialogueCompleted(P10CH01FlowController.NodeOrder001Accept), "ORDER_001 accept complete");
            AssertTrue(flow.ApplyGameplayFact(OrderFact(P10CH01GameplayFactType.OrderCompleted, P10CH01FlowController.Order001, 80)), "ORDER_001 complete");
            AssertTrue(flow.ApplyGameplayFact(OrderFact(P10CH01GameplayFactType.RewardGranted, P10CH01FlowController.Order001, 0)), "ORDER_001 reward");
            AssertTrue(flow.NotifyDialogueCompleted(P10CH01FlowController.NodeOrder001Pass), "ORDER_001 pass complete");
            AssertStep(flow, P10CH01FlowStep.Order003Accept);
            AssertTrue(flow.RequestNpcInteraction(P10CH01FlowController.NpcChenShuYuan), "Chen interaction");
            AssertTrue(flow.NotifyDialogueCompleted(P10CH01FlowController.NodeOrder003Accept), "ORDER_003 accept complete");
            AssertTrue(flow.ApplyGameplayFact(OrderFact(P10CH01GameplayFactType.OrderCompleted, P10CH01FlowController.Order003, 82)), "ORDER_003 complete");
            AssertTrue(flow.ApplyGameplayFact(OrderFact(P10CH01GameplayFactType.RewardGranted, P10CH01FlowController.Order003, 0)), "ORDER_003 reward");
            AssertTrue(flow.NotifyDialogueCompleted(P10CH01FlowController.NodeOrder003Pass), "ORDER_003 pass complete");
            AssertStep(flow, P10CH01FlowStep.Order004Accept);
            AssertTrue(flow.RequestNpcInteraction(P10CH01FlowController.NpcLuKe), "Lu interaction");
            AssertTrue(flow.NotifyDialogueCompleted(P10CH01FlowController.NodeOrder004Accept), "ORDER_004 accept complete");
            AssertTrue(flow.ApplyGameplayFact(OrderFact(P10CH01GameplayFactType.OrderCompleted, P10CH01FlowController.Order004, 96)), "ORDER_004 complete");
            AssertTrue(flow.ApplyGameplayFact(OrderFact(P10CH01GameplayFactType.RewardGranted, P10CH01FlowController.Order004, 0)), "ORDER_004 reward");
            AssertTrue(flow.NotifyDialogueCompleted(P10CH01FlowController.NodeOrder004Climax), "ORDER_004 climax complete");
            AssertTrue(flow.ApplyGameplayFact(Fact(P10CH01GameplayFactType.ChapterEndingRequested)), "Chapter ending request");
            AssertTrue(flow.NotifyDialogueCompleted(P10CH01FlowController.NodeChapterEnding), "Chapter ending complete");
            AssertStep(flow, P10CH01FlowStep.Completed);
        }

        private static void ValidateEarlyNpcBlocks()
        {
            P10CH01FlowController flow = CreateFlow();
            AssertTrue(flow.ApplyGameplayFact(Fact(P10CH01GameplayFactType.GameStarted)), "GameStarted");
            AssertFalse(flow.RequestNpcInteraction(P10CH01FlowController.NpcZhouZhangGui), "Zhou early");
            AssertFalse(flow.RequestNpcInteraction(P10CH01FlowController.NpcChenShuYuan), "Chen early");
            AssertFalse(flow.RequestNpcInteraction(P10CH01FlowController.NpcLuKe), "Lu early");
        }

        private static void ValidateEarlyOrderBlocks()
        {
            P10CH01FlowController flow = CreateFlow();
            AssertTrue(flow.ApplyGameplayFact(Fact(P10CH01GameplayFactType.GameStarted)), "GameStarted");
            AssertFalse(flow.ApplyGameplayFact(OrderFact(P10CH01GameplayFactType.OrderCompleted, P10CH01FlowController.Order001, 80)), "ORDER_001 early");
            AssertFalse(flow.ApplyGameplayFact(OrderFact(P10CH01GameplayFactType.OrderCompleted, P10CH01FlowController.Order003, 80)), "ORDER_003 early");
            AssertFalse(flow.ApplyGameplayFact(OrderFact(P10CH01GameplayFactType.OrderCompleted, P10CH01FlowController.Order004, 80)), "ORDER_004 early");
        }

        private static void ValidateDuplicateRewardBlocks()
        {
            P10CH01FlowController flow = CreateFlowToOrder001Reward();
            AssertTrue(flow.ApplyGameplayFact(OrderFact(P10CH01GameplayFactType.RewardGranted, P10CH01FlowController.Order001, 0)), "ORDER_001 first reward");
            AssertFalse(flow.ApplyGameplayFact(OrderFact(P10CH01GameplayFactType.RewardGranted, P10CH01FlowController.Order001, 0)), "ORDER_001 duplicate reward");
        }

        private static void ValidateChapterEndingGate()
        {
            P10CH01FlowController flow = CreateFlow();
            AssertTrue(flow.ApplyGameplayFact(Fact(P10CH01GameplayFactType.GameStarted)), "GameStarted");
            AssertFalse(flow.ApplyGameplayFact(Fact(P10CH01GameplayFactType.ChapterEndingRequested)), "chapter ending early");
        }

        private static void ValidateNoDirectPhase3Phase6References()
        {
            Type[] types =
            {
                typeof(P10CH01FlowController),
                typeof(P10CH01GameplayFact)
            };

            for (int i = 0; i < types.Length; i++)
            {
                string assemblyName = types[i].Assembly.GetName().Name;
                if (assemblyName.Contains("Phase3") || assemblyName.Contains("Phase6"))
                {
                    throw new InvalidOperationException("Unexpected Phase3/Phase6 assembly reference.");
                }
            }
        }

        private static P10CH01FlowController CreateFlow()
        {
            GameObject go = new GameObject("P10CH01FlowControllerValidator_Flow");
            go.hideFlags = HideFlags.HideAndDontSave;
            return go.AddComponent<P10CH01FlowController>();
        }

        private static P10CH01FlowController CreateFlowToOrder001Reward()
        {
            P10CH01FlowController flow = CreateFlow();
            flow.ApplyGameplayFact(Fact(P10CH01GameplayFactType.GameStarted));
            flow.NotifyDialogueCompleted(P10CH01FlowController.NodePrologue);
            flow.RequestNpcInteraction(P10CH01FlowController.NpcXuLaoBo);
            flow.NotifyDialogueCompleted(P10CH01FlowController.NodeTutorial);
            flow.ApplyGameplayFact(Fact(P10CH01GameplayFactType.TutorialCraftCompleted));
            flow.RequestNpcInteraction(P10CH01FlowController.NpcZhouZhangGui);
            flow.NotifyDialogueCompleted(P10CH01FlowController.NodeOrder001Accept);
            flow.ApplyGameplayFact(OrderFact(P10CH01GameplayFactType.OrderCompleted, P10CH01FlowController.Order001, 80));
            return flow;
        }

        private static P10CH01GameplayFact Fact(P10CH01GameplayFactType type)
        {
            return new P10CH01GameplayFact
            {
                FactType = type,
                ChapterId = P10CH01FlowController.ChapterId
            };
        }

        private static P10CH01GameplayFact OrderFact(P10CH01GameplayFactType type, string orderId, int score)
        {
            P10CH01GameplayFact fact = Fact(type);
            fact.OrderId = orderId;
            fact.Score = score;
            return fact;
        }

        private static void AssertStep(P10CH01FlowController flow, P10CH01FlowStep expected)
        {
            if (flow.CurrentStep != expected)
            {
                throw new InvalidOperationException("Expected step " + expected + " but got " + flow.CurrentStep);
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
