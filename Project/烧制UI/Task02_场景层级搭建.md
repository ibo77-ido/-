# Task02 — 场景层级搭建

## 目标

在 `Panel_Firing` 下搭建 `ArtRoot_Firing` 美术层级，配置 4 个 VideoPlayer + RenderTexture + RawImage，放置风门序列帧节点、风门热区、按钮根节点。本任务只搭层级和资产绑定，不写控制逻辑。

## 依赖

- Task01 完成（FiringSystem 字段已定义，本任务不直接依赖，但为后续 Task 铺垫）

## 涉及文件

### Modified
- Phase3 场景文件（Panel_Firing 层级扩展）
- 可能新建 RenderTexture 资产

### Created
- RenderTexture 资产（用于视频输出）

## 实施内容

### 1. 层级结构

```
Panel_Firing
├─ ArtRoot_Firing                      (新增空节点)
│  ├─ StaticFallback_Image             (Image, 绑 底图 阶段1.png)
│  ├─ FiringVideo_RawImage             (RawImage, 接收主画面 RenderTexture)
│  │   └─ FireStartPlayer             (VideoPlayer, 绑 火焰动画.mp4)
│  │   └─ FiringLoopPlayer            (VideoPlayer, 绑 循环.mp4)
│  ├─ TemperatureVideo_RawImage        (RawImage, 接收温度条 RenderTexture)
│  │   └─ TemperatureVideoPlayer      (VideoPlayer, 绑 温度条变化.mp4)
│  └─ Overlay_Image                    (可选, 叠加层)
├─ WindDoor_Image                      (Image, 风门序列帧显示)
├─ WindDoor_HoverArea                  (RectTransform, 透明, 滚轮检测)
├─ Btn_Fuel                            (Button, 绑 添燃料按钮.png)
├─ Btn_Stop                            (Button, 绑 停火按钮.png)
├─ Btn_Open                            (Button, 绑 打开按钮.png)
├─ Btn_Info                            (可选)
└─ PromptRoot                          (可选)
```

### 2. VideoPlayer 配置

| VideoPlayer | clip | isLooping | playOnAwake | renderMode | targetTexture |
|-------------|------|-----------|-------------|------------|---------------|
| FireStartPlayer | 火焰动画.mp4 | false | false | RenderTexture | FiringRT |
| FiringLoopPlayer | 循环.mp4 | true | false | RenderTexture | FiringRT |
| ClosingKilnPlayer | 关窑动画.mp4 | false | false | RenderTexture | FiringRT |
| TemperatureVideoPlayer | 温度条变化.mp4 | false | false | RenderTexture | TemperatureRT |

- `FiringRT`：主画面 RenderTexture，FireStart/FiringLoop/ClosingKiln 三个 Player 共享
- `TemperatureRT`：温度条独立 RenderTexture
- 所有 VideoPlayer 的 `playOnAwake = false`，由 Controller 控制

### 3. 风门节点配置

- `WindDoor_Image`：放在右侧中下留白区（停火按钮上方、信息按钮下方、窑门右侧之外）
- `WindDoor_HoverArea`：RectTransform 与 WindDoor_Image 重合，透明 Image，只用于滚轮检测
- 风门 9 帧 Sprite 数组在 Task04 由 Controller SerializeField 接收，本任务只在 WindDoor_Image 上放初始帧（1.png）

### 4. 按钮配置

- `Btn_Fuel`：Image.sprite = 添燃料按钮.png
- `Btn_Stop`：Image.sprite = 停火按钮.png
- `Btn_Open`：Image.sprite = 打开按钮.png，初始 active=false

### 5. 旧控件处理

- 旧 `Slider_Wind`、`Btn_Window`、`Text_Zone`、`Text_FireScore`、`Text_Status`：设为 inactive，不删除

### 6. ArtRoot_Firing 标识节点

`ArtRoot_Firing` 节点必须存在且命名准确，供 Task06 的 Phase3SceneBuilder 检测。

## Serialized References Changed

```
[INSPECTOR] Panel_Firing 下新增 ArtRoot_Firing 层级
[INSPECTOR] 4 个 VideoPlayer 配置 clip + RenderTexture
[INSPECTOR] WindDoor_Image 拖入初始 sprite
[INSPECTOR] 3 个按钮 Image.sprite 替换为美术 png
[INSPECTOR] 旧 Slider_Wind/Btn_Window/Text_* 设为 inactive
[NEW ASSET] FiringRT (RenderTexture)
[NEW ASSET] TemperatureRT (RenderTexture)
```

## Scene Mutation

```
- Panel_Firing 下新增 ArtRoot_Firing 子节点
  - StaticFallback_Image (Image)
  - FiringVideo_RawImage (RawImage + 3个VideoPlayer)
  - TemperatureVideo_RawImage (RawImage + 1个VideoPlayer)
- 新增 WindDoor_Image (Image)
- 新增 WindDoor_HoverArea (RectTransform)
- Btn_Fuel/Stop/Open 替换 sprite
- 旧 Slider_Wind/Btn_Window/Text_* 设为 inactive
```

## Acceptance Check

1. 场景中 Panel_Firing 下存在 ArtRoot_Firing 节点
2. 4 个 VideoPlayer 已绑定对应 mp4 clip
3. FiringRT 被 3 个主画面 VideoPlayer 共享
4. TemperatureRT 被温度条 VideoPlayer 使用
5. WindDoor_Image 位于右侧中下留白区，不遮挡窑口/烟雾/按钮
6. WindDoor_HoverArea 与 WindDoor_Image 重合
7. 3 个按钮已换美术 png
8. 旧控件已 inactive
9. Play 模式下无报错（VideoPlayer 不自动播放）

## Risks

- RenderTexture 分辨率需与视频匹配，否则画面变形
- 3 个 VideoPlayer 共享同一 RenderTexture 时，同时 Play 会冲突，需 Controller 确保同一时刻只 Play 一个
- WindDoor 位置需实际测试，避免遮挡参考图主体

## Next Recommended Task

Task03_FiringPanelController状态机
