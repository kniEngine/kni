// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class Shader
    {

        private void PlatformValidateProfile(ShaderProfileType profile)
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
