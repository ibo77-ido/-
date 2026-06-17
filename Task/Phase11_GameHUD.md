# Phase 11 — 游戏主界面 HUD 层

## 1. 概述

在现有 Phase6 世界层之上增加一个**纯引用型交互层**，提供三大功能：

| 功能 | 说明 | 阶段 |
|---|---|---|
| **快捷入口栏** | 底部居中 6 按钮 —— 一键寻路到建筑（4 个自动交互 + 2 个纯寻路） | Phase11A |
| **玩家状态栏（占位）** | 左上角 HUD —— 显示占位值，不接入真实经济数据 | Phase11A |
| **地图缩放 / 平移 / 智能复位** | 滚轮缩放 / 右键拖拽 / 主角移动时相机平滑回到追踪视角 | Phase11B |

核心原则不变：**不修改 Phase3 / Phase6 / Phase8 / CameraFollow2D 任何现有 .cs 文件**，仅通过 `FindObjectOfType<T>()` 查找 + 调用已有 public 方法。所有 Phase11 组件自持配置，不依赖下层 private 字段。

---

## 2. 架构定位

```
┌──────────────────────────────────────────────────────────────┐
│  Phase 11  HUD 层  ( 纯引用，不修改下层 )                      │
│                                                              │
│  ┌──────────────┐  ┌──────────────────┐  ┌──────────────────┐│
│  │PlayerStatusBar│  │   HUDQuickBar    │  │CameraZoomCtrl    ││
│  │  (左上角)     │  │  (底部 6按钮)    │  │(Phase11B,自配置) ││
│  └──────┬───────┘  └────────┬─────────┘  └────────┬─────────┘│
│         │ 读数据             │ 调方法               │ 操控相机 │
├─────────┼───────────────────┼─────────────────────┼──────────┤
│  Phase 6  世界层  ( 不动 )   │                      │           │
│         │                   │                      │           │
│  ┌──────┴──────┐  ┌─────────┴────────┐  ┌─────────┴────────┐ │
│  │Phase6GM     │  │PlayerCharacter    │  │CameraFollow2D    │ │
│  │.CanMove()   │  │.SetDestination()  │  │.enabled (public) │ │
│  │.CurrentState│  │.StopMoving()       │  │(其他字段为private)│ │
│  └─────────────┘  └────────┬─────────┘  └──────────────────┘ │
│                     ┌──────┴──────┐                          │
│                     │MovementCtrl  │                          │
│                     │.IsMoving()   │                          │
│                     │.HasReached() │                          │
│                     ├─────────────┤                          │
│                     │InteractionCtrl                         │
│                     │.TryInteract()                          │
│                     ├─────────────┤                          │
│                     │Workstation[]│                          │
│                     │.InteractionPoint                       │
│                     │.AreaType    │                          │
│                     └─────────────┘                          │
└──────────────────────────────────────────────────────────────┘
```

> **关键修正**：CameraFollow2D 的 `framingOffset` / `orthographicSize` 均为 `private [SerializeField]`，外部无法直接访问。Phase11B 自持 `defaultFramingOffset` / `defaultOrthographicSize` 配置，不依赖 CameraFollow2D 的私有字段。

---

## 3. UI 布局

```
┌──────────────────────────────────────────────────┐
│  💰 银两: --    ⭐ 声望: --                        │
│  📋 当前订单: --                                   │  ← PlayerStatusBar (占位)
│                                                    │
│                                                    │
│                                                    │
│                                                    │
│              [ 游戏世界 3D 视图 ]                   │
│      滚轮缩放  ·  右键拖拽  ·  主角移动时复位       │  ← CameraZoomController (Phase11B)
│                                                    │
│                                                    │
│                                                    │
│  ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐
│  │ 📋   │ │ 🏺   │ │ 🎨   │ │ 🔥   │ │ 📦   │ │ 🪨   │  ← HUDQuickBar
│  │接单板│ │拉坯台│ │配釉台│ │ 窑炉 │ │ 仓库 │ │材料库│     (Phase11A)
│  └──────┘ └──────┘ └──────┘ └──────┘ └──────┘ └──────┘
│      ⇧ 自动交互      ⇧          ⇧ 纯寻路              │
└──────────────────────────────────────────────────┘
```

