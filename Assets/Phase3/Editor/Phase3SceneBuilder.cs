using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Phase3SceneBuilder
{
    private const string ScenePath = "Assets/Phase3/Scenes/Phase3_Prototype.unity";

    [MenuItem("Phase3/Sync Scene (Ensure Mode)")]
    public static void SyncPhase3Scene()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var created = new List<string>();

        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Phase3Builder] No Canvas found. Aborting.");
            return;
        }

        Transform canvasTf = canvas.transform;

        // 1. Manager GameObjects
        created.AddRange(EnsureManagerObject("OrderManager", typeof(OrderManager)));
        created.AddRange(EnsureManagerObject("ShapeSystem", typeof(ShapeSystem)));
        created.AddRange(EnsureManagerObject("GlazeSystem", typeof(GlazeSystem)));
        created.AddRange(EnsureManagerObject("FiringSystem", typeof(FiringSystem)));
        created.AddRange(EnsureManagerObject("ResultSystem", typeof(ResultSystem)));
        created.AddRange(EnsureManagerObject("GameManager", typeof(GameManager)));

        // 2. Panel controls
        created.AddRange(EnsureOrderPanelControls(canvasTf));
        created.AddRange(EnsureShapePanelControls(canvasTf));
        created.AddRange(EnsureGlazePanelControls(canvasTf));
        created.AddRange(EnsureFiringPanelControls(canvasTf));
        created.AddRange(EnsureResultPanelControls(canvasTf));

        EditorSceneManager.SaveScene(scene);

        if (created.Count == 0)
        {
            Debug.Log("[Phase3Builder] Scene already in sync. Nothing to create.");
        }
        else
        {
            Debug.Log($"[Phase3Builder] Created {created.Count} item(s):");
            foreach (var item in created) Debug.Log($"  + {item}");
        }
    }

    // ─── Manager Objects ──────────────────────────────────────────

    private static List<string> EnsureManagerObject(string name, System.Type scriptType)
    {
        var created = new List<string>();
        GameObject obj = GameObject.Find(name);
        if (obj == null)
        {
            obj = new GameObject(name);
            created.Add($"GameObject: {name}");
        }
        if (obj.GetComponent(scriptType) == null)
        {
            obj.AddComponent(scriptType);
            created.Add($"Component: {scriptType.Name} on {name}");
        }
        return created;
    }

    // ─── Panel_Order ──────────────────────────────────────────────

    private static List<string> EnsureOrderPanelControls(Transform canvasTf)
    {
        var created = new List<string>();
        Transform panel = canvasTf.Find("Panel_Order");
        if (panel == null) return created;

        if (panel.Find("Btn_Accept") == null)
        {
            var btn = CreateButtonChild(panel, "Btn_Accept", "接单", 20);
            SetStretchAnchors(btn, new Vector2(0.7f, 0.1f), new Vector2(0.98f, 0.9f));
            created.Add("Btn_Accept in Panel_Order");
        }
        return created;
    }

    // ─── Panel_Shape ──────────────────────────────────────────────

    private static List<string> EnsureShapePanelControls(Transform canvasTf)
    {
        var created = new List<string>();
        Transform panel = canvasTf.Find("Panel_Shape");
        if (panel == null) return created;

        if (panel.Find("Btn_ToGlaze") == null)
        {
            var btn = CreateButtonChild(panel, "Btn_ToGlaze", "配釉 →", 20);
            SetStretchAnchors(btn, new Vector2(0.7f, 0.1f), new Vector2(0.98f, 0.9f));
            created.Add("Btn_ToGlaze in Panel_Shape");
        }
        return created;
    }

    // ─── Panel_Glaze ──────────────────────────────────────────────

    private static List<string> EnsureGlazePanelControls(Transform canvasTf)
    {
        var created = new List<string>();
        Transform panel = canvasTf.Find("Panel_Glaze");
        if (panel == null) return created;

        if (panel.Find("Btn_ToFiring") == null)
        {
            var btn = CreateButtonChild(panel, "Btn_ToFiring", "烧制 →", 20);
            SetStretchAnchors(btn, new Vector2(0.7f, 0.1f), new Vector2(0.98f, 0.9f));
            created.Add("Btn_ToFiring in Panel_Glaze");
        }
        return created;
    }

    // ─── Panel_Firing ─────────────────────────────────────────────

    private static List<string> EnsureFiringPanelControls(Transform canvasTf)
    {
        var created = new List<string>();
        Transform panel = canvasTf.Find("Panel_Firing");
        if (panel == null) return created;

        // ─── 美术面板检测 ─────────────────────────────
        // 若已接入美术版烧制面板（存在 ArtRoot_Firing），跳过旧占位控件创建
        if (panel.Find("ArtRoot_Firing") != null)
        {
            if (panel.GetComponent<FiringPanelController>() == null)
            {
                panel.gameObject.AddComponent<FiringPanelController>();
                created.Add("Component: FiringPanelController on Panel_Firing (art mode)");
            }
            return created;
        }
        // ─── 以下为旧占位控件创建逻辑 ─────────────────

        // Resize panel if needed (820x120 → 820x160)
        var panelRect = panel.GetComponent<RectTransform>();
        if (panelRect.sizeDelta.y < 150f)
        {
            panelRect.sizeDelta = new Vector2(820f, 160f);
            created.Add("Panel_Firing resized to 820x160");
        }

        // Row 1: info texts (top half)
        if (panel.Find("Text_Zone") == null)
        {
            var obj = CreateTextChild(panel, "Text_Zone", "正常", 20, TextAnchor.MiddleLeft);
            SetStretchAnchors(obj, new Vector2(0.35f, 0.55f), new Vector2(0.55f, 0.8f));
            created.Add("Text_Zone in Panel_Firing");
        }
        if (panel.Find("Text_FireScore") == null)
        {
            var obj = CreateTextChild(panel, "Text_FireScore", "Fire Score: 0", 20, TextAnchor.MiddleLeft);
            SetStretchAnchors(obj, new Vector2(0.55f, 0.55f), new Vector2(0.75f, 0.8f));
            created.Add("Text_FireScore in Panel_Firing");
        }
        if (panel.Find("Text_Status") == null)
        {
            var obj = CreateTextChild(panel, "Text_Status", "烧制中...", 20, TextAnchor.MiddleLeft);
            SetStretchAnchors(obj, new Vector2(0.75f, 0.55f), new Vector2(0.95f, 0.8f));
            created.Add("Text_Status in Panel_Firing");
        }

        // Row 2: controls (bottom half)
        if (panel.Find("Slider_Wind") == null)
        {
            var obj = CreateSliderChild(panel, "Slider_Wind");
            SetStretchAnchors(obj, new Vector2(0.02f, 0.25f), new Vector2(0.28f, 0.5f));
            created.Add("Slider_Wind in Panel_Firing");
        }
        if (panel.Find("Btn_Fuel") == null)
        {
            var obj = CreateButtonChild(panel, "Btn_Fuel", "加燃料", 18);
            SetStretchAnchors(obj, new Vector2(0.3f, 0.25f), new Vector2(0.45f, 0.5f));
            created.Add("Btn_Fuel in Panel_Firing");
        }
        if (panel.Find("Btn_Window") == null)
        {
            var obj = CreateButtonChild(panel, "Btn_Window", "Open", 18);
            SetStretchAnchors(obj, new Vector2(0.47f, 0.25f), new Vector2(0.62f, 0.5f));
            created.Add("Btn_Window in Panel_Firing");
        }
        if (panel.Find("Btn_Stop") == null)
        {
            var obj = CreateButtonChild(panel, "Btn_Stop", "停火", 18);
            SetStretchAnchors(obj, new Vector2(0.64f, 0.25f), new Vector2(0.79f, 0.5f));
            created.Add("Btn_Stop in Panel_Firing");
        }
        if (panel.Find("Btn_OpenKiln") == null)
        {
            var obj = CreateButtonChild(panel, "Btn_OpenKiln", "开窑", 20);
            SetStretchAnchors(obj, new Vector2(0.81f, 0.05f), new Vector2(0.98f, 0.5f));
            created.Add("Btn_OpenKiln in Panel_Firing");
        }

        // Ensure FiringPanelController is attached
        if (panel.GetComponent<FiringPanelController>() == null)
        {
            panel.gameObject.AddComponent<FiringPanelController>();
            created.Add("Component: FiringPanelController on Panel_Firing");
        }

        return created;
    }

    // ─── Panel_Result ─────────────────────────────────────────────

    private static List<string> EnsureResultPanelControls(Transform canvasTf)
    {
        var created = new List<string>();
        Transform panel = canvasTf.Find("Panel_Result");
        if (panel == null) return created;

        if (panel.Find("Img_ResultStage") != null)
        {
            if (panel.GetComponent<ResultPanelController>() == null)
            {
                panel.gameObject.AddComponent<ResultPanelController>();
                created.Add("Component: ResultPanelController on Panel_Result");
            }
            return created;
        }

        // Resize panel if needed (820x120 → 820x160)
        var panelRect = panel.GetComponent<RectTransform>();
        if (panelRect.sizeDelta.y < 150f)
        {
            panelRect.sizeDelta = new Vector2(820f, 160f);
            created.Add("Panel_Result resized to 820x160");
        }

        // Grade label - top center
        if (panel.Find("Text_Grade") == null)
        {
            var obj = CreateTextChild(panel, "Text_Grade", "品级", 32, TextAnchor.MiddleCenter);
            SetStretchAnchors(obj, new Vector2(0f, 0.6f), new Vector2(1f, 1f));
            created.Add("Text_Grade in Panel_Result");
        }

        // Three scores
        if (panel.Find("Text_ShapeMatchResult") == null)
        {
            var obj = CreateTextChild(panel, "Text_ShapeMatchResult", "器型匹配：0%", 20, TextAnchor.MiddleLeft);
            SetStretchAnchors(obj, new Vector2(0.02f, 0.35f), new Vector2(0.48f, 0.6f));
            created.Add("Text_ShapeMatchResult in Panel_Result");
        }
        if (panel.Find("Text_GlazeMatchResult") == null)
        {
            var obj = CreateTextChild(panel, "Text_GlazeMatchResult", "釉料匹配：0%", 20, TextAnchor.MiddleLeft);
            SetStretchAnchors(obj, new Vector2(0.02f, 0.1f), new Vector2(0.48f, 0.35f));
            created.Add("Text_GlazeMatchResult in Panel_Result");
        }
        if (panel.Find("Text_FireScoreResult") == null)
        {
            var obj = CreateTextChild(panel, "Text_FireScoreResult", "火候评分：0%", 20, TextAnchor.MiddleLeft);
            SetStretchAnchors(obj, new Vector2(0.52f, 0.35f), new Vector2(0.98f, 0.6f));
            created.Add("Text_FireScoreResult in Panel_Result");
        }

        // Rewards
        if (panel.Find("Text_SilverReward") == null)
        {
            var obj = CreateTextChild(panel, "Text_SilverReward", "银两：0", 20, TextAnchor.MiddleLeft);
            SetStretchAnchors(obj, new Vector2(0.52f, 0.1f), new Vector2(0.98f, 0.35f));
            created.Add("Text_SilverReward in Panel_Result");
        }
        if (panel.Find("Text_ReputationReward") == null)
        {
            var obj = CreateTextChild(panel, "Text_ReputationReward", "声望：0", 20, TextAnchor.MiddleLeft);
            SetStretchAnchors(obj, new Vector2(0.02f, -0.15f), new Vector2(0.48f, 0.1f));
            created.Add("Text_ReputationReward in Panel_Result");
        }

        // Next order button
        if (panel.Find("Btn_NextOrder") == null)
        {
            var obj = CreateButtonChild(panel, "Btn_NextOrder", "接下一单", 20);
            SetStretchAnchors(obj, new Vector2(0.52f, -0.15f), new Vector2(0.98f, 0.1f));
            created.Add("Btn_NextOrder in Panel_Result");
        }

        // Ensure ResultPanelController is attached
        if (panel.GetComponent<ResultPanelController>() == null)
        {
            panel.gameObject.AddComponent<ResultPanelController>();
            created.Add("Component: ResultPanelController on Panel_Result");
        }

        // Deactivate by default (GameManager controls visibility)
        panel.gameObject.SetActive(false);

        return created;
    }

    // ─── Factory Helpers ──────────────────────────────────────────

    private static GameObject CreateTextChild(Transform parent, string name, string content, int fontSize, TextAnchor alignment)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        obj.transform.SetParent(parent, false);
        obj.layer = LayerMask.NameToLayer("UI");

        var text = obj.GetComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        return obj;
    }

    private static GameObject CreateButtonChild(Transform parent, string name, string label, int fontSize)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        obj.layer = LayerMask.NameToLayer("UI");

        obj.GetComponent<Image>().color = new Color(0.45f, 0.35f, 0.2f, 1f);

        var labelObj = CreateTextChild(obj.transform, "Text_Label", label, fontSize, TextAnchor.MiddleCenter);
        var labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return obj;
    }

    private static GameObject CreateSliderChild(Transform parent, string name)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Slider));
        obj.transform.SetParent(parent, false);
        obj.layer = LayerMask.NameToLayer("UI");

        obj.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f);

        var slider = obj.GetComponent<Slider>();

        // Background
        var bg = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bg.transform.SetParent(obj.transform, false);
        bg.layer = LayerMask.NameToLayer("UI");
        bg.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        slider.targetGraphic = bg.GetComponent<Image>();

        // Fill Area
        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(obj.transform, false);
        var fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        fill.layer = LayerMask.NameToLayer("UI");
        fill.GetComponent<Image>().color = Color.green;
        var fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        slider.fillRect = fill.GetComponent<RectTransform>();

        // Handle Slide Area
        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(obj.transform, false);
        var handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.sizeDelta = Vector2.zero;

        var handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        handle.layer = LayerMask.NameToLayer("UI");
        handle.GetComponent<Image>().color = Color.white;
        var handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20f, 20f);
        slider.handleRect = handle.GetComponent<RectTransform>();

        return obj;
    }

    private static void SetStretchAnchors(GameObject obj, Vector2 anchorMin, Vector2 anchorMax)
    {
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
