using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arkoura.WorldGen
{
    public enum LightingZoneType { Outdoor, Settlement, Interior, Cave, Coast, Forest }

    [Serializable]
    public sealed class LightingZone
    {
        public string 标识;
        public LightingZoneType 类型;
        public Bounds 范围;
        public float 探针间距;
        public bool 需要反射探针;
        public bool 需要独立Volume;
        public int 优先级;
    }

    [Serializable]
    public sealed class LightingPlan
    {
        public List<LightingZone> 区域 = new List<LightingZone>();
        public List<Vector3> 反射探针候选 = new List<Vector3>();
        public List<Vector3> 传统光照探针候选 = new List<Vector3>();
    }

    /// <summary>
    /// 只生成照明意图。APV、Bakery、Magic Light Probes 等具体后端由编辑器适配器处理。
    /// </summary>
    public static class LightingPlanner
    {
        public static LightingPlan Build(WorldSpec 规格, IEnumerable<WorldCellManifest> 单元)
        {
            var 计划 = new LightingPlan();
            if (规格 == null || 单元 == null) return 计划;
            LightingProfile 配置 = 规格.LightingProfile;
            float 基础间距 = 配置 != null ? 配置.BaseProbeSpacing : 16f;
            float 反射间距 = 配置 != null ? 配置.ReflectionProbeSpacing : 96f;

            foreach (WorldCellManifest 单元项 in 单元)
            {
                if (单元项 == null) continue;
                LightingZoneType 类型 = 单元项.含洞穴 ? LightingZoneType.Cave : 单元项.含聚落 ? LightingZoneType.Settlement : 单元项.含水体 ? LightingZoneType.Coast : LightingZoneType.Outdoor;
                float 间距 = 类型 == LightingZoneType.Cave ? Mathf.Max(4f, 基础间距 * 0.5f) : 类型 == LightingZoneType.Settlement ? Mathf.Max(6f, 基础间距 * 0.65f) : 基础间距;
                计划.区域.Add(new LightingZone
                {
                    标识 = $"lighting_{单元项.坐标.x}_{单元项.坐标.y}",
                    类型 = 类型,
                    范围 = 单元项.世界边界,
                    探针间距 = 间距,
                    需要反射探针 = 单元项.含水体 || 单元项.含聚落 || 单元项.含洞穴,
                    需要独立Volume = 类型 != LightingZoneType.Outdoor,
                    优先级 = 类型 == LightingZoneType.Cave ? 100 : 类型 == LightingZoneType.Settlement ? 50 : 10
                });

                if (配置 != null && 配置.UseAPV) continue;
                Bounds 范围 = 单元项.世界边界;
                for (float z = 范围.min.z + 间距 * 0.5f; z < 范围.max.z; z += 间距)
                for (float x = 范围.min.x + 间距 * 0.5f; x < 范围.max.x; x += 间距)
                {
                    计划.传统光照探针候选.Add(new Vector3(x, 范围.center.y, z));
                }
                if (单元项.含水体 || 单元项.含聚落)
                {
                    int 每轴 = Mathf.Max(1, Mathf.CeilToInt(范围.size.x / 反射间距));
                    for (int z = 0; z < 每轴; z++)
                    for (int x = 0; x < 每轴; x++)
                    {
                        计划.反射探针候选.Add(new Vector3(Mathf.Lerp(范围.min.x, 范围.max.x, (x + 0.5f) / 每轴), 范围.center.y, Mathf.Lerp(范围.min.z, 范围.max.z, (z + 0.5f) / 每轴)));
                    }
                }
            }
            return 计划;
        }
    }
}
