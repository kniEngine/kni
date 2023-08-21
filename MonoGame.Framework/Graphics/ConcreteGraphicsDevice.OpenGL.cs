// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Platform.Graphics
{
    internal abstract class ConcreteGraphicsDeviceGL : GraphicsDeviceStrategy
    {
        private readonly Dictionary<int, ShaderProgram> _programCache = new Dictionary<int, ShaderProgram>();

        internal bool _supportsInvalidateFramebuffer;
        internal bool _supportsBlitFramebuffer;

        internal int _glDefaultFramebuffer = 0;


        internal ConcreteGraphicsDeviceGL(GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
            : base(adapter, graphicsProfile, preferHalfPixelOffset, presentationParameters)
        {
        }


        public override void Reset(PresentationParameters presentationParameters)
        {
            PresentationParameters = presentationParameters;
            Reset();
        }

        public override void Reset()
        {
        }

        public override void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
        {
            throw new NotImplementedException();
        }

        public override void GetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int elementCount)
        {
            Rectangle srcRect = rect ?? new Rectangle(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);
            int tSize = ReflectionHelpers.SizeOf<T>();
            int flippedY = PresentationParameters.BackBufferHeight - srcRect.Y - srcRect.Height;
            GL.ReadPixels(srcRect.X, flippedY, srcRect.Width, srcRect.Height, PixelFormat.Rgba, PixelType.UnsignedByte, data);

            // buffer is returned upside down, so we swap the rows around when copying over
            int rowSize = srcRect.Width * PresentationParameters.BackBufferFormat.GetSize() / tSize;
            T[] row = new T[rowSize];
            for (int dy = 0; dy < srcRect.Height/2; dy++)
            {
                int topRow = startIndex + dy*rowSize;
                int bottomRow = startIndex + (srcRect.Height - dy - 1)*rowSize;
                // copy the bottom row to buffer
                Array.Copy(data, bottomRow, row, 0, rowSize);
                // copy top row to bottom row
                Array.Copy(data, topRow, data, bottomRow, rowSize);
                // copy buffer to top row
                Array.Copy(row, 0, data, topRow, rowSize);
                elementCount -= rowSize;
            }
        }

        internal ShaderProgram GetProgram(Shader vertexShader, Shader pixelShader, int shaderProgramHash)
        {
            ShaderProgram shaderProgram;
            if (_programCache.TryGetValue(shaderProgramHash, out shaderProgram))
                return shaderProgram;

            // the key does not exist so we need to link the programs
            shaderProgram = CreateProgram(vertexShader, pixelShader);
            _programCache.Add(shaderProgramHash, shaderProgram);
            return shaderProgram;
        }

        private ShaderProgram CreateProgram(Shader vertexShader, Shader pixelShader)
        {
            int program = GL.CreateProgram();
            GraphicsExtensions.CheckGLError();

            GL.AttachShader(program, vertexShader.GetShaderHandle());
            GraphicsExtensions.CheckGLError();

            GL.AttachShader(program, pixelShader.GetShaderHandle());
            GraphicsExtensions.CheckGLError();

            //vertexShader.BindVertexAttributes(program);

            GL.LinkProgram(program);
            GraphicsExtensions.CheckGLError();

            GL.UseProgram(program);
            GraphicsExtensions.CheckGLError();

            vertexShader.GetVertexAttributeLocations(program);

            pixelShader.ApplySamplerTextureUnits(program);

            int linkStatus;
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out linkStatus);
            GraphicsExtensions.LogGLError("VertexShaderCache.Link(), GL.GetProgram");

            if (linkStatus == (int)Bool.True)
            {
                return new ShaderProgram(program);
            }
            else
            { 
                var log = GL.GetProgramInfoLog(program);
                Console.WriteLine(log);
                GL.DetachShader(program, vertexShader.GetShaderHandle());
                GL.DetachShader(program, pixelShader.GetShaderHandle());
                
                if (GL.IsProgram(program))
                {
                    GL.DeleteProgram(program);
                    GraphicsExtensions.CheckGLError();
                }
                throw new InvalidOperationException("Unable to link effect program");
            }
        }

        internal void ClearProgramCache()
        {
            foreach (ShaderProgram shaderProgram in _programCache.Values)
            {
                if (GL.IsProgram(shaderProgram.Program))
                {
                    GL.DeleteProgram(shaderProgram.Program);
                    GraphicsExtensions.CheckGLError();
                }
            }
            _programCache.Clear();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            base.Dispose(disposing);
        }

    }
}
