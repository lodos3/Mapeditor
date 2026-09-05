# Arkoura WorldGen 当前状态

## 已实现

- Unity 6 UPM 包结构。
- WorldSpec、Region、Landmark、TerrainRule 数据模型。
- 多频率确定性地形高度场。
- 基于高度与坡度的 Terrain Layer 权重图。
- 资产库与扩展 AssetMetadata。
- Prefab 文件夹扫描、分类和初始标签推断。
- 生物群落解析。
- 确定性抖动网格散布。
- 聚落建筑放置。
- 单 Terrain 地形代价 A* 道路求解。
- WorldValidator。
- WorldChunk 与简单距离流送。
- HTTP AI World Planner 接口。
- Editor 多工具窗口。

## 尚未达到 AAA 生产完成标准的部分

当前版本是可运行架构基础，不应被描述为完整 AAA 世界生成器。AAA 标准仍需要 GPU 生成、水文、道路工程、分区持久化、HLOD、Addressables、NavMesh 分块、实例化植被、碰撞代理、生成缓存、局部重建、AI 修复循环、性能预算系统和完整自动化测试。

## 生产原则

1. AI 不直接生成最终几何。AI 输出可验证的 WorldSpec 和局部修复操作。
2. 所有大规模几何与权重图由确定性程序系统生成。
3. 世界必须支持局部重建，禁止因单一区域变化重新生成整个地图。
4. 大型植被和道具禁止以海量独立 GameObject 作为最终运行时表现。
5. 每个生成阶段必须有可度量预算、缓存键和验证结果。
