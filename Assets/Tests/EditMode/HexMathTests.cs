using NUnit.Framework;
using UnityEngine;

namespace HexSpace.Tests.EditMode
{
    public class HexMathTests
    {
        [Test]
        public void Axial_World_RoundTrip()
        {
            var h = new Vector2Int(2, -1);
            var h2 = new Vector2Int(2, 1);
            var p = GameHex.AxialToWorld(h.x, h.y);
            var r = GameHex.WorldToAxial(p);
            // Assert.AreEqual(h2, r);
            Assert.AreEqual(h2, r, $"Expected {h2} but got {r}");
        }
    }
}