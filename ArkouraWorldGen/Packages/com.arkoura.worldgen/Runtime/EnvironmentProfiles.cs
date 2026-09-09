using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arkoura.WorldGen
{
    public enum WaterBodyType { Ocean, Lake, River, Wetland, Waterfall }
    public enum PathType { PrimaryRoad, SecondaryRoad, Trail, River, Wall, CliffBand, Rail }

    [Serializable]
    public sealed class ArkouraSpline
    {
        public string 标识;
        public PathType 类型;
        public bool 闭合;
        public List<Vector3> 控制点 = new List<Vector3>();
        public List<float> 宽度 = new List<float>();
        public float 默认宽度 = 6f;
        public float 最大坡度 = 0.14f;
        public float 最小转弯半径 = 8f;
        public float 地形影响半径 = 12f;
        public bool 允许桥梁 = true;
        public bool 允许隧道 = true;
    }

    [Serializable]
    public sealed class WaterBody
    {
        public string 标识;
        public WaterBodyType 类型;
        public ArkouraSpline 中心线;
        public List<Vector3> 岸线 = new List<Vector3>();
        public float 水面高度;
        public float 平均深度 = 2f;
        public float 流速 = 1f;
        public Vector2 流向 = Vector2.down;
        public float 泡沫强度 = 0.4f;
        public bool 生成水下体积 = true;
        public bool 允许游泳 = true;
        public bool 允许船只 = true;
    }

    [CreateAssetMenu(menuName = "Arkoura/世界生成/道路配置", fileName = "RoadProfile")]
    public sealed class RoadProfile : ScriptableObject
    {
        [SerializeField, Min(0.5f)] private float 道路宽度 = 6f;
        [SerializeField, Min(0f)] private float 路肩宽度 = 1.5f;
        [SerializeField, Range(0f, 1f)] private float 最大坡度 = 0.12f;
        [SerializeField, Min(0.5f)] private float 最小转弯半径 = 12f;
        [SerializeField, Min(0f)] private float 地形整形半径 = 10f;
        [SerializeField, Range(0f, 1f)] private float 地形平滑强度 = 0.65f;
        [SerializeField] private bool 允许桥梁 = true;
        [SerializeField] private bool 允许隧道 = true;
        [SerializeField] private Material 路面材质;
        public float Width => 道路宽度;
        public float Shoulder => 路肩宽度;
        public float MaxGrade => 最大坡度;
        public float MinimumRadius => 最小转弯半径;
        public float TerrainInfluence => 地形整形半径;
        public float TerrainSmoothing => 地形平滑强度;
        public bool AllowBridges => 允许桥梁;
        public bool AllowTunnels => 允许隧道;
        public Material Material => 路面材质;
    }

    [CreateAssetMenu(menuName = "Arkoura/世界生成/水体配置", fileName = "WaterProfile")]
    public sealed class WaterProfile : ScriptableObject
    {
        [SerializeField] private Material 水面材质;
        [SerializeField, Min(0f)] private float 河流最小宽度 = 2f;
        [SerializeField, Min(0f)] private float 河流最大宽度 = 24f;
        [SerializeField, Min(0f)] private float 默认深度 = 2f;
        [SerializeField, Min(0f)] private float 岸线影响宽度 = 8f;
        [SerializeField, Min(0f)] private float 泡沫宽度 = 2f;
        [SerializeField] private bool 自动创建水下体积 = true;
        public Material Material => 水面材质;
        public Vector2 RiverWidthRange => new Vector2(河流最小宽度, 河流最大宽度);
        public float DefaultDepth => 默认深度;
        public float ShoreInfluence => 岸线影响宽度;
        public float FoamWidth => 泡沫宽度;
        public bool CreateUnderwaterVolume => 自动创建水下体积;
    }

    [CreateAssetMenu(menuName = "Arkoura/世界生成/照明配置", fileName = "LightingProfile")]
    public sealed class LightingProfile : ScriptableObject
    {
        [SerializeField] private bool 使用AdaptiveProbeVolumes = true;
        [SerializeField, Min(4f)] private float 探针基础间距 = 16f;
        [SerializeField, Min(2f)] private float 室内探针间距 = 6f;
        [SerializeField, Min(16f)] private float 反射探针基础间距 = 96f;
        [SerializeField] private bool 聚落提高探针密度 = true;
        [SerializeField] private bool 洞穴独立照明体积 = true;
        [SerializeField] private bool 每个流送单元独立烘焙数据 = true;
        public bool UseAPV => 使用AdaptiveProbeVolumes;
        public float BaseProbeSpacing => 探针基础间距;
        public float InteriorProbeSpacing => 室内探针间距;
        public float ReflectionProbeSpacing => 反射探针基础间距;
        public bool DenseSettlements => 聚落提高探针密度;
        public bool SeparateCaveVolumes => 洞穴独立照明体积;
        public bool PerCellBakeData => 每个流送单元独立烘焙数据;
    }
}
