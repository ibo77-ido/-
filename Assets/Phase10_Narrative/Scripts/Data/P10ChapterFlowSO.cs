using System;
using System.Collections.Generic;
using UnityEngine;

namespace Phase10_Narrative
{
    [Serializable]
    public sealed class P10OrderNarrativeMapping
    {
        public string OrderId;
        public string AcceptNodeId;
        public string PassNodeId;
        public string FailNodeId;
        public string NormalEndNodeId;
        public string ClimaxNodeId;
    }

    [CreateAssetMenu(
        fileName = "P10ChapterFlow",
        menuName = "Phase10 Narrative/Chapter Flow")]
    public sealed class P10ChapterFlowSO : ScriptableObject
    {
        public string ChapterId;
        public string SourceDocument;
        public P10NarrativeState InitialState;
        public List<P10NarrativeState> StateFlow = new List<P10NarrativeState>();
        public List<string> NodeIds = new List<string>();
        public List<string> CharacterIds = new List<string>();
        public List<string> PropIds = new List<string>();
        public List<P10OrderNarrativeMapping> OrderMappings = new List<P10OrderNarrativeMapping>();
    }
}
