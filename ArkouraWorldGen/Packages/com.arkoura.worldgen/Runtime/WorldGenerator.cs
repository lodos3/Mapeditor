using UnityEngine;

namespace Arkoura.WorldGen
{
    public sealed class WorldGenerator : MonoBehaviour
    {
        [SerializeField] private WorldSpec 世界规格;
        [SerializeField] private AssetLibrary 资产库;
        [SerializeField] private bool 生成自然资产 = true;
        [SerializeField] private bool 生成聚落 = true;
        [SerializeField] private bool 生成环境语义 = true;
        [SerializeField] private bool 自动建立邻居 = true;
        [SerializeField] private Transform 生成根节点;

        public WorldSpec Spec => 世界规格;
        public AssetLibrary Library => 资产库;

        [ContextMenu("生成世界")]
        public void GenerateWorld()
        {
            if (世界规格 == null) return;
            ClearWorld();
            if (生成根节点 == null)
            {
                var 根对象 = new GameObject("ArkouraGeneratedWorld");
                根对象.transform.SetParent(transform, false);
                生成根节点 = 根对象.transform;
            }

            int 数量 = 世界规格.ChunkCount;
            var 地形网格 = new Terrain[数量, 数量];
            for (int z = 0; z < 数量; z++)
            {
                for (int x = 0; x < 数量; x++)
                {
                    WorldSemanticFields 语义场 = null;
                    if (生成环境语义)
                    {
                        语义场 = WorldSemanticFields.Build(世界规格, x, z, 世界规格.SemanticResolution);
                        HydrologyGenerator.Solve(语义场, 世界规格, 世界规格.Hydrology);
                    }

                    var 数据 = new TerrainData
                    {
                        heightmapResolution = 世界规格.HeightResolution,
                        size = new Vector3(世界规格.ChunkSize, 世界规格.MaxTerrainHeight, 世界规格.ChunkSize)
                    };
                    数据.SetHeights(0, 0, HeightFieldGenerator.Generate(世界规格, x, z));
                    SplatMapGenerator.Apply(数据, 世界规格, 语义场);
                    var 对象 = Terrain.CreateTerrainGameObject(数据);
                    对象.name = $"Chunk_{x}_{z}";
                    对象.transform.SetParent(生成根节点, false);
                    对象.transform.position = new Vector3(x * 世界规格.ChunkSize, 0f, z * 世界规格.ChunkSize);
                    var 分块 = 对象.AddComponent<WorldChunk>();
                    分块.Coordinate = new Vector2Int(x, z);
                    分块.WorldBounds = new Bounds(对象.transform.position + new Vector3(世界规格.ChunkSize * 0.5f, 世界规格.MaxTerrainHeight * 0.5f, 世界规格.ChunkSize * 0.5f), new Vector3(世界规格.ChunkSize, 世界规格.MaxTerrainHeight, 世界规格.ChunkSize));

                    if (语义场 != null)
                    {
                        var 环境数据 = 对象.AddComponent<WorldChunkEnvironmentData>();
                        var 水体 = WaterNetworkBuilder.Build(语义场, 世界规格, 对象.transform.position);
                        var 悬崖 = CliffBandExtractor.Extract(语义场, 对象.transform.position, 世界规格.ChunkSize, 世界规格.MaxTerrainHeight);
                        环境数据.Initialize(语义场, 水体, 悬崖);
                    }
                    地形网格[x, z] = 对象.GetComponent<Terrain>();
                }
            }

            if (自动建立邻居) LinkNeighbors(地形网格, 数量);
            if (生成自然资产 && 资产库 != null) GenerateScatter(地形网格, 数量);
            if (生成聚落 && 资产库 != null) GenerateSettlements(地形网格, 数量);
        }

        [ContextMenu("清除世界")]
        public void ClearWorld()
        {
            if (生成根节点 == null) return;
            if (Application.isPlaying) Destroy(生成根节点.gameObject);
            else DestroyImmediate(生成根节点.gameObject);
            生成根节点 = null;
        }

        private static void LinkNeighbors(Terrain[,] 网格, int 数量)
        {
            for (int z = 0; z < 数量; z++)
            for (int x = 0; x < 数量; x++)
            {
                Terrain 左 = x > 0 ? 网格[x - 1, z] : null;
                Terrain 右 = x < 数量 - 1 ? 网格[x + 1, z] : null;
                Terrain 下 = z > 0 ? 网格[x, z - 1] : null;
                Terrain 上 = z < 数量 - 1 ? 网格[x, z + 1] : null;
                网格[x, z].SetNeighbors(左, 上, 右, 下);
            }
        }

        private void GenerateScatter(Terrain[,] 网格, int 数量)
        {
            var 散布根 = new GameObject("Scatter").transform;
            散布根.SetParent(生成根节点, false);
            for (int z = 0; z < 数量; z++)
            for (int x = 0; x < 数量; x++)
            {
                var 分块根 = new GameObject($"Scatter_{x}_{z}").transform;
                分块根.SetParent(散布根, false);
                AssetScatterGenerator.Generate(网格[x, z], 世界规格, 资产库, new Vector2Int(x, z), 分块根);
            }
        }

        private void GenerateSettlements(Terrain[,] 网格, int 数量)
        {
            foreach (var 地标 in 世界规格.Landmarks)
            {
                if (地标 == null || string.IsNullOrWhiteSpace(地标.类型)) continue;
                string 类型 = 地标.类型.ToLowerInvariant();
                if (!类型.Contains("village") && !类型.Contains("town") && !类型.Contains("city") && !类型.Contains("settlement")) continue;
                float 世界X = 地标.X * 世界规格.WorldSize;
                float 世界Z = 地标.Z * 世界规格.WorldSize;
                int x = Mathf.Clamp(Mathf.FloorToInt(世界X / 世界规格.ChunkSize), 0, 数量 - 1);
                int z = Mathf.Clamp(Mathf.FloorToInt(世界Z / 世界规格.ChunkSize), 0, 数量 - 1);
                var 父节点 = new GameObject($"Settlement_{地标.名称}").transform;
                父节点.SetParent(生成根节点, false);
                var 局部地标 = new LandmarkSpec
                {
                    名称 = 地标.名称,
                    类型 = 地标.类型,
                    X = Mathf.Repeat(世界X, 世界规格.ChunkSize) / 世界规格.ChunkSize,
                    Z = Mathf.Repeat(世界Z, 世界规格.ChunkSize) / 世界规格.ChunkSize,
                    半径 = 地标.半径
                };
                SettlementGenerator.Generate(网格[x, z], 资产库, 局部地标, 世界规格.Seed, 父节点, 世界规格.MaxBuildingsPerSettlement);
            }
        }
    }
}
