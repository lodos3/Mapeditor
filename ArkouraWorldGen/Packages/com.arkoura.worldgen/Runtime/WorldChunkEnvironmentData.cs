using System.Collections.Generic;
using UnityEngine;

namespace Arkoura.WorldGen
{
    /// <summary>
    /// 保存生成后的高层环境结果，避免把大型二维语义数组序列化进场景。
    /// </summary>
    public sealed class WorldChunkEnvironmentData : MonoBehaviour
    {
        [SerializeField] private float 平均湿度;
        [SerializeField] private float 平均温度;
        [SerializeField] private float 水体覆盖率;
        [SerializeField] private float 悬崖覆盖率;
        [SerializeField] private List<WaterBody> 水体 = new List<WaterBody>();
        [SerializeField] private List<CliffBand> 悬崖带 = new List<CliffBand>();

        public float AverageMoisture => 平均湿度;
        public float AverageTemperature => 平均温度;
        public float WaterCoverage => 水体覆盖率;
        public float CliffCoverage => 悬崖覆盖率;
        public IReadOnlyList<WaterBody> WaterBodies => 水体;
        public IReadOnlyList<CliffBand> CliffBands => 悬崖带;

        public void Initialize(WorldSemanticFields 场, List<WaterBody> 新水体, List<CliffBand> 新悬崖)
        {
            水体 = 新水体 ?? new List<WaterBody>();
            悬崖带 = 新悬崖 ?? new List<CliffBand>();
            if (场 == null) return;
            float 湿度总计 = 0f;
            float 温度总计 = 0f;
            float 水体总计 = 0f;
            float 悬崖总计 = 0f;
            int 总数 = 场.Resolution * 场.Resolution;
            for (int z = 0; z < 场.Resolution; z++)
            for (int x = 0; x < 场.Resolution; x++)
            {
                湿度总计 += 场.Moisture01[z, x];
                温度总计 += 场.Temperature01[z, x];
                水体总计 += 场.WaterMask[z, x];
                悬崖总计 += 场.CliffMask[z, x];
            }
            float 除数 = Mathf.Max(1, 总数);
            平均湿度 = 湿度总计 / 除数;
            平均温度 = 温度总计 / 除数;
            水体覆盖率 = 水体总计 / 除数;
            悬崖覆盖率 = 悬崖总计 / 除数;
        }
    }
}
