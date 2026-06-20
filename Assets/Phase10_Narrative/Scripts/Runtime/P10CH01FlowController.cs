using System.Collections.Generic;
using UnityEngine;

namespace Phase10_Narrative
{
    public sealed class P10CH01FlowController : MonoBehaviour
    {
        public const string ChapterId = "P10_CH01";
        public const string NpcXuLaoBo = "P10_CH01_NPC_001_XuLaoBo_Placeholder";
        public const string NpcZhouZhangGui = "P10_CH01_NPC_002_ZhouZhangGui_Placeholder";
        public const string NpcChenShuYuan = "P10_CH01_NPC_003_ChenShuYuan_Placeholder";
        public const string NpcLuKe = "P10_CH01_NPC_004_LuKe_Placeholder";

        public const string NodePrologue = "P10_CH01_NODE_PROLOGUE_01";
        public const string NodeTutorial = "P10_CH01_NODE_TUTORIAL_01";
        public const string NodeOrder001Accept = "P10_CH01_NODE_ORDER_001_ACCEPT";
        public const string NodeOrder001Pass = "P10_CH01_NODE_ORDER_001_PASS";
        public const string NodeOrder003Accept = "P10_CH01_NODE_ORDER_003_ACCEPT";
        public const string NodeOrder003Pass = "P10_CH01_NODE_ORDER_003_PASS";
        public const string NodeOrder004Accept = "P10_CH01_NODE_ORDER_004_ACCEPT";
        public const string NodeOrder004PassNormal = "P10_CH01_NODE_ORDER_004_PASS_NORMAL";
        public const string NodeOrder004Climax = "P10_CH01_NODE_ORDER_004_CLIMAX";
        public const string NodeChapterEnding = "P10_CH01_NODE_CHAPTER_ENDING";

        public const string Order001 = "ORDER_001";
        public const string Order003 = "ORDER_003";
        public const string Order004 = "ORDER_004";

        [SerializeField] private P10NarrativeManager narrativeManager;
        [SerializeField] private P10CH01FlowStep currentStep = P10CH01FlowStep.None;

        private readonly HashSet<string> rewardedOrderIds = new HashSet<string>();
        private readonly List<string> rejectedRequests = new List<string>();

        public P10CH01FlowStep CurrentStep
        {
            get { return currentStep; }
        }

        public IReadOnlyList<string> RejectedRequests
        {
            get { return rejectedRequests; }
        }

        public void BindNarrativeManager(P10NarrativeManager manager)
        {
            narrativeManager = manager;
        }

        private void Awake()
        {
            if (Application.isPlaying)
            {
                ResetFlow();
            }

            if (narrativeManager == null)
            {
                narrativeManager = GetComponent<P10NarrativeManager>();
            }

            if (narrativeManager == null)
            {
                narrativeManager = FindObjectOfType<P10NarrativeManager>();
            }
        }

        public bool RequestChapterStart()
        {
            if (currentStep != P10CH01FlowStep.None)
            {
                return Reject("ChapterStart", "chapter already started");
            }

            currentStep = P10CH01FlowStep.OpeningNarration;
            return ShowNode(NodePrologue);
        }

        public bool RequestNpcInteraction(string npcId)
        {
            switch (currentStep)
            {
                case P10CH01FlowStep.XuTutorialDialogue:
                    if (npcId == NpcXuLaoBo)
                    {
                        return ShowNode(NodeTutorial);
                    }
                    break;
                case P10CH01FlowStep.Order001Accept:
                    if (npcId == NpcZhouZhangGui)
                    {
                        return ShowNode(NodeOrder001Accept);
                    }
                    break;
                case P10CH01FlowStep.Order003Accept:
                    if (npcId == NpcChenShuYuan)
                    {
                        return ShowNode(NodeOrder003Accept);
                    }
                    break;
                case P10CH01FlowStep.Order004Accept:
                    if (npcId == NpcLuKe)
                    {
                        return ShowNode(NodeOrder004Accept);
                    }
                    break;
            }

            return Reject("NpcInteraction:" + npcId, "blocked at " + currentStep);
        }

