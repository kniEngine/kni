﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace Kni.Tests.Graphics
{
    [TestFixture]
    class VertexBufferTest : GraphicsDeviceTestFixtureBase
    {
        VertexPositionTexture[] savedData = new VertexPositionTexture[] 
        {
            new VertexPositionTexture(new Vector3(1,2,3), new Vector2(0.1f,0.2f)),
            new VertexPositionTexture(new Vector3(4,5,6), new Vector2(0.3f,0.4f)),
            new VertexPositionTexture(new Vector3(7,8,9), new Vector2(0.5f,0.6f)),
            new VertexPositionTexture(new Vector3(10,11,12), new Vector2(0.7f,0.8f))
        };
        VertexPositionTexture vertexZero = new VertexPositionTexture(Vector3.Zero, Vector2.Zero);
        
        //[TestCase(true)]
        [TestCase(false)]
        public void ShouldSetAndGetData(bool dynamic)
        {
            VertexBuffer vertexBuffer = (dynamic)
                ?new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                :new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            vertexBuffer.SetData(savedData);

            VertexPositionTexture[] readData = new VertexPositionTexture[4];
            vertexBuffer.GetData(readData, 0, 4);
            Assert.AreEqual(savedData, readData);

            vertexBuffer.Dispose();
        }

        //[TestCase(true)]
        [TestCase(false)]
        public void ShouldSetAndGetData_elementCount(bool dynamic)
        {
            VertexBuffer vertexBuffer = (dynamic)
                                      ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                                      : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            vertexBuffer.SetData(savedData);

            VertexPositionTexture[] readData = new VertexPositionTexture[4];
            vertexBuffer.GetData(readData, 0, 2);
            Assert.AreEqual(savedData[0], readData[0]);
            Assert.AreEqual(savedData[1], readData[1]);
            Assert.AreEqual(vertexZero, readData[2]);
            Assert.AreEqual(vertexZero, readData[3]);

            vertexBuffer.Dispose();
        }

        //[TestCase(true)]
        [TestCase(false)]
        public void ShouldSetAndGetData_startIndex(bool dynamic)
        {
            VertexBuffer vertexBuffer = (dynamic)
                                      ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                                      : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            vertexBuffer.SetData(savedData);

            VertexPositionTexture[] readData = new VertexPositionTexture[4];
            vertexBuffer.GetData(readData, 2, 2);
            Assert.AreEqual(vertexZero, readData[0]);
            Assert.AreEqual(vertexZero, readData[1]);
            Assert.AreEqual(savedData[0], readData[2]);
            Assert.AreEqual(savedData[1], readData[3]);

            vertexBuffer.Dispose();
        }
        
        //[TestCase(true)]
        [TestCase(false)]
        public void ShouldSetAndGetData_offsetInBytes(bool dynamic)
        {
            VertexBuffer vertexBuffer = (dynamic)
                                      ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                                      : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            vertexBuffer.SetData(savedData);

            VertexPositionTexture[] readData = new VertexPositionTexture[2];
            int vertexStride = VertexPositionTexture.VertexDeclaration.VertexStride;
            int offsetInBytes = vertexStride * 2;
            vertexBuffer.GetData(offsetInBytes, readData, 0, 2, vertexStride);
            Assert.AreEqual(savedData[2], readData[0]);
            Assert.AreEqual(savedData[3], readData[1]);

            vertexBuffer.Dispose();
        }

        //[TestCase(true)]
        [TestCase(false)]
        public void ShouldSetAndGetDataBytes(bool dynamic)
        {
            VertexBuffer vertexBuffer = (dynamic)
                                      ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                                      : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            byte[] savedDataBytes = ArrayUtil.ConvertFrom(savedData);
            vertexBuffer.SetData(savedDataBytes);

            if (dynamic)
            {
                DynamicVertexBuffer dynamicVertexBuffer = vertexBuffer as DynamicVertexBuffer;
                dynamicVertexBuffer.SetData(savedDataBytes, 0, savedDataBytes.Length, SetDataOptions.None);
            }

            VertexPositionTexture[] readData = new VertexPositionTexture[4];
            vertexBuffer.GetData(readData, 0, 4);
            Assert.AreEqual(savedData, readData);

            vertexBuffer.Dispose();
        }

        //[TestCase(true)]
        [TestCase(false)]
        public void ShouldSetBytesAndGetBytes(bool dynamic)
        {
            VertexBuffer vertexBuffer = (dynamic)
                ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            byte[] savedDataBytes = ArrayUtil.ConvertFrom(savedData);
            vertexBuffer.SetData(savedDataBytes);

            if (dynamic)
            {
                DynamicVertexBuffer dynamicVertexBuffer = vertexBuffer as DynamicVertexBuffer;
                dynamicVertexBuffer.SetData(savedDataBytes, 0, savedDataBytes.Length, SetDataOptions.None);
            }

            byte[] readData = new byte[savedDataBytes.Length];
            vertexBuffer.GetData(readData, 0, readData.Length);

            Assert.AreEqual(savedDataBytes, readData);

            vertexBuffer.Dispose();
        }

        //[TestCase(true)]
        [TestCase(false)]
        public void ShouldSetBytesAndGetStructData(bool dynamic)
        {
            VertexBuffer vertexBuffer = (dynamic)
                                      ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                                      : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            byte[] savedDataBytes = new byte[] { 1,2,3,4,5,6,7,8 };
            vertexBuffer.SetData(savedDataBytes);

            if (dynamic)
            {
                DynamicVertexBuffer dynamicVertexBuffer = vertexBuffer as DynamicVertexBuffer;
                dynamicVertexBuffer.SetData(savedDataBytes, 0, savedDataBytes.Length, SetDataOptions.None);
            }

            Color[] readData = new Color[2];
            vertexBuffer.GetData(0, readData, 0, readData.Length, 1);

            Assert.AreEqual(readData[0].R, savedDataBytes[0]);
            Assert.AreEqual(readData[0].G, savedDataBytes[1]);
            Assert.AreEqual(readData[0].B, savedDataBytes[2]);
            Assert.AreEqual(readData[0].A, savedDataBytes[3]);
            Assert.AreEqual(readData[1].R, savedDataBytes[4]);
            Assert.AreEqual(readData[1].G, savedDataBytes[5]);
            Assert.AreEqual(readData[1].B, savedDataBytes[6]);
            Assert.AreEqual(readData[1].A, savedDataBytes[7]);

            vertexBuffer.Dispose();
        }

        //[TestCase(true)]
        [TestCase(false, -1, 0, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 0, 0, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 0, 1, true, null)]
        [TestCase(false, 0, -1, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 0, 80, true, null)]
        [TestCase(false, 0, 81, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 1, 0, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 1, 1, true, null)]
        [TestCase(false, 1, 79, true, null)]
        [TestCase(false, 1, 80, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 79, 1, true, null)]
        [TestCase(false, 79, 2, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 80, 0, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 80, 1, false, typeof(ArgumentOutOfRangeException))]
        public void SetDataWithElementCount(bool dynamic, int startIndex, int elementCount, bool shouldSucceed, Type expectedExceptionType)
        {
            VertexBuffer vertexBuffer = (dynamic)
                                      ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                                      : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length,BufferUsage.None);
            byte[] savedDataBytes = ArrayUtil.ConvertFrom(savedData);

            if (!shouldSucceed)
                Assert.Throws(expectedExceptionType, () => vertexBuffer.SetData(savedDataBytes, startIndex, elementCount));
            else
            {
                vertexBuffer.SetData(savedDataBytes, startIndex, elementCount);

                byte[] readDataBytes = new byte[savedDataBytes.Length];
                vertexBuffer.GetData(readDataBytes, startIndex, elementCount);
                Assert.AreEqual(
                    savedDataBytes.Skip(startIndex).Take(elementCount).ToArray(),
                    readDataBytes.Skip(startIndex).Take(elementCount).ToArray());
            }

            vertexBuffer.Dispose();
        }

        [TestCase(false, 1, -1, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 0, 0, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 80, 0, null)]
        [TestCase(false, 80, 1, null)]
        [TestCase(false, 1, 2, null)]
        [TestCase(false, 1, 40, null)]
        [TestCase(false, 2, 40, null)]
        [TestCase(false, 2, 80, typeof(InvalidOperationException))]
        [TestCase(false, 1, 80, null)]
        [TestCase(false, 4, 12, null)]
#if XNA
        [TestCase(false, 1, 81, null)]
        [TestCase(false, 2, 81, typeof(InvalidOperationException))]
#else
        // We throw when the vertex stride is too large
        [TestCase(false, 1, 81, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 2, 81, typeof(ArgumentOutOfRangeException))]
#endif
        public void SetDataWithElementCountAndVertexStride(bool dynamic, int elementCount, int vertexStride, Type expectedExceptionType)
        {
            VertexBuffer vertexBuffer = (dynamic)
                                      ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                                      : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            byte[] savedDataBytes = ArrayUtil.ConvertFrom(savedData);

            if (expectedExceptionType != null)
                Assert.Throws(expectedExceptionType, () => vertexBuffer.SetData(0, savedDataBytes, 0, elementCount, vertexStride));
            else
            {
                vertexBuffer.SetData(0, savedDataBytes, 0, elementCount, vertexStride);

                byte[] readDataBytes = new byte[savedDataBytes.Length];
                vertexBuffer.GetData(0, readDataBytes, 0, elementCount, vertexStride);
                Assert.AreEqual(
                    savedDataBytes.Take(elementCount).ToArray(), 
                    readDataBytes.Take(elementCount).ToArray());
            }

            vertexBuffer.Dispose();
        }

        [Test]
        public void GetSetDataVertexStrideTest()
        {
            const int size = 5;
            VertexPositionTexture[] data = new VertexPositionTexture[size];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new VertexPositionTexture(
                    new Vector3(i * 3, i * 3 + 1, i * 3 + 2),
                    new Vector2(i * 2 / (float) 10, (i * 2 + 1) / (float) 10));
            }

            VertexBuffer vertexBuffer = new VertexBuffer(gd, VertexPositionTexture.VertexDeclaration, data.Length, BufferUsage.None);
            vertexBuffer.SetData(data);

            Vector2[] textureCoords = new Vector2[2 * size + 1];
            textureCoords[0] = new Vector2(-42, 42);
            vertexBuffer.GetData(3 * 4, textureCoords, 1, size, 20);

            // first one should not be overwritten
            Assert.AreEqual(new Vector2(-42, 42), textureCoords[0]);
            // last should not be overwritten
            for (int i = 1 + size; i< textureCoords.Length; i++)
                Assert.AreEqual(Vector2.Zero, textureCoords[i]);
            for (int i = 0; i < size; i++)
            {
                int index = i + 1;
                Vector2 expected = new Vector2(i * 2 / (float) 10, (i * 2 + 1) / (float) 10);
                Assert.AreEqual(expected, textureCoords[index]);
            }

            vertexBuffer.SetData(3 * 4, textureCoords, 1, size, 20);
            vertexBuffer.GetData(3 * 4, textureCoords, 1, size, 20);

            // first one should not be overwritten
            Assert.AreEqual(new Vector2(-42, 42), textureCoords[0]);
            // last should not be overwritten
            for (int i = 1 + size; i< textureCoords.Length; i++)
                Assert.AreEqual(Vector2.Zero, textureCoords[i]);
            for (int i = 0; i < size; i++)
            {
                int index = i + 1;
                Vector2 expected = new Vector2(i * 2 / (float) 10, (i * 2 + 1) / (float) 10);
                Assert.AreEqual(expected, textureCoords[index]);
            }
        }

        //[TestCase(true)]
        [TestCase(false, 1, 20, true, null)]
        [TestCase(false, 3, 20, true, null)]
        [TestCase(false, 4, 0, true, null)]
        [TestCase(false, 4, 16, false, typeof(ArgumentOutOfRangeException))]
        [TestCase(false, 4, 20, true, null)]
        [TestCase(false, 5, 20, false, typeof(ArgumentOutOfRangeException))]
        public void SetDataStructWithElementCountAndVertexStride(bool dynamic, int elementCount, int vertexStride, bool shouldSucceed, Type expectedExceptionType)
        {
            VertexBuffer vertexBuffer = (dynamic)
                                      ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                                      : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length,BufferUsage.None);

            if (!shouldSucceed)
                Assert.Throws(expectedExceptionType, () => vertexBuffer.SetData(0, savedData, 0, elementCount, vertexStride));
            else
            {
                vertexBuffer.SetData(0, savedData, 0, elementCount, vertexStride);

                VertexPositionTexture[] readData = new VertexPositionTexture[savedData.Length];
                vertexBuffer.GetData(0, readData, 0, elementCount, vertexStride);
                Assert.AreEqual(
                    savedData.Take(elementCount).ToArray(),
                    readData.Take(elementCount).ToArray());
            }

            vertexBuffer.Dispose();
        }

        //[TestCase(true)]
        [TestCase(false)]
        public void GetPosition(bool dynamic)
        {
            VertexBuffer vertexBuffer = (dynamic)
                                      ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                                      : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            vertexBuffer.SetData(savedData);

            Vector3[] readData = new Vector3[5];
            readData[4] = new Vector3(42, 42, 42);
            int vertexStride = VertexPositionTexture.VertexDeclaration.VertexStride;
            vertexBuffer.GetData(0, readData, 0, 4, vertexStride);
            Assert.AreEqual(savedData[0].Position, readData[0]);
            Assert.AreEqual(savedData[1].Position, readData[1]);
            Assert.AreEqual(savedData[2].Position, readData[2]);
            Assert.AreEqual(savedData[3].Position, readData[3]);
            Assert.AreEqual(new Vector3(42, 42, 42), readData[4]); // bound check

            vertexBuffer.Dispose();
        }

        //[TestCase(true)]
        [TestCase(false)]
        public void SetPosition(bool dynamic)
        {
            VertexBuffer vertexBuffer = (dynamic)
                                      ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                                      : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            Vector3[] positions = new Vector3[]
            {
                savedData[0].Position,
                savedData[1].Position,
                savedData[2].Position,
                savedData[3].Position
            };
            int vertexStride = VertexPositionTexture.VertexDeclaration.VertexStride;
            vertexBuffer.SetData(0, positions, 0, 4, vertexStride);

            Vector3[] readData = new Vector3[5];
            readData[4] = new Vector3(42, 42, 42);
            vertexBuffer.GetData(0, readData, 0, 4, vertexStride);
            Assert.AreEqual(savedData[0].Position, readData[0]);
            Assert.AreEqual(savedData[1].Position, readData[1]);
            Assert.AreEqual(savedData[2].Position, readData[2]);
            Assert.AreEqual(savedData[3].Position, readData[3]);
            Assert.AreEqual(new Vector3(42, 42, 42), readData[4]); // bound check

            vertexBuffer.Dispose();
        }

        //[TestCase(true)]
        [TestCase(false)]
        public void GetTextureCoordinate(bool dynamic)
        {
            VertexBuffer vertexBuffer = (dynamic)
                                      ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                                      : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            vertexBuffer.SetData(savedData);

            Vector2[] readData = new Vector2[5];
            readData[4] = new Vector2(42, 42);
            int vertexStride = VertexPositionTexture.VertexDeclaration.VertexStride;
            int offsetInBytes = VertexPositionTexture.VertexDeclaration.GetVertexElements()[1].Offset;
            vertexBuffer.GetData(offsetInBytes, readData, 0, 4, vertexStride);
            Assert.AreEqual(savedData[0].TextureCoordinate, readData[0]);
            Assert.AreEqual(savedData[1].TextureCoordinate, readData[1]);
            Assert.AreEqual(savedData[2].TextureCoordinate, readData[2]);
            Assert.AreEqual(savedData[3].TextureCoordinate, readData[3]);
            Assert.AreEqual(new Vector2(42, 42), readData[4]); // bound check

            vertexBuffer.Dispose();
        }

        //[TestCase(true)]
        [TestCase(false)]
        public void SetTextureCoordinate(bool dynamic)
        {
            VertexBuffer vertexBuffer = (dynamic)
                                      ? new DynamicVertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None)
                                      : new VertexBuffer(gd, typeof(VertexPositionTexture), savedData.Length, BufferUsage.None);
            Vector2[] texCoords = new Vector2[]
            {
                savedData[0].TextureCoordinate,
                savedData[1].TextureCoordinate,
                savedData[2].TextureCoordinate,
                savedData[3].TextureCoordinate
            };
            int vertexStride = VertexPositionTexture.VertexDeclaration.VertexStride;
            int offsetInBytes = VertexPositionTexture.VertexDeclaration.GetVertexElements()[1].Offset;
            vertexBuffer.SetData(offsetInBytes, texCoords, 0, 4, vertexStride);

            Vector2[] readData = new Vector2[5];
            readData[4] = new Vector2(42, 42);
            vertexBuffer.GetData(offsetInBytes, readData, 0, 4, vertexStride);
            Assert.AreEqual(savedData[0].TextureCoordinate, readData[0]);
            Assert.AreEqual(savedData[1].TextureCoordinate, readData[1]);
            Assert.AreEqual(savedData[2].TextureCoordinate, readData[2]);
            Assert.AreEqual(savedData[3].TextureCoordinate, readData[3]);
            Assert.AreEqual(new Vector2(42, 42), readData[4]); // bound check

            vertexBuffer.Dispose();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct VertexTextureCoordinateTest : IVertexType
        {
            public Vector3 Normal;
            public Vector2 TextureCoordinate;

            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));

            VertexDeclaration IVertexType.VertexDeclaration
            {
                get { return VertexDeclaration; }
            }
        }

        [Test]
        public void ShouldSucceedWhenVertexFormatDoesMatchShader()
        {
            VertexBuffer vertexBuffer = new VertexBuffer(gd, VertexPositionTexture.VertexDeclaration, 3, BufferUsage.None);
            gd.SetVertexBuffer(vertexBuffer);

            Effect effect = new BasicEffect(gd);
            effect.CurrentTechnique.Passes[0].Apply();

            Assert.DoesNotThrow(() => gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 1));

            vertexBuffer.Dispose();
        }

        [Test]
