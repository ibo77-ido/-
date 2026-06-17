# Phase11A — Unity Editor 手动搭建步骤

## 前置条件
确保所有 `.cs` 文件已经编译通过（Unity 打开后自动编译）。

---

## Step 1：创建 HUDQuickBarConfigSO.asset

1. 在 Project 窗口右键 `Assets/Phase11/Data/`
2. Create → Phase11 → HUD QuickBar Config
3. 命名为 `HUDQuickBarConfig`
4. 在 Inspector 中配置 6 个按钮：

| Element | Area Type | Label | Auto Interact |
|---|---|---|---|
| 0 | Order | Jiedanban | ✔ |
| 1 | Wheel | Lapeitai | ✔ |
| 2 | Glaze | Peiyoutai | ✔ |
| 3 | Kiln | Yaolu | ✔ |
| 4 | Storage | Cangku | ✘ |
| 5 | Material | Cailiaoku | ✘ |

> **图标暂时留空**（Sprite = None），后续 Phase14 替换美术资源。
> Auto Interact Timeout 保持默认 15。

---

## Step 2：创建按钮 Prefab

1. Hierarchy → 右键 → UI → Button - TextMeshPro
2. 命名为 `QuickBarButton`
3. 结构：
   ```
   QuickBarButton (Button + QuickBarButton 脚本)
   └── Text (TMP) (labelText)
   ```
4. 设置：
   - RectTransform: Width=120, Height=50
   - Image/Button 组件保持默认
   - 添加 `QuickBarButton` 脚本
5. 将 labelText (TextMeshProUGUI) 拖到 QuickBarButton 的 `Label Text` 字段
6. 将 Button 组件拖到 `Button` 字段
7. 将 Image 组件拖到 `Icon Image` 字段
8. 拖到 `Assets/Phase11/Prefabs/` 保存为 prefab
9. 从 Hierarchy 中删除

---

## Step 3：创建 HUDCanvas

### 创建 Canvas
1. Hierarchy → 右键 → UI → Canvas
2. 命名为 `HUDCanvas`
3. Canvas Scaler：
   - UI Scale Mode: **Scale With Screen Size**
   - Reference Resolution: 1920 × 1080
   - Screen Match Mode: 0.5

### 创建 PlayerStatusBar（左上角）
1. Canvas 下创建空 GameObject → 命名为 `PlayerStatusBar`
2. RectTransform 锚点设为 **左上**（top-left），Pivot=(0, 1)
3. 位置：Pos X=20, Pos Y=-20
4. 添加 Vertical Layout Group
5. 添加 `PlayerStatusBar` 脚本
6. 创建 3 个子 Text（TMP）：
   - `SilverText` — 文字 "Yinliang: --"
   - `ReputationText` — 文字 "Shengwang: --"  
   - `OrderText` — 文字 "Dingdan: --"
7. 将 3 个 TMP 拖到 PlayerStatusBar 脚本的对应字段

> **文字用英文拼音**的目的：当前项目 TextMeshPro 的字体可能不支持中文字符。如已导入中文字体（如 NotoSansSC SDF），可直接写中文。

### 创建 HUDQuickBar（底部居中）
1. Canvas 下创建空 GameObject → 命名为 `HUDQuickBar`
2. RectTransform 锚点设为 **底部居中**（bottom-center），Pivot=(0.5, 1)
3. 位置：Pos Y=20
4. 添加 Horizontal Layout Group：
   - Child Alignment: Middle Center
   - Spacing: 10
   - Child Controls Size: Width + Height
5. 添加 Content Size Fitter：
   - Horizontal Fit: Preferred Size
6. **不要添加** `HUDQuickBar` 脚本到空的 GameObject 上！
7. 在 HUDQuickBar 下创建子空对象 → 命名为 `ButtonContainer`
   - 添加 Horizontal Layout Group + Content Size Fitter（同上）
   - 添加 **`HUDQuickBar` 脚本**
   - 将 `HUDQuickBarConfig` asset 拖到 Config 字段
   - 将 `QuickBarButton` prefab 拖到 Button Prefab 字段
   - 将自身 RectTransform 拖到 Button Container 字段（引用 ButtonContainer）

> 结构：
> ```
> HUDCanvas
> ├── PlayerStatusBar (脚本: PlayerStatusBar)
> │   ├── SilverText (TMP)
> │   ├── ReputationText (TMP)
> │   └── OrderText (TMP)
> └── HUDQuickBar
>     └── ButtonContainer (脚本: HUDQuickBar, Config=...)
>         └── [运行时 Instantiate 生成 6 个按钮]
> ```

---

## Step 4：保存 Prefab + 挂载到 Scene

1. 将 `HUDCanvas` 拖到 `Assets/Phase11/Prefabs/` 保存为 `HUDCanvas.prefab`
2. 将 `HUDCanvas.prefab` 拖到 `SampleScene` 的 Hierarchy 中
3. 确保 HUDCanvas 在最顶层（Sibling Index 最大，或 Sort Order 最高）
4. Canvas 的 Sort Order 设为 1（确保在 3D 视图之上）

---

## Step 5：验证

进入 Play Mode：

| 验证项 | 操作 | 期望 |
|---|---|---|
| 按钮显示 | 启动游戏 | 底部显示 6 个按钮 |
| 寻路 | 点"接单板" | 角色移动到接单板 |
| 自动交互 | 到达接单板 | 自动弹出面板 |
| Storage 不交互 | 点"仓库" | 角色移动到仓库，不弹面板 |
| UI 互斥 | 面板打开时点按钮 | 无反应 |
| UI 不触发地面移动 | 点按钮时看角色 | 角色不移动 |
| 状态栏 | 启动游戏 | 左上显示 "Yinliang: --" 等占位文字 |

---

## 常见问题

| 问题 | 解决 |
|---|---|
| 按钮点击无反应 | 检查 EventSystem 是否存在；若不存在，Hierarchy 右键 → UI → Event System |
| TMP 不显示文字 | 检查 TextMeshPro 的 Font Asset 是否包含所需字符 |
| 按钮不生成 | 检查 HUDQuickBarConfig 是否正确拖入，buttons 数组是否填了 6 个元素 |
| 角色不移动 | 检查 Phase6GameManager 是否存在且 CanMove()=true |
