using UnityEngine;

namespace Phase10_Narrative
{
    public sealed class P10NarrativeTrigger : MonoBehaviour
    {
        [SerializeField] private P10NarrativeManager narrativeManager;
        [SerializeField] private P10NarrativeState targetState = P10NarrativeState.None;
        [SerializeField] private string nodeId = string.Empty;

        private void Awake()
        {
            if (narrativeManager == null)
            {
                narrativeManager = FindObjectOfType<P10NarrativeManager>();
            }
        }

        public void SendNarrativeStateEntered()
        {
            if (narrativeManager == null)
            {
                return;
            }

            narrativeManager.PublishEvent(new P10NarrativeEvent(P10NarrativeEventType.NarrativeStateEntered)
            {
                TargetState = targetState,
                NodeId = nodeId
            });
        }

        private void OnTriggerEnter(Collider other)
        {
            SendNarrativeStateEntered();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            SendNarrativeStateEntered();
        }
    }
}
