using System;

namespace Phase10_Narrative
{
    [Serializable]
    public sealed class P10NarrativeCondition
    {
        public string ConditionKey;
        public bool ExpectedValue;
        public string RequiredNodeId;
        public string Notes;
    }
}
