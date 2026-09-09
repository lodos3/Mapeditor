using System.Collections.Generic;
using UnityEngine;

namespace Arkoura.WorldGen
{
    [CreateAssetMenu(menuName = "Arkoura/世界生成/资产库", fileName = "AssetLibrary")]
    public sealed class AssetLibrary : ScriptableObject
    {
        [SerializeField] private List<AssetMetadata> 资产 = new List<AssetMetadata>();
        public IReadOnlyList<AssetMetadata> Assets => 资产;

        public List<AssetMetadata> Query(AssetCategory 分类, string 生物群落名 = null)
        {
            var 结果 = new List<AssetMetadata>();
            for (int 索引 = 0; 索引 < 资产.Count; 索引++)
            {
                var 元数据 = 资产[索引];
                if (元数据 == null || 元数据.Prefab == null || 元数据.Category != 分类) continue;
                if (!BiomeMatches(元数据, 生物群落名)) continue;
                结果.Add(元数据);
            }
            return 结果;
        }

        public List<AssetMetadata> QueryCandidates(AssetCategory 分类, float 海拔米, float 坡度角, float 湿度, float 温度, string 生物群落名 = null)
        {
            var 结果 = new List<AssetMetadata>();
            for (int 索引 = 0; 索引 < 资产.Count; 索引++)
            {
                AssetMetadata 元数据 = 资产[索引];
                if (元数据 == null || 元数据.Prefab == null || 元数据.Category != 分类) continue;
                if (!元数据.IsAllowed(海拔米, 坡度角, 生物群落名, 湿度, 温度)) continue;
                结果.Add(元数据);
            }
            return 结果;
        }

        public void ReplaceAll(List<AssetMetadata> 新资产)
        {
            资产 = 新资产 ?? new List<AssetMetadata>();
        }

        private static bool BiomeMatches(AssetMetadata 元数据, string 生物群落名)
        {
            if (元数据.Biomes == null || 元数据.Biomes.Length == 0 || string.IsNullOrWhiteSpace(生物群落名)) return true;
            for (int i = 0; i < 元数据.Biomes.Length; i++)
            {
                if (string.Equals(元数据.Biomes[i], 生物群落名, System.StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
    }
}
