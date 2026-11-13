using System.Collections.Generic;
using ExtensaoZero.Multiplayer;
using ExtensaoZero.Telephone;
using NUnit.Framework;

namespace ExtensaoZero.Tests
{
    public sealed class LayerDistributorTests
    {
        [Test]
        public void Solo_GetsAllFourLayers()
        {
            var layers = LayerDistributor.Distribute(1, 0);
            Assert.AreEqual(4, layers.Length);
            CollectionAssert.AreEquivalent(
                new[] { CallLayer.Voice, CallLayer.Ambience, CallLayer.Signals, CallLayer.Interference },
                layers);
        }

        [Test]
        public void FourPlayers_GetOneLayerEach_WithoutDuplicates()
        {
            var seen = new HashSet<CallLayer>();
            for (var i = 0; i < 4; i++)
            {
                var layers = LayerDistributor.Distribute(4, i);
                Assert.AreEqual(1, layers.Length);
                Assert.IsTrue(seen.Add(layers[0]));
            }

            Assert.AreEqual(4, seen.Count);
        }

        [Test]
        public void TwoPlayers_GetTwoComplementaryLayers()
        {
            var p0 = LayerDistributor.Distribute(2, 0);
            var p1 = LayerDistributor.Distribute(2, 1);
            Assert.AreEqual(2, p0.Length);
            Assert.AreEqual(2, p1.Length);
            foreach (var layer in p0) CollectionAssert.DoesNotContain(p1, layer);
        }

        [Test]
        public void ThreePlayers_FirstGetsTwo_OthersGetOne()
        {
            Assert.AreEqual(2, LayerDistributor.Distribute(3, 0).Length);
            Assert.AreEqual(1, LayerDistributor.Distribute(3, 1).Length);
            Assert.AreEqual(1, LayerDistributor.Distribute(3, 2).Length);
        }

        [Test]
        public void InvalidIndices_AreClamped()
        {
            Assert.DoesNotThrow(() => LayerDistributor.Distribute(0, -4));
            Assert.DoesNotThrow(() => LayerDistributor.Distribute(9, 99));
        }
    }
}
