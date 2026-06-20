using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Phase10_Narrative
{
    public static class P10ArtAssetUIPanelValidator
    {
        private const string PassMessage = "P10 art assets UI panel validation passed.";

        private static readonly string[] RequiredResourcePaths =
        {
            "P10Art/Dialog/dialogue_bg",
            "P10Art/Dialog/nameplate",
            "P10Art/Dialog/body_texture",
            "P10Art/Dialog/button_order",
            "P10Art/Dialog/button_close",
            "P10Art/Dialog/button_log",
            "P10Art/Dialog/button_continue",
            "P10Art/Log/log_bg",
            "P10Art/Log/log_item_bg",
            "P10Art/Log/scrollbar_track",
            "P10Art/Log/scrollbar_handle",
            "P10Art/Log/divider",
            "P10Art/NPC/xulaobo_avatar",
            "P10Art/NPC/zhouzhanggui_avatar",
            "P10Art/NPC/chenshuyuan_avatar",
            "P10Art/NPC/luke_avatar",
            "P10Art/NPC/system_avatar",
            "P10Art/NPC/xulaobo_half",
            "P10Art/NPC/zhouzhanggui_half",
            "P10Art/NPC/chenshuyuan_half",
            "P10Art/NPC/luke_half",
            "P10Art/Props/reward_icon",
            "P10Art/Props/father_ledger",
            "P10Art/Props/old_kiln_tool",
            "P10Art/Props/broken_bowl",
            "P10Art/Props/ancient_order",
            "P10Art/Props/family_letter"
        };

        public static void RunP10ArtAssetUIPanelValidation()
        {
            try
            {
                ValidateResourcesLoad();
                ValidateRuntimeSurfaceUsesArtSlots();
                Debug.Log(PassMessage);
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10 art assets UI panel validation failed: " + ex);
                EditorApplication.Exit(1);
            }
        }

        private static void ValidateResourcesLoad()
        {
            for (int i = 0; i < RequiredResourcePaths.Length; i++)
            {
                string resourcePath = RequiredResourcePaths[i];
                Texture2D texture = Resources.Load<Texture2D>(resourcePath);
                if (texture == null)
                {
                    throw new InvalidOperationException("Missing P10 UI art resource: " + resourcePath);
                }

                if (texture.width <= 0 || texture.height <= 0)
                {
                    throw new InvalidOperationException("Invalid P10 UI art texture dimensions: " + resourcePath);
                }
            }
        }

        private static void ValidateRuntimeSurfaceUsesArtSlots()
        {
            GameObject root = new GameObject("P10ArtAssetUIPanelValidationRoot");
            try
            {
                P10DialogueController controller = root.AddComponent<P10DialogueController>();
                controller.EnsureRuntimeSurfaceInstance();

                Transform surface = FindDescendant(root.transform, "P10_Runtime_DialogueSurface");
                if (surface == null)
                {
                    surface = root.transform;
                }

                AssertImageHasSprite(surface, "Panel");
                AssertImageHasSprite(surface, "P10_DialogueSpeakerNameplate");
                AssertImageHasSprite(surface, "P10_DialogueBodyTexture");
                AssertImageHasSprite(surface, "LogButton");
                AssertImageHasSprite(surface, "CloseButton");
                AssertImageHasSprite(surface, "DialogueLogPanel");
                AssertImageHasSprite(surface, "DialogueLogCloseButton");
                AssertImageHasSprite(surface, "CurrentOrderPanel");
                AssertImageHasSprite(surface, "CurrentOrderCloseButton");
                AssertImageHasSprite(surface, "P10_PropIconReward");
                AssertImageHasSprite(surface, "P10_PropIconLedger");
                AssertImageHasSprite(surface, "P10_PropIconKilnTool");
                AssertImageHasSprite(surface, "P10_PropIconOrder");

                controller.SetCurrentNode("P10_CH01_NODE_ORDER_001_ACCEPT");
                AssertImageHasSprite(surface, "P10_SpeakerPortraitImage");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void AssertImageHasSprite(Transform root, string objectName, bool allowDisabled = false)
        {
            Transform match = FindDescendant(root, objectName);
            if (match == null)
            {
                throw new InvalidOperationException("Missing P10 UI art object: " + objectName);
            }

            Image image = match.GetComponent<Image>();
            if (image == null)
            {
                throw new InvalidOperationException("P10 UI art object has no Image component: " + objectName);
            }

            if (!allowDisabled && !image.enabled)
            {
                throw new InvalidOperationException("P10 UI art Image is disabled: " + objectName);
            }

            if (image.sprite == null)
            {
                throw new InvalidOperationException("P10 UI art Image has no sprite: " + objectName);
            }
        }

        private static Transform FindDescendant(Transform root, string objectName)
        {
            if (root == null || string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            if (root.name == objectName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDescendant(root.GetChild(i), objectName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }
    }
}
