using UnityEngine;

namespace Arkoura.WorldGen
{
    public static class SplatMapGenerator
    {
        public static void Apply(TerrainData 地形数据, WorldSpec 规格)
        {
            int 层数 = 规格.TerrainRules.Count;
            if (层数 == 0) return;

            var 地形层 = new TerrainLayer[层数];
            for (int i = 0; i < 层数; i++) 地形层[i] = 规格.TerrainRules[i].地形层;
            地形数据.terrainLayers = 地形层;
            int 分辨率 = 规格.AlphaResolution;
            地形数据.alphamapResolution = 分辨率;
            var 权重图 = new float[分辨率, 分辨率, 层数];

            for (int z = 0; z < 分辨率; z++)
            {
                float 归一Z = z / (分辨率 - 1f);
                for (int x = 0; x < 分辨率; x++)
                {
                    float 归一X = x / (分辨率 - 1f);
                    float 高度01 = 地形数据.GetInterpolatedHeight(归一X, 归一Z) / Mathf.Max(0.001f, 地形数据.size.y);
                    float 坡度 = 地形数据.GetSteepness(归一X, 归一Z);
                    float 总权重 = 0f;

                    for (int 层 = 0; 层 < 层数; 层++)
                    {
                        var 规则 = 规格.TerrainRules[层];
                        if (规则.地形层 == null) continue;
                        float 高度权重 = Band(高度01, 规则.最低归一高度, 规则.最高归一高度, 0.05f);
                        float 坡度权重 = Band(坡度, 规则.最低坡度, 规则.最高坡度, 6f);
                        float 权重 = Mathf.Pow(Mathf.Clamp01(高度权重 * 坡度权重), 规则.权重锐度);
                        权重图[z, x, 层] = 权重;
                        总权重 += 权重;
                    }

                    if (总权重 <= 0.0001f)
                    {
                        权重图[z, x, 0] = 1f;
                        总权重 = 1f;
                    }

                    for (int 层 = 0; 层 < 层数; 层++) 权重图[z, x, 层] /= 总权重;
                }
            }
            地形数据.SetAlphamaps(0, 0, 权重图);
        }

        private static float Band(float 值, float 最小, float 最大, float 羽化)
        {
            float 进入 = Mathf.InverseLerp(最小 - 羽化, 最小 + 羽化, 值);
            float 离开 = 1f - Mathf.InverseLerp(最大 - 羽化, 最大 + 羽化, 值);
            return Mathf.Clamp01(Mathf.Min(进入, 离开));
        }
    }
}
