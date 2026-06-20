using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Phase10_Narrative
{
    public static class P10E06TriggerGroupAggregationValidator
    {
        private const string ScenePath = "Assets/Phase6/Scenes/Workshop_TestScene.unity";
        private const string TriggerRootName = "P10_CH01_Triggers";
        private const string PassMessage = "P10E-06 Trigger Group Aggregation validation passed.";

        private static readonly TriggerSpec[] ExpectedRoutedSpecs =
        {
            new TriggerSpec("P10_CH01_TRIGGER_StartPrologue", "StartPrologue", "P10_CH01_NODE_PROLOGUE_01"),
            new TriggerSpec("P10_CH01_TRIGGER_Tutorial", "Tutorial", "P10_CH01_NODE_TUTORIAL_01"),
            new TriggerSpec("P10_CH01_TRIGGER_Order001Accept", "Order001Accept", "P10_CH01_NODE_ORDER_001_ACCEPT"),
            new TriggerSpec("P10_CH01_TRIGGER_Order001Pass", "Order001Pass", "P10_CH01_NODE_ORDER_001_PASS"),
            new TriggerSpec("P10_CH01_TRIGGER_Order003Accept", "Order003Accept", "P10_CH01_NODE_ORDER_003_ACCEPT"),
            new TriggerSpec("P10_CH01_TRIGGER_Order003Pass", "Order003Pass", "P10_CH01_NODE_ORDER_003_PASS"),
            new TriggerSpec("P10_CH01_TRIGGER_Order004Accept", "Order004Accept", "P10_CH01_NODE_ORDER_004_ACCEPT"),
            new TriggerSpec("P10_CH01_TRIGGER_Order004PassNormal", "Order004PassNormal", "P10_CH01_NODE_ORDER_004_PASS_NORMAL"),
            new TriggerSpec("P10_CH01_TRIGGER_ChapterEnding", "ChapterEnding", "P10_CH01_NODE_CHAPTER_ENDING")
        };

        [MenuItem("Phase10/P10E/Run Trigger Group Aggregation Migration")]
        public static void RunP10E06TriggerGroupAggregationMigrationAndValidation()
        {
            try
            {
                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                P10NarrativeSceneTriggerGroup group = MigrateOpenScene();
                ValidateSceneAggregation(group);
                ValidateRuntimeRouting();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log(PassMessage);
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10E-06 Trigger Group Aggregation validation failed: " + ex);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("Phase10/P10E/Validate Trigger Group Aggregation")]
        public static void RunP10E06TriggerGroupAggregationValidationOnly()
        {
            try
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                GameObject root = GameObject.Find(TriggerRootName);
                if (root == null)
                {
                    throw new InvalidOperationException("Missing trigger root: " + TriggerRootName);
                }

                ValidateSceneAggregation(root.GetComponent<P10NarrativeSceneTriggerGroup>());
                ValidateRuntimeRouting();
                Debug.Log(PassMessage);
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10E-06 Trigger Group Aggregation validation failed: " + ex);
                EditorApplication.Exit(1);
            }
        }

        private static P10NarrativeSceneTriggerGroup MigrateOpenScene()
        {
            GameObject root = GameObject.Find(TriggerRootName);
            if (root == null)
            {
                throw new InvalidOperationException("Missing trigger root: " + TriggerRootName);
            }

            P10NarrativeSceneTriggerGroup group = root.GetComponent<P10NarrativeSceneTriggerGroup>();
            if (group == null)
            {
                group = root.AddComponent<P10NarrativeSceneTriggerGroup>();
            }

            Rigidbody body = root.GetComponent<Rigidbody>();
            if (body == null)
            {
                body = root.AddComponent<Rigidbody>();
            }

            body.isKinematic = true;
            body.useGravity = false;

            P10NarrativeSceneTrigger[] oldTriggers = root.GetComponentsInChildren<P10NarrativeSceneTrigger>(true);
            if (oldTriggers.Length == 0)
            {
                if (group.EntryCount == 0)
                {
                    throw new InvalidOperationException("No child trigger scripts were found to aggregate, and the parent group has no entries.");
                }

                return group;
            }

            List<P10NarrativeSceneTriggerGroup.Entry> entries = new List<P10NarrativeSceneTriggerGroup.Entry>();
            for (int i = 0; i < oldTriggers.Length; i++)
            {
                P10NarrativeSceneTrigger oldTrigger = oldTriggers[i];
                Collider collider = oldTrigger.GetComponent<Collider>();
                if (collider == null)
                {
                    throw new InvalidOperationException("Missing child collider: " + oldTrigger.name);
                }

                SerializedObject serializedTrigger = new SerializedObject(oldTrigger);
                string triggerId = serializedTrigger.FindProperty("triggerId").stringValue;
                string targetNodeId = serializedTrigger.FindProperty("targetNodeId").stringValue;
                P10NarrativeSceneBindingHub bindingHub = serializedTrigger.FindProperty("bindingHub").objectReferenceValue as P10NarrativeSceneBindingHub;
                bool triggerOnce = serializedTrigger.FindProperty("triggerOnce").boolValue;

                if (bindingHub == null)
                {
                    bindingHub = UnityEngine.Object.FindObjectOfType<P10NarrativeSceneBindingHub>();
                }

                collider.isTrigger = true;

                P10NarrativeSceneTriggerGroup.Entry entry = new P10NarrativeSceneTriggerGroup.Entry();
                entry.Configure(collider, triggerId, targetNodeId, bindingHub, triggerOnce);
                entries.Add(entry);
            }

            for (int i = 0; i < oldTriggers.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(oldTriggers[i], true);
            }

            group.Configure(entries.ToArray());
            EditorUtility.SetDirty(group);
            EditorUtility.SetDirty(root);
            return group;
        }

        private static void ValidateSceneAggregation(P10NarrativeSceneTriggerGroup group)
        {
            if (group == null)
            {
                throw new InvalidOperationException("Trigger root does not have P10NarrativeSceneTriggerGroup.");
            }

            Rigidbody body = group.GetComponent<Rigidbody>();
            if (body == null || !body.isKinematic || body.useGravity)
            {
                throw new InvalidOperationException("Trigger root must have a kinematic Rigidbody with gravity disabled.");
            }

            P10NarrativeSceneTrigger[] childTriggers = group.GetComponentsInChildren<P10NarrativeSceneTrigger>(true);
            if (childTriggers.Length != 0)
            {
                throw new InvalidOperationException("Child trigger scripts were not aggregated; remaining count: " + childTriggers.Length);
            }

            if (group.EntryCount != ExpectedRoutedSpecs.Length)
            {
                throw new InvalidOperationException("Expected " + ExpectedRoutedSpecs.Length + " routed group entries, got " + group.EntryCount);
            }

            for (int i = 0; i < ExpectedRoutedSpecs.Length; i++)
            {
                TriggerSpec spec = ExpectedRoutedSpecs[i];
                P10NarrativeSceneTriggerGroup.Entry entry = FindEntry(group, spec.GameObjectName);
                if (entry == null)
                {
                    throw new InvalidOperationException("Missing group entry for " + spec.GameObjectName);
                }

                if (!entry.TriggerCollider.isTrigger)
                {
                    throw new InvalidOperationException("Child collider is not trigger: " + spec.GameObjectName);
                }

                if (!string.Equals(entry.TriggerId, spec.TriggerId, StringComparison.Ordinal)
                    || !string.Equals(entry.TargetNodeId, spec.TargetNodeId, StringComparison.Ordinal)
                    || !entry.TriggerOnce)
                {
                    throw new InvalidOperationException("Entry mapping mismatch for " + spec.GameObjectName);
                }
            }
        }

        private static P10NarrativeSceneTriggerGroup.Entry FindEntry(P10NarrativeSceneTriggerGroup group, string colliderName)
        {
            for (int i = 0; i < group.EntryCount; i++)
            {
                P10NarrativeSceneTriggerGroup.Entry entry = group.GetEntry(i);
                if (entry != null && entry.TriggerCollider != null && entry.TriggerCollider.name == colliderName)
                {
                    return entry;
                }
            }

            return null;
        }

        private static void ValidateRuntimeRouting()
        {
            GameObject root = new GameObject("P10E06_RuntimeRoutingRoot");
            try
            {
                P10NarrativeManager manager = root.AddComponent<P10NarrativeManager>();
                InvokeAwake(manager);

                GameObject hubObject = new GameObject("P10E06_BindingHub");
                hubObject.transform.SetParent(root.transform, false);
                P10NarrativeSceneBindingHub hub = hubObject.AddComponent<P10NarrativeSceneBindingHub>();
                SerializedObject serializedHub = new SerializedObject(hub);
                serializedHub.FindProperty("manager").objectReferenceValue = manager;
                serializedHub.ApplyModifiedPropertiesWithoutUndo();

                GameObject triggerRoot = new GameObject(TriggerRootName);
                triggerRoot.transform.SetParent(root.transform, false);
                P10NarrativeSceneTriggerGroup group = triggerRoot.AddComponent<P10NarrativeSceneTriggerGroup>();

                GameObject child = GameObject.CreatePrimitive(PrimitiveType.Cube);
                child.name = "P10_CH01_TRIGGER_StartPrologue";
                child.transform.SetParent(triggerRoot.transform, false);
                Collider collider = child.GetComponent<Collider>();
                collider.isTrigger = true;

                P10NarrativeSceneTriggerGroup.Entry entry = new P10NarrativeSceneTriggerGroup.Entry();
                entry.Configure(collider, "StartPrologue", "P10_CH01_NODE_PROLOGUE_01", hub, true);
                group.Configure(new[] { entry });

                if (!group.TryHandleTriggerForCollider(collider))
                {
                    throw new InvalidOperationException("Group did not route first trigger interaction.");
                }

                if (manager.GetCurrentState() != P10NarrativeState.Prologue || manager.GetCurrentNode() != "P10_CH01_NODE_PROLOGUE_01")
                {
                    throw new InvalidOperationException("Group trigger did not advance to prologue.");
                }

                if (group.TryHandleTriggerForCollider(collider))
                {
                    throw new InvalidOperationException("Group triggerOnce did not block duplicate trigger.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void InvokeAwake(P10NarrativeManager manager)
        {
            typeof(P10NarrativeManager)
                .GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.Invoke(manager, null);
        }

        private readonly struct TriggerSpec
        {
            public readonly string GameObjectName;
            public readonly string TriggerId;
            public readonly string TargetNodeId;

            public TriggerSpec(string gameObjectName, string triggerId, string targetNodeId)
            {
                GameObjectName = gameObjectName;
                TriggerId = triggerId;
                TargetNodeId = targetNodeId;
            }
        }
    }
}
