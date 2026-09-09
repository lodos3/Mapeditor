# Arkoura 环境系统研究与架构来源

研究日期：2026-09-05

本文件记录 Arkoura WorldGen 在大型 3D 环境生成方面参考的 Unity 原生系统、商业工具和开源工具。目的不是直接复制第三方实现，而是提炼长期可维护的能力模型、数据结构、生成顺序和质量门槛。所有第三方集成都必须保持可选，Arkoura 的世界数据不能依赖某个插件的私有序列化格式。

## 基线原则

1. AI 只负责语义规划、约束生成、局部修复建议和质量审查，不直接生成最终游戏场景。
2. 地形、水文、道路、聚落、材质、植被、照明、流送等由确定性系统构建。
3. 世界先生成语义场，再把语义场编译成 Unity Terrain、Mesh、Spline、Prefab、Probe、NavMesh 和流送单元。
4. 所有空间系统使用全局坐标采样，分块只是一种存储与流送方式，不能改变生成结果。
5. 第三方插件通过能力适配层接入，不允许核心算法硬引用插件程序集。
6. 每个生成阶段必须可单独重建，不能因为修改一条道路而重新生成整张 16 km 地图。
7. AAA 质量依赖“生成 + 验证 + 修复 + 再验证”，而不是提高随机噪声复杂度。

## Unity 原生能力

### Terrain / Terrain Tools

来源：
- https://docs.unity3d.com/6000.0/Documentation/Manual/terrain-Painting.html
- https://docs.unity3d.com/6000.0/Documentation/ScriptReference/TerrainPaintUtility.html
- https://docs.unity3d.com/Packages/com.unity.terrain-tools@latest

提炼能力：多 Terrain tile 连续编辑、高度图、holes、Terrain Layer 权重、细节散布、跨 tile 画笔上下文。Arkoura 应把 Terrain 视为可替换输出后端，而不是世界本身。

### Splines

来源：https://docs.unity3d.com/Packages/com.unity.splines@2.8/manual/index.html

提炼能力：道路、河流、围墙、悬崖带、轨道、任务路径都应先保存为 Arkoura 自有 spline/graph 数据，再可选编译到 Unity Splines。

### ProBuilder

来源：https://docs.unity3d.com/Packages/com.unity.probuilder@6.0/manual/index.html

提炼能力：编辑器内原型网格、UV、顶点色、参数化几何。Arkoura 的程序建筑和道路网格生成器应保留 UV 通道、顶点色掩码和可后处理拓扑。

### GPU Resident Drawer / GPU Occlusion

来源：
- https://docs.unity3d.com/6000.0/Documentation/Manual/urp/gpu-resident-drawer.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/urp/gpu-culling.html

提炼能力：尽量共享 Mesh/Material，避免无意义唯一实例；静态环境对象应有 GPU 批处理友好标记和 HLOD/流送预算。

### Adaptive Probe Volumes / Lighting

来源：
- https://docs.unity3d.com/6000.0/Documentation/Manual/urp/probevolumes.html
- https://unity.com/releases/unity-6/support

提炼能力：大型动态世界不能只依赖手工 Light Probe Group。Arkoura 需要根据室内、地形遮蔽、聚落密度、流送单元自动规划 APV 区域和烘焙场景。Unity 6.3 LTS 适合锁定生产；Unity 6.6 是 2026-09 的 Supported 版本，但插件兼容必须逐项验证。

### Volumes / Post Processing / Decals

来源：
- https://docs.unity3d.com/6000.0/Documentation/Manual/urp/Volume-Framework.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/urp/renderer-feature-template.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/urp/renderer-feature-decal.html

提炼能力：后处理、天气、洞穴曝光、区域雾效使用空间 Volume；道路脏迹、湿边、苔藓、积雪、墙脚污渍可以输出 decal mask，而不是烘死到基础纹理。

## 地形生成与世界构建工具

### Gaia / Gaia Pro / GeNa Pro / GTS

