using UnityEngine;

namespace Arkoura.WorldGen
{
    public static class BiomeResolver
    {
        public static string Resolve(WorldSpec 规格, float 世界X, float 世界Z)
        {
            if (规格 == null || 规格.Regions.Count == 0) return string.Empty;
            float nx = Mathf.Clamp01(世界X / Mathf.Max(1f, 规格.WorldSize));
            float nz = Mathf.Clamp01(世界Z / Mathf.Max(1f, 规格.WorldSize));
            string 最佳生物群落 = string.Empty;
            float 最佳分数 = float.NegativeInfinity;

            for (int i = 0; i < 规格.Regions.Count; i++)
            {
                var 区域 = 规格.Regions[i];
                if (区域 == null || string.IsNullOrWhiteSpace(区域.生物群落)) continue;
                float dx = nx - 区域.中心X;
                float dz = nz - 区域.中心Z;
                float 距离 = Mathf.Sqrt(dx * dx + dz * dz);
                float 归一距离 = 距离 / Mathf.Max(0.001f, 区域.半径);
                float 分数 = (1f - 归一距离) * 区域.强度;
                if (分数 <= 最佳分数) continue;
                最佳分数 = 分数;
                最佳生物群落 = 区域.生物群落;
            }
            return 最佳生物群落;
        }
    }
}
