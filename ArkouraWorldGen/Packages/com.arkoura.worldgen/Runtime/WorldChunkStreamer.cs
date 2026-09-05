using UnityEngine;

namespace Arkoura.WorldGen
{
    public sealed class WorldChunkStreamer : MonoBehaviour
    {
        [SerializeField] private Transform 观察者;
        [SerializeField, Min(100f)] private float 激活距离 = 1600f;
        [SerializeField, Min(0.1f)] private float 刷新间隔 = 0.5f;
        private float 下次刷新;
        private WorldChunk[] 分块;

        private void Start()
        {
            if (观察者 == null && Camera.main != null) 观察者 = Camera.main.transform;
            分块 = FindObjectsByType<WorldChunk>(FindObjectsSortMode.None);
        }

        private void Update()
        {
            if (观察者 == null || Time.unscaledTime < 下次刷新) return;
            下次刷新 = Time.unscaledTime + 刷新间隔;
            float 距离平方 = 激活距离 * 激活距离;
            for (int i = 0; i < 分块.Length; i++)
            {
                var 分块项 = 分块[i];
                if (分块项 == null) continue;
                Vector3 最近点 = 分块项.WorldBounds.ClosestPoint(观察者.position);
                bool 应激活 = (最近点 - 观察者.position).sqrMagnitude <= 距离平方;
                if (分块项.gameObject.activeSelf != 应激活) 分块项.gameObject.SetActive(应激活);
            }
        }
    }
}
