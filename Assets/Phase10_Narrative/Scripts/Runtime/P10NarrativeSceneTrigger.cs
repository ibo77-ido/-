using UnityEngine;

namespace Phase10_Narrative
{
    [RequireComponent(typeof(Collider))]
    public sealed class P10NarrativeSceneTrigger : MonoBehaviour
    {
        [SerializeField] private string triggerId;
        [SerializeField] private string targetNodeId;
        [SerializeField] private P10NarrativeSceneBindingHub bindingHub;
        [SerializeField] private bool triggerOnce = true;

        private bool hasTriggered;

        private void Reset()
        {
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggerOnce && hasTriggered)
            {
                return;
            }

            if (bindingHub == null)
            {
                Debug.LogWarning("[P10NarrativeSceneTrigger] Missing binding hub on " + name);
                return;
            }

            if (string.IsNullOrWhiteSpace(targetNodeId))
            {
                Debug.LogWarning("[P10NarrativeSceneTrigger] Missing target node id on " + name);
                return;
            }

            if (bindingHub.HandleSceneTrigger(triggerId, targetNodeId))
            {
                hasTriggered = true;
            }
        }
    }
}
