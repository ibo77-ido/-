using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public static class RuntimeVisualQualityGuard
{
    private const string GuardObjectName = "Phase9_StartupVisualGuard";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Apply()
    {
        if (UnityEngine.Object.FindObjectOfType<Phase9LoadingOverlay>() != null)
        {
            return;
        }

        GameObject overlayObject = new GameObject("Phase9_LoadingOverlay");
        UnityEngine.Object.DontDestroyOnLoad(overlayObject);
        overlayObject.AddComponent<Phase9LoadingOverlay>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallStartupVisualGuard()
    {
        if (UnityEngine.Object.FindObjectOfType<Phase9StartupVisualGuard>() != null)
        {
            return;
        }

        GameObject guardObject = new GameObject(GuardObjectName);
        UnityEngine.Object.DontDestroyOnLoad(guardObject);
        guardObject.AddComponent<Phase9StartupVisualGuard>();
    }

    private static void ApplyRenderPipelineQuality(RenderPipelineAsset renderPipelineAsset)
    {
        if (!renderPipelineAsset)
        {
            return;
        }

        SetFloatProperty(renderPipelineAsset, "renderScale", 1.25f);
        SetIntPropertyMinimum(renderPipelineAsset, "msaaSampleCount", 4);
        SetBoolProperty(renderPipelineAsset, "supportsCameraOpaqueTexture", false);
        SetBoolProperty(renderPipelineAsset, "supportsCameraDepthTexture", false);
    }

    private static void DisableSoftPostProcessing(ScriptableRenderContext context, Camera camera)
    {
        Component cameraData = camera.GetComponent("UniversalAdditionalCameraData");
        if (cameraData)
        {
            SetBoolProperty(cameraData, "renderPostProcessing", false);
            SetBoolProperty(cameraData, "allowXRRendering", false);
            SetEnumProperty(cameraData, "antialiasing", 0);
        }

        camera.allowDynamicResolution = false;
    }

    private sealed class Phase9StartupVisualGuard : MonoBehaviour
    {
        private const float MinimumOrthographicSize = 4.5f;
        private const float CameraHeight = 10f;
        private const float GuardDurationSeconds = 6f;

        private readonly string[] worldUiPanelNames =
        {
            "Panel_Order",
            "Panel_Shape",
            "Panel_Glaze",
            "Panel_Firing",
            "Panel_Result",
            "Panel_Debug",
            "Text_Title"
        };

        private float stopGuardAt;
        private bool screenshotRequested;
        private bool lateUpdateLogged;

        private void Start()
        {
            stopGuardAt = Time.realtimeSinceStartup + GuardDurationSeconds;
            StartCoroutine(RunStartupDiagnostics());
        }

        private void LateUpdate()
        {
            if (Time.realtimeSinceStartup > stopGuardAt)
            {
                enabled = false;
                return;
            }

            ApplyVisualGuard("LateUpdate", !lateUpdateLogged);
            lateUpdateLogged = true;
        }

        private IEnumerator RunStartupDiagnostics()
        {
            yield return null;
            ApplyVisualGuard("frame-1", true);
            LogVisualState("frame-1");

            yield return new WaitForSecondsRealtime(1f);
            ApplyVisualGuard("frame-1s", true);
            LogVisualState("frame-1s");
            CaptureStartupScreenshot();
        }

        private void ApplyVisualGuard(string reason, bool shouldLog)
        {
            Camera camera = ResolveMainCamera();
            Bounds rendererBounds;
            bool hasRendererBounds = TryGetWorldRendererBounds(out rendererBounds, out int rendererCount);
            Transform target = ResolvePrimaryTarget();

            if (camera != null)
            {
                ConfigureCamera(camera, rendererBounds, hasRendererBounds, target);
                ConfigureCameraFollow(camera, target, ResolveMapRoot(), hasRendererBounds);
            }

            HideInactiveBridgePanels();
            if (hasRendererBounds)
            {
                HideLoadingOverlay();
            }

            if (shouldLog)
            {
                Debug.Log("[RuntimeVisualQualityGuard] Applied Phase9 startup visual guard: " + reason
                    + " | worldRenderers=" + rendererCount
                    + " | hasWorldBounds=" + hasRendererBounds);
            }
        }

        private static Camera ResolveMainCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                Camera[] cameras = UnityEngine.Object.FindObjectsOfType<Camera>(true);
                for (int i = 0; i < cameras.Length; i++)
                {
                    if (camera == null || cameras[i].depth > camera.depth)
                    {
                        camera = cameras[i];
                    }
                }
            }

            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.gameObject.SetActive(true);
            camera.enabled = true;
            if (!camera.CompareTag("MainCamera"))
            {
                camera.tag = "MainCamera";
            }

            return camera;
        }

        private static void ConfigureCamera(Camera camera, Bounds bounds, bool hasBounds, Transform target)
        {
            camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(camera.orthographicSize, MinimumOrthographicSize);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.22f, 0.22f, 0.22f, 1f);
            camera.cullingMask = ~0;
            camera.rect = new Rect(0f, 0f, 1f, 1f);
            camera.targetTexture = null;
            camera.allowDynamicResolution = false;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = Mathf.Max(camera.farClipPlane, 100f);

            Vector3 focus = hasBounds ? bounds.center : Vector3.zero;
            if (target != null && (!hasBounds || bounds.Contains(target.position)))
            {
                focus = target.position;
            }

            float cameraY = hasBounds
                ? Mathf.Max(CameraHeight, bounds.max.y + CameraHeight)
                : Mathf.Max(CameraHeight, camera.transform.position.y);
            camera.transform.position = new Vector3(focus.x, cameraY, focus.z);
            camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static void ConfigureCameraFollow(Camera camera, Transform target, Transform mapRoot, bool hasRendererBounds)
        {
            Component follow = camera.GetComponent("CameraFollow2D");
            if (follow == null)
            {
                return;
            }

            SetPrivateField(follow, "target", target);
            SetPrivateField(follow, "boundsRoot", mapRoot);
            SetPrivateField(follow, "followXZPlane", true);
            SetPrivateField(follow, "orthographicSize", Mathf.Max(camera.orthographicSize, MinimumOrthographicSize));
            if (!hasRendererBounds)
            {
                SetPrivateField(follow, "clampToBounds", false);
            }
        }

        private static Transform ResolvePrimaryTarget()
        {
            GameObject target = GameObject.Find("HeroineRoot");
            if (target == null)
            {
                target = GameObject.Find("\u5973\u4E3B");
            }
            if (target == null)
            {
                target = GameObject.Find("GirlModel");
            }

            if (target != null)
            {
                return target.transform;
            }

            PlayerCharacter player = UnityEngine.Object.FindObjectOfType<PlayerCharacter>(true);
            return player != null ? player.transform : null;
        }

        private static Transform ResolveMapRoot()
        {
            GameObject root = GameObject.Find("\u9759\u6001\u5C42");
            if (root == null)
            {
                root = GameObject.Find("Walkable");
            }
            if (root == null)
            {
                root = GameObject.Find("_MapRoot");
            }
            if (root == null)
            {
                root = GameObject.Find("MapRoot");
            }

            return root != null ? root.transform : null;
        }

        private static bool TryGetWorldRendererBounds(out Bounds bounds, out int rendererCount)
        {
            bounds = new Bounds(Vector3.zero, Vector3.zero);
            bool hasBounds = false;
            rendererCount = 0;
            Renderer[] renderers = UnityEngine.Object.FindObjectsOfType<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                if (!renderer.gameObject.activeInHierarchy || renderer.GetComponentInParent<Canvas>(true) != null)
                {
                    continue;
                }

                rendererCount++;
                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds;
        }

        private void HideInactiveBridgePanels()
        {
            for (int i = 0; i < worldUiPanelNames.Length; i++)
            {
                GameObject panel = GameObject.Find(worldUiPanelNames[i]);
                if (panel != null)
                {
                    panel.SetActive(false);
                }
            }

            GameObject gameplayCanvasRoot = GameObject.Find("GameplayCanvasRoot");
            if (gameplayCanvasRoot == null)
            {
                return;
            }

            // Keep raycasters intact so Phase3 panels remain clickable after the bridge opens them.
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name == childName)
                {
                    return children[i];
                }
            }

            return null;
        }

        private void LogVisualState(string label)
        {
            Camera[] cameras = UnityEngine.Object.FindObjectsOfType<Camera>(true);
            Renderer[] renderers = UnityEngine.Object.FindObjectsOfType<Renderer>(true);
            Canvas[] canvases = UnityEngine.Object.FindObjectsOfType<Canvas>(true);
            Bounds bounds;
            bool hasBounds = TryGetWorldRendererBounds(out bounds, out int worldRendererCount);

            Debug.Log("[RuntimeVisualQualityGuard] Visual state " + label
                + " | cameras=" + cameras.Length
                + " | renderers=" + renderers.Length
                + " | worldRenderers=" + worldRendererCount
                + " | canvases=" + canvases.Length
                + " | hasWorldBounds=" + hasBounds
                + " | worldBounds=" + (hasBounds ? BoundsToString(bounds) : "<none>"));

            int loggedRendererCount = 0;
            for (int i = 0; i < renderers.Length && loggedRendererCount < 12; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.GetComponentInParent<Canvas>(true) != null)
                {
                    continue;
                }

                loggedRendererCount++;
                Debug.Log("[RuntimeVisualQualityGuard] Renderer " + renderer.name
                    + " | type=" + renderer.GetType().Name
                    + " | active=" + renderer.gameObject.activeInHierarchy
                    + " | enabled=" + renderer.enabled
                    + " | bounds=" + BoundsToString(renderer.bounds));
            }

            for (int i = 0; i < cameras.Length; i++)
            {
                Camera camera = cameras[i];
                Debug.Log("[RuntimeVisualQualityGuard] Camera " + camera.name
                    + " | active=" + camera.gameObject.activeInHierarchy
                    + " | enabled=" + camera.enabled
                    + " | tag=" + camera.tag
                    + " | position=" + camera.transform.position
                    + " | rotation=" + camera.transform.eulerAngles
                    + " | ortho=" + camera.orthographic
                    + " | size=" + camera.orthographicSize);
            }

            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                Debug.Log("[RuntimeVisualQualityGuard] Canvas " + canvas.name
                    + " | active=" + canvas.gameObject.activeInHierarchy
                    + " | enabled=" + canvas.enabled
                    + " | mode=" + canvas.renderMode
                    + " | sorting=" + canvas.sortingOrder);
            }
        }

        private void CaptureStartupScreenshot()
        {
            if (screenshotRequested)
            {
                return;
            }

            screenshotRequested = true;
            string path = Path.Combine(Application.persistentDataPath, "phase9-startup-screenshot.png");
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log("[RuntimeVisualQualityGuard] Requested startup screenshot: " + path);
        }

        private static string BoundsToString(Bounds bounds)
        {
            return "center=" + bounds.center + ", size=" + bounds.size + ", min=" + bounds.min + ", max=" + bounds.max;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            if (target == null)
            {
                return;
            }

            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }

    private sealed class Phase9LoadingOverlay : MonoBehaviour
    {
        private Canvas canvas;

        private void Awake()
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;

            gameObject.AddComponent<CanvasScaler>();

            GameObject background = new GameObject("Background", typeof(RectTransform));
            background.transform.SetParent(transform, false);
            Image image = background.AddComponent<Image>();
            image.color = new Color(0.13f, 0.12f, 0.10f, 1f);
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            GameObject textObject = new GameObject("Text", typeof(RectTransform));
            textObject.transform.SetParent(background.transform, false);
            Text text = textObject.AddComponent<Text>();
            text.text = "\u52a0\u8f7d\u7a91\u5382\u5730\u56fe\u4e2d...";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 36;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.95f, 0.86f, 0.68f, 1f);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        public void Hide()
        {
            if (canvas != null)
            {
                canvas.enabled = false;
            }

            gameObject.SetActive(false);
        }
    }

    private static void HideLoadingOverlay()
    {
        Phase9LoadingOverlay overlay = UnityEngine.Object.FindObjectOfType<Phase9LoadingOverlay>();
        if (overlay != null)
        {
            overlay.Hide();
        }
    }

    private static void SetFloatProperty(object target, string propertyName, float value)
    {
        System.Reflection.PropertyInfo property = target.GetType().GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(target, value, null);
        }
    }

    private static void SetIntPropertyMinimum(object target, string propertyName, int minimum)
    {
        System.Reflection.PropertyInfo property = target.GetType().GetProperty(propertyName);
        if (property != null && property.CanRead && property.CanWrite)
        {
            int current = (int)property.GetValue(target, null);
            property.SetValue(target, Mathf.Max(current, minimum), null);
        }
    }

    private static void SetBoolProperty(object target, string propertyName, bool value)
    {
        System.Reflection.PropertyInfo property = target.GetType().GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(target, value, null);
        }
    }

    private static void SetEnumProperty(object target, string propertyName, int enumValue)
    {
        System.Reflection.PropertyInfo property = target.GetType().GetProperty(propertyName);
        if (property != null && property.CanWrite && property.PropertyType.IsEnum)
        {
            object value = System.Enum.ToObject(property.PropertyType, enumValue);
            property.SetValue(target, value, null);
        }
    }
}
