// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace Kni.Tests.Framework
{
    [TestFixture]
    class BoundingTest
    {
        [Test]
        public void BoxContainsVector3Test()
        {
            var box = new BoundingBox(Vector3.Zero, Vector3.One);

            Assert.AreEqual(ContainmentType.Disjoint, box.Contains(-Vector3.One));
            Assert.AreEqual(ContainmentType.Disjoint, box.Contains(new Vector3(0.5f, 0.5f, -1f)));
            Assert.AreEqual(ContainmentType.Contains, box.Contains(Vector3.Zero));
            Assert.AreEqual(ContainmentType.Contains, box.Contains(new Vector3(0f, 0, 0.5f)));
            Assert.AreEqual(ContainmentType.Contains, box.Contains(new Vector3(0f, 0.5f, 0.5f)));
            Assert.AreEqual(ContainmentType.Contains, box.Contains(Vector3.One));
            Assert.AreEqual(ContainmentType.Contains, box.Contains(new Vector3(1f, 1, 0.5f)));
            Assert.AreEqual(ContainmentType.Contains, box.Contains(new Vector3(1f, 0.5f, 0.5f)));
            Assert.AreEqual(ContainmentType.Contains, box.Contains(new Vector3(0.5f, 0.5f, 0.5f)));
        }

        [Test]
        public void BoxContainsIdenticalBox()
        {
            var b1 = new BoundingBox(Vector3.Zero, Vector3.One);
            var b2 = new BoundingBox(Vector3.Zero, Vector3.One);

            Assert.AreEqual(ContainmentType.Contains, b1.Contains(b2));
        }

        [Test]
        public void BoundingBoxContainsBoundingFrustumTests()
        {
            var bbox1 = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
            var bbox2 = new BoundingBox(new Vector3(-1000, -1000, -1000), new Vector3(1000, 1000, 1000));
            var bbox3 = new BoundingBox(new Vector3(-1000, -1000, -1000), new Vector3(0, 0, 0));
            var bbox4 = new BoundingBox(new Vector3(-1000, -1000, -1000), new Vector3(-500, -500, -500));
            var bbox5 = new BoundingBox(new Vector3(-1000, -1000, -1000), new Vector3(-150, 1000, -50));

            var view = Matrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
            var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 100);
            var testFrustum = new BoundingFrustum(view * projection);

            Assert.That(bbox1.Contains(testFrustum), Is.EqualTo(ContainmentType.Intersects));
            Assert.That(bbox2.Contains(testFrustum), Is.EqualTo(ContainmentType.Contains));
            Assert.That(bbox3.Contains(testFrustum), Is.EqualTo(ContainmentType.Intersects));
            Assert.That(bbox4.Contains(testFrustum), Is.EqualTo(ContainmentType.Disjoint));
            Assert.That(bbox5.Contains(testFrustum), Is.EqualTo(ContainmentType.Disjoint));
        }

        [Test]
        public void BoundingBoxIntersectsRay()
        {
            // Our test box.
            BoundingBox box;
            box.Min = new Vector3(-10,-20,-30);
            box.Max = new Vector3(10, 20, 30);
            var center = (box.Max + box.Min) * 0.5f;

            // Test misses.
            Assert.IsNull(new Ray(center - Vector3.UnitX * 40, -Vector3.UnitX).Intersects(box));
            Assert.IsNull(new Ray(center + Vector3.UnitX * 40, Vector3.UnitX).Intersects(box));
            Assert.IsNull(new Ray(center - Vector3.UnitY * 40, -Vector3.UnitY).Intersects(box));
            Assert.IsNull(new Ray(center + Vector3.UnitY * 40, Vector3.UnitY).Intersects(box));
            Assert.IsNull(new Ray(center - Vector3.UnitZ * 40, -Vector3.UnitZ).Intersects(box));
            Assert.IsNull(new Ray(center + Vector3.UnitZ * 40, Vector3.UnitZ).Intersects(box));

            // Test middle of each face.
            Assert.AreEqual(30.0f, new Ray(center - Vector3.UnitX * 40, Vector3.UnitX).Intersects(box));
            Assert.AreEqual(30.0f, new Ray(center + Vector3.UnitX * 40, -Vector3.UnitX).Intersects(box));
            Assert.AreEqual(20.0f, new Ray(center - Vector3.UnitY * 40, Vector3.UnitY).Intersects(box));
            Assert.AreEqual(20.0f, new Ray(center + Vector3.UnitY * 40, -Vector3.UnitY).Intersects(box));
            Assert.AreEqual(10.0f, new Ray(center - Vector3.UnitZ * 40, Vector3.UnitZ).Intersects(box));
            Assert.AreEqual(10.0f, new Ray(center + Vector3.UnitZ * 40, -Vector3.UnitZ).Intersects(box));

            // Test the corners along each axis.
            Assert.AreEqual(10.0f, new Ray(box.Min - Vector3.UnitX * 10, Vector3.UnitX).Intersects(box));
            Assert.AreEqual(10.0f, new Ray(box.Min - Vector3.UnitY * 10, Vector3.UnitY).Intersects(box));
            Assert.AreEqual(10.0f, new Ray(box.Min - Vector3.UnitZ * 10, Vector3.UnitZ).Intersects(box));
            Assert.AreEqual(10.0f, new Ray(box.Max + Vector3.UnitX * 10, -Vector3.UnitX).Intersects(box));
            Assert.AreEqual(10.0f, new Ray(box.Max + Vector3.UnitY * 10, -Vector3.UnitY).Intersects(box));
            Assert.AreEqual(10.0f, new Ray(box.Max + Vector3.UnitZ * 10, -Vector3.UnitZ).Intersects(box));

            // Test inside out.
            Assert.AreEqual(0.0f, new Ray(center, Vector3.UnitX).Intersects(box));
            Assert.AreEqual(0.0f, new Ray(center, -Vector3.UnitX).Intersects(box));
            Assert.AreEqual(0.0f, new Ray(center, Vector3.UnitY).Intersects(box));
            Assert.AreEqual(0.0f, new Ray(center, -Vector3.UnitY).Intersects(box));
            Assert.AreEqual(0.0f, new Ray(center, Vector3.UnitZ).Intersects(box));
            Assert.AreEqual(0.0f, new Ray(center, -Vector3.UnitZ).Intersects(box));
        }

        [Test]
        public void BoundingBoxContainsBoundingSphere()
        {
            var bbox1 = new BoundingBox(-Vector3.One, Vector3.One);
            var bsphere1 = new BoundingSphere(Vector3.Zero, 1);
            var bsphere2 = new BoundingSphere(-Vector3.One, 1);
            var bsphere3 = new BoundingSphere(-Vector3.One*2, 1);
            var bsphere4 = new BoundingSphere(Vector3.Zero, 2);
            var bsphere5 = new BoundingSphere(-Vector3.One - (Vector3.One / (Vector3.One.Length() - 0.000001f)), 1);
            var bsphere6 = new BoundingSphere(-Vector3.One - (Vector3.One / (Vector3.One.Length() + 0.000001f)), 1);

            Assert.AreEqual(bbox1.Contains(bsphere1), ContainmentType.Contains);
            Assert.AreEqual(bbox1.Contains(bsphere2), ContainmentType.Intersects);
            Assert.AreEqual(bbox1.Contains(bsphere3), ContainmentType.Disjoint);
            Assert.AreEqual(bbox1.Contains(bsphere5), ContainmentType.Disjoint);
            Assert.AreEqual(bbox1.Contains(bsphere6), ContainmentType.Intersects);
        }

        [Test]
        public void BoundingBoxIntersectsBoundingSphere()
        {
            var bbox1 = new BoundingBox(-Vector3.One, Vector3.One);
            var bsphere1 = new BoundingSphere(Vector3.Zero, 1);
            var bsphere2 = new BoundingSphere(-Vector3.One, 1);
            var bsphere3 = new BoundingSphere(-Vector3.One * 2, 1);
            var bsphere4 = new BoundingSphere(Vector3.Zero, 2);
            var bsphere5 = new BoundingSphere(-Vector3.One - (Vector3.One / (Vector3.One.Length() - 0.000001f)), 1);
            var bsphere6 = new BoundingSphere(-Vector3.One - (Vector3.One / (Vector3.One.Length() + 0.000001f)), 1);

            Assert.AreEqual(bbox1.Intersects(bsphere1), true);
            Assert.AreEqual(bbox1.Intersects(bsphere2), true);
            Assert.AreEqual(bbox1.Intersects(bsphere3), false);
            Assert.AreEqual(bbox1.Intersects(bsphere4), true);
            Assert.AreEqual(bbox1.Intersects(bsphere5), false);
            Assert.AreEqual(bbox1.Intersects(bsphere6), true);
        }

        [Test]
        public void BoundingFrustumIntersectsBoundingBoxTests()
        {
            var view = Matrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
            var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 100);
            var testFrustum = new BoundingFrustum(view * projection);

            var bbox1 = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
            var bbox2 = new BoundingBox(new Vector3(-1000, -1000, -1000), new Vector3(1000, 1000, 1000));
            var bbox3 = new BoundingBox(new Vector3(-1000, -1000, -1000), new Vector3(0, 0, 0));
            var bbox4 = new BoundingBox(new Vector3(-1000, -1000, -1000), new Vector3(-500, -500, -500));
            var bbox5 = new BoundingBox(new Vector3(-1000, -1000, -1000), new Vector3(-150, 1000,  -50));

            Assert.That(testFrustum.Intersects(bbox1), Is.True);
            Assert.That(testFrustum.Intersects(bbox2), Is.True);
            Assert.That(testFrustum.Intersects(bbox3), Is.True);
            Assert.That(testFrustum.Intersects(bbox4), Is.False);
            Assert.That(testFrustum.Intersects(bbox5), Is.False);
        }

        [Test]
        public void BoundingFrustumContainsBoundingBoxTests()
        {
            var view = Matrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
            var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 100);
            var testFrustum = new BoundingFrustum(view * projection);

            var bbox1 = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
            var bbox2 = new BoundingBox(new Vector3(-1000, -1000, -1000), new Vector3(1000, 1000, 1000));
            var bbox3 = new BoundingBox(new Vector3(-1000, -1000, -1000), new Vector3(0, 0, 0));
            var bbox4 = new BoundingBox(new Vector3(-1000, -1000, -1000), new Vector3(-500, -500, -500));
            var bbox5 = new BoundingBox(new Vector3(-1000, -1000, -1000), new Vector3(-150, 1000,  -50));

            Assert.That(testFrustum.Contains(bbox1), Is.EqualTo(ContainmentType.Contains));
            Assert.That(testFrustum.Contains(bbox2), Is.EqualTo(ContainmentType.Intersects));
            Assert.That(testFrustum.Contains(bbox3), Is.EqualTo(ContainmentType.Intersects));
            Assert.That(testFrustum.Contains(bbox4), Is.EqualTo(ContainmentType.Disjoint));
