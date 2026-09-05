using System.Collections.Generic;
using UnityEngine;

namespace Arkoura.WorldGen
{
    public static class AssetScatterGenerator
    {
        private static readonly AssetCategory[] 可散布分类 =
        {
            AssetCategory.Cliff, AssetCategory.Rock, AssetCategory.Tree, AssetCategory.Shrub,
            AssetCategory.Grass, AssetCategory.Ruin, AssetCategory.Decoration, AssetCategory.WaterProp
        };

        public static int Generate(Terrain 地形, WorldSpec 规格, AssetLibrary 资产库, Vector2Int 分块坐标, Transform 父节点)
        {
            if (地形 == null || 规格 == null || 资产库 == null || 规格.MaxScatterPerChunk <= 0) return 0;
            var 候选资产 = new List<AssetMetadata>();
            foreach (var 资产 in 资产库.Assets)
            {
                if (资产 == null || 资产.Prefab == null) continue;
                for (int c = 0; c < 可散布分类.Length; c++)
                {
                    if (资产.Category != 可散布分类[c]) continue;
                    候选资产.Add(资产);
                    break;
                }
            }
            if (候选资产.Count == 0) return 0;

            int 目标数量 = 规格.MaxScatterPerChunk;
            int 网格边长 = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(目标数量 * 1.6f)));
            float 单元尺寸 = 规格.ChunkSize / 网格边长;
            int 已生成 = 0;
            Vector3 地形原点 = 地形.transform.position;
            var 随机 = new System.Random(Hash(规格.Seed, 分块坐标.x, 分块坐标.y));

            for (int gz = 0; gz < 网格边长 && 已生成 < 目标数量; gz++)
            {
                for (int gx = 0; gx < 网格边长 && 已生成 < 目标数量; gx++)
                {
                    // 抖动网格是一种低成本蓝噪声近似，避免普通随机散点产生明显团块与空洞。
                    if (随机.NextDouble() > 0.68) continue;
                    float 局部X = (gx + 0.15f + (float)随机.NextDouble() * 0.7f) * 单元尺寸;
                    float 局部Z = (gz + 0.15f + (float)随机.NextDouble() * 0.7f) * 单元尺寸;
                    float nx = Mathf.Clamp01(局部X / 规格.ChunkSize);
                    float nz = Mathf.Clamp01(局部Z / 规格.ChunkSize);
                    float 世界X = 地形原点.x + 局部X;
                    float 世界Z = 地形原点.z + 局部Z;
                    float 高度 = 地形原点.y + 地形.terrainData.GetInterpolatedHeight(nx, nz);
                    float 坡度 = 地形.terrainData.GetSteepness(nx, nz);
                    string 生物群落 = BiomeResolver.Resolve(规格, 世界X, 世界Z);

                    int 起始 = 随机.Next(候选资产.Count);
                    AssetMetadata 选中 = null;
                    for (int 尝试 = 0; 尝试 < 候选资产.Count; 尝试++)
                    {
                        var 资产 = 候选资产[(起始 + 尝试) % 候选资产.Count];
                        if (!资产.IsAllowed(高度, 坡度, 生物群落)) continue;
                        float 平方公里面积 = (规格.ChunkSize * 规格.ChunkSize) / 1000000f;
                        float 密度允许率 = Mathf.Clamp01((资产.MaxInstancesPerKm2 * 平方公里面积) / Mathf.Max(1f, 目标数量));
                        float 稀有允许率 = Mathf.Lerp(0.02f, 1f, 1f - 资产.Rarity);
                        if (随机.NextDouble() > 密度允许率 * 稀有允许率) continue;
                        选中 = 资产;
                        break;
                    }
                    if (选中 == null) continue;

                    Vector3 法线 = 地形.terrainData.GetInterpolatedNormal(nx, nz);
                    Quaternion 旋转 = ResolveRotation(选中, 法线, 随机);
                    var 实例 = Object.Instantiate(选中.Prefab, new Vector3(世界X, 高度, 世界Z), 旋转, 父节点);
                    float 缩放 = Mathf.Lerp(选中.ScaleRange.x, 选中.ScaleRange.y, (float)随机.NextDouble());
                    实例.transform.localScale *= 缩放;
                    if (选中.RandomMirror && 随机.NextDouble() > 0.5)
                    {
                        var 当前缩放 = 实例.transform.localScale;
                        当前缩放.x *= -1f;
                        实例.transform.localScale = 当前缩放;
                    }
                    已生成++;
                }
            }
            return 已生成;
        }

        private static Quaternion ResolveRotation(AssetMetadata 资产, Vector3 法线, System.Random 随机)
        {
            switch (资产.Rotation)
            {
                case RotationRule.AlignSlope:
                    return Quaternion.FromToRotation(Vector3.up, 法线) * Quaternion.Euler(0f, (float)随机.NextDouble() * 360f, 0f);
                case RotationRule.Fixed:
                    return Quaternion.Euler(资产.FixedEuler);
                default:
                    return Quaternion.Euler(0f, (float)随机.NextDouble() * 360f, 0f);
            }
        }

        private static int Hash(int 种子, int x, int z)
        {
            unchecked
            {
                int 哈希 = 种子;
                哈希 = 哈希 * 397 ^ x;
                哈希 = 哈希 * 397 ^ z;
                return 哈希;
            }
        }
    }
}
