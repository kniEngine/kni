// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class Shader
    {

        private static ShaderProfileType PlatformProfile()
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformConstructShader(ShaderStage stage, byte[] shaderBytecode)
        {
            throw new PlatformNotSupportedException();
        }

        private void PlatformGraphicsDeviceResetting()
        {
            throw new PlatformNotSupportedException();
        }

    }
}