        public bool ApplyGameplayFact(P10CH01GameplayFact fact)
        {
            if (fact == null)
            {
                return Reject("GameplayFact:null", "null fact");
            }

            switch (fact.FactType)
            {
                case P10CH01GameplayFactType.GameStarted:
                    return RequestChapterStart();
                case P10CH01GameplayFactType.DialogueCompleted:
                    return NotifyDialogueCompleted(fact.NodeId);
                case P10CH01GameplayFactType.TutorialCraftCompleted:
                    return HandleTutorialCraftCompleted();
                case P10CH01GameplayFactType.OrderAccepted:
                    return HandleOrderAccepted(fact.OrderId);
                case P10CH01GameplayFactType.OrderSubmitted:
                    return HandleOrderSubmitted(fact.OrderId);
                case P10CH01GameplayFactType.OrderCompleted:
                    return HandleOrderCompleted(fact.OrderId, fact.Score);
                case P10CH01GameplayFactType.RewardGranted:
                    return HandleRewardGranted(fact.OrderId);
                case P10CH01GameplayFactType.ChapterEndingRequested:
                    return HandleChapterEndingRequested();
                default:
                    return Reject("GameplayFact:" + fact.FactType, "unsupported fact");
            }
        }

        public bool NotifyDialogueCompleted(string nodeId)
        {
            switch (nodeId)
            {
                case NodePrologue:
                    return MoveStep(P10CH01FlowStep.OpeningNarration, P10CH01FlowStep.XuTutorialDialogue);
                case NodeTutorial:
                    return MoveStep(P10CH01FlowStep.XuTutorialDialogue, P10CH01FlowStep.TutorialCrafting);
                case NodeOrder001Accept:
                    return MoveStep(P10CH01FlowStep.Order001Accept, P10CH01FlowStep.Order001Crafting);
                case NodeOrder001Pass:
                    return MoveStep(P10CH01FlowStep.Order001Reward, P10CH01FlowStep.Order003Accept);
                case NodeOrder003Accept:
                    return MoveStep(P10CH01FlowStep.Order003Accept, P10CH01FlowStep.Order003Crafting);
                case NodeOrder003Pass:
                    return MoveStep(P10CH01FlowStep.Order003Reward, P10CH01FlowStep.Order004Accept);
                case NodeOrder004Accept:
                    return MoveStep(P10CH01FlowStep.Order004Accept, P10CH01FlowStep.Order004Crafting);
                case NodeOrder004PassNormal:
                case NodeOrder004Climax:
                    return MoveStep(P10CH01FlowStep.Order004Reward, P10CH01FlowStep.ChapterEnding);
                case NodeChapterEnding:
                    return MoveStep(P10CH01FlowStep.ChapterEnding, P10CH01FlowStep.Completed);
                default:
                    return Reject("DialogueCompleted:" + nodeId, "unknown node");
            }
        }

        public void ResetFlow()
        {
            currentStep = P10CH01FlowStep.None;
            rewardedOrderIds.Clear();
            rejectedRequests.Clear();
        }

        private bool HandleTutorialCraftCompleted()
        {
            return MoveStep(P10CH01FlowStep.TutorialCrafting, P10CH01FlowStep.Order001Accept);
        }

        private bool HandleOrderAccepted(string orderId)
        {
            if (orderId == Order001 && currentStep == P10CH01FlowStep.Order001Accept)
            {
                return true;
            }

            if (orderId == Order003 && currentStep == P10CH01FlowStep.Order003Accept)
            {
                return true;
            }

            if (orderId == Order004 && currentStep == P10CH01FlowStep.Order004Accept)
            {
                return true;
            }

            return Reject("OrderAccepted:" + orderId, "blocked at " + currentStep);
        }

        private bool HandleOrderSubmitted(string orderId)
        {
            if (IsCurrentCraftingOrder(orderId))
            {
                return true;
            }

            return Reject("OrderSubmitted:" + orderId, "blocked at " + currentStep);
        }

