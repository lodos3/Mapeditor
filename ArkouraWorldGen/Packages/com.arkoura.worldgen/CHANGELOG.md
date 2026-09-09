# 变更记录

## 0.2.0

- 完成 2026-09-05 大型 Unity 环境工具生态研究，覆盖地形、水体、地表着色、对象融合、植被、道路、洞穴、建筑、照明、天气、后处理和世界流送。
- 加入 `WorldSemanticFields`，统一高度、坡度、曲率、湿度、温度、水流、水体、岸线、悬崖、道路和聚落语义。
- 加入确定性 D8 水文求解、洼地修复、汇流、河流/海洋和岸线 mask。
- 加入 `SurfaceProfile` 与多语义地表权重生成，分离 splat 权重与 shader 表现意图。
- 加入后端无关的 `ArkouraSpline`、`RoadProfile`、`WaterProfile`、`WaterBody` 和道路网络求解器。
- 加入悬崖带提取与混合 Terrain/Mesh/Voxel 地形意图。
- 加入 `LightingProfile` 和照明区域规划，面向 APV、反射探针和可选烘焙后端。
- 加入 `WorldCellManifest`、`WorldBakeManifest` 与明确的 CPU/GPU/内存内容预算模型。
- 扩充资产元数据，加入湿度、温度、地表融合、三平面、湿润、积雪、GPU 实例化、HLOD、阴影距离、风、季节和交互植被意图。
- 加入可选环境插件能力目录和无硬依赖检测器。
- 加入环境能力矩阵 Editor Window。
- 世界生成器现在先构建语义场和水文，再编译地表、水体与悬崖摘要。
- 扩充验证器与环境字段自动测试。

## 0.1.0

- 建立 Unity 6 UPM 包。
- 加入 WorldSpec 和资产元数据模型。
- 加入连续分块多频率地形。
- 加入规则式 Terrain Layer 权重图。
- 加入生物群落解析与确定性资产散布。
- 加入聚落建筑放置和道路 A* 原型。
- 加入资产扫描工具、验证器、流送器和 Editor 主窗口。
- 加入 AI Planner 抽象与 HTTP 实现。
- 加入确定性与分块边界测试。
