using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Phase10_Narrative
{
    [RequireComponent(typeof(Collider))]
    public sealed class P10NarrativeNpcInteraction : MonoBehaviour
    {
        [SerializeField] private string npcId;
        [SerializeField] private string triggerId;
        [SerializeField] private string targetNodeId;
        [SerializeField] private P10NarrativeSceneBindingHub bindingHub;
        [SerializeField] private bool interactOnce = true;

        private bool hasInteracted;

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(npcId))
            {
                npcId = name;
            }

            EnsureDefaults();
            EnsureBindingHub();
        }

        private void Reset()
        {
            EnsureDefaults();

            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        private void OnMouseDown()
        {
            Interact();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsPlayerSource(other))
            {
                return;
            }

            Interact();
        }

        public void Configure(string resolvedNpcId, string resolvedTriggerId, string resolvedTargetNodeId, P10NarrativeSceneBindingHub resolvedBindingHub)
        {
            npcId = resolvedNpcId ?? string.Empty;
            triggerId = resolvedTriggerId ?? string.Empty;
            targetNodeId = resolvedTargetNodeId ?? string.Empty;
            bindingHub = resolvedBindingHub;
            EnsureDefaults();
        }

        public void Interact()
        {
            if (interactOnce && hasInteracted)
            {
                return;
            }

            EnsureDefaults();
            EnsureBindingHub();

            if (bindingHub == null)
            {
                Debug.LogWarning("[P10NarrativeNpcInteraction] Missing binding hub on " + name);
                return;
            }

            if (string.IsNullOrWhiteSpace(triggerId) || string.IsNullOrWhiteSpace(targetNodeId))
            {
                Debug.LogWarning("[P10NarrativeNpcInteraction] Missing narrative mapping on " + name);
                return;
            }

            if (bindingHub.HandleSceneTrigger(triggerId, targetNodeId))
            {
                hasInteracted = true;
            }
        }

        private void EnsureBindingHub()
        {
            if (bindingHub == null)
            {
                bindingHub = FindObjectOfType<P10NarrativeSceneBindingHub>();
            }
        }

        private void EnsureDefaults()
        {
            if (string.IsNullOrWhiteSpace(triggerId) || string.IsNullOrWhiteSpace(targetNodeId))
            {
                string resolvedTriggerId;
                string resolvedTargetNodeId;
                ResolveMappingFromNpcId(npcId, out resolvedTriggerId, out resolvedTargetNodeId);

                if (string.IsNullOrWhiteSpace(triggerId))
                {
                    triggerId = resolvedTriggerId;
                }

                if (string.IsNullOrWhiteSpace(targetNodeId))
                {
                    targetNodeId = resolvedTargetNodeId;
                }
            }
        }

        private static void ResolveMappingFromNpcId(string resolvedNpcId, out string resolvedTriggerId, out string resolvedTargetNodeId)
        {
            resolvedTriggerId = string.Empty;
            resolvedTargetNodeId = string.Empty;

            switch (resolvedNpcId)
            {
                case "P10_CH01_NPC_001_XuLaoBo_Placeholder":
                    resolvedTriggerId = "StartPrologue";
                    resolvedTargetNodeId = "P10_CH01_NODE_PROLOGUE_01";
                    break;
                case "P10_CH01_NPC_002_ZhouZhangGui_Placeholder":
                    resolvedTriggerId = "Order001Accept";
                    resolvedTargetNodeId = "P10_CH01_NODE_ORDER_001_ACCEPT";
                    break;
                case "P10_CH01_NPC_003_ChenShuYuan_Placeholder":
                    resolvedTriggerId = "Order003Accept";
                    resolvedTargetNodeId = "P10_CH01_NODE_ORDER_003_ACCEPT";
                    break;
                case "P10_CH01_NPC_004_LuKe_Placeholder":
                    resolvedTriggerId = "Order004Accept";
                    resolvedTargetNodeId = "P10_CH01_NODE_ORDER_004_ACCEPT";
                    break;
            }
        }

        private static bool IsPlayerSource(Collider other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.CompareTag("Player"))
            {
                return true;
            }

            return string.Equals(other.name, "PlayerCharacter", StringComparison.Ordinal)
                || (other.transform != null && other.transform.root != null && string.Equals(other.transform.root.name, "PlayerCharacter", StringComparison.Ordinal));
        }
    }

    internal static class P10NarrativeNpcInteractionBinder
    {
        private static readonly string[] NpcNames =
        {
            "P10_CH01_NPC_001_XuLaoBo_Placeholder",
            "P10_CH01_NPC_002_ZhouZhangGui_Placeholder",
            "P10_CH01_NPC_003_ChenShuYuan_Placeholder",
            "P10_CH01_NPC_004_LuKe_Placeholder"
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            BindCurrentScene();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BindCurrentScene();
        }

        private static void BindCurrentScene()
        {
            P10NarrativeSceneBindingHub bindingHub = UnityEngine.Object.FindObjectOfType<P10NarrativeSceneBindingHub>();

            for (int i = 0; i < NpcNames.Length; i++)
            {
                string npcName = NpcNames[i];
                GameObject npcObject = GameObject.Find(npcName);
                if (npcObject == null)
                {
                    continue;
                }

                Collider collider = npcObject.GetComponent<Collider>();
                if (collider == null)
                {
                    collider = npcObject.AddComponent<BoxCollider>();
                }

                collider.isTrigger = true;
                BoxCollider boxCollider = collider as BoxCollider;
                if (boxCollider != null)
                {
                    boxCollider.center = new Vector3(0f, 0.9f, 0f);
                    boxCollider.size = new Vector3(1.0f, 1.8f, 1.0f);
                }

                P10NarrativeNpcInteraction interaction = npcObject.GetComponent<P10NarrativeNpcInteraction>();
                if (interaction == null)
                {
                    interaction = npcObject.AddComponent<P10NarrativeNpcInteraction>();
                }

                interaction.Configure(npcName, null, null, bindingHub);
            }
        }
    }
}
