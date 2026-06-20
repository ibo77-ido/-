using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Phase10_Narrative
{
    public static class P10UIArtSceneMapper
    {
        private const string ScenePath = "Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity";
        private const string PassMessage = "P10 UI art scene mapping completed.";

        [MenuItem("Phase10/Art/Map UI Art Assets Into Overlay Scene")]
        public static void MapUIArtAssetsIntoOverlaySceneMenu()
        {
            MapUIArtAssetsIntoOverlayScene(saveScene: true, exitEditor: false);
        }

        public static void MapUIArtAssetsIntoOverlaySceneBatch()
        {
            try
            {
                MapUIArtAssetsIntoOverlayScene(saveScene: true, exitEditor: true);
            }
            catch (Exception ex)
            {
                Debug.LogError("P10 UI art scene mapping failed: " + ex);
                EditorApplication.Exit(1);
            }
        }

        private static void MapUIArtAssetsIntoOverlayScene(bool saveScene, bool exitEditor)
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject uiRoot = GameObject.Find("P10_CH01_DialogueUIRoot");
            if (uiRoot == null)
            {
                throw new InvalidOperationException("Missing P10_CH01_DialogueUIRoot in overlay scene.");
            }

            Transform surface = FindOrCreateChild(uiRoot.transform, "P10_Runtime_DialogueSurface", typeof(RectTransform));
            EnsureRootUiComponents(surface.gameObject);

            Transform dialoguePanel = FindOrCreateChild(surface, "DialoguePanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            ConfigureRect(dialoguePanel, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.36f), Vector2.zero, Vector2.zero);

            Transform panel = FindOrCreateChild(dialoguePanel, "Panel", typeof(RectTransform), typeof(Image));
            ConfigureRect(panel, Vector2.zero, Vector2.one, new Vector2(12f, 12f), new Vector2(-12f, -12f));
            ApplySprite(panel, "Assets/Phase10_Narrative/Resources/P10Art/Dialog/dialogue_bg.png", preserveAspect: false);

            Transform portrait = FindOrCreateChild(panel, "P10_SpeakerPortraitImage", typeof(RectTransform), typeof(Image));
            ConfigureRect(portrait, new Vector2(0.02f, 0.23f), new Vector2(0.18f, 0.86f), Vector2.zero, Vector2.zero);
            ApplySprite(portrait, "Assets/Phase10_Narrative/Resources/P10Art/NPC/xulaobo_avatar.png", preserveAspect: true);

            Transform nameplate = FindOrCreateChild(panel, "P10_DialogueSpeakerNameplate", typeof(RectTransform), typeof(Image));
            ConfigureRect(nameplate, new Vector2(0.20f, 0.77f), new Vector2(0.58f, 0.96f), Vector2.zero, Vector2.zero);
            ApplySprite(nameplate, "Assets/Phase10_Narrative/Resources/P10Art/Dialog/nameplate.png", preserveAspect: false);

            Transform bodyTexture = FindOrCreateChild(panel, "P10_DialogueBodyTexture", typeof(RectTransform), typeof(Image));
            ConfigureRect(bodyTexture, new Vector2(0.20f, 0.23f), new Vector2(0.98f, 0.77f), Vector2.zero, Vector2.zero);
            ApplySprite(bodyTexture, "Assets/Phase10_Narrative/Resources/P10Art/Dialog/body_texture.png", preserveAspect: false);

            MoveBeforeTextSlots(panel, portrait, nameplate, bodyTexture);

            Transform actionBar = FindOrCreateChild(surface, "P10_TopRightActionBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            ConfigureRect(actionBar, new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform actionBarRect = actionBar.GetComponent<RectTransform>();
            actionBarRect.pivot = new Vector2(1f, 1f);
            actionBarRect.anchoredPosition = new Vector2(-24f, -24f);
            actionBarRect.sizeDelta = new Vector2(168f, 72f);
            ConfigureActionBarLayout(actionBar.GetComponent<HorizontalLayoutGroup>());

            Transform persistentLog = FindOrCreateButton(actionBar, "PersistentLogButton");
            ConfigureIconButton(persistentLog, "Assets/Phase10_Narrative/Resources/P10Art/Dialog/button_log.png");

            Transform persistentOrder = FindOrCreateButton(actionBar, "PersistentOrderButton");
            ConfigureIconButton(persistentOrder, "Assets/Phase10_Narrative/Resources/P10Art/Dialog/button_order.png");

            Transform logPanel = FindOrCreateChild(surface, "DialogueLogPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            ConfigureRect(logPanel, new Vector2(0.06f, 0.18f), new Vector2(0.94f, 0.96f), Vector2.zero, Vector2.zero);
            ApplySprite(logPanel, "Assets/Phase10_Narrative/Resources/P10Art/Log/log_bg.png", preserveAspect: false);

            Transform logClose = FindOrCreateButton(logPanel, "DialogueLogCloseButton");
            ConfigureRect(logClose, new Vector2(0.82f, 0.91f), new Vector2(0.96f, 0.98f), Vector2.zero, Vector2.zero);
            ApplySprite(logClose, "Assets/Phase10_Narrative/Resources/P10Art/Dialog/button_close.png", preserveAspect: false);

            Transform logScroll = FindOrCreateChild(logPanel, "DialogueLogScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            ConfigureRect(logScroll, new Vector2(0.04f, 0.07f), new Vector2(0.96f, 0.84f), Vector2.zero, Vector2.zero);
            ApplySprite(logScroll, "Assets/Phase10_Narrative/Resources/P10Art/Dialog/body_texture.png", preserveAspect: false);
            EnsureScrollbar(logScroll, "DialogueLogScrollbar");

            Transform orderPanel = FindOrCreateChild(surface, "CurrentOrderPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            ConfigureRect(orderPanel, new Vector2(0.60f, 0.30f), new Vector2(0.96f, 0.88f), Vector2.zero, Vector2.zero);
            ApplySprite(orderPanel, "Assets/Phase10_Narrative/Resources/P10Art/Dialog/dialogue_bg.png", preserveAspect: false);

            Transform orderClose = FindOrCreateButton(orderPanel, "CurrentOrderCloseButton");
            ConfigureRect(orderClose, new Vector2(0.76f, 0.91f), new Vector2(0.96f, 0.98f), Vector2.zero, Vector2.zero);
            ApplySprite(orderClose, "Assets/Phase10_Narrative/Resources/P10Art/Dialog/button_close.png", preserveAspect: false);

            Transform orderScroll = FindOrCreateChild(orderPanel, "CurrentOrderScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            ConfigureRect(orderScroll, new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.86f), Vector2.zero, Vector2.zero);
            ApplySprite(orderScroll, "Assets/Phase10_Narrative/Resources/P10Art/Dialog/body_texture.png", preserveAspect: false);
            EnsureScrollbar(orderScroll, "CurrentOrderScrollbar");

            Transform orderContent = EnsureScrollContent(orderScroll);
            Transform propStrip = FindOrCreateChild(orderContent, "P10_CurrentOrderPropStrip", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            ConfigurePropStrip(propStrip);
            ConfigurePropIcon(propStrip, "P10_PropIconReward", "Assets/Phase10_Narrative/Resources/P10Art/Props/reward_icon.png");
            ConfigurePropIcon(propStrip, "P10_PropIconLedger", "Assets/Phase10_Narrative/Resources/P10Art/Props/father_ledger.png");
            ConfigurePropIcon(propStrip, "P10_PropIconKilnTool", "Assets/Phase10_Narrative/Resources/P10Art/Props/old_kiln_tool.png");
            ConfigurePropIcon(propStrip, "P10_PropIconOrder", "Assets/Phase10_Narrative/Resources/P10Art/Props/ancient_order.png");

            Transform mappingRoot = FindOrCreateChild(uiRoot.transform, "P10_UIArtMappingRoot", typeof(RectTransform));
            mappingRoot.SetAsLastSibling();
            mappingRoot.gameObject.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            if (saveScene)
            {
                EditorSceneManager.SaveScene(scene);
            }

            Debug.Log(PassMessage);
            if (exitEditor)
            {
                EditorApplication.Exit(0);
            }
        }

        private static void EnsureRootUiComponents(GameObject surface)
        {
            if (surface.GetComponent<Canvas>() == null)
            {
                Canvas canvas = surface.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            if (surface.GetComponent<CanvasScaler>() == null)
            {
                CanvasScaler scaler = surface.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280f, 720f);
            }

            if (surface.GetComponent<GraphicRaycaster>() == null)
            {
                surface.AddComponent<GraphicRaycaster>();
            }
        }

        private static Transform FindOrCreateChild(Transform parent, string name, params Type[] componentTypes)
        {
            Transform child = parent.Find(name);
            if (child == null)
            {
                GameObject childObject = new GameObject(name, componentTypes);
                childObject.transform.SetParent(parent, false);
                child = childObject.transform;
            }

            for (int i = 0; i < componentTypes.Length; i++)
            {
                if (child.GetComponent(componentTypes[i]) == null)
                {
                    child.gameObject.AddComponent(componentTypes[i]);
                }
            }

            return child;
        }

        private static Transform FindOrCreateButton(Transform parent, string name)
        {
            Transform button = FindOrCreateChild(parent, name, typeof(RectTransform), typeof(Image), typeof(Button));
            if (button.GetComponent<Button>() == null)
            {
                button.gameObject.AddComponent<Button>();
            }

            return button;
        }

        private static void ConfigureRect(Transform target, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rect = target.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = target.gameObject.AddComponent<RectTransform>();
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void ApplySprite(Transform target, string assetPath, bool preserveAspect)
        {
            Image image = target.GetComponent<Image>();
            if (image == null)
            {
                image = target.gameObject.AddComponent<Image>();
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
            {
                throw new InvalidOperationException("Missing sprite asset for P10 UI art mapping: " + assetPath);
            }

            image.sprite = sprite;
            image.color = Color.white;
            image.type = Image.Type.Simple;
            image.preserveAspect = preserveAspect;
        }

        private static void MoveBeforeTextSlots(Transform panel, params Transform[] artTransforms)
        {
            for (int i = 0; i < artTransforms.Length; i++)
            {
                artTransforms[i].SetSiblingIndex(Mathf.Min(i + 1, panel.childCount - 1));
            }

            Transform speakerText = panel.Find("P10_DialogueSpeakerText");
            if (speakerText != null)
            {
                speakerText.SetAsLastSibling();
            }

            Transform bodyText = panel.Find("P10_DialogueBodyText");
            if (bodyText != null)
            {
                bodyText.SetAsLastSibling();
            }
        }

        private static void ConfigureActionBarLayout(HorizontalLayoutGroup layout)
        {
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.spacing = 16f;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        private static void ConfigureIconButton(Transform button, string spritePath)
        {
            ConfigureRect(button, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(72f, 72f);
            ApplySprite(button, spritePath, preserveAspect: true);

            LayoutElement layout = button.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = button.gameObject.AddComponent<LayoutElement>();
            }

            layout.minWidth = 72f;
            layout.minHeight = 72f;
            layout.preferredWidth = 72f;
            layout.preferredHeight = 72f;
            layout.flexibleWidth = 0f;
            layout.flexibleHeight = 0f;

            Text[] labels = button.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].text = string.Empty;
                labels[i].enabled = false;
            }
        }

        private static void EnsureScrollbar(Transform scrollRoot, string name)
        {
            Transform scrollbar = FindOrCreateChild(scrollRoot, name, typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            ConfigureRect(scrollbar, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-18f, 8f), new Vector2(0f, -8f));
            RectTransform scrollbarRect = scrollbar.GetComponent<RectTransform>();
            scrollbarRect.pivot = new Vector2(1f, 0.5f);
            scrollbarRect.sizeDelta = new Vector2(18f, 0f);
            ApplySprite(scrollbar, "Assets/Phase10_Narrative/Resources/P10Art/Log/scrollbar_track.png", preserveAspect: false);

            Transform slidingArea = FindOrCreateChild(scrollbar, "Sliding Area", typeof(RectTransform));
            ConfigureRect(slidingArea, Vector2.zero, Vector2.one, new Vector2(3f, 3f), new Vector2(-3f, -3f));

            Transform handle = FindOrCreateChild(slidingArea, "Handle", typeof(RectTransform), typeof(Image));
            ConfigureRect(handle, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            ApplySprite(handle, "Assets/Phase10_Narrative/Resources/P10Art/Log/scrollbar_handle.png", preserveAspect: false);

            Scrollbar scrollbarComponent = scrollbar.GetComponent<Scrollbar>();
            scrollbarComponent.direction = Scrollbar.Direction.BottomToTop;
            scrollbarComponent.targetGraphic = handle.GetComponent<Image>();
            scrollbarComponent.handleRect = handle.GetComponent<RectTransform>();
            scrollbarComponent.size = 0.35f;
            scrollbarComponent.value = 1f;
        }

        private static Transform EnsureScrollContent(Transform scrollRoot)
        {
            Transform viewport = FindOrCreateChild(scrollRoot, "Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            ConfigureRect(viewport, Vector2.zero, Vector2.one, new Vector2(18f, 16f), new Vector2(-34f, -16f));
            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            Transform content = FindOrCreateChild(viewport, "Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            ConfigureRect(content, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.pivot = new Vector2(0.5f, 1f);

            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 0f;
            layout.padding = new RectOffset(6, 10, 6, 14);

            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return content;
        }

        private static void ConfigurePropStrip(Transform strip)
        {
            ConfigureRect(strip, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            strip.SetAsFirstSibling();
            RectTransform rect = strip.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 1f);

            HorizontalLayoutGroup layout = strip.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 8f;
            layout.padding = new RectOffset(0, 0, 0, 8);

            LayoutElement layoutElement = strip.GetComponent<LayoutElement>();
            layoutElement.minHeight = 54f;
            layoutElement.preferredHeight = 54f;
        }

        private static void ConfigurePropIcon(Transform strip, string name, string spritePath)
        {
            Transform icon = FindOrCreateChild(strip, name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            RectTransform rect = icon.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(46f, 46f);
            ApplySprite(icon, spritePath, preserveAspect: true);

            LayoutElement layout = icon.GetComponent<LayoutElement>();
            layout.minWidth = 46f;
            layout.minHeight = 46f;
            layout.preferredWidth = 46f;
            layout.preferredHeight = 46f;
        }
    }
}