| 区域 | 定位方式 | Canvas 类型 |
|---|---|---|
| PlayerStatusBar | 左上角锚定 | Screen Space - Overlay |
| HUDQuickBar | 底部居中锚定 | Screen Space - Overlay |

---

## 4. 核心交互流程

### 4.1 快捷按钮一键直达（Phase11A）

```
用户点击按钮(AreaType target, bool autoInteract)
│
├─ [1] 状态守卫
│     if (!Phase6GameManager.CanMove()) → 忽略点击，直接返回
│
├─ [2] 查找目标 Workstation
│     Workstation[] ws = FindObjectsOfType<Workstation>()
│     ws.FirstOrDefault(w => w.AreaType == target)
│     若未找到 → 忽略点击
│
├─ [3] 判断是否自动交互
│     if (autoInteract && interactionController.IsInRange
│         && interactionController.NearestWorkstation?.AreaType == target)
│         → 跳过寻路，直接 TryInteract()，返回
│     if (!autoInteract) → [跳到步骤5，不带 autoInteract 标记]
│
├─ [4] 取消上一个 pending
│     if (pendingAutoInteract != null) playerCharacter.StopMoving()
│     pendingAutoInteract = target
│     pendingStartTime = Time.time
│
├─ [5] 发起寻路
│     Vector3 dest = targetWS.InteractionPoint?.transform.position
│         ?? targetWS.transform.position
│     playerCharacter.SetDestination(dest)
│
└─ [6] 进入等待闭环（见 4.2）
```

### 4.2 到达检测闭环（Phase11A Update 驱动）

```
HUDQuickBar.Update()
│
├─ if (pendingAutoInteract == null) → return
│
├─ [超时保护] 15s 超时 → CancelPending("无法到达目标建筑")
│
├─ [被打断检测]
│     if (!movementController.IsMoving() && !movementController.HasReachedDestination())
│         → 静默取消
│
├─ [到达 + 自动交互]
│     if (movementController.HasReachedDestination())
│         if (buttonData.autoInteract) → interactionController.TryInteract()
│         pendingAutoInteract = null
│
└─ [寻路中] 对应按钮高亮（视觉反馈）
```

### 4.3 地图缩放 / 平移 / 智能复位（Phase11B）

CameraZoomController 三态机：

```
  ┌──────────► [跟踪模式]  ◄─────────────────┐
  │   CameraFollow2D.enabled = true           │
  │   相机紧追主角                              │
  │                                           │
  │   滚轮/右键拖拽         主角开始移动(IsMoving)
  │   ↓                   ↓                   │
  │ [手动浏览模式] ──→ [复位过渡中] ──────────┘
  │   Follow2D=false     Follow2D=false
  │   orthoSize自控       Lerp→自持默认值
  │   位置自控             点鼠标→取消复位
  └────────────────────   回到手动浏览
```

#### 关键设计决策（与 CameraFollow2D 零侵入）

| 问题 | CameraFollow2D 现状 | Phase11B 方案 |
|---|---|---|
| framingOffset | `private [SerializeField]` | Phase11B 自持 `[SerializeField] defaultFramingOffset`，Inspector 独立配置 |
| orthographicSize | `private [SerializeField]` | Phase11B 自持 `[SerializeField] defaultOrthographicSize` |
| boundsRoot | `private [SerializeField]` | Phase11B MVP 不 clamp，或自持 `[SerializeField] mapBounds` |
| enabled | `public` (MonoBehaviour) | 通过 `.enabled = false` 切换 |

#### 手动浏览模式

```
触发：滚轮滚动 OR 右键拖拽
│
├─ 进入时：CameraFollow2D.enabled = false
│     读取 cam.orthographicSize（当前值，CameraFollow2D.ConfigureCamera 最后一次设的）
│     作为手动模式的起始 orthoSize
│
├─ 滚轮缩放
│     cam.orthographicSize = Mathf.Clamp(
│         currentSize - scrollDelta * zoomSensitivity,
│         minZoom, maxZoom)
│     注：minZoom/maxZoom 是 Phase11B 自持配置
│
├─ 右键拖拽平移
│     Vector3 worldDelta = ScreenToWorldDelta(mouseDelta)
│     cam.transform.position -= worldDelta
│     MVP 阶段不 clamp（不写死地图边界）
│
└─ 退出：主角开始移动时进入"复位过渡"
```

