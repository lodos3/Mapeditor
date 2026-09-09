using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arkoura.WorldGen
{
    [Flags]
    public enum EnvironmentCapability
    {
        None = 0,
        TerrainGeneration = 1 << 0,
        TerrainShader = 1 << 1,
        TerrainObjectBlending = 1 << 2,
        WaterOcean = 1 << 3,
        WaterRiver = 1 << 4,
        VegetationRendering = 1 << 5,
        RoadSpline = 1 << 6,
        RiverSpline = 1 << 7,
        ProceduralBuildings = 1 << 8,
        CaveVoxel = 1 << 9,
        LightBaking = 1 << 10,
        ProbePlacement = 1 << 11,
        SkyWeather = 1 << 12,
        PostProcessing = 1 << 13,
        WorldStreaming = 1 << 14,
        GISStreaming = 1 << 15,
        ExternalProceduralDcc = 1 << 16
    }

    [Serializable]
    public sealed class EnvironmentIntegrationDescriptor
    {
        public string 标识;
        public string 显示名;
        public EnvironmentCapability 能力;
        public string[] 类型线索 = Array.Empty<string>();
        public string[] 程序集线索 = Array.Empty<string>();
        [TextArea] public string 备注;
    }

    /// <summary>
    /// 插件只作为输出后端。核心世界数据永远不依赖第三方类型。
    /// </summary>
    public static class EnvironmentIntegrationCatalog
    {
        public static IReadOnlyList<EnvironmentIntegrationDescriptor> Known => 已知;

        private static readonly EnvironmentIntegrationDescriptor[] 已知 =
        {
            D("unity-native", "Unity Native", EnvironmentCapability.TerrainGeneration | EnvironmentCapability.RoadSpline | EnvironmentCapability.LightBaking | EnvironmentCapability.ProbePlacement | EnvironmentCapability.PostProcessing | EnvironmentCapability.WorldStreaming, "UnityEngine.Terrain", "UnityEngine"),
            D("gaia-pro", "Gaia Pro", EnvironmentCapability.TerrainGeneration | EnvironmentCapability.WorldStreaming | EnvironmentCapability.TerrainShader | EnvironmentCapability.VegetationRendering, "Gaia.GaiaSessionManager", "Gaia"),
            D("gena-pro", "GeNa Pro", EnvironmentCapability.RoadSpline | EnvironmentCapability.RiverSpline | EnvironmentCapability.VegetationRendering | EnvironmentCapability.ProbePlacement, "GeNa.Core.Spawner", "GeNa"),
            D("gts", "GTS Terrain Shader", EnvironmentCapability.TerrainShader | EnvironmentCapability.TerrainObjectBlending, "GTS.GTSComponent", "GTS"),
            D("microsplat", "MicroSplat", EnvironmentCapability.TerrainShader | EnvironmentCapability.TerrainObjectBlending, "JBooth.MicroSplat.MicroSplatTerrain", "MicroSplat"),
            D("mapmagic2", "MapMagic 2", EnvironmentCapability.TerrainGeneration | EnvironmentCapability.VegetationRendering, "MapMagic.Core.MapMagicObject", "MapMagic"),
            D("microverse", "MicroVerse", EnvironmentCapability.TerrainGeneration | EnvironmentCapability.VegetationRendering, "JBooth.MicroVerseCore.MicroVerse", "MicroVerse"),
            D("crest", "Crest Water", EnvironmentCapability.WaterOcean | EnvironmentCapability.WaterRiver, "Crest.OceanRenderer", "Crest"),
            D("ram3", "R.A.M 3", EnvironmentCapability.WaterOcean | EnvironmentCapability.WaterRiver | EnvironmentCapability.RiverSpline | EnvironmentCapability.RoadSpline, "NatureManufacture.RAM", "NatureManufacture"),
            D("easyroads3d", "EasyRoads3D Pro", EnvironmentCapability.RoadSpline, "EasyRoads3Dv3.ERRoadNetwork", "EasyRoads3D"),
            D("curvy", "Curvy Splines", EnvironmentCapability.RoadSpline | EnvironmentCapability.RiverSpline, "FluffyUnderware.Curvy.CurvySpline", "Curvy"),
            D("dreamteck", "Dreamteck Splines", EnvironmentCapability.RoadSpline | EnvironmentCapability.RiverSpline, "Dreamteck.Splines.SplineComputer", "Dreamteck"),
            D("digger", "Digger", EnvironmentCapability.CaveVoxel, "Digger.Modules.Core.Sources.DiggerMaster", "Digger"),
            D("nature-renderer", "Nature Renderer", EnvironmentCapability.VegetationRendering, "VisualDesignCafe.Nature.NatureRenderer", "NatureRenderer"),
            D("gpu-instancer", "GPU Instancer Pro", EnvironmentCapability.VegetationRendering, "GPUInstancer.GPUInstancerManager", "GPUInstancer"),
            D("bakery", "Bakery GPU Lightmapper", EnvironmentCapability.LightBaking, "ftRenderLightmap", "Bakery"),
            D("magic-light-probes", "Magic Light Probes", EnvironmentCapability.ProbePlacement, "MagicLightProbes.MagicLightProbes", "MagicLightProbes"),
            D("enviro", "Enviro", EnvironmentCapability.SkyWeather | EnvironmentCapability.PostProcessing, "Enviro.EnviroManager", "Enviro"),
            D("dungeon-architect", "Dungeon Architect", EnvironmentCapability.ProceduralBuildings, "DungeonArchitect.Dungeon", "DungeonArchitect"),
            D("houdini-engine", "Houdini Engine", EnvironmentCapability.ExternalProceduralDcc | EnvironmentCapability.ProceduralBuildings | EnvironmentCapability.TerrainGeneration, "HoudiniEngineUnity.HEU_HoudiniAssetRoot", "HoudiniEngineUnity"),
            D("cesium", "Cesium for Unity", EnvironmentCapability.GISStreaming | EnvironmentCapability.WorldStreaming, "CesiumForUnity.Cesium3DTileset", "CesiumForUnity")
        };

        private static EnvironmentIntegrationDescriptor D(string 标识, string 名称, EnvironmentCapability 能力, string 类型, string 程序集)
        {
            return new EnvironmentIntegrationDescriptor { 标识 = 标识, 显示名 = 名称, 能力 = 能力, 类型线索 = new[] { 类型 }, 程序集线索 = new[] { 程序集 } };
        }
    }
}
