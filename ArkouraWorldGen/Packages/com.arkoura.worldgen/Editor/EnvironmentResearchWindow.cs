using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Arkoura.WorldGen.Editor
{
    public sealed class EnvironmentResearchWindow : EditorWindow
    {
        private Vector2 滚动;
        private List<IntegrationDetectionResult> 结果;

        [MenuItem("Tools/Arkoura/Environment Capability Matrix")]
        public static void Open() => GetWindow<EnvironmentResearchWindow>("Arkoura Environment");

        private void OnEnable() => 结果 = EnvironmentIntegrationDetector.Scan();

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Arkoura 核心不依赖任何第三方插件。此窗口只检测可用后端，并显示它们能接管的环境能力。", MessageType.Info);
            if (GUILayout.Button("重新扫描")) 结果 = EnvironmentIntegrationDetector.Scan();
            滚动 = EditorGUILayout.BeginScrollView(滚动);
            if (结果 != null)
            {
                foreach (var 项 in 结果)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(项.Descriptor.显示名, EditorStyles.boldLabel, GUILayout.Width(190));
                    GUILayout.Label(项.Installed ? "已检测" : "未检测", GUILayout.Width(70));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.LabelField("能力", 项.Descriptor.能力.ToString());
                    if (!string.IsNullOrWhiteSpace(项.Evidence)) EditorGUILayout.LabelField("证据", 项.Evidence);
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
