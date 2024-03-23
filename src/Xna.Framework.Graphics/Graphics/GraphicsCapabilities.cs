// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    /// <summary>
    /// Provides information about the capabilities of the
    /// current graphics device. A very useful thread for investigating GL extenion names
    /// http://stackoverflow.com/questions/3881197/opengl-es-2-0-extensions-on-android-devices
    /// </summary>
    public abstract class GraphicsCapabilities
    {

        /// <summary>
        /// Whether the device fully supports non power-of-two textures, including
        /// mip maps and wrap modes other than CLAMP_TO_EDGE
        /// </summary>
        public bool SupportsNonPowerOfTwo { get; protected set; }

        /// <summary>
        /// Whether the device supports anisotropic texture filtering
        /// </summary>
        public bool SupportsTextureFilterAnisotropic { get; protected set; }

        public bool SupportsDepth24 { get; protected set; }

        public bool SupportsPackedDepthStencil { get; protected set; }

        public bool SupportsDepthNonLinear { get; protected set; }

        public bool SupportsBgra5551 { get; protected set; }
        public bool SupportsAbgr5551 { get; protected set; }
        public bool SupportsBgra4444 { get; protected set; }
        public bool SupportsAbgr4444 { get; protected set; }

        /// <summary>
        /// Gets the support for DXT1
        /// </summary>
        public bool SupportsDxt1 { get; protected set; }

        /// <summary>
        /// Gets the support for S3TC (DXT1, DXT3, DXT5)
        /// </summary>
        public bool SupportsS3tc { get; protected set; }

        /// <summary>
        /// Gets the support for PVRTC
        /// </summary>
        public bool SupportsPvrtc { get; protected set; }

        /// <summary>
        /// Gets the support for ETC1
        /// </summary>
        public bool SupportsEtc1 { get; protected set; }

        /// <summary>
        /// Gets the support for ETC2
        /// </summary>
        public bool SupportsEtc2 { get; protected set; }

        /// <summary>
        /// Gets the support for ATITC
        /// </summary>
        public bool SupportsAtitc { get; protected set; }

        public bool SupportsTextureMaxLevel { get; protected set; }

        /// <summary>
        /// True, if sRGB is supported. On Direct3D platforms, this is always <code>true</code>.
        /// On OpenGL platforms, it is <code>true</code> if both framebuffer sRGB
        /// and texture sRGB are supported.
        /// </summary>
        public bool SupportsSRgb { get; protected set; }

        public bool SupportsTextureArrays { get; protected set; }

        public bool SupportsDepthClamp { get; protected set; }

        protected int _maxTextureSize;
        protected int _maxTextureSlots;
        protected int _maxVertexTextureSlots;

        internal int MaxTextureSize { get { return _maxTextureSize; } }
        internal int MaxTextureSlots { get { return _maxTextureSlots; } }
        internal int MaxVertexTextureSlots { get { return _maxVertexTextureSlots; } }

        protected int _maxVertexBufferSlots;

        public int MaxVertexBufferSlots { get { return _maxVertexBufferSlots; } }

        /// <summary>
        /// True, if the underlying platform supports floating point textures. 
        /// For Direct3D platforms this is always <code>true</code>.
        /// For OpenGL Desktop platforms it is always <code>true</code>.
        /// For OpenGL Mobile platforms it requires `GL_EXT_color_buffer_float`.
        /// If the requested format is not supported an <code>NotSupportedException</code>
        /// will be thrown.
        /// </summary>
        public bool SupportsFloatTextures { get; protected set; }

        /// <summary>
        /// True, if the underlying platform supports half floating point textures. 
        /// For Direct3D platforms this is always <code>true</code>.
        /// For OpenGL Desktop platforms it is always <code>true</code>.
        /// For OpenGL Mobile platforms it requires `GL_EXT_color_buffer_half_float`.
        /// If the requested format is not supported an <code>NotSupportedException</code>
        /// will be thrown.
        /// </summary>
        public bool SupportsHalfFloatTextures { get; protected set; }

        public bool SupportsNormalized { get; protected set; }

        /// <summary>
        /// Gets the max texture anisotropy. This value typically lies
        /// between 0 and 16, where 0 means anisotropic filtering is not
        /// supported.
        /// </summary>
        public int MaxTextureAnisotropy { get; protected set; }

        public bool SupportsInstancing { get; protected set; }

        public bool SupportsBaseIndexInstancing { get; protected set; }

        public bool SupportsSeparateBlendStates { get; protected set; }
    }
}