        private bool HandleOrderCompleted(string orderId, int score)
        {
            if (orderId == Order001 && currentStep == P10CH01FlowStep.Order001Crafting)
            {
                currentStep = P10CH01FlowStep.Order001Reward;
                return ShowNode(NodeOrder001Pass);
            }

            if (orderId == Order003 && currentStep == P10CH01FlowStep.Order003Crafting)
            {
                currentStep = P10CH01FlowStep.Order003Reward;
                return ShowNode(NodeOrder003Pass);
            }

            if (orderId == Order004 && currentStep == P10CH01FlowStep.Order004Crafting)
            {
                currentStep = P10CH01FlowStep.Order004Reward;
                return ShowNode(score >= 95 ? NodeOrder004Climax : NodeOrder004PassNormal);
            }

            return Reject("OrderCompleted:" + orderId, "blocked at " + currentStep);
        }

        private bool HandleRewardGranted(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return Reject("RewardGranted", "missing order id");
            }

            if (!IsRewardStepForOrder(orderId))
            {
                return Reject("RewardGranted:" + orderId, "blocked at " + currentStep);
            }

            if (rewardedOrderIds.Contains(orderId))
            {
                return Reject("RewardGranted:" + orderId, "duplicate reward");
            }

            rewardedOrderIds.Add(orderId);
            return true;
        }

        private bool HandleChapterEndingRequested()
        {
            if (currentStep != P10CH01FlowStep.ChapterEnding)
            {
                return Reject("ChapterEndingRequested", "blocked at " + currentStep);
            }

            return ShowNode(NodeChapterEnding);
        }

        private bool MoveStep(P10CH01FlowStep expected, P10CH01FlowStep next)
        {
            if (currentStep != expected)
            {
                return Reject("MoveStep:" + expected + "->" + next, "actual " + currentStep);
            }

            currentStep = next;
            return true;
        }

        private bool IsCurrentCraftingOrder(string orderId)
        {
            return (orderId == Order001 && currentStep == P10CH01FlowStep.Order001Crafting)
                || (orderId == Order003 && currentStep == P10CH01FlowStep.Order003Crafting)
                || (orderId == Order004 && currentStep == P10CH01FlowStep.Order004Crafting);
        }

        private bool IsRewardStepForOrder(string orderId)
        {
            return (orderId == Order001 && currentStep == P10CH01FlowStep.Order001Reward)
                || (orderId == Order003 && currentStep == P10CH01FlowStep.Order003Reward)
                || (orderId == Order004 && currentStep == P10CH01FlowStep.Order004Reward);
        }

        private bool ShowNode(string nodeId)
        {
            if (narrativeManager == null)
            {
                narrativeManager = FindObjectOfType<P10NarrativeManager>();
            }

            if (narrativeManager == null)
            {
                return Reject("ShowNode:" + nodeId, "missing narrative manager");
            }

            return narrativeManager.HandleSceneTrigger(ResolveTriggerId(nodeId), nodeId);
        }

        private static string ResolveTriggerId(string nodeId)
        {
            switch (nodeId)
            {
                case NodePrologue:
                    return "StartPrologue";
                case NodeTutorial:
                    return "Tutorial";
                case NodeOrder001Accept:
                    return "Order001Accept";
                case NodeOrder001Pass:
                    return "Order001Pass";
                case NodeOrder003Accept:
                    return "Order003Accept";
                case NodeOrder003Pass:
                    return "Order003Pass";
                case NodeOrder004Accept:
                    return "Order004Accept";
                case NodeOrder004PassNormal:
                    return "Order004PassNormal";
                case NodeOrder004Climax:
                    return "Order004Climax";
                case NodeChapterEnding:
                    return "ChapterEnding";
                default:
                    return string.Empty;
            }
        }

        private bool Reject(string request, string reason)
        {
            rejectedRequests.Add(request + " :: " + reason);
            return false;
        }
    }
}