来源：
- https://proceduralworlds.com/products/professional/gaia-pro/
- https://proceduralworlds.com/products/professional/gena-pro/
- https://assetstore.unity.com/packages/tools/terrain/gts-glyph-terrain-shader-252531

提炼能力：stamp 工作流、生物群落、世界流送、基于规则的 spawn、道路/河流 spline、自动探针和完整生态预设。Arkoura 不采用单体“生物群落按钮”，而把这些拆成可组合语义层。

### MapMagic 2

来源：https://assetstore.unity.com/packages/tools/terrain/mapmagic-2-165180

提炼能力：节点图、无限/分块地形、结果缓存、局部更新。Arkoura 生成流水线必须是 DAG，并缓存每个阶段输入哈希。

### MicroVerse

来源：https://assetstore.unity.com/packages/tools/terrain/microverse-core-232976

提炼能力：GPU 优先、非破坏式 stamp、局部快速迭代。Arkoura 的长期方向是把高度、材质和散布场搬到 Compute Shader/Burst 后端，但数据模型不能依赖 GPU。

### Terrain Composer 2

来源：https://assetstore.unity.com/packages/tools/terrain/terrain-composer-2-65563

提炼能力：多 tile 世界空间连续生成，以及高度、splat、颜色、植被和对象的统一规则图。

### World Creator Unity Bridge

来源：
- https://docs.world-creator.com/reference/export/conventional-export/unity-bridge
- https://www.world-creator.com/en/news.phtml

提炼能力：外部 DCC 与 Unity 之间应交换明确的世界尺度、最小/最大高度、tile、层和 splat 元数据。Arkoura 导入器应支持 RAW/EXR/PNG 高度与控制图，而不是只接受 Unity TerrainData。

### Houdini Engine for Unity

来源：https://www.sidefx.com/docs/houdini/unity/intro.html

提炼能力：HDA、heightfield、曲线输入、建筑和几何处理可以作为高端外部生成器。Arkoura 应提供稳定中间格式，允许 Houdini 生成结果回灌，而不把 Houdini 当运行时依赖。

### Cesium for Unity

来源：https://cesium.com/platform/cesium-for-unity/

提炼能力：超大地理世界采用分层流送和高精度坐标。Arkoura 的普通游戏世界不需要 WGS84，但需要同样严格的分块 manifest、浮点原点策略和可逐层加载的数据。

## 地形材质、UV 与对象融合

### MicroSplat

来源：
- https://assetstore.unity.com/packages/tools/terrain/microsplat-96478
- https://assetstore.unity.com/packages/tools/terrain/microsplat-terrain-blending-97264
- https://assetstore.unity.com/packages/tools/terrain/microsplat-mesh-terrains-112739

提炼能力：triplanar、height blend、anti-tiling、对象到地形融合、mesh terrain、湿润/水坑/溪流层。Arkoura 的 SurfaceProfile 因此区分“材质选择权重”和“着色器表现参数”。Splat map 只表达覆盖权重，不承担 anti-tiling。

### GTS

来源：https://assetstore.unity.com/packages/tools/terrain/gts-glyph-terrain-shader-252531

提炼能力：高度融合、地质细节注入、雪、随机化、悬崖 triplanar、Mesh/Terrain 统一。悬崖区域需要独立语义 mask，不应仅由坡度临时判断。

### Dynamic Nature / Terrain-object blending 类工具

来源：https://assetstore.unity.com/publishers/6887

提炼能力：世界空间投影、顶部积雪/苔藓、对象与地表共享纹理逻辑。Arkoura 资产元数据应带 BlendWidth、TerrainTint、ProjectedNormal、TopProjection 等意图字段。

## 水体

### Crest Water

来源：https://crest.readthedocs.io/

提炼能力：海洋使用 GPU LOD、波浪输入、foam、动态扰动。海洋表面是渲染后端，世界数据仍应保存 coastline、深度、风、波区和交互层。

