using System.Collections.Generic;
using UnityEngine;

namespace Arkoura.WorldGen
{
    public static class RoadPathSolver
    {
        private struct 节点
        {
            public int x;
            public int z;
            public float g;
            public float f;
            public int 父索引;
        }

        public static List<Vector3> Solve(Terrain 地形, Vector3 起点, Vector3 终点, int 网格 = 96)
        {
            var 结果 = new List<Vector3>();
            if (地形 == null || 网格 < 8) return 结果;
            var 数据 = 地形.terrainData;
            Vector3 原点 = 地形.transform.position;
            int 起X = Mathf.Clamp(Mathf.RoundToInt((起点.x - 原点.x) / 数据.size.x * (网格 - 1)), 0, 网格 - 1);
            int 起Z = Mathf.Clamp(Mathf.RoundToInt((起点.z - 原点.z) / 数据.size.z * (网格 - 1)), 0, 网格 - 1);
            int 终X = Mathf.Clamp(Mathf.RoundToInt((终点.x - 原点.x) / 数据.size.x * (网格 - 1)), 0, 网格 - 1);
            int 终Z = Mathf.Clamp(Mathf.RoundToInt((终点.z - 原点.z) / 数据.size.z * (网格 - 1)), 0, 网格 - 1);

            var 开放 = new List<节点>();
            var 已关闭 = new bool[网格, 网格];
            var 最佳G = new float[网格, 网格];
            var 父 = new Vector2Int[网格, 网格];
            for (int z = 0; z < 网格; z++) for (int x = 0; x < 网格; x++) 最佳G[x, z] = float.PositiveInfinity;

            开放.Add(new 节点 { x = 起X, z = 起Z, g = 0f, f = Heuristic(起X, 起Z, 终X, 终Z), 父索引 = -1 });
            最佳G[起X, 起Z] = 0f;
            var 方向 = new[] { new Vector2Int(1,0), new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,-1), new Vector2Int(1,1), new Vector2Int(1,-1), new Vector2Int(-1,1), new Vector2Int(-1,-1) };

            while (开放.Count > 0)
            {
                int 最佳索引 = 0;
                for (int i = 1; i < 开放.Count; i++) if (开放[i].f < 开放[最佳索引].f) 最佳索引 = i;
                var 当前 = 开放[最佳索引];
                开放.RemoveAt(最佳索引);
                if (已关闭[当前.x, 当前.z]) continue;
                已关闭[当前.x, 当前.z] = true;
                if (当前.x == 终X && 当前.z == 终Z) break;

                for (int d = 0; d < 方向.Length; d++)
                {
                    int nx = 当前.x + 方向[d].x;
                    int nz = 当前.z + 方向[d].y;
                    if (nx < 0 || nz < 0 || nx >= 网格 || nz >= 网格 || 已关闭[nx, nz]) continue;
                    float ux = nx / (网格 - 1f);
                    float uz = nz / (网格 - 1f);
                    float 坡度 = 数据.GetSteepness(ux, uz);
                    float 距离 = 方向[d].x != 0 && 方向[d].y != 0 ? 1.4142f : 1f;
                    float 代价 = 距离 * (1f + Mathf.Pow(坡度 / 18f, 2f) * 8f);
                    float 新G = 当前.g + 代价;
                    if (新G >= 最佳G[nx, nz]) continue;
                    最佳G[nx, nz] = 新G;
                    父[nx, nz] = new Vector2Int(当前.x, 当前.z);
                    开放.Add(new 节点 { x = nx, z = nz, g = 新G, f = 新G + Heuristic(nx, nz, 终X, 终Z), 父索引 = -1 });
                }
            }

            if (!已关闭[终X, 终Z]) return 结果;
            var 游标 = new Vector2Int(终X, 终Z);
            int 安全计数 = 网格 * 网格;
            while (安全计数-- > 0)
            {
                float ux = 游标.x / (网格 - 1f);
                float uz = 游标.y / (网格 - 1f);
                float wx = 原点.x + ux * 数据.size.x;
                float wz = 原点.z + uz * 数据.size.z;
                float wy = 原点.y + 数据.GetInterpolatedHeight(ux, uz);
                结果.Add(new Vector3(wx, wy, wz));
                if (游标.x == 起X && 游标.y == 起Z) break;
                游标 = 父[游标.x, 游标.y];
            }
            结果.Reverse();
            return 结果;
        }

        private static float Heuristic(int x, int z, int tx, int tz)
        {
            float dx = tx - x;
            float dz = tz - z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
