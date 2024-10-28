// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using NUnit.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kni.Tests.Graphics
{
    [TestFixture]
    class IndexBufferTest: GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void ShouldSetAndGetData()
        {   
            var savedData = new short[] { 1, 2, 3, 4 };
            var indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, savedData.Length, BufferUsage.None);
            indexBuffer.SetData(savedData);

            var readData = new short[4];
            indexBuffer.GetData(readData, 0, 4);
            Assert.AreEqual(savedData, readData);
            
            indexBuffer.Dispose();
        }

        [Test]
        public void ShouldSetAndGetData_elementCount()
        {
            var savedData = new short[] { 1, 2, 3, 4 };
            var indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, savedData.Length, BufferUsage.None);
            indexBuffer.SetData(savedData);
            
            var readData = new short[4];
            indexBuffer.GetData(readData, 0, 2);
            Assert.AreEqual(1, readData[0]);
            Assert.AreEqual(2, readData[1]);
            Assert.AreEqual(0, readData[2]);
            Assert.AreEqual(0, readData[3]);

            indexBuffer.Dispose();
        }

        [Test]
        public void ShouldSetAndGetData_startIndex()
        {
            var savedData = new short[] { 1, 2, 3, 4 };
            var indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, savedData.Length, BufferUsage.None);
            indexBuffer.SetData(savedData);

            var readData = new short[4];
            indexBuffer.GetData(readData, 2, 2);
            Assert.AreEqual(0, readData[0]);
            Assert.AreEqual(0, readData[1]);
            Assert.AreEqual(1, readData[2]);
            Assert.AreEqual(2, readData[3]);

            indexBuffer.Dispose();
        }

        [Test]
        public void ShouldSetAndGetData_offsetInBytes()
        {
            var savedData = new short[] { 1, 2, 3, 4 };
            var indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, savedData.Length, BufferUsage.None);
            indexBuffer.SetData(savedData);

            var readData = new short[2];
            indexBuffer.GetData(sizeof(short) * 2, readData, 0, 2);
            Assert.AreEqual(3, readData[0]);
            Assert.AreEqual(4, readData[1]);

            indexBuffer.Dispose();
        }


        struct TriangleIndices16
        {
            public short A, B, C;

            public TriangleIndices16(short a, short b, short c)
            {
                this.A = a;
                this.B = b;
                this.C = c;
            }

            public int VertexStride { get { return (sizeof(short)) * 3; } }

            public override string ToString()
            {
                return String.Format("A:{0,4} B:{1,4} C:{2,4}", A, B, C);
            }
        }

        [Test]
        public void ShouldSetAndGetStructData()
        {
            short[] savedData = new short[] { 1, 2, 3, 4, 5, 6 };
            IndexBuffer indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, savedData.Length, BufferUsage.None);
            indexBuffer.SetData(savedData);

            TriangleIndices16[] readData = new TriangleIndices16[2];
            indexBuffer.GetData(readData, 0, 2);
            Assert.AreEqual(savedData[0], readData[0].A);
            Assert.AreEqual(savedData[1], readData[0].B);
            Assert.AreEqual(savedData[2], readData[0].C);
            Assert.AreEqual(savedData[3], readData[1].A);
            Assert.AreEqual(savedData[4], readData[1].B);
            Assert.AreEqual(savedData[5], readData[1].C);

            indexBuffer.Dispose();
        }

        [Test]
        public void ShouldSetStructAndGetData()
        {
            TriangleIndices16[] savedData = new TriangleIndices16[] { new TriangleIndices16(1,2,3), new TriangleIndices16(4, 5, 6) };
            IndexBuffer indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, savedData.Length*3, BufferUsage.None);
            indexBuffer.SetData(savedData);

            short[] readData = new short[6];
            indexBuffer.GetData(readData, 0, 6);
            Assert.AreEqual(savedData[0].A, readData[0]);
            Assert.AreEqual(savedData[0].B, readData[1]);
            Assert.AreEqual(savedData[0].C, readData[2]);
            Assert.AreEqual(savedData[1].A, readData[3]);
            Assert.AreEqual(savedData[1].B, readData[4]);
            Assert.AreEqual(savedData[1].C, readData[5]);

            indexBuffer.Dispose();
        }

        [Test]
        public void ShouldDynamicSetAndGetStructData()
        {
            short[] savedData = new short[] { 1, 2, 3, 4, 5, 6 };
            IndexBuffer indexBuffer = new DynamicIndexBuffer(gd, IndexElementSize.SixteenBits, savedData.Length, BufferUsage.None);
            indexBuffer.SetData(savedData);

            TriangleIndices16[] readData = new TriangleIndices16[2];
            indexBuffer.GetData(readData, 0, 2);
            Assert.AreEqual(savedData[0], readData[0].A);
            Assert.AreEqual(savedData[1], readData[0].B);
            Assert.AreEqual(savedData[2], readData[0].C);
            Assert.AreEqual(savedData[3], readData[1].A);
            Assert.AreEqual(savedData[4], readData[1].B);
            Assert.AreEqual(savedData[5], readData[1].C);

            indexBuffer.Dispose();
        }

        [Test]
        public void ShouldDynamicSetStructAndGetData()
        {
            TriangleIndices16[] savedData = new TriangleIndices16[] { new TriangleIndices16(1, 2, 3), new TriangleIndices16(4, 5, 6) };
            IndexBuffer indexBuffer = new DynamicIndexBuffer(gd, IndexElementSize.SixteenBits, savedData.Length*3, BufferUsage.None);
            indexBuffer.SetData(savedData);

            short[] readData = new short[6];
            indexBuffer.GetData(readData, 0, 6);
            Assert.AreEqual(savedData[0].A, readData[0]);
            Assert.AreEqual(savedData[0].B, readData[1]);
            Assert.AreEqual(savedData[0].C, readData[2]);
            Assert.AreEqual(savedData[1].A, readData[3]);
            Assert.AreEqual(savedData[1].B, readData[4]);
            Assert.AreEqual(savedData[1].C, readData[5]);

            indexBuffer.Dispose();
        }

        [Test]
        public void NullDeviceShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
            {
                var indexBuffer = new IndexBuffer(null, IndexElementSize.SixteenBits, 3, BufferUsage.None);
                indexBuffer.Dispose();
            });
            GC.GetTotalMemory(true); // collect uninitialized IndexBuffer
        }
    }
}
