using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Phase10_Narrative
{
    public static class P10C05SaveStateSafetyValidator
    {
        private const string ChapterId = "P10_CH01";
        private const string NodePrologue = "P10_CH01_NODE_PROLOGUE_01";
        private const string NodeTutorial = "P10_CH01_NODE_TUTORIAL_01";
        private const string NodeOrder001Accept = "P10_CH01_NODE_ORDER_001_ACCEPT";
        private const string NodeOrder001Pass = "P10_CH01_NODE_ORDER_001_PASS";
        private const string NodeOrder003Accept = "P10_CH01_NODE_ORDER_003_ACCEPT";
        private const string NodeOrder003Pass = "P10_CH01_NODE_ORDER_003_PASS";
        private const string NodeOrder004Accept = "P10_CH01_NODE_ORDER_004_ACCEPT";
        private const string NodeOrder004PassNormal = "P10_CH01_NODE_ORDER_004_PASS_NORMAL";
        private const string NodeChapterEnding = "P10_CH01_NODE_CHAPTER_ENDING";

        public static void RunP10C05SaveStateSafetyValidation()
        {
            try
            {
                ValidateSnapshotRoundTrip();
                ValidateSceneTriggerLoopSafety();
                Debug.Log("P10C-05 Save/State Safety validation passed.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10C-05 Save/State Safety validation failed: " + ex.Message);
                EditorApplication.Exit(1);
            }
        }

        private static void ValidateSnapshotRoundTrip()
        {
            P10NarrativeStateMachine source = new P10NarrativeStateMachine();
            source.HandleEvent(new P10NarrativeEvent(P10NarrativeEventType.ChapterStartRequested)
            {
                ChapterId = ChapterId
            });
            AdvanceNode(source, NodePrologue);
            AdvanceState(source, P10NarrativeState.Tutorial);
            AdvanceNode(source, NodeTutorial);
            AdvanceState(source, P10NarrativeState.Order001);
            AdvanceNode(source, NodeOrder001Accept);
            AdvanceNode(source, NodeOrder001Pass);

            P10NarrativeSnapshot snapshot = source.SaveSnapshot();
            if (snapshot == null)
            {
                throw new InvalidOperationException("Snapshot is null.");
            }

            if (snapshot.ChapterState != P10NarrativeState.Order001)
            {
                throw new InvalidOperationException("Snapshot state mismatch.");
            }

            if (snapshot.CurrentNodeId != NodeOrder001Pass)
            {
                throw new InvalidOperationException("Snapshot current node mismatch.");
            }

            if (snapshot.PlayedNodeIds == null
                || !snapshot.PlayedNodeIds.Contains(NodePrologue)
                || !snapshot.PlayedNodeIds.Contains(NodeTutorial)
                || !snapshot.PlayedNodeIds.Contains(NodeOrder001Accept))
            {
                throw new InvalidOperationException("Snapshot played node set mismatch.");
            }

            P10NarrativeStateMachine restored = new P10NarrativeStateMachine();
            restored.LoadSnapshot(snapshot);

            if (restored.GetCurrentState() != source.GetCurrentState())
            {
                throw new InvalidOperationException("Loaded state mismatch.");
            }

            if (restored.GetCurrentNode() != source.GetCurrentNode())
            {
                throw new InvalidOperationException("Loaded node mismatch.");
            }

            if (!restored.HasPlayedNode(NodePrologue)
                || !restored.HasPlayedNode(NodeTutorial)
                || !restored.HasPlayedNode(NodeOrder001Accept))
            {
                throw new InvalidOperationException("Loaded played node set mismatch.");
            }
        }

        private static void ValidateSceneTriggerLoopSafety()
        {
            GameObject host = new GameObject("P10C05_SaveStateSafety_ManagerHost");
            try
            {
                P10NarrativeManager manager = host.AddComponent<P10NarrativeManager>();

                AssertTrigger(manager, "StartPrologue", NodePrologue, true);
                AssertTrigger(manager, "Tutorial", NodeTutorial, true);
                AssertTrigger(manager, "Order001Accept", NodeOrder001Accept, true);
                AssertTrigger(manager, "Order001Pass", NodeOrder001Pass, true);

                P10NarrativeState duplicateState = manager.GetCurrentState();
                string duplicateNode = manager.GetCurrentNode();
                AssertTrigger(manager, "Order001Pass", NodeOrder001Pass, false);
                AssertStateUnchanged(manager, duplicateState, duplicateNode, "duplicate trigger changed state.");

                AssertTrigger(manager, "Order003Accept", NodeOrder003Accept, true);
                AssertTrigger(manager, "Order003Pass", NodeOrder003Pass, true);
                AssertTrigger(manager, "Order004Accept", NodeOrder004Accept, true);
                AssertTrigger(manager, "Order004PassNormal", NodeOrder004PassNormal, true);
                AssertTrigger(manager, "ChapterEnding", NodeChapterEnding, true);

                if (manager.GetCurrentState() != P10NarrativeState.Completed)
                {
                    throw new InvalidOperationException("Manager did not reach Completed.");
                }

                if (manager.GetCurrentNode() != NodeChapterEnding)
                {
                    throw new InvalidOperationException("Manager did not end on chapter ending node.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }

            GameObject outOfOrderHost = new GameObject("P10C05_SaveStateSafety_OutOfOrderHost");
            try
            {
                P10NarrativeManager manager = outOfOrderHost.AddComponent<P10NarrativeManager>();
                P10NarrativeState beforeState = manager.GetCurrentState();
                string beforeNode = manager.GetCurrentNode();
                AssertTrigger(manager, "Order003Pass", NodeOrder003Pass, false);
                AssertStateUnchanged(manager, beforeState, beforeNode, "out-of-order trigger changed state.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(outOfOrderHost);
            }
        }

        private static void AdvanceState(P10NarrativeStateMachine stateMachine, P10NarrativeState state)
        {
            stateMachine.HandleEvent(new P10NarrativeEvent(P10NarrativeEventType.StateAdvanceRequested)
            {
                TargetState = state
            });
        }

        private static void AdvanceNode(P10NarrativeStateMachine stateMachine, string nodeId)
        {
            stateMachine.HandleEvent(new P10NarrativeEvent(P10NarrativeEventType.NodeAdvanceRequested)
            {
                NodeId = nodeId
            });
        }

        private static void AssertTrigger(P10NarrativeManager manager, string triggerId, string nodeId, bool expected)
        {
            bool actual = manager.HandleSceneTrigger(triggerId, nodeId);
            if (actual != expected)
            {
                throw new InvalidOperationException("Unexpected trigger result for " + triggerId + ".");
            }
        }

        private static void AssertStateUnchanged(
            P10NarrativeManager manager,
            P10NarrativeState expectedState,
            string expectedNode,
            string message)
        {
            if (manager.GetCurrentState() != expectedState || manager.GetCurrentNode() != expectedNode)
            {
                throw new InvalidOperationException(message);
            }
        }
    }

    public static class P10D05InGameAcceptanceValidator
    {
        private const string NodePrologue = "P10_CH01_NODE_PROLOGUE_01";
        private const string NodeTutorial = "P10_CH01_NODE_TUTORIAL_01";
        private const string NodeOrder001Accept = "P10_CH01_NODE_ORDER_001_ACCEPT";
        private const string NodeOrder001Pass = "P10_CH01_NODE_ORDER_001_PASS";

        public static void RunP10D05InGameAcceptanceValidation()
        {
            try
            {
                ValidateDialogueOrder001Loop();
                ValidateNpcStartPrologueEntry();
                ValidateNpcOrder001AcceptEntry();
                Debug.Log("P10D-05 In-Game Acceptance validation passed.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10D-05 In-Game Acceptance validation failed: " + ex.Message);
                EditorApplication.Exit(1);
            }
        }

        private static void ValidateDialogueOrder001Loop()
        {
            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            GameObject root = new GameObject("P10D05_InGameAcceptance_DialogueRoot");

            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "Manager");
                P10NarrativeSceneBindingHub hub = CreateHub(root.transform, manager);
                P10DialogueController controller = CreateDialogueController(root.transform, manager);

                if (hub == null)
                {
                    throw new InvalidOperationException("Binding hub was not created.");
                }

                controller.EnsureRuntimeSurfaceInstance();
                if (controller.transform.Find("P10_Runtime_DialogueSurface") == null)
                {
                    throw new InvalidOperationException("Dialogue runtime surface was not created.");
                }

                AssertTrigger(manager, "StartPrologue", NodePrologue, true);

                controller.SetCurrentNode(manager.GetCurrentNode());
                controller.AdvanceDialogue();
                AssertNode(manager, P10NarrativeState.Tutorial, NodeTutorial, "Dialogue Next did not enter tutorial.");

                controller.SetCurrentNode(manager.GetCurrentNode());
                controller.AdvanceDialogue();
                AssertNode(manager, P10NarrativeState.Order001, NodeOrder001Accept, "Dialogue Next did not enter ORDER_001 accept.");

                controller.SetCurrentNode(manager.GetCurrentNode());
                controller.AdvanceDialogue();
                AssertNode(manager, P10NarrativeState.Order001, NodeOrder001Accept, "ORDER_001 accept Dialogue Next skipped to pass.");

                manager.PublishEvent(new P10NarrativeEvent(P10NarrativeEventType.OrderCompleted)
                {
                    OrderId = "ORDER_001"
                });
                AssertNode(manager, P10NarrativeState.Order001, NodeOrder001Pass, "OrderCompleted(ORDER_001) did not enter pass node.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                DestroyGeneratedEventSystemIfNeeded(existingEventSystem);
            }
        }

        private static void ValidateNpcStartPrologueEntry()
        {
            GameObject root = new GameObject("P10D05_InGameAcceptance_NpcPrologueRoot");

            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "Manager");
                P10NarrativeSceneBindingHub hub = CreateHub(root.transform, manager);
                P10NarrativeNpcInteraction interaction = CreateNpcInteraction(
                    root.transform,
                    "P10_CH01_NPC_001_XuLaoBo_Placeholder",
                    hub);

                interaction.Interact();
                AssertNode(manager, P10NarrativeState.Prologue, NodePrologue, "NPC interaction did not start prologue.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void ValidateNpcOrder001AcceptEntry()
        {
            GameObject root = new GameObject("P10D05_InGameAcceptance_NpcOrder001Root");

            try
            {
                P10NarrativeManager manager = CreateManager(root.transform, "Manager");
                P10NarrativeSceneBindingHub hub = CreateHub(root.transform, manager);

                AssertTrigger(manager, "StartPrologue", NodePrologue, true);
                if (!manager.TryAdvanceDialogueNode(NodePrologue, NodeTutorial))
                {
                    throw new InvalidOperationException("Setup could not advance from prologue to tutorial.");
                }

                P10NarrativeNpcInteraction interaction = CreateNpcInteraction(
                    root.transform,
                    "P10_CH01_NPC_002_ZhouZhangGui_Placeholder",
                    hub);

                interaction.Interact();
                AssertNode(manager, P10NarrativeState.Order001, NodeOrder001Accept, "NPC interaction did not enter ORDER_001 accept.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static P10NarrativeManager CreateManager(Transform parent, string name)
        {
            GameObject host = new GameObject("P10D05_" + name);
            host.transform.SetParent(parent, false);
            P10NarrativeManager manager = host.AddComponent<P10NarrativeManager>();
            InvokeAwake(manager);
            return manager;
        }

        private static P10NarrativeSceneBindingHub CreateHub(Transform parent, P10NarrativeManager manager)
        {
            GameObject host = new GameObject("P10D05_BindingHub");
            host.transform.SetParent(parent, false);

            P10NarrativeSceneBindingHub hub = host.AddComponent<P10NarrativeSceneBindingHub>();
            SerializedObject serializedHub = new SerializedObject(hub);
            serializedHub.FindProperty("manager").objectReferenceValue = manager;
            serializedHub.ApplyModifiedPropertiesWithoutUndo();
            return hub;
        }

        private static P10DialogueController CreateDialogueController(Transform parent, P10NarrativeManager manager)
        {
            GameObject host = new GameObject("P10D05_DialogueController");
            host.transform.SetParent(parent, false);

            P10DialogueController controller = host.AddComponent<P10DialogueController>();
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("narrativeManager").objectReferenceValue = manager;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            return controller;
        }

        private static P10NarrativeNpcInteraction CreateNpcInteraction(
            Transform parent,
            string npcId,
            P10NarrativeSceneBindingHub hub)
        {
            GameObject host = new GameObject(npcId);
            host.transform.SetParent(parent, false);
            host.AddComponent<BoxCollider>().isTrigger = true;

            P10NarrativeNpcInteraction interaction = host.AddComponent<P10NarrativeNpcInteraction>();
            interaction.Configure(npcId, null, null, hub);
            return interaction;
        }

        private static void AssertTrigger(P10NarrativeManager manager, string triggerId, string nodeId, bool expected)
        {
            bool actual = manager.HandleSceneTrigger(triggerId, nodeId);
            if (actual != expected)
            {
                throw new InvalidOperationException("Unexpected trigger result for " + triggerId + ".");
            }
        }

        private static void AssertNode(P10NarrativeManager manager, P10NarrativeState expectedState, string expectedNode, string message)
        {
            if (manager.GetCurrentState() != expectedState || manager.GetCurrentNode() != expectedNode)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void DestroyGeneratedEventSystemIfNeeded(EventSystem existingEventSystem)
        {
            if (existingEventSystem != null)
            {
                return;
            }

            EventSystem generatedEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            if (generatedEventSystem != null && generatedEventSystem.name == "P10_Runtime_EventSystem")
            {
                UnityEngine.Object.DestroyImmediate(generatedEventSystem.gameObject);
            }
        }

        private static void InvokeAwake(MonoBehaviour behaviour)
        {
            MethodInfo awake = behaviour.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            if (awake != null)
            {
                awake.Invoke(behaviour, null);
            }
        }
    }
}