#### 复位过渡中

```
触发：MovementController.IsMoving() == true && 当前不在跟踪模式
│
├─ 进入时：记录 resetOriginPos / resetOriginSize
│
├─ 每帧 Lerp 到自持的 default 参数
│     targetPos = playerTransform.position + defaultFramingOffset
│     cam.transform.position = Vector3.Lerp(resetOriginPos, targetPos, t)
│     cam.orthographicSize = Mathf.Lerp(resetOriginSize, defaultOrthographicSize, t)
│
├─ [取消复位] 过渡中任意鼠标点击 / 滚轮
│     → 停止 Lerp，保持当前位置，回到手动浏览模式
│
└─ [完成复位] t >= 1.0
│     → CameraFollow2D.enabled = true，回到跟踪模式
```

### 4.4 状态栏更新（Phase11A 占位）

每 0.5 秒低频率刷新，全部显示占位值 `"--"`。Phase13 再接入真实数据。

---

## 5. 文件清单

### Phase11A 新增文件

```
Assets/Phase11/
├── Prefabs/
│   └── HUDCanvas.prefab              # Canvas 预制体（PlayerStatusBar + HUDQuickBar）
├── Scripts/
│   ├── HUDQuickBar.cs                # 快捷入口栏主控
│   ├── PlayerStatusBar.cs            # 玩家状态栏（占位）
│   ├── HUDButtonData.cs              # 按钮数据结构
│   └── QuickBarButton.cs             # 按钮 UI 组件
└── Data/
    └── HUDQuickBarConfigSO.cs        # 快捷栏配置 SO
```

### Phase11B 新增文件

```
Assets/Phase11/
└── Scripts/
    └── CameraZoomController.cs       # 地图缩放/平移/智能复位（自持配置）
```

### 不修改任何现有文件

| 文件 | 操作 | 原因 |
|---|---|---|
| `CameraFollow2D.cs` | **不修改** | 通过 `.enabled = false` 覆盖；所有参数由 Phase11B 自持 |
| `Phase6GameManager.cs` | **不修改** | `FindObjectOfType<>()` + 调 `CanMove()` / `CurrentState` |
| `PlayerCharacter.cs` | **不修改** | 调 `SetDestination()` / `StopMoving()` |
| `MovementController.cs` | **不修改** | 调 `IsMoving()` / `HasReachedDestination()` / `Stop()` |
| `InteractionController.cs` | **不修改** | 调 `TryInteract()` / 读 `IsInRange` / `NearestWorkstation` |
| `Workstation.cs` | **不修改** | 读 `AreaType` / `InteractionPoint` |
| `InputManager.cs` | **不修改** | `IsPointerOverGameObject()` 天然互斥 |
| `Phase3/Scripts/*` | **不修改** | 暂不读取 —— Phase13 再接入 |
| `SampleScene.unity` | **不修改** | 仅挂载 prefab + 组件，不改场景已有内容 |

---

## 6. 详细规格

### 6.1 HUDButtonData

```csharp
// Assets/Phase11/Scripts/HUDButtonData.cs
using UnityEngine;

[System.Serializable]
public class HUDButtonData
{
    public AreaType areaType;
    public string label;
    public Sprite icon;
    public string tooltip;
    public bool autoInteract = true;    // false → 纯寻路，到达后不触发交互
}
```

### 6.2 HUDQuickBarConfigSO

```csharp
// Assets/Phase11/Data/HUDQuickBarConfigSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "HUDQuickBarConfig", menuName = "Phase11/HUD QuickBar Config")]
public class HUDQuickBarConfigSO : ScriptableObject
{
    [Header("Button Definitions")]
    public HUDButtonData[] buttons;    // 6 个按钮定义

    [Header("Timing")]
    [Range(5f, 30f)] public float autoInteractTimeout = 15f;
}
```

> 默认 6 个按钮映射：

| 索引 | areaType | label | autoInteract | 说明 |
|---|---|---|---|---|
| 0 | Order | 接单板 | ✅ true | 到达后自动打开订单面板 |
| 1 | Wheel | 拉坯台 | ✅ true | 到达后自动打开拉坯面板 |
| 2 | Glaze | 配釉台 | ✅ true | 到达后自动打开配釉面板 |
| 3 | Kiln | 窑炉 | ✅ true | 到达后自动打开烧窑面板 |
| 4 | Storage | 仓库 | ❌ false | 纯寻路到建筑，不自动交互 |
| 5 | Material | 材料库 | ❌ false | 纯寻路到建筑，不自动交互 |

