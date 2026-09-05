using NUnit.Framework;
using UnityEngine;

namespace Arkoura.WorldGen.Tests
{
    public sealed class HeightFieldTests
    {
        [Test]
        public void SameSpecAndChunkProduceSameHeightField()
        {
            var 规格 = ScriptableObject.CreateInstance<WorldSpec>();
            try
            {
                float[,] 第一次 = HeightFieldGenerator.Generate(规格, 2, 3);
                float[,] 第二次 = HeightFieldGenerator.Generate(规格, 2, 3);
                Assert.AreEqual(第一次.GetLength(0), 第二次.GetLength(0));
                Assert.AreEqual(第一次.GetLength(1), 第二次.GetLength(1));
                int[] 样本 = { 0, 17, 83, 128, 256, 400, 512 };
                foreach (int z in 样本)
                foreach (int x in 样本)
                    Assert.AreEqual(第一次[z, x], 第二次[z, x], 0.0000001f);
            }
            finally
            {
                Object.DestroyImmediate(规格);
            }
        }

        [Test]
        public void AdjacentChunksShareIdenticalBorderSamples()
        {
            var 规格 = ScriptableObject.CreateInstance<WorldSpec>();
            try
            {
                float[,] 左块 = HeightFieldGenerator.Generate(规格, 0, 0);
                float[,] 右块 = HeightFieldGenerator.Generate(规格, 1, 0);
                int 最大索引 = 规格.HeightResolution - 1;
                for (int z = 0; z <= 最大索引; z += 16)
                    Assert.AreEqual(左块[z, 最大索引], 右块[z, 0], 0.0000001f);
            }
            finally
            {
                Object.DestroyImmediate(规格);
            }
        }
    }
}
