using UnityEngine;

namespace Phase10_Narrative
{
    [DisallowMultipleComponent]
    public sealed class Phase9Phase10NpcForwarder : MonoBehaviour
    {
        [SerializeField] private Phase9Phase10Bridge bridge;
        [SerializeField] private string npcId = string.Empty;

        public string NpcId
        {
            get { return npcId; }
        }

        public void Bind(Phase9Phase10Bridge owner, string phase10NpcId)
        {
            bridge = owner;
            npcId = phase10NpcId ?? string.Empty;
        }

        public bool RequestInteraction()
        {
            if (bridge == null)
            {
                bridge = FindObjectOfType<Phase9Phase10Bridge>();
            }

            return bridge != null && bridge.RequestNpcInteraction(npcId);
        }

        private void OnMouseDown()
        {
            RequestInteraction();
        }
    }
}