### 6.3 HUDQuickBar（Phase11A）

```csharp
// Assets/Phase11/Scripts/HUDQuickBar.cs
using UnityEngine;

public class HUDQuickBar : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private HUDQuickBarConfigSO config;

    [Header("Button Container")]
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject buttonPrefab;

    // 运行时引用
    private Phase6GameManager gameManager;
    private PlayerCharacter playerCharacter;
    private MovementController movementController;
    private InteractionController interactionController;

    // pending 状态
    private AreaType? pendingAutoInteract;
    private float pendingStartTime;
    private float autoInteractTimeout;

    private QuickBarButton[] runtimeButtons;
    private HUDButtonData[] buttonDataCache;

    private void Start()
    {
        gameManager = FindObjectOfType<Phase6GameManager>();
        var pc = FindObjectOfType<PlayerCharacter>();
        if (pc != null)
        {
            playerCharacter = pc;
            movementController = pc.GetComponent<MovementController>();
            interactionController = pc.GetComponent<InteractionController>();
        }

        autoInteractTimeout = config != null ? config.autoInteractTimeout : 15f;
        BuildButtons();
    }

    private void BuildButtons()
    {
        if (config == null || config.buttons == null) return;

        buttonDataCache = config.buttons;
        runtimeButtons = new QuickBarButton[buttonDataCache.Length];
        for (int i = 0; i < buttonDataCache.Length; i++)
        {
            var go = Instantiate(buttonPrefab, buttonContainer);
            var qbb = go.GetComponent<QuickBarButton>();
            qbb.Initialize(buttonDataCache[i], OnButtonClicked);
            runtimeButtons[i] = qbb;
        }
    }

    private void Update()
    {
        if (pendingAutoInteract == null) return;

        // 超时保护
        if (Time.time - pendingStartTime > autoInteractTimeout)
        {
            CancelPending("超时：无法到达目标建筑");
            return;
        }

        if (movementController == null) return;

        // 被打断检测
        if (!movementController.IsMoving() && !movementController.HasReachedDestination())
        {
            CancelPending(null);
            return;
        }

        // 到达检测
        if (movementController.HasReachedDestination())
        {
            AreaType arrived = pendingAutoInteract.Value;
            pendingAutoInteract = null;

            if (ShouldAutoInteract(arrived) && interactionController != null)
            {
                interactionController.TryInteract();
            }
        }
    }

    private bool ShouldAutoInteract(AreaType areaType)
    {
        if (buttonDataCache == null) return false;
        foreach (var data in buttonDataCache)
        {
            if (data.areaType == areaType)
                return data.autoInteract;
        }
        return false;
    }

    private void OnButtonClicked(AreaType target)
    {
        if (gameManager != null && !gameManager.CanMove())
            return;

        if (playerCharacter == null) return;

        // 查找目标 Workstation
        var workstations = FindObjectsOfType<Workstation>();
        Workstation targetWS = null;
        foreach (var ws in workstations)
        {
            if (ws.AreaType == target)
            {
                targetWS = ws;
                break;
            }
        }

        if (targetWS == null) return;

        bool shouldInteract = ShouldAutoInteract(target);

        // 已在范围内 + 配置了自动交互 → 直接触发
        if (shouldInteract &&
            interactionController != null &&
            interactionController.IsInRange &&
            interactionController.NearestWorkstation?.AreaType == target)
        {
            interactionController.TryInteract();
            return;
        }

        // 取消上一个 pending
        if (pendingAutoInteract != null)
            playerCharacter.StopMoving();

        // 标记 pending（仅 autoInteract=true）
        if (shouldInteract)
        {
            pendingAutoInteract = target;
            pendingStartTime = Time.time;
        }

        // 导航目标：优先 InteractionPoint，否则退到 Workstation 自身位置
        Vector3 dest;
        if (targetWS.InteractionPoint != null)
            dest = targetWS.InteractionPoint.transform.position;
        else
            dest = targetWS.transform.position;

        playerCharacter.SetDestination(dest);
    }

    private void CancelPending(string reason)
    {
        if (playerCharacter != null)
            playerCharacter.StopMoving();
        pendingAutoInteract = null;

        if (!string.IsNullOrEmpty(reason))
        {
            Debug.LogWarning($"[HUDQuickBar] {reason}");
        }
    }
}
```

