using System;
using UnityEngine;

namespace Arkoura.WorldGen
{
    public enum AssetCategory
    {
        Cliff, Rock, Tree, Shrub, Grass, Building, Wall, RoadProp, WaterProp, Ruin, Landmark, Decoration, Cave, Bridge, TerrainPatch
    }

    public enum RotationRule
    {
        RandomY, AlignSlope, FaceRoad, FaceWater, FaceDownSlope, Fixed
    }

    [Serializable]
    public sealed class SurfaceBlendIntent
    {
        public bool 启用 = true;
        [Min(0f)] public float 融合宽度 = 1.5f;
        [Range(0f, 1f)] public float 地形颜色影响 = 0.35f;
        [Range(0f, 1f)] public float 地形法线影响 = 0.35f;
        [Range(0f, 1f)] public float 顶部投影强度 = 0.25f;
        public bool 三平面投影;
        public bool 接收湿润 = true;
        public bool 接收积雪 = true;
        public bool 允许Decal过渡 = true;
    }

    [Serializable]
    public sealed class RenderingIntent
    {
        public bool GPU实例化优先 = true;
        public bool 静态批处理候选 = true;
        public bool HLOD候选 = true;
        [Min(0f)] public float 最大可见距离 = 500f;
        [Min(0f)] public float 阴影距离 = 120f;
        public int LOD组 = 0;
        public string 风分组;
        public bool 季节响应;
        public bool 交互植被;
    }

    [CreateAssetMenu(menuName = "Arkoura/世界生成/资产元数据", fileName = "AssetMetadata")]
    public sealed class AssetMetadata : ScriptableObject
    {
        [Header("身份")]
        [SerializeField] private string 资产标识;
        [SerializeField] private GameObject 预制体;
        [SerializeField] private AssetCategory 分类;
        [SerializeField] private string[] 标签 = Array.Empty<string>();
        [SerializeField] private string[] 生物群落 = Array.Empty<string>();

        [Header("空间")]
        [SerializeField] private Vector2 占地尺寸 = new Vector2(2f, 2f);
        [SerializeField, Min(0.1f)] private float 最小间距 = 2f;
        [SerializeField] private Vector2 缩放范围 = new Vector2(0.9f, 1.1f);
        [SerializeField, Range(0f, 90f)] private float 最小坡度;
        [SerializeField, Range(0f, 90f)] private float 最大坡度 = 35f;
        [SerializeField] private Vector2 海拔范围 = new Vector2(-1000f, 10000f);
        [SerializeField] private RotationRule 朝向规则 = RotationRule.RandomY;
        [SerializeField] private Vector3 固定欧拉角;

        [Header("环境约束")]
        [SerializeField] private Vector2 道路距离范围 = new Vector2(0f, 100000f);
        [SerializeField] private Vector2 水体距离范围 = new Vector2(0f, 100000f);
        [SerializeField, Range(0f, 1f)] private float 最低湿度;
        [SerializeField, Range(0f, 1f)] private float 最高湿度 = 1f;
        [SerializeField, Range(0f, 1f)] private float 最低温度;
        [SerializeField, Range(0f, 1f)] private float 最高温度 = 1f;
        [SerializeField, Range(0f, 1f)] private float 聚类倾向 = 0.5f;
        [SerializeField, Range(0f, 1f)] private float 稀有度 = 0.5f;
        [SerializeField, Min(0)] private int 每平方公里最大实例 = 5000;

        [Header("地表融合")]
        [SerializeField] private SurfaceBlendIntent 地表融合 = new SurfaceBlendIntent();

        [Header("渲染")]
        [SerializeField] private RenderingIntent 渲染意图 = new RenderingIntent();

        [Header("游戏规则")]
        [SerializeField] private bool 阻挡导航;
        [SerializeField] private bool 需要地基;
        [SerializeField] private Vector3 入口本地偏移;
        [SerializeField] private bool 允许随机镜像;

        public string Id => 资产标识;
        public GameObject Prefab => 预制体;
        public AssetCategory Category => 分类;
        public string[] Tags => 标签;
        public string[] Biomes => 生物群落;
        public Vector2 Footprint => 占地尺寸;
        public float MinimumSpacing => 最小间距;
        public Vector2 ScaleRange => 缩放范围;
        public RotationRule Rotation => 朝向规则;
        public Vector3 FixedEuler => 固定欧拉角;
        public bool BlocksNavigation => 阻挡导航;
        public bool RequiresFoundation => 需要地基;
        public Vector3 EntranceOffset => 入口本地偏移;
        public bool RandomMirror => 允许随机镜像;
        public int MaxInstancesPerKm2 => 每平方公里最大实例;
        public float ClusterBias => 聚类倾向;
        public float Rarity => 稀有度;
        public Vector2 RoadDistanceRange => 道路距离范围;
        public Vector2 WaterDistanceRange => 水体距离范围;
        public SurfaceBlendIntent SurfaceBlend => 地表融合;
        public RenderingIntent Rendering => 渲染意图;

        public bool IsAllowed(float 高度, float 坡度, string 生物群落名, float 湿度 = 0.5f, float 温度 = 0.5f)
        {
            if (高度 < 海拔范围.x || 高度 > 海拔范围.y) return false;
            if (坡度 < 最小坡度 || 坡度 > 最大坡度) return false;
            if (湿度 < 最低湿度 || 湿度 > 最高湿度) return false;
            if (温度 < 最低温度 || 温度 > 最高温度) return false;
            if (生物群落 == null || 生物群落.Length == 0 || string.IsNullOrWhiteSpace(生物群落名)) return true;
            for (int 索引 = 0; 索引 < 生物群落.Length; 索引++)
            {
                if (string.Equals(生物群落[索引], 生物群落名, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
    }
}
