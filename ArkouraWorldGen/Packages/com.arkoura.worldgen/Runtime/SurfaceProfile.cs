using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arkoura.WorldGen
{
    [Serializable]
    public sealed class SurfaceRule
    {
        public string 名称;
        public TerrainLayer 地形层;
        [Range(0f, 1f)] public float 最低高度;
        [Range(0f, 1f)] public float 最高高度 = 1f;
        [Range(0f, 1f)] public float 最低坡度;
        [Range(0f, 1f)] public float 最高坡度 = 1f;
        [Range(0f, 1f)] public float 最低湿度;
        [Range(0f, 1f)] public float 最高湿度 = 1f;
        [Range(-1f, 1f)] public float 最低曲率 = -1f;
        [Range(-1f, 1f)] public float 最高曲率 = 1f;
        [Range(0f, 1f)] public float 最低水流;
        [Range(0f, 1f)] public float 最高水流 = 1f;
        [Range(0f, 1f)] public float 悬崖偏好;
        [Range(0f, 1f)] public float 岸线偏好;
        [Min(0.1f)] public float 锐度 = 1f;
        public bool 使用三平面意图;
        public bool 使用高度混合意图 = true;
        public bool 抗平铺意图 = true;
        public bool 接收湿润意图 = true;
        public bool 接收积雪意图 = true;
    }

    [CreateAssetMenu(menuName = "Arkoura/世界生成/地表配置", fileName = "SurfaceProfile")]
    public sealed class SurfaceProfile : ScriptableObject
    {
        [SerializeField] private List<SurfaceRule> 规则 = new List<SurfaceRule>();
        [SerializeField, Range(0f, 1f)] private float 全局混合羽化 = 0.08f;
        [SerializeField, Min(1f)] private float 世界空间纹理尺度 = 8f;
        [SerializeField] private bool 输出对象融合掩码 = true;
        [SerializeField] private bool 输出Decal掩码 = true;
        public IReadOnlyList<SurfaceRule> Rules => 规则;
        public float Feather => 全局混合羽化;
        public float WorldTextureScale => 世界空间纹理尺度;
        public bool OutputBlendMask => 输出对象融合掩码;
        public bool OutputDecalMask => 输出Decal掩码;
    }

    public static class SurfaceWeightGenerator
    {
        public static float[,,] Generate(WorldSemanticFields 场, SurfaceProfile 配置)
        {
            if (场 == null || 配置 == null || 配置.Rules.Count == 0) return null;
            int 分辨率 = 场.Resolution;
            int 层数 = 配置.Rules.Count;
            var 权重 = new float[分辨率, 分辨率, 层数];

            for (int z = 0; z < 分辨率; z++)
            for (int x = 0; x < 分辨率; x++)
            {
                float 总计 = 0f;
                for (int i = 0; i < 层数; i++)
                {
                    SurfaceRule 规则 = 配置.Rules[i];
                    if (规则 == null || 规则.地形层 == null) continue;
                    float 值 = Band(场.Height01[z, x], 规则.最低高度, 规则.最高高度, 配置.Feather);
                    值 *= Band(场.Slope01[z, x], 规则.最低坡度, 规则.最高坡度, 配置.Feather);
                    值 *= Band(场.Moisture01[z, x], 规则.最低湿度, 规则.最高湿度, 配置.Feather);
                    值 *= Band(场.Curvature[z, x], 规则.最低曲率, 规则.最高曲率, 配置.Feather * 2f);
                    值 *= Band(场.Flow01[z, x], 规则.最低水流, 规则.最高水流, 配置.Feather);
                    if (规则.悬崖偏好 > 0f) 值 *= Mathf.Lerp(1f, 场.CliffMask[z, x], 规则.悬崖偏好);
                    if (规则.岸线偏好 > 0f) 值 *= Mathf.Lerp(1f, 场.ShoreMask[z, x], 规则.岸线偏好);
                    值 = Mathf.Pow(Mathf.Clamp01(值), 规则.锐度);
                    权重[z, x, i] = 值;
                    总计 += 值;
                }

                if (总计 <= 0.000001f)
                {
                    权重[z, x, 0] = 1f;
                    总计 = 1f;
                }
                for (int i = 0; i < 层数; i++) 权重[z, x, i] /= 总计;
            }
            return 权重;
        }

        private static float Band(float 值, float 最小, float 最大, float 羽化)
        {
            float 进入 = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(最小 - 羽化, 最小 + 羽化, 值));
            float 离开 = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(最大 - 羽化, 最大 + 羽化, 值));
            return Mathf.Clamp01(Mathf.Min(进入, 离开));
        }
    }
}