### 6.4 QuickBarButton（Phase11A）

```csharp
// Assets/Phase11/Scripts/QuickBarButton.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class QuickBarButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private Button button;

    private AreaType areaType;
    private Action<AreaType> onClick;

    public void Initialize(HUDButtonData data, Action<AreaType> callback)
    {
        areaType = data.areaType;
        onClick = callback;

        if (iconImage != null && data.icon != null)
            iconImage.sprite = data.icon;
        if (labelText != null)
            labelText.text = data.label;

        if (button != null)
            button.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        onClick?.Invoke(areaType);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);
    }
}
```

### 6.5 PlayerStatusBar（Phase11A 占位）

```csharp
// Assets/Phase11/Scripts/PlayerStatusBar.cs
using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerStatusBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI silverText;
    [SerializeField] private TextMeshProUGUI reputationText;
    [SerializeField] private TextMeshProUGUI orderText;

    [Header("Settings")]
    [SerializeField] private float refreshInterval = 0.5f;

    private void Start()
    {
        StartCoroutine(RefreshLoop());
    }

    private IEnumerator RefreshLoop()
    {
        while (true)
        {
            RefreshDisplay();
            yield return new WaitForSeconds(refreshInterval);
        }
    }

    private void RefreshDisplay()
    {
        // Phase11A：占位值，Phase13 接入真实数据
        if (silverText != null)
            silverText.text = "银两: --";
        if (reputationText != null)
            reputationText.text = "声望: --";
        if (orderText != null)
            orderText.text = "当前订单: --";
    }
}
```

### 6.6 CameraZoomController（Phase11B，自持配置）

