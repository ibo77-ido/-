# Task06 — Phase3SceneBuilder 检测分支

## 目标

修改 `Phase3SceneBuilder.EnsureFiringPanelControls()`，检测 `Panel_Firing` 下是否存在 `ArtRoot_Firing` 节点。若存在则跳过旧文字版占位控件创建，避免 Builder 污染已接入的美术面板。

## 依赖

- Task02 完成（`ArtRoot_Firing` 节点已在场景中搭建）

## 涉及文件

### Modified
- `Assets/Phase3/Editor/Phase3SceneBuilder.cs`

## 实施内容

### 1. 定位方法

`Phase3SceneBuilder.cs:127` 的 `EnsureFiringPanelControls(Transform canvasTf)` 方法。

### 2. 修改方案

在方法开头（找到 Panel_Firing 之后、创建占位控件之前）加入检测分支：

```csharp
private static List<string> EnsureFiringPanelControls(Transform canvasTf)
{
    List<string> created = new List<string>();
    
    Transform panel = canvasTf.Find("Panel_Firing");
    if (panel == null) return created;
    
    // ─── 美术面板检测 ─────────────────────────────
    // 若已接入美术版烧制面板（存在 ArtRoot_Firing），跳过旧占位控件创建
    if (panel.Find("ArtRoot_Firing") != null)
    {
        // 只确保 FiringPanelController 存在
        if (panel.GetComponent<FiringPanelController>() == null)
        {
            panel.gameObject.AddComponent<FiringPanelController>();
            created.Add("Component: FiringPanelController on Panel_Firing (art mode)");
        }
        return created;
    }
    // ─── 以下保持原有占位控件创建逻辑 ─────────────
    
    // ... 原有代码不变
}
```

### 3. 不改部分

- Order、Shape、Glaze、Result 面板的 Builder 逻辑不变
- 原 Panel_Firing 无 ArtRoot_Firing 时的占位控件创建逻辑不变
- 其他所有 Editor 方法不变

## 检测逻辑说明

```
EnsureFiringPanelControls(canvasTf)
  ├─ 找 Panel_Firing
  │   ├─ 不存在 → 返回
  │   └─ 存在
  │       ├─ 检测 ArtRoot_Firing
  │       │   ├─ 存在 → 仅确保 FiringPanelController 组件，返回（跳过占位控件）
  │       │   └─ 不存在 → 走原有逻辑（创建 Text_Zone/Text_FireScore/Slider_Wind/Btn_*）
```

## Serialized References Changed

```
NONE
```

## Scene Mutation

```
NONE（仅改 Editor 脚本逻辑）
```

## Acceptance Check

1. 编译通过
2. 场景中 Panel_Firing 下有 ArtRoot_Firing 时，运行 Phase3SceneBuilder 不再创建 Text_Zone、Text_FireScore、Text_Status、Slider_Wind、Btn_Window 等旧占位控件
3. 场景中 Panel_Firing 下无 ArtRoot_Firing 时，原有占位控件创建逻辑正常
4. FiringPanelController 组件在美术模式下仍被确保存在
5. Order/Shape/Glaze/Result 面板不受影响
6. 控制台无报错

## Risks

- 若 ArtRoot_Firing 节点被误删，Builder 会回退到旧逻辑创建占位控件，可能与已接入的美术按钮重复
- 检测依赖节点名 `ArtRoot_Firing` 严格匹配，大小写敏感
- 若场景中同时存在 ArtRoot_Firing 和旧占位控件，Builder 不会清理旧控件，需手动确认场景干净

## Next Recommended Task

无（本任务为烧制 UI 接入的最后一个 Task）

完成后按 `Project/烧制UI方案.md` §验收清单 做整体验收。
