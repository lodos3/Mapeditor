using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arkoura.WorldGen
{
    [Serializable]
    public sealed class HydrologySettings
    {
        [Range(0f, 1f)] public float 河流阈值 = 0.72f;
        [Range(0f, 1f)] public float 岸线宽度 = 0.08f;
        [Range(0, 128)] public int 洼地修复迭代 = 24;
        [Range(0f, 0.02f)] public float 最小排水落差 = 0.00005f;
    }

    /// <summary>
    /// 确定性 D8 水文求解器。它不负责水面渲染，只生成流向、汇流、水体与岸线语义。
    /// </summary>
    public static class HydrologyGenerator
    {
        private static readonly Vector2Int[] 邻居 =
        {
            new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
            new Vector2Int(-1, 0),                              new Vector2Int(1, 0),
            new Vector2Int(-1, 1),  new Vector2Int(0, 1),  new Vector2Int(1, 1)
        };

        public static void Solve(WorldSemanticFields 场, WorldSpec 规格, HydrologySettings 设置)
        {
            if (场 == null || 规格 == null) return;
            设置 ??= new HydrologySettings();
            int 分辨率 = 场.Resolution;
            var 排水高度 = (float[,])场.Height01.Clone();
            RepairSinks(排水高度, 设置);

            var 下游X = new int[分辨率, 分辨率];
            var 下游Z = new int[分辨率, 分辨率];
            var 汇流 = new float[分辨率, 分辨率];
            var 顺序 = new List<Cell>(分辨率 * 分辨率);

            for (int z = 0; z < 分辨率; z++)
            for (int x = 0; x < 分辨率; x++)
            {
                汇流[z, x] = 1f;
                顺序.Add(new Cell(x, z, 排水高度[z, x]));
                FindDownstream(排水高度, x, z, out 下游X[z, x], out 下游Z[z, x]);
            }

            顺序.Sort((甲, 乙) => 乙.高度.CompareTo(甲.高度));
            foreach (var 单元 in 顺序)
            {
                int dx = 下游X[单元.z, 单元.x];
                int dz = 下游Z[单元.z, 单元.x];
                if (dx == 单元.x && dz == 单元.z) continue;
                汇流[dz, dx] += 汇流[单元.z, 单元.x];
            }

            float 最大汇流 = 1f;
            for (int z = 0; z < 分辨率; z++)
            for (int x = 0; x < 分辨率; x++) 最大汇流 = Mathf.Max(最大汇流, 汇流[z, x]);
            float 对数最大值 = Mathf.Log(1f + 最大汇流);

            for (int z = 0; z < 分辨率; z++)
            for (int x = 0; x < 分辨率; x++)
            {
                float 流量01 = Mathf.Clamp01(Mathf.Log(1f + 汇流[z, x]) / Mathf.Max(0.0001f, 对数最大值));
                场.Flow01[z, x] = 流量01;
                bool 海洋 = 场.Height01[z, x] <= 规格.SeaLevel01;
                bool 河流 = !海洋 && 流量01 >= 设置.河流阈值 && 场.Slope01[z, x] > 0.002f;
                场.WaterMask[z, x] = 海洋 || 河流 ? 1f : 0f;
                场.Moisture01[z, x] = Mathf.Clamp01(场.Moisture01[z, x] + 流量01 * 0.35f);
            }

            BuildShoreMask(场, 设置);
        }

        private static void RepairSinks(float[,] 高度, HydrologySettings 设置)
        {
            int 分辨率 = 高度.GetLength(0);
            for (int 迭代 = 0; 迭代 < 设置.洼地修复迭代; 迭代++)
            {
                bool 有变化 = false;
                for (int z = 1; z < 分辨率 - 1; z++)
                for (int x = 1; x < 分辨率 - 1; x++)
                {
                    float 最低邻居 = float.MaxValue;
                    for (int i = 0; i < 邻居.Length; i++)
                    {
                        int nx = x + 邻居[i].x;
                        int nz = z + 邻居[i].y;
                        最低邻居 = Mathf.Min(最低邻居, 高度[nz, nx]);
                    }
                    if (高度[z, x] <= 最低邻居)
                    {
                        高度[z, x] = 最低邻居 + 设置.最小排水落差;
                        有变化 = true;
                    }
                }
                if (!有变化) break;
            }
        }

        private static void FindDownstream(float[,] 高度, int x, int z, out int 下游X, out int 下游Z)
        {
            int 分辨率 = 高度.GetLength(0);
            下游X = x;
            下游Z = z;
            float 最低 = 高度[z, x];
            for (int i = 0; i < 邻居.Length; i++)
            {
                int nx = x + 邻居[i].x;
                int nz = z + 邻居[i].y;
                if (nx < 0 || nz < 0 || nx >= 分辨率 || nz >= 分辨率) continue;
                float 候选 = 高度[nz, nx];
                if (候选 >= 最低) continue;
                最低 = 候选;
                下游X = nx;
                下游Z = nz;
            }
        }

        private static void BuildShoreMask(WorldSemanticFields 场, HydrologySettings 设置)
        {
            int 分辨率 = 场.Resolution;
            int 半径 = Mathf.Max(1, Mathf.RoundToInt(设置.岸线宽度 * 分辨率));
            for (int z = 0; z < 分辨率; z++)
            for (int x = 0; x < 分辨率; x++)
            {
                if (场.WaterMask[z, x] > 0.5f) continue;
                float 最近 = 半径 + 1f;
                for (int dz = -半径; dz <= 半径; dz++)
                for (int dx = -半径; dx <= 半径; dx++)
                {
                    int nx = x + dx;
                    int nz = z + dz;
                    if (nx < 0 || nz < 0 || nx >= 分辨率 || nz >= 分辨率) continue;
                    if (场.WaterMask[nz, nx] < 0.5f) continue;
                    最近 = Mathf.Min(最近, Mathf.Sqrt(dx * dx + dz * dz));
                }
                场.ShoreMask[z, x] = Mathf.Clamp01(1f - 最近 / Mathf.Max(1f, 半径));
            }
        }

        private readonly struct Cell
        {
            public readonly int x;
            public readonly int z;
            public readonly float 高度;
            public Cell(int 单元X, int 单元Z, float 值) { x = 单元X; z = 单元Z; 高度 = 值; }
        }
    }
}
