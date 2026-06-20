using System.Collections.Generic;
using UnityEngine;

namespace Phase10_Narrative
{
    public sealed class P10NarrativeAnchorMapper
    {
        private readonly Dictionary<string, Vector3> anchorPositions = new Dictionary<string, Vector3>();

        public IReadOnlyDictionary<string, Vector3> AnchorPositions
        {
            get { return anchorPositions; }
        }

        public void RegisterAnchorPosition(string anchorId, Vector3 position)
        {
            if (string.IsNullOrWhiteSpace(anchorId))
            {
                return;
            }

            anchorPositions[anchorId] = position;
        }

        public void RegisterAnchorPositions(IEnumerable<P10NarrativeAnchorMapping> mappings)
        {
            if (mappings == null)
            {
                return;
            }

            foreach (P10NarrativeAnchorMapping mapping in mappings)
            {
                if (mapping == null)
                {
                    continue;
                }

                RegisterAnchorPosition(mapping.AnchorId, mapping.Position);
            }
        }

        public bool TryGetAnchorPosition(string anchorId, out Vector3 position)
        {
            if (string.IsNullOrWhiteSpace(anchorId))
            {
                position = Vector3.zero;
                return false;
            }

            return anchorPositions.TryGetValue(anchorId, out position);
        }

        public bool ApplyPositionToTransform(string anchorId, Transform target)
        {
            if (target == null || !TryGetAnchorPosition(anchorId, out Vector3 position))
            {
                return false;
            }

            target.position = position;
            return true;
        }

        public void Clear()
        {
            anchorPositions.Clear();
        }
    }
}
