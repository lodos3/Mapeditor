using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Arkoura.WorldGen.Editor
{
    public readonly struct IntegrationDetectionResult
    {
        public readonly EnvironmentIntegrationDescriptor Descriptor;
        public readonly bool Installed;
        public readonly string Evidence;
        public IntegrationDetectionResult(EnvironmentIntegrationDescriptor 描述, bool 已安装, string 证据)
        {
            Descriptor = 描述;
            Installed = 已安装;
            Evidence = 证据;
        }
    }

    /// <summary>
    /// 通过程序集和类型名称线索检测可选插件，不建立编译期依赖。
    /// 第三方升级导致类型名称变化时，只需更新目录，不影响核心世界数据。
    /// </summary>
    public static class EnvironmentIntegrationDetector
    {
        public static List<IntegrationDetectionResult> Scan()
        {
            var 结果 = new List<IntegrationDetectionResult>();
            var 程序集 = AppDomain.CurrentDomain.GetAssemblies();
            foreach (EnvironmentIntegrationDescriptor 描述 in EnvironmentIntegrationCatalog.Known)
            {
                bool 找到 = 描述.标识 == "unity-native";
                string 证据 = 找到 ? "Unity 原生后端" : string.Empty;

                if (!找到 && 描述.程序集线索 != null)
                {
                    foreach (string 线索 in 描述.程序集线索)
                    {
                        var 命中 = 程序集.FirstOrDefault(项 => 项.GetName().Name.IndexOf(线索, StringComparison.OrdinalIgnoreCase) >= 0);
                        if (命中 == null) continue;
                        找到 = true;
                        证据 = "程序集: " + 命中.GetName().Name;
                        break;
                    }
                }

                if (!找到 && 描述.类型线索 != null)
                {
                    foreach (string 类型名称 in 描述.类型线索)
                    {
                        foreach (var 程序集项 in 程序集)
                        {
                            Type 类型 = 程序集项.GetType(类型名称, false);
                            if (类型 == null) continue;
                            找到 = true;
                            证据 = "类型: " + 类型.FullName;
                            break;
                        }
                        if (找到) break;
                    }
                }

                结果.Add(new IntegrationDetectionResult(描述, 找到, 证据));
            }
            return 结果;
        }

        [MenuItem("Tools/Arkoura/Scan Environment Integrations")]
        public static void LogScan()
        {
            foreach (var 结果 in Scan())
            {
                string 状态 = 结果.Installed ? "已检测" : "未检测";
                UnityEngine.Debug.Log($"Arkoura [{状态}] {结果.Descriptor.显示名} | {结果.Descriptor.能力} | {结果.Evidence}");
            }
        }
    }
}
