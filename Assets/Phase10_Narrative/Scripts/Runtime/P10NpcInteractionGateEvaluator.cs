using System;
using UnityEngine;

namespace Phase10_Narrative
{
    public static class P10NpcInteractionGateEvaluator
    {
        public const float DefaultInteractionDistance = 2.5f;

        public const string NpcXuLaoBo = "XuLaoBo";
        public const string NpcZhouZhangGui = "ZhouZhangGui";
        public const string NpcChenShuYuan = "ChenShuYuan";
        public const string NpcLuKe = "LuKe";

        public static P10NpcInteractionGateResult Evaluate(
            Vector3 playerPosition,
            Vector3 interactPointPosition,
            P10NarrativeState currentState,
            string npcId,
            float interactionDistance)
        {
            if (!IsKnownNpc(npcId))
            {
                return P10NpcInteractionGateResult.InvalidNpc;
            }

            float resolvedInteractionDistance = Mathf.Max(0f, interactionDistance);
            if ((playerPosition - interactPointPosition).sqrMagnitude > resolvedInteractionDistance * resolvedInteractionDistance)
            {
                return P10NpcInteractionGateResult.TooFar;
            }

            return IsCurrentMainlineNpc(currentState, npcId)
                ? P10NpcInteractionGateResult.AllowDialogue
                : P10NpcInteractionGateResult.QuestNotAvailableYet;
        }

        public static bool IsCurrentMainlineNpc(P10NarrativeState currentState, string npcId)
        {
            switch (currentState)
            {
                case P10NarrativeState.Prologue:
                case P10NarrativeState.Ending:
                    return string.Equals(npcId, NpcXuLaoBo, StringComparison.Ordinal);
                case P10NarrativeState.Order001:
                    return string.Equals(npcId, NpcZhouZhangGui, StringComparison.Ordinal);
                case P10NarrativeState.Order003:
                    return string.Equals(npcId, NpcChenShuYuan, StringComparison.Ordinal);
                case P10NarrativeState.Order004:
                    return string.Equals(npcId, NpcLuKe, StringComparison.Ordinal);
                default:
                    return false;
            }
        }

        public static bool IsKnownNpc(string npcId)
        {
            return string.Equals(npcId, NpcXuLaoBo, StringComparison.Ordinal)
                || string.Equals(npcId, NpcZhouZhangGui, StringComparison.Ordinal)
                || string.Equals(npcId, NpcChenShuYuan, StringComparison.Ordinal)
                || string.Equals(npcId, NpcLuKe, StringComparison.Ordinal);
        }
    }
}