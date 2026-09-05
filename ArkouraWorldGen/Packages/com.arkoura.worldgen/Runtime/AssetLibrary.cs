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
                if (!元数据.IsAllowed(0f, 0f, 生物群落名) && !string.IsNullOrWhiteSpace(生物群落名)) continue;
                结果.Add(元数据);
            }
            return 结果;
        }

        public void ReplaceAll(List<AssetMetadata> 新资产)
        {
            资产 = 新资产 ?? new List<AssetMetadata>();
        }
    }
}
