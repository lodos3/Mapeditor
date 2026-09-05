using UnityEngine;

namespace Arkoura.WorldGen
{
    public static class HeightFieldGenerator
    {
        public static float[,] Generate(WorldSpec 规格, int 分块X, int 分块Z)
        {
            int 分辨率 = 规格.HeightResolution;
            var 高度 = new float[分辨率, 分辨率];
            float 步长 = 规格.ChunkSize / (分辨率 - 1f);
            float 种子偏移X = Hash01(规格.Seed * 17 + 11) * 100000f;
            float 种子偏移Z = Hash01(规格.Seed * 31 + 29) * 100000f;

            for (int z = 0; z < 分辨率; z++)
            {
                for (int x = 0; x < 分辨率; x++)
                {
                    float 世界X = 分块X * 规格.ChunkSize + x * 步长;
                    float 世界Z = 分块Z * 规格.ChunkSize + z * 步长;
                    float 宏观 = Fractal(世界X, 世界Z, 种子偏移X, 种子偏移Z, 规格.MacroFrequency, 4);
                    float 中观 = Fractal(世界X, 世界Z, 种子偏移X + 173f, 种子偏移Z - 91f, 规格.MesoFrequency, 3);
                    float 微观 = Fractal(世界X, 世界Z, 种子偏移X - 47f, 种子偏移Z + 223f, 规格.MicroFrequency, 2);
                    float 山脊源 = Fractal(世界X, 世界Z, 种子偏移X + 401f, 种子偏移Z + 761f, 规格.MesoFrequency * 0.55f, 3);
                    float 山脊 = 1f - Mathf.Abs(山脊源 * 2f - 1f);
                    山脊 *= 山脊;

                    float 合成 = 宏观 * 规格.MacroWeight + 中观 * 规格.MesoWeight + 微观 * 规格.MicroWeight;
                    合成 = Mathf.Lerp(合成, Mathf.Max(合成, 山脊), 规格.RidgeStrength);
                    高度[z, x] = Mathf.Clamp01(Smooth01(合成));
                }
            }
            return 高度;
        }

        private static float Fractal(float 世界X, float 世界Z, float 偏移X, float 偏移Z, float 基础频率, int 层数)
        {
            float 总和 = 0f;
            float 总权重 = 0f;
            float 振幅 = 1f;
            float 频率 = 基础频率;
            for (int 层 = 0; 层 < 层数; 层++)
            {
                总和 += Mathf.PerlinNoise((世界X + 偏移X) * 频率, (世界Z + 偏移Z) * 频率) * 振幅;
                总权重 += 振幅;
                振幅 *= 0.5f;
                频率 *= 2f;
            }
            return 总权重 <= 0f ? 0f : 总和 / 总权重;
        }

        private static float Smooth01(float 值) => 值 * 值 * (3f - 2f * 值);

        private static float Hash01(int 值)
        {
            unchecked
            {
                uint 哈希 = (uint)值;
                哈希 ^= 哈希 >> 16;
                哈希 *= 0x7feb352d;
                哈希 ^= 哈希 >> 15;
                哈希 *= 0x846ca68b;
                哈希 ^= 哈希 >> 16;
                return (哈希 & 0x00ffffff) / 16777215f;
            }
        }
    }
}
