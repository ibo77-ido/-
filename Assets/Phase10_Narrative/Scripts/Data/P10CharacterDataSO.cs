using UnityEngine;

namespace Phase10_Narrative
{
    [CreateAssetMenu(
        fileName = "P10CharacterData",
        menuName = "Phase10 Narrative/Character Data")]
    public sealed class P10CharacterDataSO : ScriptableObject
    {
        public string CharacterId;
        public string DisplayName;
        public string Role;
        public string RelationshipSummary;
        public string DefaultNodeId;
    }
}
