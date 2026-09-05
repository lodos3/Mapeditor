using System.Collections.Generic;
using UnityEngine;

namespace Arkoura.WorldGen
{
    [System.Serializable]
    public sealed class CliffBand
    {
        public ArkouraSpline 边缘线 = new ArkouraSpline { 类型 = PathType.CliffBand, 默认宽度 = 8f };
        public float 平均坡度;
        public float 平均曲率;
        public float 强度;
    }

    /// <summary>
    /// 从坡度和曲率语义中提取悬崖候选带。最终表现可以是 Terrain triplanar、Mesh cliff、Digger 洞穴入口或混合方案。
    /// </summary>
    public static class CliffBandExtractor
    {
        public static List<CliffBand> Extract(WorldSemanticFields 场, Vector3 世界原点, float 块尺寸, float 最大高度, float 阈值 = 0.62f)
        {
            var 结果 = new List<CliffBand>();
            if (场 == null) return 结果;
            int 分辨率 = 场.Resolution;
            var 已访问 = new bool[分辨率, 分辨率];
            var 方向 = new[] { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

            for (int z = 1; z < 分辨率 - 1; z++)
            for (int x = 1; x < 分辨率 - 1; x++)
            {
                if (已访问[z, x] || 场.CliffMask[z, x] < 阈值) continue;
                var 队列 = new Queue<Vector2Int>();
                var 组件 = new List<Vector2Int>();
                队列.Enqueue(new Vector2Int(x, z));
                已访问[z, x] = true;
                while (队列.Count > 0)
                {
                    Vector2Int 当前 = 队列.Dequeue();
                    组件.Add(当前);
                    for (int i = 0; i < 方向.Length; i++)
                    {
                        Vector2Int 下一个 = 当前 + 方向[i];
                        if (下一个.x <= 0 || 下一个.y <= 0 || 下一个.x >= 分辨率 - 1 || 下一个.y >= 分辨率 - 1) continue;
                        if (已访问[下一个.y, 下一个.x] || 场.CliffMask[下一个.y, 下一个.x] < 阈值) continue;
                        已访问[下一个.y, 下一个.x] = true;
                        队列.Enqueue(下一个);
                    }
                }
                if (组件.Count < 4) continue;

                组件.Sort((甲, 乙) => 甲.x != 乙.x ? 甲.x.CompareTo(乙.x) : 甲.y.CompareTo(乙.y));
                var 带 = new CliffBand();
                带.边缘线.标识 = $"cliff_{x}_{z}";
                float 坡度总和 = 0f;
                float 曲率总和 = 0f;
                int 步长 = Mathf.Max(1, 组件.Count / 32);
                for (int i = 0; i < 组件.Count; i += 步长)
                {
                    Vector2Int 单元 = 组件[i];
                    float nx = 单元.x / (分辨率 - 1f);
                    float nz = 单元.y / (分辨率 - 1f);
                    带.边缘线.控制点.Add(new Vector3(世界原点.x + nx * 块尺寸, 世界原点.y + 场.Height01[单元.y, 单元.x] * 最大高度, 世界原点.z + nz * 块尺寸));
                    坡度总和 += 场.Slope01[单元.y, 单元.x];
                    曲率总和 += 场.Curvature[单元.y, 单元.x];
                }
                int 数量 = Mathf.Max(1, 带.边缘线.控制点.Count);
                带.平均坡度 = 坡度总和 / 数量;
                带.平均曲率 = 曲率总和 / 数量;
                带.强度 = Mathf.Clamp01(带.平均坡度 * 0.8f + Mathf.Abs(带.平均曲率) * 0.2f);
                结果.Add(带);
            }
            return 结果;
        }
    }
}
