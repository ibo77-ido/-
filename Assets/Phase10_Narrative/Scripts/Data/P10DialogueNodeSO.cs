using System;
using System.Collections.Generic;
using UnityEngine;

namespace Phase10_Narrative
{
    public enum P10DialogueNodeKind
    {
        Unknown,
        Story,
        Tutorial,
        OrderAccept,
        OrderPass,
        OrderFail,
        ChapterEnding,
        OrderClimax
    }

    [Serializable]
    public sealed class P10DialogueLine
    {
        public string SpeakerCharacterId;
        public string DialogueText;
    }

    [CreateAssetMenu(
        fileName = "P10DialogueNode",
        menuName = "Phase10 Narrative/Dialogue Node")]
    public sealed class P10DialogueNodeSO : ScriptableObject
    {
        public string NodeId;
        public P10NarrativeState ChapterState;
        public P10DialogueNodeKind NodeKind;
        public string OrderId;
        public string SpeakerCharacterId;
        public string TargetId;
        public string DialogueText;
        public List<P10DialogueLine> DialogueLines = new List<P10DialogueLine>();
        public List<string> NextNodeIds = new List<string>();
        public List<string> ReferencedPropIds = new List<string>();
        public List<P10NarrativeCondition> Conditions = new List<P10NarrativeCondition>();
    }
}
