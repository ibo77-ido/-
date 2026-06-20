using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Phase10_Narrative
{
    public static partial class P10E03AAnchorLayoutValidator
    {
        private const string P10E03AScenePath = "Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity";
        private const string P10E03APassMessage = "P10E-03A NPC Anchor Layout validation passed.";

        private static readonly P10E03ANpcLayout[] P10E03ANpcs =
        {
            new P10E03ANpcLayout("P10_NPC_XuLaoBo", new Vector3(-3f, 0f, 0f)),
            new P10E03ANpcLayout("P10_NPC_ZhouZhangGui", new Vector3(-1f, 0f, 0f)),
            new P10E03ANpcLayout("P10_NPC_ChenShuYuan", new Vector3(1f, 0f, 0f)),
            new P10E03ANpcLayout("P10_NPC_LuKe", new Vector3(3f, 0f, 0f))
        };

        private static readonly string[] P10E03ANpcAnchorNames =
        {
            "P10_InteractPoint",
            "P10_DialogueAnchor",
            "P10_QuestMarkerAnchor"
        };

        private static readonly string[] P10E03ATriggerNames =
        {
            "P10_Trigger_PrologueStart",
            "P10_Trigger_TutorialStart",
            "P10_Trigger_Order001Accept",
            "P10_Trigger_Order003Accept",
            "P10_Trigger_Order004Accept",
            "P10_Trigger_ChapterEnding"
        };

        private static readonly P10E03AAnchorLayout[] P10E03ASceneAnchors =
        {
            new P10E03AAnchorLayout("P10_Anchor_OrderStation", new Vector3(-3f, 0f, -2f)),
            new P10E03AAnchorLayout("P10_Anchor_WheelStation", new Vector3(-1.5f, 0f, -2f)),
            new P10E03AAnchorLayout("P10_Anchor_GlazeStation", new Vector3(0f, 0f, -2f)),
            new P10E03AAnchorLayout("P10_Anchor_KilnStation", new Vector3(1.5f, 0f, -2f)),
            new P10E03AAnchorLayout("P10_Anchor_CourtyardCenter", new Vector3(0f, 0f, 1.5f)),
            new P10E03AAnchorLayout("P10_Anchor_Gate", new Vector3(3f, 0f, 1.5f))
        };

        public static void SetupP10E03AAnchorLayout()
        {
            RunP10E03A("P10E-03A NPC Anchor Layout setup passed.", () =>
            {
                Scene scene = EditorSceneManager.OpenScene(P10E03AScenePath, OpenSceneMode.Single);
                Transform narrativeRoot = EnsureP10E03ARoot("P10_CH01_NarrativeRoot");
                Transform npcRoot = EnsureP10E03AChild(narrativeRoot, "P10_CH01_NPCRoot", Vector3.zero);
                Transform triggerRoot = EnsureP10E03AChild(narrativeRoot, "P10_CH01_TriggerRoot", Vector3.zero);
                Transform anchorRoot = EnsureP10E03AChild(narrativeRoot, "P10_CH01_AnchorRoot", Vector3.zero);

                EnsureP10E03ANpcLayout(npcRoot);
                EnsureP10E03ATriggerLayout(triggerRoot);
                EnsureP10E03ASceneAnchorLayout(anchorRoot);

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                ValidateP10E03AAnchorLayout();
            });
        }

        public static void RunP10E03AAnchorLayoutValidation()
        {
            RunP10E03A(P10E03APassMessage, () =>
            {
                EditorSceneManager.OpenScene(P10E03AScenePath, OpenSceneMode.Single);
                ValidateP10E03AAnchorLayout();
            });
        }

        private static void RunP10E03A(string passMessage, Action action)
        {
            try
            {
                action();
                Debug.Log(passMessage);
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError(passMessage.Replace(" passed.", " failed: ") + ex);
                EditorApplication.Exit(1);
            }
        }

        private static void EnsureP10E03ANpcLayout(Transform npcRoot)
        {
            for (int i = 0; i < P10E03ANpcs.Length; i++)
            {
                P10E03ANpcLayout npc = P10E03ANpcs[i];
                Transform npcTransform = EnsureP10E03AChild(npcRoot, npc.Name, npc.LocalPosition);
                EnsureP10E03AChild(npcTransform, "P10_InteractPoint", Vector3.zero);
                EnsureP10E03AChild(npcTransform, "P10_DialogueAnchor", new Vector3(0f, 1.35f, 0f));
                EnsureP10E03AChild(npcTransform, "P10_QuestMarkerAnchor", new Vector3(0f, 2.1f, 0f));
            }
        }

        private static void EnsureP10E03ATriggerLayout(Transform triggerRoot)
        {
            for (int i = 0; i < P10E03ATriggerNames.Length; i++)
            {
                EnsureP10E03AChild(triggerRoot, P10E03ATriggerNames[i], new Vector3(-2.5f + i, 0f, 3f));
            }
        }

        private static void EnsureP10E03ASceneAnchorLayout(Transform anchorRoot)
        {
            for (int i = 0; i < P10E03ASceneAnchors.Length; i++)
            {
                EnsureP10E03AChild(anchorRoot, P10E03ASceneAnchors[i].Name, P10E03ASceneAnchors[i].LocalPosition);
            }
        }

        private static Transform EnsureP10E03ARoot(string objectName)
        {
            GameObject existing = GameObject.Find(objectName);
            if (existing != null)
            {
                existing.transform.SetParent(null, true);
                return existing.transform;
            }

            GameObject created = new GameObject(objectName);
            created.transform.localPosition = Vector3.zero;
            created.transform.localRotation = Quaternion.identity;
            created.transform.localScale = Vector3.one;
            return created.transform;
        }

        private static Transform EnsureP10E03AChild(Transform parent, string objectName, Vector3 localPosition)
        {
            Transform existing = parent.Find(objectName);
            if (existing != null)
            {
                existing.localPosition = localPosition;
                existing.localRotation = Quaternion.identity;
                existing.localScale = Vector3.one;
                return existing;
            }

            GameObject created = new GameObject(objectName);
            created.transform.SetParent(parent, false);
            created.transform.localPosition = localPosition;
            created.transform.localRotation = Quaternion.identity;
            created.transform.localScale = Vector3.one;
            return created.transform;
        }

        private static void ValidateP10E03AAnchorLayout()
        {
            Transform narrativeRoot = RequireP10E03ARoot("P10_CH01_NarrativeRoot");
            Transform npcRoot = RequireP10E03AChild(narrativeRoot, "P10_CH01_NPCRoot");
            Transform triggerRoot = RequireP10E03AChild(narrativeRoot, "P10_CH01_TriggerRoot");
            Transform anchorRoot = RequireP10E03AChild(narrativeRoot, "P10_CH01_AnchorRoot");

            ValidateP10E03ANpcLayout(npcRoot);
            ValidateP10E03ANamedChildren(triggerRoot, P10E03ATriggerNames, "trigger");
            ValidateP10E03ANamedChildren(anchorRoot, GetP10E03ASceneAnchorNames(), "scene anchor");
            ValidateNoP10E03APhase3OrPhase6Components(narrativeRoot);
        }

        private static void ValidateP10E03ANpcLayout(Transform npcRoot)
        {
            for (int i = 0; i < P10E03ANpcs.Length; i++)
            {
                Transform npc = RequireP10E03AChild(npcRoot, P10E03ANpcs[i].Name);
                ValidateP10E03ANamedChildren(npc, P10E03ANpcAnchorNames, "NPC anchor");

                Transform interactPoint = RequireP10E03AChild(npc, "P10_InteractPoint");
                Transform dialogueAnchor = RequireP10E03AChild(npc, "P10_DialogueAnchor");
                Transform questMarkerAnchor = RequireP10E03AChild(npc, "P10_QuestMarkerAnchor");

                if (dialogueAnchor.localPosition == interactPoint.localPosition ||
                    questMarkerAnchor.localPosition == interactPoint.localPosition ||
                    questMarkerAnchor.localPosition == dialogueAnchor.localPosition)
                {
                    throw new InvalidOperationException("NPC anchors overlap exactly under " + npc.name + ".");
                }
            }
        }

        private static void ValidateP10E03ANamedChildren(Transform parent, string[] childNames, string label)
        {
            for (int i = 0; i < childNames.Length; i++)
            {
                RequireP10E03AChild(parent, childNames[i]);
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (IsP10E03APlannedName(child.name, childNames))
                {
                    ValidateNoP10E03APhase3OrPhase6Components(child);
                }
            }
        }

        private static bool IsP10E03APlannedName(string objectName, string[] plannedNames)
        {
            for (int i = 0; i < plannedNames.Length; i++)
            {
                if (string.Equals(objectName, plannedNames[i], StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static Transform RequireP10E03ARoot(string objectName)
        {
            GameObject root = GameObject.Find(objectName);
            if (root == null)
            {
                throw new InvalidOperationException("Missing root object: " + objectName + ".");
            }

            return root.transform;
        }

        private static Transform RequireP10E03AChild(Transform parent, string childName)
        {
            Transform child = parent != null ? parent.Find(childName) : null;
            if (child == null)
            {
                throw new InvalidOperationException("Missing child object: " + childName + " under " + (parent != null ? parent.name : "<null>") + ".");
            }

            return child;
        }

        private static string[] GetP10E03ASceneAnchorNames()
        {
            string[] names = new string[P10E03ASceneAnchors.Length];
            for (int i = 0; i < P10E03ASceneAnchors.Length; i++)
            {
                names[i] = P10E03ASceneAnchors[i].Name;
            }

            return names;
        }

        private static void ValidateNoP10E03APhase3OrPhase6Components(Transform root)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    throw new InvalidOperationException("Missing script component under " + root.name + ".");
                }

                Type type = behaviour.GetType();
                string fullName = type.FullName ?? type.Name;
                string assemblyName = type.Assembly.GetName().Name;
                if (ContainsP10E03AForbiddenRuntimeToken(fullName) || ContainsP10E03AForbiddenRuntimeToken(assemblyName))
                {
                    throw new InvalidOperationException("Forbidden Phase3/Phase6 component found: " + fullName + " on " + behaviour.name + ".");
                }

                MonoScript script = MonoScript.FromMonoBehaviour(behaviour);
                string scriptPath = script != null ? AssetDatabase.GetAssetPath(script) : string.Empty;
                if (scriptPath.StartsWith("Assets/Phase3/", StringComparison.Ordinal) ||
                    scriptPath.StartsWith("Assets/Phase6/", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Forbidden Phase3/Phase6 script reference found: " + scriptPath + ".");
                }
            }
        }

        private static bool ContainsP10E03AForbiddenRuntimeToken(string value)
        {
            return !string.IsNullOrEmpty(value) &&
                (value.IndexOf("Phase3", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 value.IndexOf("Phase6", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private readonly struct P10E03ANpcLayout
        {
            public readonly string Name;
            public readonly Vector3 LocalPosition;

            public P10E03ANpcLayout(string name, Vector3 localPosition)
            {
                Name = name;
                LocalPosition = localPosition;
            }
        }

        private readonly struct P10E03AAnchorLayout
        {
            public readonly string Name;
            public readonly Vector3 LocalPosition;

            public P10E03AAnchorLayout(string name, Vector3 localPosition)
            {
                Name = name;
                LocalPosition = localPosition;
            }
        }
    }
}
