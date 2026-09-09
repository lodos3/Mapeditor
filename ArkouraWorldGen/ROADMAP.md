# Arkoura WorldGen 路线图

## 架构目标

最终目标不是单一“随机地图按钮”，而是一个可审计、可局部重建、可切换环境后端的 AAA 世界编译系统。AI 负责世界意图和修复建议，所有可交付场景数据由确定性阶段生成。

## 已建立

- 世界级种子和全局坐标连续采样。
- 分块 Terrain 高度生成。
- 宏观/中观/微观地貌分层。
- WorldSemanticFields。
- 水文汇流、海洋/河流/岸线 mask。
- 语义驱动地表权重。
- 生物群落和资产约束库。
- 自然资产散布。
- 聚落初始生成。
- Terrain 路径 A* 与语义场道路求解器。
- WaterBody / ArkouraSpline 中间格式。
- 悬崖带提取。
- 照明规划数据。
- Chunk 流送原型。
- WorldCellManifest / 性能预算数据。
- AI Planner 服务端接口。
- 第三方环境插件能力目录和检测器。
- 基础确定性、边界和水文测试。

## P0：从框架升级为生产世界编译器

1. Compute Shader / Burst 语义场与高度场后端，CPU 实现保留作为参考后端。
2. Priority-Flood 洼地填充、真实 watershed、湖泊盆地、河网等级、侵蚀/沉积近似。
3. 可缓存 DAG：每个生成节点有输入哈希、输出哈希、依赖和脏区。
4. 只重建受影响 chunk + halo，禁止全世界重算。
5. World Origin / 大坐标策略，处理 16 km+ 世界。
6. Addressables/Scene 分区烘焙器，生成 manifest 和异步依赖。
7. 自动 HLOD，支持 Terrain 远景 mesh、建筑合并和自然资产 cluster HLOD。
8. GPU 实例缓冲输出，避免高密度植被全部实例化为 GameObject。

## P0：地形与地质

- 宏观构造板块式山脉和主要盆地。
- 山脊图、谷底图、坡肩、崖底和侵蚀通道。
- Cliff Band 编译成 Terrain triplanar / Mesh cliff / voxel cave 三种后端。
- Terrain holes + cave portal 图。
- 高度图/控制图导入：RAW、EXR、PNG、World Creator、Houdini Heightfield。
- 多 tile seam/stitch 审计。
- 海岸侵蚀、沙滩坡面、河岸 terrace。

## P0：地表和 UV

- SurfaceProfile 语义规则扩展到土壤深度、积雪、湿润、海岸盐蚀、道路污染。
- Shader 后端适配器：Unity Terrain Lit、GTS、MicroSplat。
- Triplanar cliff intent。
- Height blend / anti-tiling / world projection intent。
- 对象到地形的底部融合：深度/高度融合、terrain tint、projected normal、decal skirt。
- 地表 decal 编译：泥、水渍、车辙、苔藓、落叶、雪线。

## P0：水体

- 河流中心线从真实下游拓扑提取，不再使用连通块排序近似。
- 河级、宽度、深度、流速和洪泛区。
- Lake basin 和溢流口。
- 海岸线简化与 LOD。
- Waterfall 节点和悬崖落差检测。
- Crest / R.A.M 3 / HDRP Water 可选编译后端。
- 岸线 foam、湿边、植被清除、河床 carve/paint。
- 水下 Volume、游泳、船只、浮力和音频区域数据。

## P0：道路与路径

- 连接所有关键聚落/地标的最小生成树 + 次级网络。
- 路径代价考虑坡度、曲率、水体、悬崖、湿地、土地价值和危险区。
- 道路 spline 平滑和最小转弯半径。
- Cut/fill 地形工程。
- 桥梁跨度检测和桥台。
- 隧道候选和 cave mesh 连接。
- 路肩、沟渠、护坡、护栏、路标、道路 decal。
- Unity Splines / EasyRoads3D / GeNa / Curvy / Dreamteck 编译后端。

## P0：聚落与建筑

- 道路图 → block → parcel → building footprint。
- 地块退界、入口朝路、地基找平、后院和服务通道。
- 建筑用途 zoning：住宅、商业、工业、宗教、行政、防御。
- 城墙/门/塔与地形关系。
- 建筑模块连接器和 interior hooks。
- Dungeon Architect / Houdini / Archimatix 可选编译后端。
- 景观物件语义：围栏、灯、箱、井、市场、码头。

## P1：植被生态

- 基于生态位而不是随机密度的植被。
- 土壤、湿度、温度、坡向、光照、水距、道路距和演替阶段。
- 蓝噪声 + cluster + edge ecology。
- 林缘、林间空地、倒木、幼树、灌木层和地被层。
- GPU Instancer / Nature Renderer 后端。
- 风分组、季节、湿润、雪、交互植被元数据。

## P1：照明与天气

- APV 自动区域规划和 streaming bake set。
- 传统 Light Probe fallback。
- Reflection Probe 依据水体、室内和聚落规划。
- 洞穴/室内局部 Volume。
- 昼夜、天气、云、雾、高度雾、曝光区域。
- Unity Compute Light Baker / Bakery 可选烘焙后端。
- Enviro / Azure Sky 可选天气后端。
- Render Pipeline capability profile，URP 与 HDRP 分开审计。

## P1：后处理和视觉质量

- Volume 图层：世界、区域、洞穴、室内、天气、剧情区域。
- TAA/FSR/DLSS 平台策略由项目后端决定。
- Bloom、AO、color grading、fog 的预算和可读性验证。
- 自动截图审计，检查过曝、纯黑、雾遮挡和重复资产模式。

## P1：AI Director / Repair Agent

- 自然语言 → WorldPlan，不直接返回 Unity 对象。
- 规划 schema 带 hard/soft constraints。
- 生成后分析世界指标。
- 局部修复命令：移动建筑、重算道路、加桥、清树、重建岸线等。
- 每个修复动作记录输入、原因、结果和可回滚 diff。
- 绝不允许模型直接执行未验证任意 C# 或 Editor 脚本。

## P1：AAA 验证门槛

- 地形 seam = 0。
- 河流不能逆坡，除瀑布/人工水渠等明确节点。
- 道路不超过 profile 最大坡度，除明确特殊段。
- 建筑不得互相穿插或遮挡入口。
- 所有关键地点必须存在可导航连接。
- Terrain Layer 权重每像素归一化。
- 每单元对象、材质、三角形、显存、阴影灯和探针数量必须在预算内。
- HLOD 转换距离必须无明显 pop。
- 流送时不能出现邻接 seam、未加载 collider 或错误 NavMesh 断层。
- 所有随机阶段必须在相同 seed + 输入下位级确定性。

## P2：高级方向

- GPU erosion 和 sediment transport。
- 道路车辆/行人使用热图反向优化城市布局。
- 游戏玩法热图反馈到 POI/资源/敌人分布。
- 多层地下世界和 cave graph。
- World Creator/Houdini 双向同步。
- Cesium 地理数据导入作为现实世界起点。
- 自动 benchmark 场景矩阵和平台 profile。
