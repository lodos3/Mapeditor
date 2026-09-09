using System.Collections.Generic;
using UnityEngine;

namespace Arkoura.WorldGen
{
    public static class SettlementGenerator
    {
        public static void Generate(Terrain 地形, AssetLibrary 资产库, LandmarkSpec 地标, int 种子, Transform 父节点, int 最大建筑)
        {
            if (地形 == null || 资产库 == null || 地标 == null || 最大建筑 <= 0) return;
            var 建筑 = new List<AssetMetadata>();
            foreach (var 资产 in 资产库.Assets)
                if (资产 != null && 资产.Category == AssetCategory.Building && 资产.Prefab != null) 建筑.Add(资产);
            if (建筑.Count == 0) return;

            var 随机 = new System.Random(种子 ^ 地标.名称.GetHashCode());
            Vector3 地形原点 = 地形.transform.position;
            float 中心X = 地标.X * 地形.terrainData.size.x + 地形原点.x;
            float 中心Z = 地标.Z * 地形.terrainData.size.z + 地形原点.z;
            int 数量 = Mathf.Min(最大建筑, Mathf.Max(4, Mathf.RoundToInt(地标.半径 / 8f)));

            for (int i = 0; i < 数量; i++)
            {
                float 角度 = (float)随机.NextDouble() * Mathf.PI * 2f;
                float 半径 = Mathf.Sqrt((float)随机.NextDouble()) * 地标.半径;
                float x = 中心X + Mathf.Cos(角度) * 半径;
                float z = 中心Z + Mathf.Sin(角度) * 半径;
                float nx = (x - 地形原点.x) / 地形.terrainData.size.x;
                float nz = (z - 地形原点.z) / 地形.terrainData.size.z;
                if (nx < 0f || nz < 0f || nx > 1f || nz > 1f) continue;
                float 坡度 = 地形.terrainData.GetSteepness(nx, nz);
                float y = 地形原点.y + 地形.terrainData.GetInterpolatedHeight(nx, nz);
                var 资产 = 建筑[随机.Next(建筑.Count)];
                if (!资产.IsAllowed(y, 坡度, null)) continue;
                Quaternion 旋转 = Quaternion.Euler(0f, Mathf.Atan2(中心X - x, 中心Z - z) * Mathf.Rad2Deg, 0f);
                var 实例 = Object.Instantiate(资产.Prefab, new Vector3(x, y, z), 旋转, 父节点);
                float 缩放 = Mathf.Lerp(资产.ScaleRange.x, 资产.ScaleRange.y, (float)随机.NextDouble());
                实例.transform.localScale *= 缩放;
            }
        }
    }
}
