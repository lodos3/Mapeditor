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
            long 分块数 = (long)规格.ChunkCount * 规格.ChunkCount;
            long 理论对象 = 分块数 * 规格.MaxScatterPerChunk;
            if (理论对象 > 1000000) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, $"理论散布对象上限为 {理论对象:N0}。生产版本必须使用 GPU 实例化、HLOD 或流送。"));
            if (资产库 == null) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, "缺少 AssetLibrary，将只生成地形。"));
            else
            {
                int 缺失预制体 = 0;
                foreach (var 资产 in 资产库.Assets) if (资产 != null && 资产.Prefab == null) 缺失预制体++;
                if (缺失预制体 > 0) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, $"有 {缺失预制体} 个资产元数据没有 Prefab。"));
            }
            if (规格.TerrainRules.Count == 0) 消息.Add(new ValidationMessage(ValidationSeverity.Warning, "没有 Terrain Layer 规则，不会生成地形材质权重图。"));
            if (消息.Count == 0) 消息.Add(new ValidationMessage(ValidationSeverity.Info, "基础验证通过。"));
            return 消息;
        }

        private static bool IsPowerOfTwoPlusOne(int 值)
        {
            int 减一 = 值 - 1;
            return 减一 > 0 && (减一 & (减一 - 1)) == 0;
        }
    }
}
