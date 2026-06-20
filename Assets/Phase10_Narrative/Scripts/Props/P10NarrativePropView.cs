using UnityEngine;

namespace Phase10_Narrative
{
    public sealed class P10NarrativePropView : MonoBehaviour
    {
        [SerializeField] private P10NarrativeManager narrativeManager;
        [SerializeField] private string propId = string.Empty;
        [SerializeField] private string relatedNodeId = string.Empty;

        private void Awake()
        {
            if (narrativeManager == null)
            {
                narrativeManager = FindObjectOfType<P10NarrativeManager>();
            }

            if (string.IsNullOrWhiteSpace(propId))
            {
                propId = gameObject.name;
            }
        }

        public void Inspect()
        {
            if (narrativeManager == null)
            {
                return;
            }

            narrativeManager.PublishEvent(new P10NarrativeEvent(P10NarrativeEventType.NarrativePropInspected)
            {
                Payload = propId,
                NodeId = relatedNodeId
            });
        }
    }
}
