using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arkoura.WorldGen
{
    [Serializable]
    public sealed class RegionSpec
    {
        public string 名称;
        public string 生物群落;
        [Range(0f, 1f)] public float 中心X = 0.5f;
        [Range(0f, 1f)] public float 中心Z = 0.5f;
        [Range(0.01f, 1f)] public float 半径 = 0.25f;
        [Range(0f, 1f)] public float 强度 = 1f;
    }

    [Serializable]
    public sealed class LandmarkSpec
    {
        public string 名称;
        public string 类型;
        [Range(0f, 1f)] public float X = 0.5f;
        [Range(0f, 1f)] public float Z = 0.5f;
        public float 半径 = 120f;
    }

    [Serializable]
    public sealed class TerrainRule
    {
        public TerrainLayer 地形层;
        [Range(0f, 1f)] public float 最低归一高度;
        [Range(0f, 1f)] public float 最高归一高度 = 1f;
        [Range(0f, 90f)] public float 最低坡度;
        [Range(0f, 90f)] public float 最高坡度 = 90f;
        [Min(0.0001f)] public float 权重锐度 = 1f;
    }

    [CreateAssetMenu(menuName = "Arkoura/世界生成/世界规格", fileName = "WorldSpec")]
    public sealed class WorldSpec : ScriptableObject
    {
        [Header("世界")]
        [SerializeField, Min(256f)] private float 世界尺寸米 = 8192f;
        [SerializeField, Min(128f)] private float 分块尺寸米 = 512f;
        [SerializeField, Min(64f)] private float 最大地形高度 = 900f;
        [SerializeField] private int 种子 = 1337;
        [SerializeField] private int 高度图分辨率 = 513;
        [SerializeField] private int 材质图分辨率 = 256;
        [SerializeField, Range(33, 513)] private int 语义场分辨率 = 129;

        [Header("分层地形")]
        [SerializeField, Range(0f, 1f)] private float 海平面归一高度 = 0.18f;
        [SerializeField] private float 宏观频率 = 0.00022f;
        [SerializeField] private float 中观频率 = 0.0011f;
        [SerializeField] private float 微观频率 = 0.006f;
        [SerializeField, Range(0f, 1f)] private float 宏观权重 = 0.65f;
        [SerializeField, Range(0f, 1f)] private float 中观权重 = 0.27f;
        [SerializeField, Range(0f, 1f)] private float 微观权重 = 0.08f;
        [SerializeField, Range(0f, 1f)] private float 山脊强度 = 0.35f;

        [Header("环境系统")]
        [SerializeField] private HydrologySettings 水文 = new HydrologySettings();
        [SerializeField] private SurfaceProfile 地表配置;
        [SerializeField] private WaterProfile 水体配置;
        [SerializeField] private RoadProfile 道路配置;
        [SerializeField] private LightingProfile 照明配置;
        [SerializeField] private WorldPerformanceBudget 性能预算;

        [Header("内容")]
        [SerializeField] private List<RegionSpec> 区域 = new List<RegionSpec>();
        [SerializeField] private List<LandmarkSpec> 地标 = new List<LandmarkSpec>();
        [SerializeField] private List<TerrainRule> 地形材质规则 = new List<TerrainRule>();
        [SerializeField, Min(0)] private int 每块最大散布对象 = 2500;
        [SerializeField, Min(0)] private int 每个聚落最大建筑 = 80;

        public float WorldSize => 世界尺寸米;
        public float ChunkSize => 分块尺寸米;
        public float MaxTerrainHeight => 最大地形高度;
        public int Seed => 种子;
        public int HeightResolution => 高度图分辨率;
        public int AlphaResolution => 材质图分辨率;
        public int SemanticResolution => 语义场分辨率;
        public float SeaLevel01 => 海平面归一高度;
        public float MacroFrequency => 宏观频率;
        public float MesoFrequency => 中观频率;
        public float MicroFrequency => 微观频率;
        public float MacroWeight => 宏观权重;
        public float MesoWeight => 中观权重;
        public float MicroWeight => 微观权重;
        public float RidgeStrength => 山脊强度;
        public HydrologySettings Hydrology => 水文;
        public SurfaceProfile SurfaceProfile => 地表配置;
        public WaterProfile WaterProfile => 水体配置;
        public RoadProfile RoadProfile => 道路配置;
        public LightingProfile LightingProfile => 照明配置;
        public WorldPerformanceBudget PerformanceBudget => 性能预算;
        public IReadOnlyList<RegionSpec> Regions => 区域;
        public IReadOnlyList<LandmarkSpec> Landmarks => 地标;
        public IReadOnlyList<TerrainRule> TerrainRules => 地形材质规则;
        public int MaxScatterPerChunk => 每块最大散布对象;
        public int MaxBuildingsPerSettlement => 每个聚落最大建筑;
        public int ChunkCount => Mathf.Max(1, Mathf.RoundToInt(世界尺寸米 / 分块尺寸米));
    }
}
