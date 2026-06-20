using System;
using UnityEngine;

namespace Phase10_Narrative
{
    [DisallowMultipleComponent]
    public sealed class P10NarrativeSceneTriggerGroup : MonoBehaviour
    {
        [Serializable]
        public sealed class Entry
        {
            [SerializeField] private Collider triggerCollider;
            [SerializeField] private string triggerId;
            [SerializeField] private string targetNodeId;
            [SerializeField] private P10NarrativeSceneBindingHub bindingHub;
            [SerializeField] private bool triggerOnce = true;

            private bool hasTriggered;

            public Collider TriggerCollider
            {
                get { return triggerCollider; }
            }

            public string TriggerId
            {
                get { return triggerId; }
            }

            public string TargetNodeId
            {
                get { return targetNodeId; }
            }

            public bool TriggerOnce
            {
                get { return triggerOnce; }
            }

            public bool HasTriggered
            {
                get { return hasTriggered; }
            }

            public void Configure(Collider resolvedCollider, string resolvedTriggerId, string resolvedTargetNodeId, P10NarrativeSceneBindingHub resolvedBindingHub, bool resolvedTriggerOnce)
            {
                triggerCollider = resolvedCollider;
                triggerId = resolvedTriggerId ?? string.Empty;
                targetNodeId = resolvedTargetNodeId ?? string.Empty;
                bindingHub = resolvedBindingHub;
                triggerOnce = resolvedTriggerOnce;
                hasTriggered = false;
            }

            public bool TryHandle(GameObject owner)
            {
                if (triggerOnce && hasTriggered)
                {
                    return false;
                }

                if (bindingHub == null)
                {
                    Debug.LogWarning("[P10NarrativeSceneTriggerGroup] Missing binding hub on " + GetOwnerName(owner));
                    return false;
                }

                if (string.IsNullOrWhiteSpace(targetNodeId))
                {
                    Debug.LogWarning("[P10NarrativeSceneTriggerGroup] Missing target node id on " + GetOwnerName(owner));
                    return false;
                }

                if (bindingHub.HandleSceneTrigger(triggerId, targetNodeId))
                {
                    hasTriggered = true;
                    return true;
                }

                return false;
            }

            private static string GetOwnerName(GameObject owner)
            {
                return owner != null ? owner.name : "<unknown>";
            }
        }

        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        public int EntryCount
        {
            get { return entries != null ? entries.Length : 0; }
        }

        public Entry GetEntry(int index)
        {
            if (entries == null || index < 0 || index >= entries.Length)
            {
                return null;
            }

            return entries[index];
        }

        public void Configure(Entry[] resolvedEntries)
        {
            entries = resolvedEntries ?? Array.Empty<Entry>();
            EnsureChildCollidersAreTriggers();
        }

        public bool TryHandleTriggerForCollider(Collider triggerCollider)
        {
            int index = FindEntryIndex(triggerCollider);
            if (index < 0)
            {
                return false;
            }

            return entries[index].TryHandle(gameObject);
        }

        private void Reset()
        {
            EnsureRigidbody();
            EnsureChildCollidersAreTriggers();
        }

        private void Awake()
        {
            EnsureRigidbody();
            EnsureChildCollidersAreTriggers();
        }

        private void OnTriggerEnter(Collider other)
        {
            int index = ResolveEntryIndexForOtherCollider(other);
            if (index < 0)
            {
                return;
            }

            entries[index].TryHandle(gameObject);
        }

        private int FindEntryIndex(Collider triggerCollider)
        {
            if (triggerCollider == null || entries == null)
            {
                return -1;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                Entry entry = entries[i];
                if (entry != null && entry.TriggerCollider == triggerCollider)
                {
                    return i;
                }
            }

            return -1;
        }

        private int ResolveEntryIndexForOtherCollider(Collider other)
        {
            if (other == null || entries == null || entries.Length == 0)
            {
                return -1;
            }

            int bestIndex = -1;
            float bestDistance = float.MaxValue;
            Bounds otherBounds = other.bounds;

            for (int i = 0; i < entries.Length; i++)
            {
                Entry entry = entries[i];
                Collider triggerCollider = entry != null ? entry.TriggerCollider : null;
                if (triggerCollider == null || !triggerCollider.enabled || !triggerCollider.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Bounds triggerBounds = triggerCollider.bounds;
                if (!triggerBounds.Intersects(otherBounds))
                {
                    continue;
                }

                float distance = (triggerBounds.center - otherBounds.center).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private void EnsureRigidbody()
        {
            Rigidbody body = GetComponent<Rigidbody>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody>();
            }

            body.isKinematic = true;
            body.useGravity = false;
        }

        private void EnsureChildCollidersAreTriggers()
        {
            if (entries == null)
            {
                return;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                Entry entry = entries[i];
                Collider triggerCollider = entry != null ? entry.TriggerCollider : null;
                if (triggerCollider != null)
                {
                    triggerCollider.isTrigger = true;
                }
            }
        }
    }
}
