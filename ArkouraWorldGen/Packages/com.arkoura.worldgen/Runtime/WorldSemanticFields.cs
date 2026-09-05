using System;
using UnityEngine;

namespace Arkoura.WorldGen
{
    /// <summary>
    /// 世界生成阶段共享的语义场。所有二维数组均使用 [z, x]，并使用统一分辨率。
    /// 这些数据是地形、材质、水体、道路和资产放置之间的稳定中间层。
    /// </summary>
    [Serializable]
    public sealed class WorldSemanticFields
    {
        public int Resolution { get; }
        public float[,] Height01 { get; }
        public float[,] Slope01 { get; }
        public float[,] Curvature { get; }
        public float[,] Moisture01 { get; }
        public float[,] Temperature01 { get; }
        public float[,] Flow01 { get; }
        public float[,] WaterMask { get; }
        public float[,] ShoreMask { get; }
        public float[,] CliffMask { get; }
        public float[,] RoadMask { get; }
        public float[,] SettlementMask { get; }

        public WorldSemanticFields(int 分辨率)
        {
            Resolution = Mathf.Max(3, 分辨率);
            Height01 = new float[Resolution, Resolution];
            Slope01 = new float[Resolution, Resolution];
            Curvature = new float[Resolution, Resolution];
            Moisture01 = new float[Resolution, Resolution];
            Temperature01 = new float[Resolution, Resolution];
            Flow01 = new float[Resolution, Resolution];
            WaterMask = new float[Resolution, Resolution];
            ShoreMask = new float[Resolution, Resolution];
            CliffMask = new float[Resolution, Resolution];
            RoadMask = new float[Resolution, Resolution];
            SettlementMask = new float[Resolution, Resolution];
        }

        public static WorldSemanticFields Build(WorldSpec 规格, int 分块X, int 分块Z, int 分辨率)
        {
            var 场 = new WorldSemanticFields(分辨率);
            float 步长 = 规格.ChunkSize / (场.Resolution - 1f);

            for (int z = 0; z < 场.Resolution; z++)
            {
                for (int x = 0; x < 场.Resolution; x++)
                {
                    float 世界X = 分块X * 规格.ChunkSize + x * 步长;
                    float 世界Z = 分块Z * 规格.ChunkSize + z * 步长;
                    场.Height01[z, x] = HeightFieldGenerator.Sample(规格, 世界X, 世界Z);
                }
            }

            for (int z = 0; z < 场.Resolution; z++)
            {
                int 下 = Mathf.Max(0, z - 1);
                int 上 = Mathf.Min(场.Resolution - 1, z + 1);
                for (int x = 0; x < 场.Resolution; x++)
                {
                    int 左 = Mathf.Max(0, x - 1);
                    int 右 = Mathf.Min(场.Resolution - 1, x + 1);
                    float 世界X = 分块X * 规格.ChunkSize + x * 步长;
                    float 世界Z = 分块Z * 规格.ChunkSize + z * 步长;
                    float 中 = 场.Height01[z, x];
                    float 差X = (场.Height01[z, 右] - 场.Height01[z, 左]) * 规格.MaxTerrainHeight / Mathf.Max(步长 * 2f, 0.001f);
                    float 差Z = (场.Height01[上, x] - 场.Height01[下, x]) * 规格.MaxTerrainHeight / Mathf.Max(步长 * 2f, 0.001f);
                    场.Slope01[z, x] = Mathf.Clamp01(Mathf.Atan(Mathf.Sqrt(差X * 差X + 差Z * 差Z)) / (Mathf.PI * 0.5f));
                    float 拉普拉斯 = 场.Height01[z, 左] + 场.Height01[z, 右] + 场.Height01[下, x] + 场.Height01[上, x] - 4f * 中;
                    场.Curvature[z, x] = Mathf.Clamp(拉普拉斯 * 64f, -1f, 1f);

                    float 纬度 = Mathf.Abs((世界Z / Mathf.Max(规格.WorldSize, 1f)) * 2f - 1f);
                    float 温度噪声 = Mathf.PerlinNoise((世界X + 规格.Seed * 7f) * 0.0007f, (世界Z - 规格.Seed * 3f) * 0.0007f);
                    场.Temperature01[z, x] = Mathf.Clamp01(0.92f - 纬度 * 0.35f - 中 * 0.55f + (温度噪声 - 0.5f) * 0.15f);
                    float 湿度噪声 = Mathf.PerlinNoise((世界X - 912f) * 0.0009f, (世界Z + 331f) * 0.0009f);
                    float 海岸湿度 = 1f - Mathf.Clamp01(Mathf.Abs(中 - 规格.SeaLevel01) / 0.18f);
                    场.Moisture01[z, x] = Mathf.Clamp01(湿度噪声 * 0.65f + 海岸湿度 * 0.35f);
                    场.CliffMask[z, x] = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.42f, 0.72f, 场.Slope01[z, x]));
                }
            }

            return 场;
        }
    }
}
