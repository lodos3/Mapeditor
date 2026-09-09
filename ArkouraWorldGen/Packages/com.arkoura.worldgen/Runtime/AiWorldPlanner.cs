using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Arkoura.WorldGen
{
    public interface IAiWorldPlanner
    {
        IEnumerator Plan(string 描述, Action<string> 成功, Action<string> 失败);
    }

    [Serializable]
    internal sealed class 规划请求
    {
        public string prompt;
        public string schemaVersion = "arkoura-worldspec-0.1";
    }

    public sealed class HttpAiWorldPlanner : IAiWorldPlanner
    {
        private readonly string 服务地址;
        private readonly string 授权头;

        public HttpAiWorldPlanner(string 服务地址, string 授权头 = null)
        {
            this.服务地址 = 服务地址;
            this.授权头 = 授权头;
        }

        public IEnumerator Plan(string 描述, Action<string> 成功, Action<string> 失败)
        {
            if (string.IsNullOrWhiteSpace(服务地址))
            {
                失败?.Invoke("AI 服务地址为空。生产环境应配置自己的服务端代理。 ");
                yield break;
            }
            var 请求体 = JsonUtility.ToJson(new 规划请求 { prompt = 描述 });
            using var 请求 = new UnityWebRequest(服务地址, UnityWebRequest.kHttpVerbPOST);
            请求.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(请求体));
            请求.downloadHandler = new DownloadHandlerBuffer();
            请求.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrWhiteSpace(授权头)) 请求.SetRequestHeader("Authorization", 授权头);
            yield return 请求.SendWebRequest();
            if (请求.result != UnityWebRequest.Result.Success) 失败?.Invoke(请求.error);
            else 成功?.Invoke(请求.downloadHandler.text);
        }
    }
}