#if !XNA    // XNA reports a false Intersects
            Assert.That(testFrustum.Contains(bbox5), Is.EqualTo(ContainmentType.Disjoint));
#endif
        }

        [Test]
        public void BoundingFrustumIntersectsBoundingFrustumTests()
        {
            var view = Matrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
            var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 100);
            var testFrustum = new BoundingFrustum(view * projection);

            // Same frustum.
            Assert.That(testFrustum.Intersects(testFrustum), Is.True);

            var otherFrustum = new BoundingFrustum(Matrix.Identity);

            // Smaller frustum contained entirely inside.
            var view2 = Matrix.CreateLookAt(new Vector3(0, 0, 4), Vector3.Zero, Vector3.Up);
            var projection2 = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 50);
            otherFrustum.Matrix = view2 * projection2;

            Assert.That(testFrustum.Intersects(otherFrustum), Is.True);
            Assert.That(otherFrustum.Intersects(testFrustum), Is.True);

            // Same size frustum, pointing in the same direction and offset by a small amount.
            otherFrustum.Matrix = view2 * projection;

            Assert.That(testFrustum.Intersects(otherFrustum), Is.True);
            Assert.That(otherFrustum.Intersects(testFrustum), Is.True);

            // Same size frustum, pointing in the opposite direction and not overlapping.
            var view3 = Matrix.CreateLookAt(new Vector3(0, 0, 6), new Vector3(0, 0, 7), Vector3.Up);
            otherFrustum.Matrix = view3 * projection;

            Assert.That(testFrustum.Intersects(otherFrustum), Is.False);
            Assert.That(otherFrustum.Intersects(testFrustum), Is.False);

            // Larger frustum, entirely containing test frustum.
            var view4 = Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up);
            var projection4 = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 1000);
            otherFrustum.Matrix = view4 * projection4;

            Assert.That(testFrustum.Intersects(otherFrustum), Is.True);
            Assert.That(otherFrustum.Intersects(testFrustum), Is.True);

            // Same size frustum,  pointing to the right, behind test frustum.
            var view5 = Matrix.CreateLookAt(new Vector3(-1, 0, 5), new Vector3(5, 0, 5), Vector3.Up);
            var projection5 = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 100);
            otherFrustum.Matrix = view5 * projection5;

            Assert.That(testFrustum.Intersects(otherFrustum), Is.False);
            Assert.That(otherFrustum.Intersects(testFrustum), Is.False);
        }

        [Test]
        public void BoundingFrustumContainsBoundingFrustumTests()
        {
            var view = Matrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
            var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 100);
            var testFrustum = new BoundingFrustum(view * projection);

            // Same frustum.
            Assert.That(testFrustum.Contains(testFrustum), Is.EqualTo(ContainmentType.Contains));

            var otherFrustum = new BoundingFrustum(Matrix.Identity);

            // Smaller frustum contained entirely inside.
            var view2 = Matrix.CreateLookAt(new Vector3(0, 0, 4), Vector3.Zero, Vector3.Up);
            var projection2 = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 50);
            otherFrustum.Matrix = view2 * projection2;

            Assert.That(testFrustum.Contains(otherFrustum), Is.EqualTo(ContainmentType.Contains));
            Assert.That(otherFrustum.Contains(testFrustum), Is.EqualTo(ContainmentType.Intersects));

            // Same size frustum, pointing in the same direction and offset by a small amount.
            otherFrustum.Matrix = view2 * projection;

            Assert.That(testFrustum.Contains(otherFrustum), Is.EqualTo(ContainmentType.Intersects));
            Assert.That(otherFrustum.Contains(testFrustum), Is.EqualTo(ContainmentType.Intersects));

            // Same size frustum, pointing in the opposite direction and not overlapping.
            var view3 = Matrix.CreateLookAt(new Vector3(0, 0, 6), new Vector3(0, 0, 7), Vector3.Up);
            otherFrustum.Matrix = view3 * projection;

            Assert.That(testFrustum.Contains(otherFrustum), Is.EqualTo(ContainmentType.Disjoint));
            Assert.That(otherFrustum.Contains(testFrustum), Is.EqualTo(ContainmentType.Disjoint));

            // Larger frustum, entirely containing test frustum.
            var view4 = Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up);
            var projection4 = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 1000);
            otherFrustum.Matrix = view4 * projection4;

            Assert.That(testFrustum.Contains(otherFrustum), Is.EqualTo(ContainmentType.Intersects));
            Assert.That(otherFrustum.Contains(testFrustum), Is.EqualTo(ContainmentType.Contains));
            Assert.That(testFrustum.Intersects(otherFrustum), Is.True);

            // Same size frustum,  pointing to the right, behind test frustum.
            var view5 = Matrix.CreateLookAt(new Vector3(-1, 0, 5), new Vector3(5, 0, 5), Vector3.Up);
            var projection5 = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 100);
            otherFrustum.Matrix = view5 * projection5;

            Assert.That(testFrustum.Contains(otherFrustum), Is.EqualTo(ContainmentType.Disjoint));
            Assert.That(otherFrustum.Contains(testFrustum), Is.EqualTo(ContainmentType.Disjoint));
        }

        [Test]
        public void BoundingFrustumIntersectsRayTests()
        {
            var testFrustum =
                new BoundingFrustum(Matrix.CreateLookAt(new Vector3(0, 1, 1), new Vector3(0, 0, 0), Vector3.Up) *
                                    Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                        1.3f, 0.1f, 1000.0f));

            Ray ray1 = new Ray(new Vector3(0, 0.5f, 0.5f), new Vector3(0, 0, 0));
            Ray ray2 = new Ray(new Vector3(0, 1.0f, 1.0f), new Vector3(0, 0, 0));

            float? value1 = testFrustum.Intersects(ray1);
            Assert.AreEqual(0.0f, value1);

            float? value2 = testFrustum.Intersects(ray2);
            Assert.AreEqual(null, value2);
        }

        [Test]
        public void BoundingSphereContainsTests()
        {
            var zeroPoint = BoundingSphere.CreateFromPoints( new[] {Vector3.Zero} );
            Assert.AreEqual(new BoundingSphere(), zeroPoint);

            var onePoint = BoundingSphere.CreateFromPoints(new[] { Vector3.One });
            Assert.AreEqual(new BoundingSphere(Vector3.One, 0), onePoint);

            var twoPoint = BoundingSphere.CreateFromPoints(new[] { Vector3.Zero, Vector3.One });
            Assert.AreEqual(new BoundingSphere(new Vector3(0.5f, 0.5f, 0.5f), 0.8660254f), twoPoint);

            var threePoint = BoundingSphere.CreateFromPoints(new[] { new Vector3(0, 0, 0), new Vector3(-1, 0, 0), new Vector3(1, 1, 1) });
            Assert.That(new BoundingSphere(new Vector3(0, 0.5f, 0.5f), 1.224745f), Is.EqualTo(threePoint).Using(BoundingSphereComparer.Epsilon));

            var eightPointTestInput = new Vector3[]
            {
                new Vector3(54.58071f, 124.9063f, 56.0016f),
                new Vector3(54.52138f, 124.9063f, 56.13985f),
                new Vector3(54.52208f, 124.8235f, 56.14014f),
                new Vector3(54.5814f, 124.8235f, 56.0019f),
                new Vector3(1145.415f, 505.913f, -212.5173f),
                new Vector3(611.4731f, 505.9535f, 1031.893f),
                new Vector3(617.7462f, -239.7422f, 1034.584f),
                new Vector3(1151.687f, -239.7035f, -209.8246f)
            };
            var eightPoint = BoundingSphere.CreateFromPoints(eightPointTestInput);
            for (int i = 0; i < eightPointTestInput.Length; i++)
            {
                Assert.That(eightPoint.Contains(eightPointTestInput[i]) != ContainmentType.Disjoint);
            }

            Assert.Throws<ArgumentException>(() => BoundingSphere.CreateFromPoints(new Vector3[] {}));
        }

#if !XNA
        [Test]
        public void BoundingBoxDeconstruct()
        {
            BoundingBox boundingBox = new BoundingBox(new Vector3(255, 255, 255), new Vector3(0, 0, 0));

            Vector3 min, max;

            boundingBox.Deconstruct(out min, out max);

            Assert.AreEqual(min, boundingBox.Min);
            Assert.AreEqual(max, boundingBox.Max);
        }

        [Test]
        public void BoundingSphereDeconstruct()
        {
            BoundingSphere boundingSphere = new BoundingSphere(new Vector3(255, 255, 255), float.MaxValue);

            Vector3 center;
            float radius;

            boundingSphere.Deconstruct(out center, out radius);

            Assert.AreEqual(center, boundingSphere.Center);
            Assert.AreEqual(radius, boundingSphere.Radius);
        }
#endif
    }
}