```csharp
// Assets/Phase11/Scripts/CameraZoomController.cs
using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    [Header("Zoom")]
    [SerializeField] private float zoomSensitivity = 1.5f;
    [SerializeField] private float minZoom = 1.0f;
    [SerializeField] private float maxZoom = 8.0f;

    [Header("Pan")]
    [SerializeField] private float panSpeed = 1.0f;
    [SerializeField] private MouseButton panButton = MouseButton.Right;

    [Header("Reset — 自持默认追踪参数（不依赖 CameraFollow2D 私有字段）")]
    [SerializeField] private float defaultOrthographicSize = 2.2f;
    [SerializeField] private Vector2 defaultFramingOffset = new Vector2(0f, 0.45f);
    [SerializeField] private float resetDuration = 0.8f;
    [SerializeField] private AnimationCurve resetCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // 运行时引用
    private Camera cam;
    private CameraFollow2D cameraFollow2D;
    private MovementController movementController;
    private Transform playerTransform;

    // 手动浏览状态
    private bool isManualMode;
    private Vector3 lastMousePosition;

    // 复位过渡状态
    private bool isResetting;
    private float resetElapsed;
    private Vector3 resetOriginPos;
    private float resetOriginSize;

    public bool IsManualMode => isManualMode;
    public bool IsResetting => isResetting;

    private void Start()
    {
        cam = Camera.main;
        cameraFollow2D = cam != null ? cam.GetComponent<CameraFollow2D>() : null;

        var pc = FindObjectOfType<PlayerCharacter>();
        if (pc != null)
        {
            movementController = pc.GetComponent<MovementController>();
            playerTransform = pc.transform;
        }
    }

    private void Update()
    {
        if (cam == null) return;

        if (isResetting)
        {
            UpdateResetTransition();
            return;
        }

        if (isManualMode)
        {
            HandleZoom();
            HandlePan();
            return;
        }

        DetectManualEntry();
    }

    // ── 进入手动浏览 ──

    private void DetectManualEntry()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            EnterManualMode();
            ApplyZoom(scroll);
            return;
        }

        if (Input.GetMouseButtonDown((int)panButton))
        {
            EnterManualMode();
            lastMousePosition = Input.mousePosition;
        }
    }

    private void EnterManualMode()
    {
        if (isManualMode) return;

        isManualMode = true;
        if (cameraFollow2D != null)
        {
            cameraFollow2D.enabled = false;
        }
    }

    // ── 手动操作 ──

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            ApplyZoom(scroll);
        }

        CheckAutoReset();
    }

    private void ApplyZoom(float scrollDelta)
    {
        float newSize = cam.orthographicSize - scrollDelta * zoomSensitivity;
        cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
    }

    private void HandlePan()
    {
        if (Input.GetMouseButtonDown((int)panButton))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton((int)panButton))
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            float worldDeltaX = -mouseDelta.x * cam.orthographicSize * 2f / Screen.height * cam.aspect;
            float worldDeltaY = -mouseDelta.y * cam.orthographicSize * 2f / Screen.height;

            cam.transform.position += new Vector3(worldDeltaX, worldDeltaY, 0f);
            // MVP 不 clamp 地图边界，避免与当前地图大小不匹配
        }

        if (Input.GetMouseButtonUp((int)panButton))
        {
            CheckAutoReset();
        }
    }

    // ── 智能复位 ──

    private void CheckAutoReset()
    {
        if (movementController != null && movementController.IsMoving())
        {
            StartReset();
        }
    }

    private void StartReset()
    {
        isManualMode = false;
        isResetting = true;
        resetElapsed = 0f;
        resetOriginPos = cam.transform.position;
        resetOriginSize = cam.orthographicSize;
    }

    private void UpdateResetTransition()
    {
        resetElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(resetElapsed / resetDuration);
        float easedT = resetCurve.Evaluate(t);

        // 目标位置 = 主角位置 + 自持的 framing offset
        Vector3 targetPos = playerTransform != null
            ? (Vector3)((Vector2)playerTransform.position + defaultFramingOffset) + Vector3.forward * cam.transform.position.z
            : resetOriginPos;

        cam.transform.position = Vector3.Lerp(resetOriginPos, targetPos, easedT);
        cam.orthographicSize = Mathf.Lerp(resetOriginSize, defaultOrthographicSize, easedT);

        // 取消复位：过渡中任意鼠标点击或滚轮
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)
            || Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.001f)
        {
            CancelReset();
            return;
        }

        // 完成复位
        if (t >= 1f)
        {
            isResetting = false;
            if (cameraFollow2D != null)
            {
                cameraFollow2D.enabled = true;
            }
        }
    }

    private void CancelReset()
    {
        isResetting = false;
        isManualMode = true;
        // CameraFollow2D 保持 disabled，相机停在中断位置
    }

    private enum MouseButton
    {
        Left = 0,
        Right = 1,
        Middle = 2
    }
}
```

---

## 7. 与 InputManager / CameraFollow2D 的兼容性

### 7.1 HUD 按钮 vs 地面点击

`InputManager.Update()` 中：
```csharp
if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
    return;  // 点击在 UI 上 → 不触发地面移动
```

HUD 按钮是 UI GameObject，天然互斥。

### 7.2 CameraZoomController vs CameraFollow2D（Phase11B）

| 模式 | CameraFollow2D.enabled | Camera 控制权 | LateUpdate 行为 |
|---|---|---|---|
| 跟踪模式 | **true** | CameraFollow2D | Follow2D.LateUpdate() 正常运行 |
| 手动浏览 | **false** | CameraZoomController | Follow2D.LateUpdate() 被 skip |
| 复位过渡中 | **false** | CameraZoomController | Follow2D.LateUpdate() 被 skip |

> CameraZoomController 在 `Update()` 中执行，CameraFollow2D 在 `LateUpdate()`。Follow2D disabled 后其 LateUpdate 不执行 —— 天然解决执行顺序问题。

### 7.3 Phase11B 自持配置 vs CameraFollow2D 私有字段

| CameraFollow2D 字段 | 可见性 | Phase11B 方案 |
|---|---|---|
| `framingOffset` | `private` | Phase11B 自持 `defaultFramingOffset`，Inspector 独立配置 |
| `orthographicSize` | `private` | Phase11B 自持 `defaultOrthographicSize` |
| `smoothSpeed` | `private` | 不需要，Phase11B 用 `resetCurve` 独立控制过渡速率 |
| `clampToBounds` / `mapBounds` | `private` | MVP 阶段不 clamp |
| `enabled` | `public` (MonoBehaviour) | 通过 `.enabled = false` 切换 |

