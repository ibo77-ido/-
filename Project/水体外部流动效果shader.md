# 水体外部流动效果 Shader 方案

## 当前项目判断

- 项目已经使用 `Assets/Phase9/Shaders/SpriteTreeWind.shader`、`SpriteCloudFogDrift.shader` 做 Sprite 级动态效果。
- Phase9 地图资源里已经有 `Tiles/Water`、`Transition/*Water*`，水体更像 2D/等距手绘 Tile 或 Sprite 叠层。
- 画面是浅青水墨、低饱和、手绘白色水痕风格，不适合真实反射水面或强法线折射。

因此本方案采用 **Transparent Unlit Sprite Shader**：保留原水体贴图，只叠加慢速水纹、边缘泡沫和轻微墨色明暗流动。

## 推荐落地方式

1. 将同目录下 `水体外部流动效果shader.shader` 复制到：
   `Assets/Phase9/Shaders/SpriteInkWaterFlow.shader`
2. 创建材质：
   `Assets/Phase9/Materials/Mat_Water_External_Flow.mat`
3. 材质 Shader 选择：
   `Director/Phase9/Sprite Ink Water Flow`
4. 将材质赋给“水体外部流动”对应的 SpriteRenderer、TilemapRenderer 或外部水体叠加面片。

## 推荐参数

### 外围河流

- `_Color`: `(0.78, 0.96, 0.90, 0.78)`
- `_FlowDirection`: `(1, 0.28, 0, 0)`
- `_FlowSpeed`: `0.055`
- `_RippleScale`: `18`
- `_RippleStrength`: `0.035`
- `_FoamIntensity`: `0.42`
- `_FoamThinness`: `0.62`
- `_InkWashStrength`: `0.12`
- `_AlphaSoftness`: `0.92`

### 岸边白纹

- `_FlowSpeed`: `0.035`
- `_RippleScale`: `26`
- `_FoamIntensity`: `0.65`
- `_FoamThinness`: `0.72`
- `_InkWashStrength`: `0.08`

### 静水池

- `_FlowSpeed`: `0.018`
- `_RippleScale`: `12`
- `_RippleStrength`: `0.018`
- `_FoamIntensity`: `0.16`
- `_InkWashStrength`: `0.06`

## 美术表现目标

- 水流应该像“纸面水纹在缓慢游动”，不要像真实海水。
- 白色流线只在局部出现，避免铺满整个水面。
- 主体建筑和地面仍然是视觉中心，水体只提供轻微生命感。
- 外围河流可以比内院水池更明显；内院水池建议只做轻微呼吸和慢速波纹。

## 使用建议

- 若“水体外部流动”是 Tilemap，优先给外层水体 Tilemap 单独分层并替换材质。
- 若水体和地形在同一张大图里，建议额外切一张水体透明遮罩 Sprite 盖在水面上，用这个 Shader 做叠加。
- 若需要表现水绕岛流动，可为不同河段拆成 3 到 4 个 SpriteRenderer，分别调整 `_FlowDirection`。
- 不建议开启强反射、高光、真实折射或大幅顶点位移。

