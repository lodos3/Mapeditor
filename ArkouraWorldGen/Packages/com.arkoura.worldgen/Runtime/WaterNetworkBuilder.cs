using System.Collections.Generic;
using UnityEngine;

namespace Arkoura.WorldGen
{
    /// <summary>
    /// 将水文语义编译成渲染后端无关的 WaterBody。海洋、河流和湖泊可分别交给 Crest、R.A.M、HDRP Water 或自定义后端。
    /// </summary>
    public static class WaterNetworkBuilder
    {
        public static List<WaterBody> Build(WorldSemanticFields 场, WorldSpec 规格, Vector3 世界原点)
        {
            var 结果 = new List<WaterBody>();
            if (场 == null || 规格 == null) return 结果;
            int 分辨率 = 场.Resolution;
            float 步长 = 规格.ChunkSize / (分辨率 - 1f);

            var 海洋 = new WaterBody
            {
                标识 = $"ocean_{世界原点.x:F0}_{世界原点.z:F0}",
                类型 = WaterBodyType.Ocean,
                水面高度 = 规格.SeaLevel01 * 规格.MaxTerrainHeight,
                平均深度 = 规格.WaterProfile != null ? 规格.WaterProfile.DefaultDepth * 8f : 16f,
                流速 = 0f,
                允许船只 = true
            };

            bool 有海洋 = false;
            for (int z = 1; z < 分辨率 - 1; z++)
            for (int x = 1; x < 分辨率 - 1; x++)
            {
                if (场.WaterMask[z, x] < 0.5f || 场.Height01[z, x] > 规格.SeaLevel01 + 0.001f) continue;
                有海洋 = true;
                bool 岸边 = false;
                for (int dz = -1; dz <= 1 && !岸边; dz++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (场.Height01[z + dz, x + dx] > 规格.SeaLevel01) { 岸边 = true; break; }
                }
                if (岸边 && 海洋.岸线.Count < 2048)
                    海洋.岸线.Add(new Vector3(世界原点.x + x * 步长, 海洋.水面高度, 世界原点.z + z * 步长));
            }
            if (有海洋) 结果.Add(海洋);

            var 已访问 = new bool[分辨率, 分辨率];
            for (int z = 1; z < 分辨率 - 1; z++)
            for (int x = 1; x < 分辨率 - 1; x++)
            {
                if (已访问[z, x] || 场.WaterMask[z, x] < 0.5f || 场.Height01[z, x] <= 规格.SeaLevel01) continue;
                var 队列 = new Queue<Vector2Int>();
                var 河段 = new List<Vector2Int>();
                队列.Enqueue(new Vector2Int(x, z));
                已访问[z, x] = true;
                while (队列.Count > 0)
                {
                    Vector2Int 当前 = 队列.Dequeue();
                    河段.Add(当前);
                    for (int dz = -1; dz <= 1; dz++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dz == 0) continue;
                        int nx = 当前.x + dx;
                        int nz = 当前.y + dz;
                        if (nx <= 0 || nz <= 0 || nx >= 分辨率 - 1 || nz >= 分辨率 - 1 || 已访问[nz, nx]) continue;
                        if (场.WaterMask[nz, nx] < 0.5f || 场.Height01[nz, nx] <= 规格.SeaLevel01) continue;
                        已访问[nz, nx] = true;
                        队列.Enqueue(new Vector2Int(nx, nz));
                    }
                }
                if (河段.Count < 3) continue;
                河段.Sort((甲, 乙) => 场.Height01[乙.y, 乙.x].CompareTo(场.Height01[甲.y, 甲.x]));
                var 水体 = new WaterBody
                {
                    标识 = $"river_{世界原点.x:F0}_{世界原点.z:F0}_{x}_{z}",
                    类型 = WaterBodyType.River,
                    中心线 = new ArkouraSpline { 类型 = PathType.River, 默认宽度 = 4f },
                    平均深度 = 规格.WaterProfile != null ? 规格.WaterProfile.DefaultDepth : 2f,
                    流速 = Mathf.Lerp(0.4f, 4f, 场.Flow01[z, x]),
                    允许船只 = 河段.Count > 20
                };
                int 取样 = Mathf.Max(1, 河段.Count / 48);
                for (int i = 0; i < 河段.Count; i += 取样)
                {
                    Vector2Int 单元 = 河段[i];
                    float 流量 = 场.Flow01[单元.y, 单元.x];
                    Vector2 宽度范围 = 规格.WaterProfile != null ? 规格.WaterProfile.RiverWidthRange : new Vector2(2f, 20f);
                    水体.中心线.宽度.Add(Mathf.Lerp(宽度范围.x, 宽度范围.y, 流量));
                    水体.中心线.控制点.Add(new Vector3(世界原点.x + 单元.x * 步长, 世界原点.y + 场.Height01[单元.y, 单元.x] * 规格.MaxTerrainHeight, 世界原点.z + 单元.y * 步长));
                }
                结果.Add(水体);
            }
            return 结果;
        }
    }
}
