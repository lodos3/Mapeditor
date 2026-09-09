using System.Collections.Generic;
using UnityEngine;

namespace Arkoura.WorldGen
{
    /// <summary>
    /// 基于语义场的道路求解器。道路先生成 ArkouraSpline，再由 Unity Splines、EasyRoads、GeNa 等后端编译。
    /// </summary>
    public static class RoadNetworkGenerator
    {
        private static readonly Vector2Int[] 方向 =
        {
            new Vector2Int(1,0), new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,-1),
            new Vector2Int(1,1), new Vector2Int(1,-1), new Vector2Int(-1,1), new Vector2Int(-1,-1)
        };

        public static ArkouraSpline Solve(WorldSemanticFields 场, Vector2 起点01, Vector2 终点01, Vector3 世界原点, float 区域尺寸, float 最大地形高度, RoadProfile 配置)
        {
            var 路径 = new ArkouraSpline
            {
                标识 = $"road_{起点01.x:F3}_{起点01.y:F3}_{终点01.x:F3}_{终点01.y:F3}",
                类型 = PathType.PrimaryRoad,
                默认宽度 = 配置 != null ? 配置.Width : 6f,
                最大坡度 = 配置 != null ? 配置.MaxGrade : 0.12f,
                最小转弯半径 = 配置 != null ? 配置.MinimumRadius : 10f,
                地形影响半径 = 配置 != null ? 配置.TerrainInfluence : 10f,
                允许桥梁 = 配置 == null || 配置.AllowBridges,
                允许隧道 = 配置 == null || 配置.AllowTunnels
            };
            if (场 == null) return 路径;

            int 分辨率 = 场.Resolution;
            Vector2Int 起点 = ToCell(起点01, 分辨率);
            Vector2Int 终点 = ToCell(终点01, 分辨率);
            var 开放 = new List<Vector2Int> { 起点 };
            var 关闭 = new bool[分辨率, 分辨率];
            var g = new float[分辨率, 分辨率];
            var 父 = new Vector2Int[分辨率, 分辨率];
            for (int z = 0; z < 分辨率; z++) for (int x = 0; x < 分辨率; x++) g[z, x] = float.PositiveInfinity;
            g[起点.y, 起点.x] = 0f;

            while (开放.Count > 0)
            {
                int 最佳索引 = 0;
                float 最佳F = float.PositiveInfinity;
                for (int i = 0; i < 开放.Count; i++)
                {
                    Vector2Int 候选 = 开放[i];
                    float f = g[候选.y, 候选.x] + Vector2Int.Distance(候选, 终点);
                    if (f >= 最佳F) continue;
                    最佳F = f;
                    最佳索引 = i;
                }
                Vector2Int 当前 = 开放[最佳索引];
                开放.RemoveAt(最佳索引);
                if (关闭[当前.y, 当前.x]) continue;
                关闭[当前.y, 当前.x] = true;
                if (当前 == 终点) break;

                for (int i = 0; i < 方向.Length; i++)
                {
                    Vector2Int 下一个 = 当前 + 方向[i];
                    if (下一个.x < 0 || 下一个.y < 0 || 下一个.x >= 分辨率 || 下一个.y >= 分辨率 || 关闭[下一个.y, 下一个.x]) continue;
                    float 对角 = 方向[i].x != 0 && 方向[i].y != 0 ? 1.4142f : 1f;
                    float 坡度 = 场.Slope01[下一个.y, 下一个.x];
                    float 水体 = 场.WaterMask[下一个.y, 下一个.x];
                    float 悬崖 = 场.CliffMask[下一个.y, 下一个.x];
                    float 代价 = 对角 * (1f + 坡度 * 坡度 * 36f + 悬崖 * 24f + 水体 * (路径.允许桥梁 ? 7f : 1000f));
                    float 新G = g[当前.y, 当前.x] + 代价;
                    if (新G >= g[下一个.y, 下一个.x]) continue;
                    g[下一个.y, 下一个.x] = 新G;
                    父[下一个.y, 下一个.x] = 当前;
                    开放.Add(下一个);
                }
            }

            if (!关闭[终点.y, 终点.x]) return 路径;
            var 反向 = new List<Vector2Int>();
            Vector2Int 游标 = 终点;
            int 安全 = 分辨率 * 分辨率;
            while (安全-- > 0)
            {
                反向.Add(游标);
                if (游标 == 起点) break;
                游标 = 父[游标.y, 游标.x];
            }
            反向.Reverse();

            int 抽样步长 = Mathf.Max(1, 反向.Count / 64);
            for (int i = 0; i < 反向.Count; i += 抽样步长)
            {
                AddPoint(路径, 场, 反向[i], 世界原点, 区域尺寸, 最大地形高度);
            }
            if (反向.Count > 0)
            {
                Vector2Int 最后 = 反向[反向.Count - 1];
                if (路径.控制点.Count == 0 || Vector3.SqrMagnitude(路径.控制点[路径.控制点.Count - 1] - CellToWorld(场, 最后, 世界原点, 区域尺寸, 最大地形高度)) > 0.001f)
                    AddPoint(路径, 场, 最后, 世界原点, 区域尺寸, 最大地形高度);
            }
            return 路径;
        }

        private static void AddPoint(ArkouraSpline 路径, WorldSemanticFields 场, Vector2Int 单元, Vector3 世界原点, float 区域尺寸, float 最大地形高度)
        {
            路径.控制点.Add(CellToWorld(场, 单元, 世界原点, 区域尺寸, 最大地形高度));
            路径.宽度.Add(路径.默认宽度);
            场.RoadMask[单元.y, 单元.x] = 1f;
        }

        private static Vector3 CellToWorld(WorldSemanticFields 场, Vector2Int 单元, Vector3 世界原点, float 区域尺寸, float 最大地形高度)
        {
            float nx = 单元.x / (场.Resolution - 1f);
            float nz = 单元.y / (场.Resolution - 1f);
            float y = 场.Height01[单元.y, 单元.x] * 最大地形高度;
            return new Vector3(世界原点.x + nx * 区域尺寸, 世界原点.y + y, 世界原点.z + nz * 区域尺寸);
        }

        private static Vector2Int ToCell(Vector2 值, int 分辨率)
        {
            return new Vector2Int(Mathf.Clamp(Mathf.RoundToInt(值.x * (分辨率 - 1)), 0, 分辨率 - 1), Mathf.Clamp(Mathf.RoundToInt(值.y * (分辨率 - 1)), 0, 分辨率 - 1));
        }
    }
}