---

## 8. 边界情况

### HUDQuickBar / 快捷按钮（Phase11A）

| 场景 | 行为 |
|---|---|
| UI 打开时点按钮 | `CanMove()=false` → 忽略 |
| 目标 Workstation 不存在 | 忽略点击 |
| 已在交互范围内 | 跳过寻路，直接 `TryInteract()` |
| Storage/Material（autoInteract=false） | 寻路到建筑，不触发交互 |
| Storage/Material 无 InteractionPoint | 退到 `Workstation.transform.position` |
| 寻路中被点地板打断 | `!IsMoving() && !HasReachedDestination()` → 静默取消 |
| 寻路中再点另一个按钮 | `StopMoving()` → 新导航 |
| 寻路被障碍物阻挡 | 超时保护 15s |
| Scene 重载 | `Start()` 重新 `FindObjectOfType()` |

### CameraZoomController（Phase11B）

| 场景 | 行为 |
|---|---|
| 滚轮缩放 | 进入手动模式 → 调整 orthographicSize（Clamp 到自持 min/max） |
| 右键拖拽平移 | 进入手动模式 → 跟随鼠标偏移（MVP 不 clamp 地图边界） |
| 主角移动时 | 自动触发复位过渡 → Lerp 到自持的 default 参数 |
| 复位过渡中点鼠标/滚轮 | 取消复位 → 停在当前视角，回到手动模式 |
| 复位完成 | CameraFollow2D.enabled = true → 回到追踪模式 |
| CameraFollow2D 不存在 | 手动模式仍可操作，复位跳过恢复 Follow2D |
| defaultFramingOffset 与 Follow2D 不一致 | 用户需在 Inspector 手动对齐（两者独立配置） |

---

## 9. 实现计划

### Phase11A：HUDQuickBar + 占位状态栏 + Prefab

| Step | 内容 | 预估 |
|---|---|---|
| 1 | 创建 `Assets/Phase11/` 目录结构 | — |
| 2 | 编写 `HUDButtonData.cs` + `HUDQuickBarConfigSO.cs` + 创建 .asset | 0.5h |
| 3 | 编写 `QuickBarButton.cs` + `HUDQuickBar.cs` + `PlayerStatusBar.cs` | 1h |
| 4 | 搭建 Canvas Prefab：Screen Space - Overlay，PlayerStatusBar 左上，HUDQuickBar 底部 | 1h |
| 5 | 在 SampleScene 中挂载 HUDCanvas.prefab | 0.25h |
| 6 | 验证：6 按钮寻路 / 4 自动交互 / 2 纯寻路 / 打断 / 超时 / UI 互斥 | 0.5h |

### Phase11B：相机缩放/平移/复位

| Step | 内容 | 预估 |
|---|---|---|
| 1 | 编写 `CameraZoomController.cs`（自持 defaultFramingOffset / defaultOrthographicSize） | 1h |
| 2 | 在 SampleScene 相机上挂载 + 配置参数 | 0.25h |
| 3 | 验证：缩放 / 平移 / 复位 / 取消复位 / 与 Follow2D 切换 | 0.5h |

### Phase13（后续，不在本轮）

| 内容 | 说明 |
|---|---|
| PlayerStatusBar 接入 Phase3 真实数据 | 银两 / 声望 / 当前订单 |
| 需 Phase3 GameManager 先暴露 public 属性 | — |

---

## 10. 后续扩展

| 扩展项 | 依赖 | 目标 |
|---|---|---|
| 状态栏接入真实数值 | Phase3 暴露 public 属性 | Phase 13 |
| 按钮图标替换为美术资源 | UI 美术到位 | Phase 14 |
| 相机地图边界 clamp | 确定地图精确大小 | Phase 12 |
| Toast 消息系统 | UGUI Toast 组件 | Phase 12 |
| 建筑名称浮标 | World Space Canvas + Raycast | Phase 14 |

---

> 最后更新：2026-06-17
> 所属：景德镇·窑火千年 Unity 项目
> 修正点：CameraZoomController 改为自持配置，不依赖 CameraFollow2D 私有字段；状态栏明确占位；拆分为 Phase11A/11B 两阶段
