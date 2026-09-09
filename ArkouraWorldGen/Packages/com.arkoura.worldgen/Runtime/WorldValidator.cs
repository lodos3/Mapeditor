using System.Collections.Generic;
using UnityEngine;

namespace Arkoura.WorldGen
{
    public enum ValidationSeverity { Info, Warning, Error }

    public readonly struct ValidationMessage
    {
        public readonly ValidationSeverity Severity;
        public readonly string Message;
        public ValidationMessage(ValidationSeverity 严重级别, string 信息) { Severity = 严重级别; Message = 信息; }
    }

    public static class WorldValidator
    {
        public static List<ValidationMessage> Validate(WorldSpec 规格, AssetLibrary 资产库)
        {
            var 消息 = new List<ValidationMessage>();
            if (规格 == null)
            {
                消息.Add(new ValidationMessage(ValidationSeverity.Error, "缺少 WorldSpec。"));
                return 消息;
            }

            if (规格.WorldSize % 规格.ChunkSize > 0.01f) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, "世界尺寸不能被分块尺寸整除，会产生边缘尺寸误差。"));
            if (!IsPowerOfTwoPlusOne(规格.HeightResolution)) 消息.Add(new ValidationMessage(ValidationSeverity.Error, "Terrain 高度图分辨率应为 2^n + 1，例如 257、513、1025。"));
            if (!IsPowerOfTwoPlusOne(规格.SemanticResolution)) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, "语义场分辨率建议为 2^n + 1，便于跨尺度采样和 Compute Shader 后端。"));
            if (规格.SemanticResolution > 规格.HeightResolution) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, "语义场分辨率高于高度图，没有实际空间精度收益。"));

            long 分块数 = (long)规格.ChunkCount * 规格.ChunkCount;
            long 理论对象 = 分块数 * 规格.MaxScatterPerChunk;
            if (理论对象 > 1000000) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, $"理论散布对象上限为 {理论对象:N0}。生产版本必须使用 GPU 实例化、HLOD 或流送。"));
            if (规格.ChunkCount > 64) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, $"世界每轴包含 {规格.ChunkCount} 个单元。应启用分层 manifest、异步加载和浮点原点策略。"));

            if (资产库 == null) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, "缺少 AssetLibrary，将只生成地形。"));
            else ValidateLibrary(资产库, 消息);

            bool 有传统材质 = 规格.TerrainRules.Count > 0;
            bool 有语义材质 = 规格.SurfaceProfile != null && 规格.SurfaceProfile.Rules.Count > 0;
            if (!有传统材质 && !有语义材质) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, "没有地表规则，不会生成 Terrain Layer 权重。"));
            if (有语义材质 && 规格.SurfaceProfile.Rules.Count > 16) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, "单个地表配置超过 16 层。应按生物群落/流送区域裁剪材质集合，避免着色器和纹理采样成本失控。"));

            if (规格.Hydrology == null) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, "缺少水文配置，河流和岸线语义不可用。"));
            if (规格.WaterProfile == null) 消息.Add(new ValidationMessage(ValidationSeverity.Info, "没有 WaterProfile。水文仍会生成，但不会有完整水体编译配置。"));
            if (规格.RoadProfile == null) 消息.Add(new ValidationMessage(ValidationSeverity.Info, "没有 RoadProfile。道路可规划，但无法应用项目级道路规格。"));
            if (规格.LightingProfile == null) 消息.Add(new ValidationMessage(ValidationSeverity.Info, "没有 LightingProfile。不会生成项目级 APV/探针规划。"));
            if (规格.PerformanceBudget == null) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, "缺少性能预算。AAA 世界生成不能在没有 CPU/GPU/内存预算的情况下批准内容。"));

            float 权重总和 = 规格.MacroWeight + 规格.MesoWeight + 规格.MicroWeight;
            if (Mathf.Abs(权重总和 - 1f) > 0.12f) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, $"宏观/中观/微观地形权重总和为 {权重总和:F2}，建议归一化接近 1。"));
            if (规格.MicroWeight > 规格.MesoWeight || 规格.MesoWeight > 规格.MacroWeight) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, "地形频率权重会使高频细节主导地貌，可能重新产生噪声感。"));

            if (消息.Count == 0) 消息.Add(new ValidationMessage(ValidationSeverity.Info, "基础验证通过。"));
            return 消息;
        }

        public static List<ValidationMessage> ValidateSemanticFields(WorldSemanticFields 场)
        {
            var 消息 = new List<ValidationMessage>();
            if (场 == null)
            {
                消息.Add(new ValidationMessage(ValidationSeverity.Error, "语义场为空。"));
                return 消息;
            }
            int 分辨率 = 场.Resolution;
            int 上坡河流 = 0;
            int 非法值 = 0;
            for (int z = 0; z < 分辨率; z++)
            for (int x = 0; x < 分辨率; x++)
            {
                if (float.IsNaN(场.Height01[z, x]) || float.IsInfinity(场.Height01[z, x])) 非法值++;
                if (场.WaterMask[z, x] > 0.5f && 场.Flow01[z, x] < 0f) 上坡河流++;
            }
            if (非法值 > 0) 消息.Add(new ValidationMessage(ValidationSeverity.Error, $"语义场包含 {非法值} 个 NaN/Infinity 高度。"));
            if (上坡河流 > 0) 消息.Add(new ValidationMessage(ValidationSeverity.Error, $"检测到 {上坡河流} 个非法水流单元。"));
            if (消息.Count == 0) 消息.Add(new ValidationMessage(ValidationSeverity.Info, "语义场基础验证通过。"));
            return 消息;
        }

        private static void ValidateLibrary(AssetLibrary 资产库, List<ValidationMessage> 消息)
        {
            int 缺失预制体 = 0;
            int 缺失标识 = 0;
            int 不适合实例化 = 0;
            foreach (var 资产 in 资产库.Assets)
            {
                if (资产 == null) continue;
                if (资产.Prefab == null) 缺失预制体++;
                if (string.IsNullOrWhiteSpace(资产.Id)) 缺失标识++;
                if ((资产.Category == AssetCategory.Tree || 资产.Category == AssetCategory.Grass || 资产.Category == AssetCategory.Rock) && 资产.Rendering != null && !资产.Rendering.GPU实例化优先) 不适合实例化++;
            }
            if (缺失预制体 > 0) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, $"有 {缺失预制体} 个资产元数据没有 Prefab。"));
            if (缺失标识 > 0) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, $"有 {缺失标识} 个资产缺少稳定标识。"));
            if (不适合实例化 > 0) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, $"有 {不适合实例化} 个高密度自然资产未标记 GPU 实例化优先。"));
        }

        private static bool IsPowerOfTwoPlusOne(int 值)
        {
            int 减一 = 值 - 1;
            return 减一 > 0 && (减一 & (减一 - 1)) == 0;
        }
    }
}
