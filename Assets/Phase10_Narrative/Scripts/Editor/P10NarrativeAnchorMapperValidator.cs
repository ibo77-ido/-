using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Phase10_Narrative
{
    public static class P10NarrativeAnchorMapperValidator
    {
        private static readonly string[] RequiredAnchorIds =
        {
            "P10_CH01_Anchor_Order",
            "P10_CH01_Anchor_Wheel",
            "P10_CH01_Anchor_Glaze",
            "P10_CH01_Anchor_Kiln",
            "P10_CH01_Anchor_CourtyardCenter",
            "P10_CH01_Anchor_Gate"
        };

        public static void RunP10B_04AnchorMapperValidation()
        {
            try
            {
                ValidateRequiredAnchorIds();
                ValidateStoreReturnAndApply();
                ValidateRejectsInvalidInput();
                Debug.Log("P10B-04 Anchor Mapping validation passed. Placeholder overlay anchors can be aligned through neutral anchor id and Vector3 mappings.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10B-04 Anchor Mapping validation failed: " + ex.Message);
                EditorApplication.Exit(1);
            }
        }

        private static void ValidateRequiredAnchorIds()
        {
            string sceneText = System.IO.File.ReadAllText("Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity");

            foreach (string anchorId in RequiredAnchorIds)
            {
                if (!sceneText.Contains("m_Name: " + anchorId))
                {
                    throw new InvalidOperationException("Missing overlay anchor: " + anchorId);
                }
            }
        }

        private static void ValidateStoreReturnAndApply()
        {
            P10NarrativeAnchorMapper mapper = new P10NarrativeAnchorMapper();
            List<P10NarrativeAnchorMapping> simulatedRuntimeMappings = new List<P10NarrativeAnchorMapping>();

            for (int i = 0; i < RequiredAnchorIds.Length; i++)
            {
                simulatedRuntimeMappings.Add(new P10NarrativeAnchorMapping(
                    RequiredAnchorIds[i],
                    new Vector3(i + 1.5f, 0.25f * i, -2.0f - i)));
            }

            mapper.RegisterAnchorPositions(simulatedRuntimeMappings);

            if (mapper.AnchorPositions.Count != RequiredAnchorIds.Length)
            {
                throw new InvalidOperationException("Anchor mapping count mismatch.");
            }

            for (int i = 0; i < simulatedRuntimeMappings.Count; i++)
            {
                P10NarrativeAnchorMapping mapping = simulatedRuntimeMappings[i];

                if (!mapper.TryGetAnchorPosition(mapping.AnchorId, out Vector3 storedPosition))
                {
                    throw new InvalidOperationException("Failed to read anchor position: " + mapping.AnchorId);
                }

                if (storedPosition != mapping.Position)
                {
                    throw new InvalidOperationException("Stored position mismatch: " + mapping.AnchorId);
                }

                GameObject validationAnchor = new GameObject(mapping.AnchorId + "_ValidationTransform");
                try
                {
                    validationAnchor.transform.position = Vector3.zero;

                    if (!mapper.ApplyPositionToTransform(mapping.AnchorId, validationAnchor.transform))
                    {
                        throw new InvalidOperationException("Failed to apply anchor position: " + mapping.AnchorId);
                    }

                    if (validationAnchor.transform.position != mapping.Position)
                    {
                        throw new InvalidOperationException("Applied position mismatch: " + mapping.AnchorId);
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(validationAnchor);
                }
            }

            mapper.Clear();

            if (mapper.AnchorPositions.Count != 0)
            {
                throw new InvalidOperationException("Mapper did not clear anchor positions.");
            }
        }

        private static void ValidateRejectsInvalidInput()
        {
            P10NarrativeAnchorMapper mapper = new P10NarrativeAnchorMapper();
            mapper.RegisterAnchorPosition(null, Vector3.one);
            mapper.RegisterAnchorPosition(string.Empty, Vector3.one);
            mapper.RegisterAnchorPositions(null);
            mapper.RegisterAnchorPositions(new P10NarrativeAnchorMapping[] { null });

            if (mapper.AnchorPositions.Count != 0)
            {
                throw new InvalidOperationException("Invalid anchor input was recorded.");
            }

            if (mapper.TryGetAnchorPosition(null, out _))
            {
                throw new InvalidOperationException("Null anchor id was read as valid.");
            }

            GameObject validationAnchor = new GameObject("P10B_04_InvalidAnchorValidationTransform");
            try
            {
                if (mapper.ApplyPositionToTransform("P10_CH01_Anchor_Order", validationAnchor.transform))
                {
                    throw new InvalidOperationException("Apply succeeded for unmapped anchor.");
                }

                mapper.RegisterAnchorPosition("P10_CH01_Anchor_Order", Vector3.one);

                if (mapper.ApplyPositionToTransform("P10_CH01_Anchor_Order", null))
                {
                    throw new InvalidOperationException("Apply succeeded with null target.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(validationAnchor);
            }
        }
    }
}