### R.A.M 3

来源：https://naturemanufacture.com/river-auto-material-3/

提炼能力：河、湖、海、瀑布、沼泽、flow map、岸线、地形 carve/paint、植被清理、物理流速和运行时 API。Arkoura WaterBody 必须包含流向、宽度、深度、速度、岸线、瀑布节点和水下体积。

### Stylized Water 3

来源：https://assetstore.unity.com/packages/vfx/shaders/stylized-water-3-295975

提炼能力：水下效果、动态交互和 Render Graph。即便最终视觉不是写实水，水体数据层也必须与着色器解耦。

### HDRP Water

来源：https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@latest/manual/WaterSystem.html

提炼能力：原生水体可作为无第三方插件时的后端，尤其适合 HDRP。

## 植被、散布和性能

### Nature Renderer

来源：https://assetstore.unity.com/packages/tools/terrain/nature-renderer-6-pro-329365

提炼能力：GPU culling、LOD、实例化和大规模植被。散布算法必须输出可批处理实例，不应默认创建数百万独立 GameObject。

### GPU Instancer Pro

来源：https://assetstore.unity.com/packages/tools/utilities/gpu-instancer-pro-290012

提炼能力：compute/GPU 实例化、地形树草和遮挡。Arkoura 的散布输出需要“逻辑实例”和“GameObject 实例”两种模式。

### The Vegetation Engine

来源：https://assetstore.unity.com/packages/tools/terrain/the-vegetation-engine-159647

提炼能力：风、交互、季节、湿润和植被材质统一。植被资产元数据应包含风分组、交互层、季节性和湿润响应。

## 道路、路径、河流和样条

### EasyRoads3D Pro

来源：
- https://assetstore.unity.com/packages/tools/terrain/easyroads3d-pro-v3-469
- https://www.easyroads3d.com/v3/html/terrain.html

提炼能力：道路网络、地形整形、山路 cut/fill。Arkoura RoadProfile 需要最大坡度、最小曲率半径、路肩、边坡、桥梁和隧道候选标记。

### Curvy Splines / Dreamteck Splines

来源：
- https://assetstore.unity.com/packages/tools/utilities/curvy-splines-7038
- https://assetstore.unity.com/packages/tools/utilities/dreamteck-splines-61926

提炼能力：高质量 spline 编辑、mesh extrusion、轨道/围栏/道路。核心层只保存 ArkouraSpline，具体插件只是输出后端。

### R.A.M 3 / GeNa Pro

同样提供道路、河流、围墙和地形整形经验，说明 spline 不是单一“线”，而是带宽度、侧坡、影响范围和材质通道的走廊。

## 悬崖、洞穴和山体

### Digger

来源：https://ofux.github.io/Digger-Documentation/

提炼能力：保留 Unity Terrain 的同时用 voxel/mesh 表达洞穴、悬挑和垂直切面。Arkoura 应采用混合地形：Terrain 表达可高度场区域，Mesh/Voxel 表达非高度场几何。

### Terrain Holes

来源：https://docs.unity3d.com/6000.0/Documentation/Manual/terrain-holes.html

提炼能力：洞口可通过 Terrain hole 连接地下 Mesh，但 hole 不是洞穴生成算法本身。

### Mesh Terrain Editor / MicroSplat Mesh Terrains

来源：
- https://assetstore.unity.com/packages/tools/terrain/mesh-terrain-editor-free-67758
- https://assetstore.unity.com/packages/tools/terrain/microsplat-mesh-terrains-112739

提炼能力：远景山体、悬崖、洞穴和固定区块可以转 mesh，避免强迫所有地貌适配 Terrain heightfield。

## 照明、探针、天空和后处理

### Bakery GPU Lightmapper

来源：https://geom.io/bakery/wiki/index.php?title=Bakery_-_GPU_Lightmapper

提炼能力：高质量 GPU 烘焙可以作为可选后端。Arkoura 不能假设所有场景实时光照，也不能假设所有目标平台能运行同一种烘焙工具。

