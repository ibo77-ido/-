using UnityEngine;

namespace Phase10_Narrative
{
    [CreateAssetMenu(
        fileName = "P10NarrativePropData",
        menuName = "Phase10 Narrative/Narrative Prop Data")]
    public sealed class P10NarrativePropDataSO : ScriptableObject
    {
        public string PropId;
        public string DisplayName;
        public string Description;
        public string StoryHint;
        public string RelatedNodeId;
        public string OptionalInspectText;
    }
}
