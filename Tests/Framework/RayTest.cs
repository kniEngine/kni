using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace Kni.Tests.Framework
{
    class RayTest
    {

#if !XNA
        [Test]
        public void Deconstruct()
        {
            Ray ray = new Ray(Vector3.Backward, Vector3.Right);

            Vector3 position, direction;

            ray.Deconstruct(out position, out direction);

            Assert.AreEqual(position, ray.Position);
            Assert.AreEqual(direction, ray.Direction);
        }
#endif
    }
}
