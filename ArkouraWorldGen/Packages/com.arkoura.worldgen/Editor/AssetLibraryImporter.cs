using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Arkoura.WorldGen.Editor
{
    public static class AssetLibraryImporter
    {
        public static int ScanFolder(DefaultAsset 文件夹, AssetLibrary 资产库, string 元数据目录)
        {
            if (文件夹 == null || 资产库 == null) return 0;
            string 路径 = AssetDatabase.GetAssetPath(文件夹);
            if (!AssetDatabase.IsValidFolder(路径)) return 0;
            EnsureFolders(元数据目录);
            var GUID列表 = AssetDatabase.FindAssets("t:Prefab", new[] { 路径 });
            var 结果 = new List<AssetMetadata>();
            int 新建数 = 0;

            foreach (string guid in GUID列表)
            {
                string 预制体路径 = AssetDatabase.GUIDToAssetPath(guid);
                var 预制体 = AssetDatabase.LoadAssetAtPath<GameObject>(预制体路径);
                if (预制体 == null) continue;
                string 安全名 = Path.GetFileNameWithoutExtension(预制体路径).Replace(' ', '_');
                string 元数据路径 = $"{元数据目录}/{安全名}_Meta.asset";
                var 元数据 = AssetDatabase.LoadAssetAtPath<AssetMetadata>(元数据路径);
                if (元数据 == null)
                {
                    元数据 = ScriptableObject.CreateInstance<AssetMetadata>();
                    AssetDatabase.CreateAsset(元数据, AssetDatabase.GenerateUniqueAssetPath(元数据路径));
                    新建数++;
                }
                var 序列化 = new SerializedObject(元数据);
                序列化.FindProperty("资产标识").stringValue = guid;
                序列化.FindProperty("预制体").objectReferenceValue = 预制体;
                序列化.FindProperty("分类").enumValueIndex = (int)InferCategory(预制体.name);
                var 标签属性 = 序列化.FindProperty("标签");
                var 推断标签 = InferTags(预制体.name);
                标签属性.arraySize = 推断标签.Count;
                for (int i = 0; i < 推断标签.Count; i++) 标签属性.GetArrayElementAtIndex(i).stringValue = 推断标签[i];
                序列化.ApplyModifiedPropertiesWithoutUndo();
                结果.Add(元数据);
            }

            资产库.ReplaceAll(结果);
            EditorUtility.SetDirty(资产库);
            AssetDatabase.SaveAssets();
            return 新建数;
        }

        private static AssetCategory InferCategory(string 名称)
        {
            string n = 名称.ToLowerInvariant();
            if (n.Contains("cliff")) return AssetCategory.Cliff;
            if (n.Contains("rock") || n.Contains("stone")) return AssetCategory.Rock;
            if (n.Contains("tree") || n.Contains("pine") || n.Contains("oak")) return AssetCategory.Tree;
            if (n.Contains("grass")) return AssetCategory.Grass;
            if (n.Contains("bush") || n.Contains("shrub")) return AssetCategory.Shrub;
            if (n.Contains("house") || n.Contains("building") || n.Contains("hut")) return AssetCategory.Building;
            if (n.Contains("wall") || n.Contains("fence")) return AssetCategory.Wall;
            if (n.Contains("ruin")) return AssetCategory.Ruin;
            return AssetCategory.Decoration;
        }

        private static List<string> InferTags(string 名称)
        {
            var 标签 = new List<string>();
            string n = 名称.ToLowerInvariant();
            string[] 关键词 = { "forest", "desert", "snow", "swamp", "coast", "medieval", "fantasy", "ancient", "pine", "oak", "large", "small", "ruin", "village", "city" };
            foreach (string 关键词项 in 关键词) if (n.Contains(关键词项)) 标签.Add(关键词项);
            return 标签;
        }

        private static void EnsureFolders(string 完整路径)
        {
            string[] 部分 = 完整路径.Split('/');
            string 当前 = 部分[0];
            for (int i = 1; i < 部分.Length; i++)
            {
                string 下一个 = 当前 + "/" + 部分[i];
                if (!AssetDatabase.IsValidFolder(下一个)) AssetDatabase.CreateFolder(当前, 部分[i]);
                当前 = 下一个;
            }
        }
    }
}
