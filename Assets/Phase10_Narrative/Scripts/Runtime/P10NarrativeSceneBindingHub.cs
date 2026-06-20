using UnityEngine;

namespace Phase10_Narrative
{
    public sealed class P10NarrativeSceneBindingHub : MonoBehaviour
    {
        [SerializeField] private P10NarrativeManager manager;

        public bool HandleSceneTrigger(string triggerId, string targetNodeId)
        {
            if (manager == null)
            {
                Debug.LogWarning("[P10NarrativeSceneBindingHub] Missing manager on " + name);
                return false;
            }

            return manager.HandleSceneTrigger(triggerId, targetNodeId);
        }
    }
}
