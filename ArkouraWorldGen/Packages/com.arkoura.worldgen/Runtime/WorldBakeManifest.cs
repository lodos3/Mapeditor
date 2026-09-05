using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arkoura.WorldGen
{
    [Serializable]
    public sealed class WorldCellManifest
    {
        public Vector2Int 坐标;
        public Bounds 世界边界;
        public string 内容哈希;
        public int 静态实例数;
        public int 动态实例数;
        public int 三角形预算;
        public int 材质预算;
        public bool 含水体;
        public bool 含道路;
        public bool 含聚落;
        public bool 含洞穴;
        public bool 需要导航数据;
        public bool 需要照明数据;
        public string 场景地址;
        public string HLOD地址;
        public string 导航地址;
        public string 照明地址;
        public List<Vector2Int> 依赖单元 = new List<Vector2Int>();
    }

    [CreateAssetMenu(menuName = "Arkoura/世界生成/世界烘焙清单", fileName = "WorldBakeManifest")]
    public sealed class WorldBakeManifest : ScriptableObject
    {
        [SerializeField] private string 世界标识;
        [SerializeField] private int 世界种子;
        [SerializeField] private string 生成器版本 = "0.2.0";
        [SerializeField] private List<WorldCellManifest> 单元 = new List<WorldCellManifest>();
        public string WorldId => 世界标识;
        public int Seed => 世界种子;
        public string GeneratorVersion => 生成器版本;
        public IReadOnlyList<WorldCellManifest> Cells => 单元;

        public void Replace(string 标识, int 种子, IEnumerable<WorldCellManifest> 新单元)
        {
            世界标识 = 标识;
            世界种子 = 种子;
            单元.Clear();
            if (新单元 != null) 单元.AddRange(新单元);
        }
    }

    /// <summary>
    /// 统一性能预算。生成器必须在内容被提交到最终场景前进行预算审计。
    /// </summary>
    [CreateAssetMenu(menuName = "Arkoura/世界生成/性能预算", fileName = "WorldPerformanceBudget")]
    public sealed class WorldPerformanceBudget : ScriptableObject
    {
        [SerializeField, Min(1)] private int 每单元最大静态实例 = 30000;
        [SerializeField, Min(1)] private int 每单元最大动态实例 = 1500;
        [SerializeField, Min(1000)] private int 每单元最大可见三角形 = 4000000;
        [SerializeField, Min(1)] private int 每单元最大材质 = 128;
        [SerializeField, Min(1)] private int 每帧最大活跃单元 = 49;
        [SerializeField, Min(64)] private int 目标系统内存MB = 8192;
        [SerializeField, Min(64)] private int 目标显存MB = 6144;
        [SerializeField, Min(1)] private int 最大实时阴影灯 = 8;
        public int MaxStaticInstances => 每单元最大静态实例;
        public int MaxDynamicInstances => 每单元最大动态实例;
        public int MaxVisibleTriangles => 每单元最大可见三角形;
        public int MaxMaterials => 每单元最大材质;
        public int MaxActiveCells => 每帧最大活跃单元;
        public int TargetRamMb => 目标系统内存MB;
        public int TargetVramMb => 目标显存MB;
        public int MaxRealtimeShadowLights => 最大实时阴影灯;
    }
}
