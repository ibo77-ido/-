using System;
using UnityEditor;
using UnityEngine;

namespace Phase10_Narrative
{
    public static class P10E05StoryOnlyNpcGateValidator
    {
        private const string NodePrologue = "P10_CH01_NODE_PROLOGUE_01";
        private const string NodeTutorial = "P10_CH01_NODE_TUTORIAL_01";
        private const string NodeOrder001Accept = "P10_CH01_NODE_ORDER_001_ACCEPT";
        private const string NodeOrder001Pass = "P10_CH01_NODE_ORDER_001_PASS";
        private const string NodeOrder003Accept = "P10_CH01_NODE_ORDER_003_ACCEPT";
        private const string NodeOrder003Pass = "P10_CH01_NODE_ORDER_003_PASS";
        private const string NodeOrder004Accept = "P10_CH01_NODE_ORDER_004_ACCEPT";

        private const string PassMessage = "P10E-05 Story-Only NPC Gate validation passed.";

        public static void RunP10E05StoryOnlyNpcGateValidation()
        {
            try
            {
                ValidateNpcStoryOrderGate();
                ValidateNoDistanceOrMovementLockTypes();
                Debug.Log(PassMessage);
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10E-05 Story-Only NPC Gate validation failed: " + ex);
                EditorApplication.Exit(1);
            }
        }

        private static void ValidateNpcStoryOrderGate()
        {
            GameObject root = new GameObject("P10E05_StoryOnlyNpcGateRoot");
            try
            {
                P10NarrativeManager manager = root.AddComponent<P10NarrativeManager>();
                InvokeAwake(manager);
                P10NarrativeSceneBindingHub hub = CreateHub(root.transform, manager);

                P10NarrativeNpcInteraction xu = CreateNpc(root.transform, "P10_CH01_NPC_001_XuLaoBo_Placeholder", hub);
                P10NarrativeNpcInteraction zhou = CreateNpc(root.transform, "P10_CH01_NPC_002_ZhouZhangGui_Placeholder", hub);
                P10NarrativeNpcInteraction chen = CreateNpc(root.transform, "P10_CH01_NPC_003_ChenShuYuan_Placeholder", hub);
                P10NarrativeNpcInteraction lu = CreateNpc(root.transform, "P10_CH01_NPC_004_LuKe_Placeholder", hub);

                AssertBlocked(zhou, manager, "Zhou should be blocked before prologue/tutorial.");
                AssertBlocked(chen, manager, "Chen should be blocked before ORDER_001 pass.");
                AssertBlocked(lu, manager, "Lu should be blocked before ORDER_003 pass.");

                xu.Interact();
                AssertNode(manager, P10NarrativeState.Prologue, NodePrologue, "Xu should start prologue.");

                AssertBlocked(chen, manager, "Chen should still be blocked after prologue only.");
                AssertBlocked(lu, manager, "Lu should still be blocked after prologue only.");

                if (!manager.TryAdvanceDialogueNode(NodePrologue, NodeTutorial))
                {
                    throw new InvalidOperationException("Could not advance prologue to tutorial.");
                }

                zhou.Interact();
                AssertNode(manager, P10NarrativeState.Order001, NodeOrder001Accept, "Zhou should open ORDER_001 accept after tutorial.");

                AssertBlocked(chen, manager, "Chen should be blocked until ORDER_001 completes.");
                AssertBlocked(lu, manager, "Lu should be blocked until ORDER_003 completes.");

                manager.PublishEvent(new P10NarrativeEvent(P10NarrativeEventType.OrderCompleted)
                {
                    OrderId = "ORDER_001"
                });
                AssertNode(manager, P10NarrativeState.Order001, NodeOrder001Pass, "ORDER_001 completion should enter pass node.");

                chen.Interact();
                AssertNode(manager, P10NarrativeState.Order003, NodeOrder003Accept, "Chen should open ORDER_003 accept after ORDER_001 pass.");

                AssertBlocked(lu, manager, "Lu should be blocked until ORDER_003 completes.");

                manager.PublishEvent(new P10NarrativeEvent(P10NarrativeEventType.OrderCompleted)
                {
                    OrderId = "ORDER_003"
                });
                AssertNode(manager, P10NarrativeState.Order003, NodeOrder003Pass, "ORDER_003 completion should enter pass node.");

                lu.Interact();
                AssertNode(manager, P10NarrativeState.Order004, NodeOrder004Accept, "Lu should open ORDER_004 accept after ORDER_003 pass.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void AssertBlocked(P10NarrativeNpcInteraction interaction, P10NarrativeManager manager, string message)
        {
            string beforeNode = manager.GetCurrentNode();
            P10NarrativeState beforeState = manager.GetCurrentState();
            int debugCount = manager.DebugLog.Count;
            interaction.Interact();
            if (manager.GetCurrentNode() != beforeNode || manager.GetCurrentState() != beforeState)
            {
                throw new InvalidOperationException(message + " Actual: " + manager.GetCurrentState() + " / " + manager.GetCurrentNode());
            }

            if (manager.DebugLog.Count <= debugCount)
            {
                throw new InvalidOperationException(message + " Expected a blocked debug log entry.");
            }
        }

        private static P10NarrativeSceneBindingHub CreateHub(Transform parent, P10NarrativeManager manager)
        {
            GameObject hubObject = new GameObject("P10E05_BindingHub");
            hubObject.transform.SetParent(parent, false);
            P10NarrativeSceneBindingHub hub = hubObject.AddComponent<P10NarrativeSceneBindingHub>();
            SerializedObject serializedHub = new SerializedObject(hub);
            serializedHub.FindProperty("manager").objectReferenceValue = manager;
            serializedHub.ApplyModifiedPropertiesWithoutUndo();
            return hub;
        }

        private static P10NarrativeNpcInteraction CreateNpc(Transform parent, string npcName, P10NarrativeSceneBindingHub hub)
        {
            GameObject npcObject = new GameObject(npcName);
            npcObject.transform.SetParent(parent, false);
            npcObject.AddComponent<BoxCollider>();
            P10NarrativeNpcInteraction interaction = npcObject.AddComponent<P10NarrativeNpcInteraction>();
            interaction.Configure(npcName, null, null, hub);
            return interaction;
        }

        private static void AssertNode(P10NarrativeManager manager, P10NarrativeState expectedState, string expectedNode, string message)
        {
            if (manager.GetCurrentState() != expectedState || manager.GetCurrentNode() != expectedNode)
            {
                throw new InvalidOperationException(message + " Actual: " + manager.GetCurrentState() + " / " + manager.GetCurrentNode());
            }
        }

        private static void ValidateNoDistanceOrMovementLockTypes()
        {
            Type movementLockType = typeof(P10NarrativeManager).Assembly.GetType("Phase10_Narrative.P10MvpTutorialMovementLock");
            if (movementLockType != null)
            {
                throw new InvalidOperationException("P10E-05 story-only gate must not restore P10MvpTutorialMovementLock.");
            }

            Type mvpInteractionType = typeof(P10NarrativeManager).Assembly.GetType("Phase10_Narrative.P10MvpTutorialNpcInteraction");
            if (mvpInteractionType != null)
            {
                throw new InvalidOperationException("P10E-05 story-only gate must not restore P10MvpTutorialNpcInteraction.");
            }
        }

        private static void InvokeAwake(P10NarrativeManager manager)
        {
            typeof(P10NarrativeManager)
                .GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.Invoke(manager, null);
        }
    }
}