using NUnit.Framework;
using UnityEngine;

namespace Arkoura.WorldGen.Tests
{
    public sealed class EnvironmentFieldTests
    {
        [Test]
        public void SemanticFields_AreDeterministicAndFinite()
        {
            var 规格 = ScriptableObject.CreateInstance<WorldSpec>();
            WorldSemanticFields 甲 = WorldSemanticFields.Build(规格, 0, 0, 33);
            WorldSemanticFields 乙 = WorldSemanticFields.Build(规格, 0, 0, 33);
            for (int z = 0; z < 33; z++)
            for (int x = 0; x < 33; x++)
            {
                Assert.AreEqual(甲.Height01[z, x], 乙.Height01[z, x], 0.0000001f);
                Assert.IsFalse(float.IsNaN(甲.Slope01[z, x]));
                Assert.IsFalse(float.IsInfinity(甲.Curvature[z, x]));
            }
            Object.DestroyImmediate(规格);
        }

        [Test]
        public void SemanticFields_AdjacentChunksShareHeightBorder()
        {
            var 规格 = ScriptableObject.CreateInstance<WorldSpec>();
            WorldSemanticFields 左 = WorldSemanticFields.Build(规格, 0, 0, 65);
            WorldSemanticFields 右 = WorldSemanticFields.Build(规格, 1, 0, 65);
            for (int z = 0; z < 65; z++) Assert.AreEqual(左.Height01[z, 64], 右.Height01[z, 0], 0.000001f);
            Object.DestroyImmediate(规格);
        }

        [Test]
        public void Hydrology_ProducesNormalizedFields()
        {
            var 规格 = ScriptableObject.CreateInstance<WorldSpec>();
            WorldSemanticFields 场 = WorldSemanticFields.Build(规格, 0, 0, 65);
            HydrologyGenerator.Solve(场, 规格, new HydrologySettings());
            for (int z = 0; z < 场.Resolution; z++)
            for (int x = 0; x < 场.Resolution; x++)
            {
                Assert.That(场.Flow01[z, x], Is.InRange(0f, 1f));
                Assert.That(场.WaterMask[z, x], Is.InRange(0f, 1f));
                Assert.That(场.ShoreMask[z, x], Is.InRange(0f, 1f));
            }
            Object.DestroyImmediate(规格);
        }
    }
}
