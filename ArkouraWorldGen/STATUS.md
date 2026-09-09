# Arkoura WorldGen 状态

版本：0.2.0 研究与环境语义阶段

## 当前能够真正执行

- Unity Terrain 分块生成。
- 全局坐标连续高度采样。
- 宏观/中观/微观地形合成。
- Terrain Layer 权重生成。
- WorldSemanticFields：高度、坡度、曲率、温度、湿度、悬崖、水流、水体、岸线、道路、聚落 mask。
- D8 水文汇流与基础洼地修复。
- 语义 SurfaceProfile 权重生成。
- 水体高层数据构建。
- 悬崖带提取。
- 生物群落资产散布。
- 聚落建筑基础放置。
- Terrain A* 道路和语义 A* 道路数据。
- World chunk 邻接。
- Runtime 距离流送原型。
- AI Planner HTTP 抽象。
- AssetLibrary 扫描和详细元数据。
- 环境插件能力检测。
- 世界和语义字段验证。

## 当前只属于数据/规划层，还不是最终 AAA 输出

- WaterBody 已生成，但 Crest/R.A.M/HDRP Water 编译器尚未实现。
- CliffBand 已生成，但真实 cliff mesh/voxel 编译器尚未实现。
- LightingPlan 数据模型已建立，但 APV/Compute Light Baker/Bakery 自动烘焙器尚未实现。
- WorldCellManifest 数据模型已建立，但 Addressables/Scene/HLOD 烘焙器尚未实现。
- SurfaceProfile 已表达 triplanar、height blend、anti-tiling 和 terrain-object blend 意图，但具体 shader adapter 尚未实现。
- RoadProfile/ArkouraSpline 已建立，但道路网格、cut/fill、桥梁和隧道编译尚未完成。
- 聚落目前仍是基础建筑放置，不是完整 block/parcel/zoning 城市系统。

## 明确禁止的错误方向

- 不把数百万树木和草全部变成独立 GameObject。
- 不让 LLM 返回 Transform 列表作为最终世界。
- 不在每个 chunk 独立随机生成边界。
- 不把河流当作随机蓝色 spline。
- 不把道路只画到 terrain splat map 而没有图结构。
- 不把悬崖强行限制为高度场。
- 不把 anti-tiling、triplanar 和对象融合混进 splat 权重逻辑。
- 不硬引用任何商业插件程序集。
- 不把第三方插件私有数据作为 Arkoura 世界真相来源。

## 生产基线

推荐锁版本时以 Unity 6.3 LTS 为稳定生产基线，并维护 Unity 6.6 Supported 验证分支。UPM 包本身保持最低 Unity 6000.0 API 兼容，直到有明确的性能收益值得抬高最低版本。

## 下一阶段

优先级从高到低：

1. 世界生成 DAG 和局部脏区缓存。
2. 完整 watershed/river topology。
3. RoadNetwork 全世界图 + spline smoothing + cut/fill。
4. Water backend interface 与 Unity 原生后端。
5. Surface backend interface 与 Terrain Lit 后端。
6. GPU/Burst 语义生成后端。
7. GPU vegetation instance buffer。
8. Scene/Addressables/HLOD bake pipeline。
9. Parcel/zoning 建筑系统。
10. APV/lighting bake pipeline。
11. 第三方插件 adapters。
12. AI validator/repair loop。
