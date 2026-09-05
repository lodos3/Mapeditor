using UnityEditor;
using UnityEngine;

namespace Arkoura.WorldGen.Editor
{
    public sealed class ArkouraWorldGenWindow : EditorWindow
    {
        private WorldGenerator 生成器;
        private WorldSpec 世界规格;
        private AssetLibrary 资产库;
        private DefaultAsset 资产文件夹;
        private Vector2 滚动;
        private int 页签;
        private string AI描述 = "生成一个 8km x 8km 的黑暗奇幻海岛，西岸主城，北部山脉，三个村庄，古代道路与森林。";
        private string AI服务地址;
        private string AI返回JSON;

        [MenuItem("Tools/Arkoura/AI World Generator")]
        public static void Open() => GetWindow<ArkouraWorldGenWindow>("Arkoura WorldGen");

        private void OnGUI()
        {
            页签 = GUILayout.Toolbar(页签, new[] { "世界", "资产库", "AI规划", "生成", "验证" });
            滚动 = EditorGUILayout.BeginScrollView(滚动);
            switch (页签)
            {
                case 0: DrawWorld(); break;
                case 1: DrawLibrary(); break;
                case 2: DrawAI(); break;
                case 3: DrawGeneration(); break;
                case 4: DrawValidation(); break;
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawWorld()
        {
            EditorGUILayout.HelpBox("AI 负责世界规划，确定性系统负责实际构建。大地图必须保持分块和全局连续采样。", MessageType.Info);
            世界规格 = (WorldSpec)EditorGUILayout.ObjectField("World Spec", 世界规格, typeof(WorldSpec), false);
            if (世界规格 != null) UnityEditor.Editor.CreateEditor(世界规格).OnInspectorGUI();
        }

        private void DrawLibrary()
        {
            资产库 = (AssetLibrary)EditorGUILayout.ObjectField("Asset Library", 资产库, typeof(AssetLibrary), false);
            资产文件夹 = (DefaultAsset)EditorGUILayout.ObjectField("Prefab 文件夹", 资产文件夹, typeof(DefaultAsset), false);
            EditorGUILayout.HelpBox("扫描后会为每个 Prefab 创建独立 AssetMetadata。自动推断只是初始值，AAA 质量依赖人工补充坡度、海拔、间距、生物群落、道路和水体关系等约束。", MessageType.Warning);
            using (new EditorGUI.DisabledScope(资产库 == null || 资产文件夹 == null))
            {
                if (GUILayout.Button("扫描并建立元数据"))
                {
                    int 新建数 = AssetLibraryImporter.ScanFolder(资产文件夹, 资产库, "Assets/ArkouraWorldGen/Metadata");
                    Debug.Log($"Arkoura: 新建 {新建数} 个元数据资产。总资产数 {资产库.Assets.Count}。");
                }
            }
            if (资产库 != null) EditorGUILayout.LabelField("资产数量", 资产库.Assets.Count.ToString());
        }

        private void DrawAI()
        {
            EditorGUILayout.HelpBox("建议让 Unity 调用你自己的服务端 AI 代理。代理返回 WorldSpec JSON，避免把供应商密钥放入客户端或项目仓库。", MessageType.Info);
            AI服务地址 = EditorGUILayout.TextField("AI 服务地址", AI服务地址);
            EditorGUILayout.LabelField("世界描述");
            AI描述 = EditorGUILayout.TextArea(AI描述, GUILayout.MinHeight(100));
            if (GUILayout.Button("生成规划请求模板"))
            {
                AI返回JSON = JsonUtility.ToJson(new AI请求预览 { prompt = AI描述, schemaVersion = "arkoura-worldspec-0.1" }, true);
            }
            EditorGUILayout.LabelField("请求/返回预览");
            AI返回JSON = EditorGUILayout.TextArea(AI返回JSON, GUILayout.MinHeight(220));
        }

        private void DrawGeneration()
        {
            生成器 = (WorldGenerator)EditorGUILayout.ObjectField("场景生成器", 生成器, typeof(WorldGenerator), true);
            EditorGUILayout.HelpBox("v0.1 会真正创建 Unity Terrain 分块、连续高度和材质权重，并可生成聚落。对于 8km 以上世界，不要一次性生成所有高分辨率块用于最终运行时。", MessageType.Info);
            using (new EditorGUI.DisabledScope(生成器 == null))
            {
                if (GUILayout.Button("生成世界")) 生成器.GenerateWorld();
                if (GUILayout.Button("清除生成结果")) 生成器.ClearWorld();
            }
        }

        private void DrawValidation()
        {
            世界规格 = (WorldSpec)EditorGUILayout.ObjectField("World Spec", 世界规格, typeof(WorldSpec), false);
            资产库 = (AssetLibrary)EditorGUILayout.ObjectField("Asset Library", 资产库, typeof(AssetLibrary), false);
            if (GUILayout.Button("运行生产前基础验证"))
            {
                var 消息 = WorldValidator.Validate(世界规格, 资产库);
                foreach (var 项 in 消息)
                {
                    if (项.Severity == ValidationSeverity.Error) Debug.LogError(项.Message);
                    else if (项.Severity == ValidationSeverity.Warning) Debug.LogWarning(项.Message);
                    else Debug.Log(项.Message);
                }
            }
        }

        [System.Serializable]
        private sealed class AI请求预览
        {
            public string prompt;
            public string schemaVersion;
        }
    }
}