#if DESKTOPGL
        [Ignore("we should figure out if there's a way to check this in OpenGL")]
#endif
        public void ShouldThrowHelpfulExceptionWhenVertexFormatDoesNotMatchShader()
        {
            VertexBuffer vertexBuffer = new VertexBuffer(gd, VertexTextureCoordinateTest.VertexDeclaration, 3, BufferUsage.None);
            gd.SetVertexBuffer(vertexBuffer);

            Effect effect = new BasicEffect(gd);
            effect.CurrentTechnique.Passes[0].Apply();

            Exception ex = Assert.Throws<InvalidOperationException>(() => gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 1));
#if XNA
            Assert.That(ex.Message, Is.EqualTo("The current vertex declaration does not include all the elements required by the current vertex shader. Position0 is missing."));
#else
            Assert.That(ex.Message, Is.EqualTo("An error occurred while preparing to draw. "
                + "This is probably because the current vertex declaration does not include all the elements "
                + "required by the current vertex shader. The current vertex declaration includes these elements: " 
                + "NORMAL0, TEXCOORD0."));
#endif

            vertexBuffer.Dispose();
        }

        [Test]
        public void NullDeviceShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
            {
                VertexBuffer vertexBuffer = new VertexBuffer(null, typeof(VertexPositionTexture), 3, BufferUsage.None);
                vertexBuffer.Dispose();
            });
            GC.GetTotalMemory(true); // collect uninitialized vertexBuffer
        }
    }
}