### Magic Light Probes

来源：https://assetstore.unity.com/packages/tools/utilities/magic-light-probes-157560

提炼能力：探针自动放置应关注几何、阴影、室内外和可达空间，而不是规则网格。

### Enviro / Azure[Sky] / Volumetric Fog & Mist / Beautify

来源：
- https://assetstore.unity.com/packages/tools/particles-effects/enviro-3-sky-and-weather-252642
- https://assetstore.unity.com/packages/tools/particles-effects/azure-sky-dynamic-skybox-36050
- https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/volumetric-fog-mist-2-162694
- https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/beautify-3-urp-image-enhancer-287146

提炼能力：时间、天气、曝光、雾、高度雾、色调、Bloom、AO 等是 WorldProfile/RegionProfile，而不是每个 chunk 的随机设置。洞穴、室内、沼泽、雪山等应通过 Volume 进行局部覆盖。

## 建筑和聚落

### Dungeon Architect

来源：https://assetstore.unity.com/packages/tools/game-toolkits/dungeon-architect-53895

提炼能力：规则图、模块连接器、城市/洞穴/室内生成。Arkoura 建筑系统应把“街道图 -> block -> parcel -> footprint -> module graph -> prefab”分层。

### Archimatix Pro

来源：https://assetstore.unity.com/packages/tools/modeling/archimatix-pro-59733

提炼能力：参数化几何、UV、LOD、节点组合。建筑不是随机 prefab 散布，必须有地块、入口、朝向、退界和服务空间。

### Houdini Engine

适合复杂建筑 kitbash、立面和批量 UV/LOD 生成，因此 Arkoura 建筑输出要允许外部 DCC 接管局部阶段。

## 大世界流送

### World Streamer 2 / SECTR / Gaia Pro

来源：
- https://assetstore.unity.com/packages/tools/terrain/world-streamer-2-176482
- https://assetstore.unity.com/packages/tools/terrain/sectr-complete-2019-144433
- https://proceduralworlds.com/products/professional/gaia-pro/

提炼能力：chunk scene、依赖、邻接、浮点原点、异步加载、LOD/HLOD、内存预算必须是一等数据。Arkoura WorldCellManifest 负责描述这些内容。

## Arkoura 采用的统一分层

WorldSpec
→ Macro Layout
→ Semantic Fields
→ Height / Hydrology
→ Biomes
→ Cliffs / Caves
→ Road & River Graphs
→ Settlements / Parcels
→ Surface Weights
→ Asset Instances
→ Water Backend
→ Lighting / Probe Plan
→ Nav / Gameplay Bake
→ HLOD / Streaming Manifest
→ Validation
→ Targeted Repair

## 插件适配规则

Arkoura 将识别和记录以下能力，而不是硬编码插件名称：

- TerrainGeneration
- TerrainShader
- TerrainObjectBlending
- WaterOcean
- WaterRiver
- VegetationRendering
- RoadSpline
- RiverSpline
- ProceduralBuildings
- CaveVoxel
- LightBaking
- ProbePlacement
- SkyWeather
- PostProcessing
- WorldStreaming
- GISStreaming
- ExternalProceduralDcc

同一能力可以由 Unity 原生后端或多个第三方后端实现。选择插件只能改变“如何编译”，不能改变 WorldSpec、SemanticField、WaterBody、RoadNetwork 或 SettlementPlan 的含义。

## 版本策略

- 核心包保持最低 Unity 6.x 兼容，不把 6.6 专属 API 写入核心数据层。
- 推荐锁生产版本时优先 Unity 6.3 LTS；需要 6.6 新功能时建立独立验证分支。
- 任何插件升级都先跑 Arkoura 的确定性、边界、水文、道路、材质归一化和流送 manifest 测试。
- 禁止把付费插件源码复制进 Arkoura 仓库。只实现公开 API 的可选适配器或通过反射检测安装状态。
