// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Diagnostics;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Matrix = Microsoft.Xna.Framework.Matrix;

#if __IOS__ || __TVOS__
using ObjCRuntime;
#endif


namespace Microsoft.Xna.Platform.Graphics.OpenGL
{
    internal enum BufferAccess
    {
        ReadOnly = 0x88B8,
    }

    internal enum BufferUsageHint
    {
        StreamDraw  = 0x88E0,
        StaticDraw  = 0x88E4,
        DynamicDraw = 0x88E8,
    }

    internal enum StencilFace
    {
        Front = 0x0404,
        Back  = 0x0405,
    }
    internal enum DrawBuffersEnum
    {
        None             = 0x0000,
        Back             = 0x0405,
        ColorAttachment0 = 0x8CE0,
    }

    internal enum ShaderType
    {
        VertexShader   = 0x8B31,
        FragmentShader = 0x8B30,
    }

    internal enum ShaderParameter
    {
        LogLength     = 0x8B84,
        CompileStatus = 0x8B81,
        SourceLength  = 0x8B88,
    }

    internal enum GetProgramParameterName
    {
        LogLength  = 0x8B84,
        LinkStatus = 0x8B82,
    }

    internal enum DrawElementsType
    {
        UnsignedShort = 0x1403,
        UnsignedInt   = 0x1405,
    }

    internal enum QueryTarget
    {
        SamplesPassed    = 0x8914,
        SamplesPassedExt = 0x8C2F,
    }

    internal enum GetQueryObjectParam
    {
        QueryResultAvailable = 0x8867,
        QueryResult = 0x8866,
    }

    internal enum BlitFramebufferFilter
    {
        Nearest = 0x2600,
    }

    internal enum ReadBufferMode
    {
        ColorAttachment0 = 0x8CE0,
    }

    internal enum DrawBufferMode
    {
        ColorAttachment0 = 0x8CE0,
    }

    internal enum FramebufferErrorCode
    {
        FramebufferUndefined = 0x8219,
        FramebufferComplete = 0x8CD5,
        FramebufferCompleteExt = 0x8CD5,
        FramebufferIncompleteAttachment = 0x8CD6,
        FramebufferIncompleteAttachmentExt = 0x8CD6,
        FramebufferIncompleteMissingAttachment = 0x8CD7,
        FramebufferIncompleteMissingAttachmentExt = 0x8CD7,
        FramebufferIncompleteDimensionsExt = 0x8CD9,
        FramebufferIncompleteFormatsExt = 0x8CDA,
        FramebufferIncompleteDrawBuffer = 0x8CDB,
        FramebufferIncompleteDrawBufferExt = 0x8CDB,
        FramebufferIncompleteReadBuffer = 0x8CDC,
        FramebufferIncompleteReadBufferExt = 0x8CDC,
        FramebufferUnsupported = 0x8CDD,
        FramebufferUnsupportedExt = 0x8CDD,
        FramebufferIncompleteMultisample = 0x8D56,
        FramebufferIncompleteLayerTargets = 0x8DA8,
        FramebufferIncompleteLayerCount = 0x8DA9,
    }

    internal enum BufferTarget
    {
        ArrayBuffer        = 0x8892,
        ElementArrayBuffer = 0x8893,
    }

    internal enum RenderbufferTarget
    {
        Renderbuffer = 0x8D41,
    }

    internal enum FramebufferTarget
    {
        Framebuffer     = 0x8D40,
        FramebufferExt  = 0x8D40,
        ReadFramebuffer = 0x8CA8,
    }

    internal enum RenderbufferStorage
    {
        Rgba8 = 0x8058,
        DepthComponent16 = 0x81a5,
        DepthComponent24 = 0x81a6,
        Depth24Stencil8  = 0x88F0,

        // GLES Values
        DepthComponent24Oes = 0x81A6,
        Depth24Stencil8Oes  = 0x88F0,
        StencilIndex8 = 0x8D48,

        DepthComponent16NonlinearNv = 0x8E2C,
    }

    internal enum EnableCap : int
    {
        PointSmooth = 0x0B10,
        LineSmooth = 0x0B20,
        CullFace = 0x0B44,
        Lighting = 0x0B50,
        ColorMaterial = 0x0B57,
        Fog = 0x0B60,
        DepthTest = 0x0B71,
        StencilTest = 0x0B90,
        Normalize = 0x0BA1,
        AlphaTest = 0x0BC0,
        Dither = 0x0BD0,
        Blend = 0x0BE2,
        ColorLogicOp = 0x0BF2,
        ScissorTest = 0x0C11,
        Texture2D = 0x0DE1,
        PolygonOffsetFill = 0x8037,
        RescaleNormal = 0x803A,
        VertexArray = 0x8074,
        NormalArray = 0x8075,
        ColorArray = 0x8076,
        TextureCoordArray = 0x8078,
        Multisample = 0x809D,
        SampleAlphaToCoverage = 0x809E,
        SampleAlphaToOne = 0x809F,
        SampleCoverage = 0x80A0,
        DebugOutputSynchronous = 0x8242,
        DepthClamp = 0x864F,
        DebugOutput = 0x92E0,
    }

    internal enum VertexAttribPointerType
    {
        Byte = 0x1400,
        UnsignedByte = 0x1401,
        Short = 0x1402,
        UnsignedShort = 0x1403,
        Float = 0x1406,
        HalfFloat = 0x140B,
    }

    internal enum CullFaceMode
    {
        Back  = 0x0405,
        Front = 0x0404,
    }

    internal enum FrontFaceDirection
    {
        Cw  = 0x0900,
        Ccw = 0x0901,
    }

    internal enum MaterialFace
    {
        FrontAndBack = 0x0408,
    }

    internal enum PolygonMode
    {
        Fill = 0x1B02,
        Line = 0x1B01,
    }

    internal enum BlendEquationMode
    {
        FuncAdd = 0x8006,
        Max = 0x8008,  // ios MaxExt
        Min = 0x8007,  // ios MinExt
        FuncReverseSubtract = 0x800B,
        FuncSubtract = 0x800A,
    }

    internal enum BlendingFunc
    {
        Zero = 0,
        SrcColor = 0x0300,
        OneMinusSrcColor = 0x0301,
        SrcAlpha = 0x0302,
        OneMinusSrcAlpha = 0x0303,
        DstAlpha = 0x0304,
        OneMinusDstAlpha = 0x0305,
        DstColor = 0x0306,
        OneMinusDstColor = 0x0307,
        SrcAlphaSaturate = 0x0308,
        ConstantColor = 0x8001,
        OneMinusConstantColor = 0x8002,
        ConstantAlpha = 0x8003,
        OneMinusConstantAlpha = 0x8004,
        One = 1,
    }

    internal enum ComparisonFunc
    {
        Never    = 0x0200,
        Less     = 0x0201,
        Equal    = 0x0202,
        Greater  = 0x0204,

        Lequal   = 0x0203, // Less | Equal
        Notequal = 0x0205, // Less | Greater
        Gequal   = 0x0206, // Equal | Greater
        Always   = 0x0207, // Less | Equal | Greater
    }

    internal enum GetPName : int
    {
        ArrayBufferBinding = 0x8894,
        MaxTextureImageUnits = 0x8872,
        MaxVertexTextureImageUnits = 0x8B4C,
        MaxCombinedTextureImageUnits = 0x8B4D,
        MaxVertexAttribs = 0x8869,
        MaxTextureSize = 0x0D33,
        MaxDrawBuffers = 0x8824,
        TextureBinding2D = 0x8069,
        MaxTextureMaxAnisotropyExt = 0x84FF,
        MaxSamples = 0x8D57,
    }

    internal enum StringName
    {
        Vendor     = 0x1F00,
        Renderer   = 0x1F01,
        Version    = 0x1F02,
        Extensions = 0x1F03,
    }

    internal enum FramebufferAttachment
    {
        ColorAttachment0     = 0x8CE0,
        DepthAttachment      = 0x8D00,
        StencilAttachment    = 0x8D20,
        ColorAttachmentExt   = 0x1800,
        DepthAttachementExt  = 0x1801,
        StencilAttachmentExt = 0x1802,
    }

    internal enum GLPrimitiveType
    {
        Points    = 0x0000,
        Lines     = 0x0001,
        LineStrip = 0x0003,
        Triangles = 0x0004,
        TriangleStrip = 0x0005,
    }

    [Flags]
    internal enum ClearBufferMask
    {
        DepthBufferBit   = 0x00000100,
        StencilBufferBit = 0x00000400,
        ColorBufferBit   = 0x00004000,
    }

    internal enum ErrorCode
    {
        NO_ERROR          = 0x0000,
        INVALID_ENUM      = 0x0500,
        INVALID_VALUE     = 0x0501,
        INVALID_OPERATION = 0x0502,
        STACK_OVERFLOW    = 0x0503,
        STACK_UNDERFLOW   = 0x0504,
        OUT_OF_MEMORY     = 0x0505,
        INVALID_FRAMEBUFFER_OPERATION = 0x0506,
        CONTEXT_LOST      = 0x0507, //(with OpenGL 4.5 or ARB_KHR_robustness)
    }

    internal enum TextureUnit
    {
        Texture0 = 0x84C0,
    }

    internal enum TextureTarget
    {
        Texture1D = 0x0DE0,
        Texture2D = 0x0DE1,
        Texture3D = 0x806F,
        TextureCubeMap = 0x8513,
        Texture1DArray = 0x8C18,
        Texture2DArray = 0x8C1A,
        Texture2DMultisample = 0x9100,
        Texture2DMultisampleArray = 0x9102,

        TextureCubeMapPositiveX = 0x8515,
        TextureCubeMapNegativeX = 0x8516,
        TextureCubeMapPositiveY = 0x8517,
        TextureCubeMapNegativeY = 0x8518,
        TextureCubeMapPositiveZ = 0x8519,
        TextureCubeMapNegativeZ = 0x851A,
    }

    internal enum PixelInternalFormat
    {
        Rgba = 0x1908,
        Rgb = 0x1907,
        Rgba4 = 0x8056,
        Luminance = 0x1909,
        CompressedRgbS3tcDxt1Ext = 0x83F0,
        CompressedSrgbS3tcDxt1Ext = 0x8C4C,
        CompressedRgbaS3tcDxt1Ext = 0x83F1,
        CompressedRgbaS3tcDxt3Ext = 0x83F2,
        CompressedSrgbAlphaS3tcDxt3Ext = 0x8C4E,
        CompressedRgbaS3tcDxt5Ext = 0x83F3,
        CompressedSrgbAlphaS3tcDxt5Ext = 0x8C4F,
        R32f = 0x822E,
        Rg16f = 0x822F,
        Rgba16f = 0x881A,
        R16f = 0x822D,
        Rg32f = 0x8230,
        Rgba32f = 0x8814,
        Rg8i = 0x8237,
        Rgba8i = 0x8D8E,
        Rg16ui = 0x823A,
        Rgba16ui = 0x8D76,
        Rgb10A2ui = 0x906F,
        Rgba16 = 0x805B,
        // PVRTC
        CompressedRgbPvrtc2Bppv1Img = 0x8C01,
        CompressedRgbPvrtc4Bppv1Img = 0x8C00,
        CompressedRgbaPvrtc2Bppv1Img = 0x8C03,
        CompressedRgbaPvrtc4Bppv1Img = 0x8C02,
        // ATITC
        AtcRgbaExplicitAlphaAmd = 0x8C93,
        AtcRgbaInterpolatedAlphaAmd = 0x87EE,
        // ETC1
        Etc1 = 0x8D64,
        Srgb = 0x8C40,

        // ETC2 RGB8A1
        Etc2Rgb8 = 0x9274,
        Etc2Srgb8 = 0x9275,
        Etc2Rgb8A1 = 0x9276,
        Etc2Srgb8A1 = 0x9277,
        Etc2Rgba8Eac = 0x9278,
        Etc2SRgb8A8Eac = 0x9279,
    }

    internal enum PixelFormat
    {
        Rgba = 0x1908,
        Rgb = 0x1907,
        Luminance = 0x1909,
        CompressedTextureFormats = 0x86A3,
        Red = 0x1903,
        Rg = 0x8227,
    }

    internal enum PixelType
    {
        UnsignedByte = 0x1401,
        UnsignedShort565 = 0x8363,
        UnsignedShort4444 = 0x8033,
        UnsignedShort5551 = 0x8034,
        Float = 0x1406,
        HalfFloat = 0x140B,
        HalfFloatOES = 0x8D61,
        Byte = 0x1400,
        UnsignedShort = 0x1403,
        UnsignedInt1010102 = 0x8036,
    }

    internal enum PixelStoreParameter
    {
        UnpackAlignment = 0x0CF5,
        PackAlignment   = 0x0D05,
    }

    internal enum StencilOp
    {
        Keep = 0x1E00,
        DecrWrap = 0x8508,
        Decr = 0x1E03,
        Incr = 0x1E02,
        IncrWrap = 0x8507,
        Invert = 0x150A,
        Replace = 0x1E01,
        Zero = 0,
    }

    internal enum TextureParameterName
    {
        TextureMaxAnisotropyExt = 0x84FE,
        TextureBaseLevel = 0x813C,
        TextureMaxLevel = 0x813D,
        TextureMinFilter = 0x2801,
        TextureMagFilter = 0x2800,
        TextureWrapS = 0x2802,
        TextureWrapT = 0x2803,
        TextureBorderColor = 0x1004,
        TextureLodBias = 0x8501,
        TextureCompareMode = 0x884C,
        TextureCompareFunc = 0x884D,
        GenerateMipmap = 0x8191,
    }

    internal enum Bool
    {
        True = 1,
        False = 0,
    }

    internal enum TextureMinFilter
    {
        LinearMipmapNearest = 0x2701,
        NearestMipmapLinear = 0x2702,
        LinearMipmapLinear = 0x2703,
        Linear = 0x2601,
        NearestMipmapNearest = 0x2700,
        Nearest = 0x2600,
    }

    internal enum TextureMagFilter
    {
        Linear  = 0x2601,
        Nearest = 0x2600,
    }

    internal enum TextureCompareMode
    {
        CompareRefToTexture = 0x884E,
        None = 0,
    }

    internal enum TextureWrapMode
    {
        ClampToEdge = 0x812F,
        Repeat = 0x2901,
        MirroredRepeat = 0x8370,
        //GLES
        ClampToBorder = 0x812D,
    }

    internal struct ColorFormat
    {
        internal readonly int R;
        internal readonly int G;
        internal readonly int B;
        internal readonly int A;

        internal ColorFormat(int r, int g, int b, int a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }

    internal abstract class OGL
    {
        internal enum RenderApi
        {
            ES = 12448,
            GL = 12450,
        }

        internal RenderApi BoundApi = RenderApi.GL;
        private const CallingConvention callingConvention = CallingConvention.Winapi;

        protected static OGL _current;

        public static OGL Current { get { return _current; } }

        protected OGL()
        {
            GetNativeLibrary();
            LoadEntryPoints();
        }

        protected abstract IntPtr GetNativeLibrary();
        protected abstract T LoadFunction<T>(string function);
        protected abstract T LoadFunctionOrNull<T>(string function);
        

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void EnableVertexAttribArrayDelegate(int attrib);
        internal EnableVertexAttribArrayDelegate EnableVertexAttribArray;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void DisableVertexAttribArrayDelegate(int attrib);
        internal DisableVertexAttribArrayDelegate DisableVertexAttribArray;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void MakeCurrentDelegate(IntPtr window);
        internal MakeCurrentDelegate MakeCurrent;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void GetIntegerDelegate(int param, [Out] int* data);
        internal GetIntegerDelegate GetIntegerv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate IntPtr GetStringDelegate(StringName param);
        internal GetStringDelegate GetStringInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void ClearDepthDelegate(float depth);
        internal ClearDepthDelegate ClearDepth;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void DepthRangedDelegate(double min, double max);
        internal DepthRangedDelegate DepthRanged;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void DepthRangefDelegate(float min, float max);
        internal DepthRangefDelegate DepthRangef;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void ClearDelegate(ClearBufferMask mask);
        internal ClearDelegate Clear;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void ClearColorDelegate(float red, float green, float blue, float alpha);
        internal ClearColorDelegate ClearColor;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void ClearStencilDelegate(int stencil);
        internal ClearStencilDelegate ClearStencil;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void ViewportDelegate(int x, int y, int w, int h);
        internal ViewportDelegate Viewport;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate ErrorCode GetErrorDelegate();
        internal GetErrorDelegate GetError;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void FlushDelegate();
        internal FlushDelegate Flush;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void GenTexturesDelegate(int count, int* ptextures);
        internal GenTexturesDelegate GenTextures;

        internal unsafe int GenTexture()
        {
            int texture;
            this.GenTextures(1, &texture);
            return texture;
        }

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void BindTextureDelegate(TextureTarget target, int id);
        internal BindTextureDelegate BindTexture;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate int EnableDelegate(EnableCap cap);
        internal EnableDelegate Enable;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate int DisableDelegate(EnableCap cap);
        internal DisableDelegate Disable;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void CullFaceDelegate(CullFaceMode mode);
        internal CullFaceDelegate CullFace;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void FrontFaceDelegate(FrontFaceDirection direction);
        internal FrontFaceDelegate FrontFace;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void PolygonModeDelegate(MaterialFace face, PolygonMode mode);
        internal PolygonModeDelegate PolygonMode;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void PolygonOffsetDelegate(float slopeScaleDepthBias, float depthbias);
        internal PolygonOffsetDelegate PolygonOffset;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void DrawBuffersDelegate(int count, DrawBuffersEnum[] buffers);
        internal DrawBuffersDelegate DrawBuffers;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void UseProgramDelegate(int program);
        internal UseProgramDelegate UseProgram;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void Uniform1iDelegate(int location, int value);
        internal Uniform1iDelegate Uniform1i;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void Uniform1fDelegate(int location, float value);
        internal Uniform1fDelegate Uniform1f;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void Uniform1ivDelegate(int location, int count, int* values);
        internal Uniform1ivDelegate Uniform1iv;
        
        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void Uniform1fvDelegate(int location, int count, float* values);
        internal Uniform1fvDelegate Uniform1fv;
        
        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void Uniform2fvDelegate(int location, int count, Vector2* values);
        internal Uniform2fvDelegate Uniform2fv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void Uniform3fvDelegate(int location, int count, Vector3* values);
        internal Uniform3fvDelegate Uniform3fv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void Uniform4fvDelegate(int location, int count, Vector4* values);
        internal Uniform4fvDelegate Uniform4fv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void UniformMatrix2fvDelegate(int location, int count, bool transpose, float* values);
        internal UniformMatrix2fvDelegate UniformMatrix2fv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void UniformMatrix3fvDelegate(int location, int count, bool transpose, float* values);
        internal UniformMatrix3fvDelegate UniformMatrix3fv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void UniformMatrix4fvDelegate(int location, int count, bool transpose, Matrix* values);
        internal UniformMatrix4fvDelegate UniformMatrix4fv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void ScissorDelegate(int x, int y, int width, int height);
        internal ScissorDelegate Scissor;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void ReadPixelsDelegate(int x, int y, int width, int height, PixelFormat format, PixelType type, IntPtr data);
        internal ReadPixelsDelegate ReadPixelsInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void BindBufferDelegate(BufferTarget target, int buffer);
        internal BindBufferDelegate BindBuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void DrawElementsDelegate(GLPrimitiveType primitiveType, int count, DrawElementsType elementType, IntPtr offset);
        internal DrawElementsDelegate DrawElements;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void DrawElementsBaseVertexDelegate(GLPrimitiveType primitiveType, int count, DrawElementsType elementType, IntPtr offset, int baseVertex);
        internal DrawElementsBaseVertexDelegate DrawElementsBaseVertex; // OpenGL 3.2, GLES 3.2, or GL_ARB_draw_elements_base_vertex

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void DrawArraysDelegate(GLPrimitiveType primitiveType, int offset, int count);
        internal DrawArraysDelegate DrawArrays;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void GenRenderbuffersDelegate(int count, int* prenderBuffers);
        internal GenRenderbuffersDelegate GenRenderbuffers;

        internal unsafe int GenRenderbuffer()
        {
            int renderBuffer;
            this.GenRenderbuffers(1, &renderBuffer);
            return renderBuffer;
        }

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void BindRenderbufferDelegate(RenderbufferTarget target, int buffer);
        internal BindRenderbufferDelegate BindRenderbuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void DeleteRenderbuffersDelegate(int count, int* pbuffers);
        internal DeleteRenderbuffersDelegate DeleteRenderbuffers;

        internal unsafe void DeleteRenderbuffer(int renderBuffer)
        {
            DeleteRenderbuffers(1, &renderBuffer);
        }

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void RenderbufferStorageMultisampleDelegate(RenderbufferTarget target, int sampleCount,
            RenderbufferStorage storage, int width, int height);
        internal RenderbufferStorageMultisampleDelegate RenderbufferStorageMultisample;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void GenFramebuffersDelegate(int count, int* pframeBuffers);
        internal GenFramebuffersDelegate GenFramebuffers;

        internal unsafe int GenFramebuffer()
        {
            int frameBuffer;
            this.GenFramebuffers(1, &frameBuffer);
            return frameBuffer;
        }

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void BindFramebufferDelegate(FramebufferTarget target, int buffer);
        internal BindFramebufferDelegate BindFramebuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void DeleteFramebuffersDelegate(int count, int* pbuffers);
        internal DeleteFramebuffersDelegate DeleteFramebuffers;

        internal unsafe void DeleteFramebuffer(int frameBuffer)
        {
            DeleteFramebuffers(1, &frameBuffer);
        }

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        public delegate void InvalidateFramebufferDelegate(FramebufferTarget target, int numAttachments, FramebufferAttachment[] attachments);
        public InvalidateFramebufferDelegate InvalidateFramebuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void FramebufferTexture2DDelegate(FramebufferTarget target, FramebufferAttachment attachement,
            TextureTarget textureTarget, int texture, int level);
        internal FramebufferTexture2DDelegate FramebufferTexture2D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void FramebufferTexture2DMultiSampleDelegate(FramebufferTarget target, FramebufferAttachment attachement,
            TextureTarget textureTarget, int texture, int level, int samples);
        internal FramebufferTexture2DMultiSampleDelegate FramebufferTexture2DMultiSample;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void FramebufferRenderbufferDelegate(FramebufferTarget target, FramebufferAttachment attachement,
            RenderbufferTarget renderBufferTarget, int buffer);
        internal FramebufferRenderbufferDelegate FramebufferRenderbuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        public delegate void RenderbufferStorageDelegate(RenderbufferTarget target, RenderbufferStorage storage, int width, int hegiht);
        public RenderbufferStorageDelegate RenderbufferStorage;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void GenerateMipmapDelegate(TextureTarget target);
        internal GenerateMipmapDelegate GenerateMipmap;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void ReadBufferDelegate(ReadBufferMode buffer);
        internal ReadBufferDelegate ReadBuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void DrawBufferDelegate(DrawBufferMode buffer);
        internal DrawBufferDelegate DrawBuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void BlitFramebufferDelegate(int srcX0,
            int srcY0,
            int srcX1,
            int srcY1,
            int dstX0,
            int dstY0,
            int dstX1,
            int dstY1,
            ClearBufferMask mask,
            BlitFramebufferFilter filter);
        internal BlitFramebufferDelegate BlitFramebuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate FramebufferErrorCode CheckFramebufferStatusDelegate(FramebufferTarget target);
        internal CheckFramebufferStatusDelegate CheckFramebufferStatus;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void TexParameterFloatDelegate(TextureTarget target, TextureParameterName name, float value);
        internal TexParameterFloatDelegate TexParameterf;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void TexParameterFloatArrayDelegate(TextureTarget target, TextureParameterName name, float* values);
        internal TexParameterFloatArrayDelegate TexParameterfv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void TexParameterIntDelegate(TextureTarget target, TextureParameterName name, int value);
        internal TexParameterIntDelegate TexParameteri;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void GenQueriesDelegate(int count, int* pqueries);
        internal GenQueriesDelegate GenQueries;

        internal unsafe int GenQuery()
        {
            int query;
            this.GenQueries(1, &query);
            return query;
        }

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void BeginQueryDelegate(QueryTarget target, int queryId);
        internal BeginQueryDelegate BeginQuery;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void EndQueryDelegate(QueryTarget target);
        internal EndQueryDelegate EndQuery;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void GetQueryObjectDelegate(int queryId, GetQueryObjectParam getparam, [Out] out int ready);
        internal GetQueryObjectDelegate GetQueryObject;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void DeleteQueriesDelegate(int count, int* pqueries);
        internal DeleteQueriesDelegate DeleteQueries;

        internal unsafe void DeleteQuery(int query)
        {
            DeleteQueries(1, &query);
        }

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void ActiveTextureDelegate(TextureUnit textureUnit);
        internal ActiveTextureDelegate ActiveTexture;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate int CreateShaderDelegate(ShaderType type);
        internal CreateShaderDelegate CreateShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void ShaderSourceDelegate(int shaderId, int count, IntPtr code, int* length);
        internal ShaderSourceDelegate ShaderSourceInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void CompileShaderDelegate(int shaderId);
        internal CompileShaderDelegate CompileShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void GetShaderDelegate(int shaderId, int parameter, int* value);
        internal GetShaderDelegate GetShaderiv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void GetShaderInfoLogDelegate(int shader, int bufSize, IntPtr length, StringBuilder infoLog);
        internal GetShaderInfoLogDelegate GetShaderInfoLogInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate bool IsShaderDelegate(int shaderId);
        internal IsShaderDelegate IsShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void DeleteShaderDelegate(int shaderId);
        internal DeleteShaderDelegate DeleteShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate int GetAttribLocationDelegate(int programId, string name);
        internal GetAttribLocationDelegate GetAttribLocation;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate int GetUniformLocationDelegate(int programId, string name);
        internal GetUniformLocationDelegate GetUniformLocation;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate bool IsProgramDelegate(int programId);
        internal IsProgramDelegate IsProgram;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void DeleteProgramDelegate(int programId);
        internal DeleteProgramDelegate DeleteProgram;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate int CreateProgramDelegate();
        internal CreateProgramDelegate CreateProgram;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void AttachShaderDelegate(int programId, int shaderId);
        internal AttachShaderDelegate AttachShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void LinkProgramDelegate(int programId);
        internal LinkProgramDelegate LinkProgram;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void GetProgramDelegate(int programId, int name, int* linked);
        internal GetProgramDelegate GetProgramiv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void GetProgramInfoLogDelegate(int program, int bufSize, IntPtr length, StringBuilder infoLog);
        internal GetProgramInfoLogDelegate GetProgramInfoLogInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void DetachShaderDelegate(int programId, int shaderId);
        internal DetachShaderDelegate DetachShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void BlendColorDelegate(float r, float g, float b, float a);
        internal BlendColorDelegate BlendColor;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void BlendEquationSeparateDelegate(BlendEquationMode colorMode, BlendEquationMode alphaMode);
        internal BlendEquationSeparateDelegate BlendEquationSeparate;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void BlendEquationSeparateiDelegate(int buffer, BlendEquationMode colorMode, BlendEquationMode alphaMode);
        internal BlendEquationSeparateiDelegate BlendEquationSeparatei;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void BlendFuncSeparateDelegate(BlendingFunc colorSrc, BlendingFunc colorDst,
            BlendingFunc alphaSrc, BlendingFunc alphaDst);
        internal BlendFuncSeparateDelegate BlendFuncSeparate;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void BlendFuncSeparateiDelegate(int buffer, BlendingFunc colorSrc, BlendingFunc colorDst,
            BlendingFunc alphaSrc, BlendingFunc alphaDst);
        internal BlendFuncSeparateiDelegate BlendFuncSeparatei;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void ColorMaskDelegate(bool r, bool g, bool b, bool a);
        internal ColorMaskDelegate ColorMask;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void DepthFuncDelegate(ComparisonFunc function);
        internal DepthFuncDelegate DepthFunc;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void DepthMaskDelegate(bool enabled);
        internal DepthMaskDelegate DepthMask;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void StencilFuncSeparateDelegate(StencilFace face, ComparisonFunc function, int referenceStencil, int mask);
        internal StencilFuncSeparateDelegate StencilFuncSeparate;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void StencilOpSeparateDelegate(StencilFace face, StencilOp stencilfail, StencilOp depthFail, StencilOp pass);
        internal StencilOpSeparateDelegate StencilOpSeparate;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void StencilFuncDelegate(ComparisonFunc function, int referenceStencil, int mask);
        internal StencilFuncDelegate StencilFunc;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void StencilOpDelegate(StencilOp stencilfail, StencilOp depthFail, StencilOp pass);
        internal StencilOpDelegate StencilOp;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void StencilMaskDelegate(int mask);
        internal StencilMaskDelegate StencilMask;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void CompressedTexImage2DDelegate(TextureTarget target, int level, PixelInternalFormat internalFormat,
            int width, int height, int border, int size, IntPtr data);
        internal CompressedTexImage2DDelegate CompressedTexImage2D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void TexImage2DDelegate(TextureTarget target, int level, PixelInternalFormat internalFormat,
            int width, int height, int border, PixelFormat format, PixelType pixelType, IntPtr data);
        internal TexImage2DDelegate TexImage2D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void CompressedTexSubImage2DDelegate(TextureTarget target, int level,
            int x, int y, int width, int height, PixelInternalFormat format, int size, IntPtr data);
        internal CompressedTexSubImage2DDelegate CompressedTexSubImage2D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void TexSubImage2DDelegate(TextureTarget target, int level,
            int x, int y, int width, int height, PixelFormat format, PixelType pixelType, IntPtr data);
        internal TexSubImage2DDelegate TexSubImage2D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void PixelStoreDelegate(PixelStoreParameter parameter, int size);
        internal PixelStoreDelegate PixelStore;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void FinishDelegate();
        internal FinishDelegate Finish;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void GetTexImageDelegate(TextureTarget target, int level, PixelFormat format, PixelType type, [Out] IntPtr pixels);
        internal GetTexImageDelegate GetTexImageInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void GetCompressedTexImageDelegate(TextureTarget target, int level, [Out] IntPtr pixels);
        internal GetCompressedTexImageDelegate GetCompressedTexImageInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void TexImage3DDelegate(TextureTarget target, int level, PixelInternalFormat internalFormat,
            int width, int height, int depth, int border, PixelFormat format, PixelType pixelType, IntPtr data);
        internal TexImage3DDelegate TexImage3D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void TexSubImage3DDelegate(TextureTarget target, int level,
            int x, int y, int z, int width, int height, int depth, PixelFormat format, PixelType pixelType, IntPtr data);
        internal TexSubImage3DDelegate TexSubImage3D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void DeleteTexturesDelegate(int count, int* ptextures);
        internal DeleteTexturesDelegate DeleteTextures;

        internal unsafe void DeleteTexture(int texture)
        {
            DeleteTextures(1, &texture);
        }

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void GenBuffersDelegate(int count, int* pbuffers);
        internal GenBuffersDelegate GenBuffers;

        internal unsafe int GenBuffer()
        {
            int buffer;
            this.GenBuffers(1, &buffer);
            return buffer;
        }

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void BufferDataDelegate(BufferTarget target, IntPtr size, IntPtr n, BufferUsageHint usage);
        internal BufferDataDelegate BufferData;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate IntPtr MapBufferDelegate(BufferTarget target, BufferAccess access);
        internal MapBufferDelegate MapBuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void UnmapBufferDelegate(BufferTarget target);
        internal UnmapBufferDelegate UnmapBuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void BufferSubDataDelegate(BufferTarget target, IntPtr offset, IntPtr size, IntPtr data);
        internal BufferSubDataDelegate BufferSubData;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal unsafe delegate void DeleteBuffersDelegate(int count, int* pbuffers);
        internal DeleteBuffersDelegate DeleteBuffers;

        internal unsafe void DeleteBuffer(int buffer)
        {
            DeleteBuffers(1, &buffer);

        }

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void VertexAttribPointerDelegate(int location, int elementCount, VertexAttribPointerType type, bool normalize,
            int stride, IntPtr data);
        internal VertexAttribPointerDelegate VertexAttribPointer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void DrawElementsInstancedDelegate(GLPrimitiveType primitiveType, int count, DrawElementsType elementType,
            IntPtr offset, int instanceCount);
        internal DrawElementsInstancedDelegate DrawElementsInstanced;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void DrawElementsInstancedBaseVertexDelegate(GLPrimitiveType primitiveType, int count, DrawElementsType elementType,
            IntPtr offset, int instanceCount, int baseVertex);
        internal DrawElementsInstancedBaseVertexDelegate DrawElementsInstancedBaseVertex; // OpenGL 3.2, GLES 3.2.

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void DrawElementsInstancedBaseInstanceDelegate(GLPrimitiveType primitiveType, int count, DrawElementsType elementType,
            IntPtr offset, int instanceCount, int baseInstance);
        internal DrawElementsInstancedBaseInstanceDelegate DrawElementsInstancedBaseInstance;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [UnmanagedFunctionPointer(callingConvention)]
        [MonoNativeFunctionWrapper]
        internal delegate void VertexAttribDivisorDelegate(int location, int frequency);
        internal VertexAttribDivisorDelegate VertexAttribDivisor;

#if DEBUG
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void DebugMessageCallbackProc(int source, int type, int id, int severity, int length, IntPtr message, IntPtr userParam);
        static DebugMessageCallbackProc DebugProc;
        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        delegate void DebugMessageCallbackDelegate(DebugMessageCallbackProc callback, IntPtr userParam);
        static DebugMessageCallbackDelegate DebugMessageCallback;

        internal delegate void ErrorDelegate(string message);
        internal static event ErrorDelegate OnError;

        static void DebugMessageCallbackHandler(int source, int type, int id, int severity, int length, IntPtr message, IntPtr userParam)
        {
            string errorMessage = Marshal.PtrToStringAnsi(message);
            System.Diagnostics.Debug.WriteLine(errorMessage);
            if (OnError != null)
                OnError(errorMessage);
        }
#endif

        internal int SwapInterval { get; set; }

        private void LoadEntryPoints()
        {

            if (Viewport == null)
                Viewport = LoadFunctionOrNull<ViewportDelegate>("glViewport");
            if (Scissor == null)
                Scissor = LoadFunctionOrNull<ScissorDelegate>("glScissor");
            if (MakeCurrent == null)
                MakeCurrent = LoadFunctionOrNull<MakeCurrentDelegate>("glMakeCurrent");

            GetError = LoadFunctionOrNull<GetErrorDelegate>("glGetError");

            TexParameterf = LoadFunctionOrNull<TexParameterFloatDelegate>("glTexParameterf");
            TexParameterfv = LoadFunctionOrNull<TexParameterFloatArrayDelegate>("glTexParameterfv");
            TexParameteri = LoadFunctionOrNull<TexParameterIntDelegate>("glTexParameteri");

            EnableVertexAttribArray = LoadFunctionOrNull<EnableVertexAttribArrayDelegate>("glEnableVertexAttribArray");
            DisableVertexAttribArray = LoadFunctionOrNull<DisableVertexAttribArrayDelegate>("glDisableVertexAttribArray");
            GetIntegerv = LoadFunctionOrNull<GetIntegerDelegate>("glGetIntegerv");
            GetStringInternal = LoadFunctionOrNull<GetStringDelegate>("glGetString");
            ClearDepth = LoadFunctionOrNull<ClearDepthDelegate>("glClearDepth");
            if (ClearDepth == null)
                ClearDepth = LoadFunctionOrNull<ClearDepthDelegate>("glClearDepthf");
            DepthRanged = LoadFunctionOrNull<DepthRangedDelegate>("glDepthRange");
            DepthRangef = LoadFunctionOrNull<DepthRangefDelegate>("glDepthRangef");
            Clear = LoadFunctionOrNull<ClearDelegate>("glClear");
            ClearColor = LoadFunctionOrNull<ClearColorDelegate>("glClearColor");
            ClearStencil = LoadFunctionOrNull<ClearStencilDelegate>("glClearStencil");
            Flush = LoadFunctionOrNull<FlushDelegate>("glFlush");
            GenTextures = LoadFunctionOrNull<GenTexturesDelegate>("glGenTextures");
            BindTexture = LoadFunctionOrNull<BindTextureDelegate>("glBindTexture");

            Enable = LoadFunctionOrNull<EnableDelegate>("glEnable");
            Disable = LoadFunctionOrNull<DisableDelegate>("glDisable");
            CullFace = LoadFunctionOrNull<CullFaceDelegate>("glCullFace");
            FrontFace = LoadFunctionOrNull<FrontFaceDelegate>("glFrontFace");
            PolygonMode = LoadFunctionOrNull<PolygonModeDelegate>("glPolygonMode");
            PolygonOffset = LoadFunctionOrNull<PolygonOffsetDelegate>("glPolygonOffset");

            BindBuffer = LoadFunctionOrNull<BindBufferDelegate>("glBindBuffer");
            DrawBuffers = LoadFunctionOrNull<DrawBuffersDelegate>("glDrawBuffers");
            DrawElements = LoadFunctionOrNull<DrawElementsDelegate>("glDrawElements");
            DrawArrays = LoadFunctionOrNull<DrawArraysDelegate>("glDrawArrays");


            // uniforms OpenGL Version >= 2.0
            Uniform1i = LoadFunctionOrNull<Uniform1iDelegate>("glUniform1i");
            Uniform1f = LoadFunctionOrNull<Uniform1fDelegate>("glUniform1f");
            Uniform1iv = LoadFunctionOrNull<Uniform1ivDelegate>("glUniform1iv");
            Uniform1fv = LoadFunctionOrNull<Uniform1fvDelegate>("glUniform1fv");
            Uniform2fv = LoadFunctionOrNull<Uniform2fvDelegate>("glUniform2fv");
            Uniform3fv = LoadFunctionOrNull<Uniform3fvDelegate>("glUniform3fv");
            Uniform4fv = LoadFunctionOrNull<Uniform4fvDelegate>("glUniform4fv");
            UniformMatrix2fv = LoadFunctionOrNull<UniformMatrix2fvDelegate>("glUniformMatrix2fv");
            UniformMatrix3fv = LoadFunctionOrNull<UniformMatrix3fvDelegate>("glUniformMatrix3fv");
            UniformMatrix4fv = LoadFunctionOrNull<UniformMatrix4fvDelegate>("glUniformMatrix4fv");
            ReadPixelsInternal = LoadFunctionOrNull<ReadPixelsDelegate>("glReadPixels");
            // uniforms OpenGL Version >= 2.1
            // ... UniformMatrix 2x3,2x4, 3x2, 3x4, 4x2, 4x3
            // uniforms OpenGL Version >= 3.0
            // ... Uniform 1ui,1uiv,2ui,2uiv,2ui,2uiv,2ui,2uiv

            ReadBuffer = LoadFunctionOrNull<ReadBufferDelegate>("glReadBuffer");
            DrawBuffer = LoadFunctionOrNull<DrawBufferDelegate>("glDrawBuffer");

            // Render Target Support. These might be null if they are not supported
            GenRenderbuffers = LoadFunctionOrNull<GenRenderbuffersDelegate>("glGenRenderbuffers");
            BindRenderbuffer = LoadFunctionOrNull<BindRenderbufferDelegate>("glBindRenderbuffer");
            DeleteRenderbuffers = LoadFunctionOrNull<DeleteRenderbuffersDelegate>("glDeleteRenderbuffers");
            GenFramebuffers = LoadFunctionOrNull<GenFramebuffersDelegate>("glGenFramebuffers");
            BindFramebuffer = LoadFunctionOrNull<BindFramebufferDelegate>("glBindFramebuffer");
            DeleteFramebuffers = LoadFunctionOrNull<DeleteFramebuffersDelegate>("glDeleteFramebuffers");
            FramebufferTexture2D = LoadFunctionOrNull<FramebufferTexture2DDelegate>("glFramebufferTexture2D");
            FramebufferRenderbuffer = LoadFunctionOrNull<FramebufferRenderbufferDelegate>("glFramebufferRenderbuffer");
            RenderbufferStorage = LoadFunctionOrNull<RenderbufferStorageDelegate>("glRenderbufferStorage");
            RenderbufferStorageMultisample = LoadFunctionOrNull<RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisample");
            GenerateMipmap = LoadFunctionOrNull<GenerateMipmapDelegate>("glGenerateMipmap");
            BlitFramebuffer = LoadFunctionOrNull<BlitFramebufferDelegate>("glBlitFramebuffer");
            CheckFramebufferStatus = LoadFunctionOrNull<CheckFramebufferStatusDelegate>("glCheckFramebufferStatus");

            GenQueries = LoadFunctionOrNull<GenQueriesDelegate>("glGenQueries");
            BeginQuery = LoadFunctionOrNull<BeginQueryDelegate>("glBeginQuery");
            EndQuery = LoadFunctionOrNull<EndQueryDelegate>("glEndQuery");
            GetQueryObject = LoadFunctionOrNull<GetQueryObjectDelegate>("glGetQueryObjectuiv");
            if (GetQueryObject == null)
                GetQueryObject = LoadFunctionOrNull<GetQueryObjectDelegate>("glGetQueryObjectivARB");
            if (GetQueryObject == null)
                GetQueryObject = LoadFunctionOrNull<GetQueryObjectDelegate>("glGetQueryObjectiv");
            DeleteQueries = LoadFunctionOrNull<DeleteQueriesDelegate>("glDeleteQueries");

            ActiveTexture = LoadFunctionOrNull<ActiveTextureDelegate>("glActiveTexture");
            CreateShader = LoadFunctionOrNull<CreateShaderDelegate>("glCreateShader");
            ShaderSourceInternal = LoadFunctionOrNull<ShaderSourceDelegate>("glShaderSource");
            CompileShader = LoadFunctionOrNull<CompileShaderDelegate>("glCompileShader");
            GetShaderiv = LoadFunctionOrNull<GetShaderDelegate>("glGetShaderiv");
            GetShaderInfoLogInternal = LoadFunctionOrNull<GetShaderInfoLogDelegate>("glGetShaderInfoLog");
            IsShader = LoadFunctionOrNull<IsShaderDelegate>("glIsShader");
            DeleteShader = LoadFunctionOrNull<DeleteShaderDelegate>("glDeleteShader");
            GetAttribLocation = LoadFunctionOrNull<GetAttribLocationDelegate>("glGetAttribLocation");
            GetUniformLocation = LoadFunctionOrNull<GetUniformLocationDelegate>("glGetUniformLocation");

            IsProgram = LoadFunctionOrNull<IsProgramDelegate>("glIsProgram");
            DeleteProgram = LoadFunctionOrNull<DeleteProgramDelegate>("glDeleteProgram");
            CreateProgram = LoadFunctionOrNull<CreateProgramDelegate>("glCreateProgram");
            AttachShader = LoadFunctionOrNull<AttachShaderDelegate>("glAttachShader");
            UseProgram = LoadFunctionOrNull<UseProgramDelegate>("glUseProgram");
            LinkProgram = LoadFunctionOrNull<LinkProgramDelegate>("glLinkProgram");
            GetProgramiv = LoadFunctionOrNull<GetProgramDelegate>("glGetProgramiv");
            GetProgramInfoLogInternal = LoadFunctionOrNull<GetProgramInfoLogDelegate>("glGetProgramInfoLog");
            DetachShader = LoadFunctionOrNull<DetachShaderDelegate>("glDetachShader");

            BlendColor = LoadFunctionOrNull<BlendColorDelegate>("glBlendColor");
            BlendEquationSeparate = LoadFunctionOrNull<BlendEquationSeparateDelegate>("glBlendEquationSeparate");
            BlendEquationSeparatei = LoadFunctionOrNull<BlendEquationSeparateiDelegate>("glBlendEquationSeparatei");
            BlendFuncSeparate = LoadFunctionOrNull<BlendFuncSeparateDelegate>("glBlendFuncSeparate");
            BlendFuncSeparatei = LoadFunctionOrNull<BlendFuncSeparateiDelegate>("glBlendFuncSeparatei");
            ColorMask = LoadFunctionOrNull<ColorMaskDelegate>("glColorMask");
            DepthFunc = LoadFunctionOrNull<DepthFuncDelegate>("glDepthFunc");
            DepthMask = LoadFunctionOrNull<DepthMaskDelegate>("glDepthMask");
            StencilFuncSeparate = LoadFunctionOrNull<StencilFuncSeparateDelegate>("glStencilFuncSeparate");
            StencilOpSeparate = LoadFunctionOrNull<StencilOpSeparateDelegate>("glStencilOpSeparate");
            StencilFunc = LoadFunctionOrNull<StencilFuncDelegate>("glStencilFunc");
            StencilOp = LoadFunctionOrNull<StencilOpDelegate>("glStencilOp");
            StencilMask = LoadFunctionOrNull<StencilMaskDelegate>("glStencilMask");

            CompressedTexImage2D = LoadFunctionOrNull<CompressedTexImage2DDelegate>("glCompressedTexImage2D");
            TexImage2D = LoadFunctionOrNull<TexImage2DDelegate>("glTexImage2D");
            CompressedTexSubImage2D = LoadFunctionOrNull<CompressedTexSubImage2DDelegate>("glCompressedTexSubImage2D");
            TexSubImage2D = LoadFunctionOrNull<TexSubImage2DDelegate>("glTexSubImage2D");
            PixelStore = LoadFunctionOrNull<PixelStoreDelegate>("glPixelStorei");
            Finish = LoadFunctionOrNull<FinishDelegate>("glFinish");
            GetTexImageInternal = LoadFunctionOrNull<GetTexImageDelegate>("glGetTexImage");
            GetCompressedTexImageInternal = LoadFunctionOrNull<GetCompressedTexImageDelegate>("glGetCompressedTexImage");
            TexImage3D = LoadFunctionOrNull<TexImage3DDelegate>("glTexImage3D");
            TexSubImage3D = LoadFunctionOrNull<TexSubImage3DDelegate>("glTexSubImage3D");
            DeleteTextures = LoadFunctionOrNull<DeleteTexturesDelegate>("glDeleteTextures");

            GenBuffers = LoadFunctionOrNull<GenBuffersDelegate>("glGenBuffers");
            BufferData = LoadFunctionOrNull<BufferDataDelegate>("glBufferData");
            MapBuffer = LoadFunctionOrNull<MapBufferDelegate>("glMapBuffer");
            UnmapBuffer = LoadFunctionOrNull<UnmapBufferDelegate>("glUnmapBuffer");
            BufferSubData = LoadFunctionOrNull<BufferSubDataDelegate>("glBufferSubData");
            DeleteBuffers = LoadFunctionOrNull<DeleteBuffersDelegate>("glDeleteBuffers");

            VertexAttribPointer = LoadFunctionOrNull<VertexAttribPointerDelegate>("glVertexAttribPointer");

            // Instanced drawing requires GL 3.2 or up, if the either of the following entry points can not be loaded
            // this will get flagged by setting SupportsInstancing in GraphicsCapabilities to false.
            try
            {
                DrawElementsInstanced = LoadFunctionOrNull<DrawElementsInstancedDelegate>("glDrawElementsInstanced");
                VertexAttribDivisor = LoadFunctionOrNull<VertexAttribDivisorDelegate>("glVertexAttribDivisor");
                DrawElementsInstancedBaseInstance = LoadFunctionOrNull<DrawElementsInstancedBaseInstanceDelegate>("glDrawElementsInstancedBaseInstance");
            }
            catch (EntryPointNotFoundException)
            {
                // this will be detected in the initialization of GraphicsCapabilities
            }

#if DEBUG
            try
            {
                DebugMessageCallback = LoadFunctionOrNull<DebugMessageCallbackDelegate>("glDebugMessageCallback");
                if (DebugMessageCallback != null)
                {
                    DebugProc = DebugMessageCallbackHandler;
                    DebugMessageCallback(DebugProc, IntPtr.Zero);
                    Enable(EnableCap.DebugOutput);
                    Enable(EnableCap.DebugOutputSynchronous);
                }
            }
            catch (EntryPointNotFoundException)
            {
                // Ignore the debug message callback if the entry point can not be found
            }
#endif

            if (BoundApi == RenderApi.ES)
            {
                InvalidateFramebuffer = LoadFunctionOrNull<InvalidateFramebufferDelegate>("glDiscardFramebufferEXT");
            }

        }

        private List<string> _extensions;
        internal List<string> Extensions { get { return _extensions; } }

        [Conditional("DEBUG")]
        void LogExtensions(string extstring, List<string> extensions)
        {
#if __ANDROID__
            Android.Util.Log.Verbose("GL","Supported Extensions");
            foreach (string ext in extensions)
                Android.Util.Log.Verbose("GL", "   " + ext);
#else
            System.Diagnostics.Debug.WriteLine("Supported GL Extensions");
            System.Diagnostics.Debug.WriteLine("   " + extstring.Replace(" ", "\n   "));
#endif
        }

        internal void InitExtensions()
        {
            if (Extensions != null)
                throw new InvalidOperationException("Extensions allready initialized.");

            string extstring = this.GetString(StringName.Extensions);
            ErrorCode error = this.GetError();
            if (error != ErrorCode.NO_ERROR || string.IsNullOrEmpty(extstring))
                return;

            string[] extList = extstring.Split(' ');
            _extensions = new List<string>(extList.Length);
            _extensions.AddRange(extList);
            LogExtensions(extstring, _extensions);

            if (GenRenderbuffers == null && Extensions.Contains("GL_EXT_framebuffer_object"))
            {
                GenRenderbuffers = LoadFunctionOrNull<GenRenderbuffersDelegate>("glGenRenderbuffersEXT");
                BindRenderbuffer = LoadFunctionOrNull<BindRenderbufferDelegate>("glBindRenderbufferEXT");
                DeleteRenderbuffers = LoadFunctionOrNull<DeleteRenderbuffersDelegate>("glDeleteRenderbuffersEXT");
                GenFramebuffers = LoadFunctionOrNull<GenFramebuffersDelegate>("glGenFramebuffersEXT");
                BindFramebuffer = LoadFunctionOrNull<BindFramebufferDelegate>("glBindFramebufferEXT");
                DeleteFramebuffers = LoadFunctionOrNull<DeleteFramebuffersDelegate>("glDeleteFramebuffersEXT");
                FramebufferTexture2D = LoadFunctionOrNull<FramebufferTexture2DDelegate>("glFramebufferTexture2DEXT");
                FramebufferRenderbuffer = LoadFunctionOrNull<FramebufferRenderbufferDelegate>("glFramebufferRenderbufferEXT");
                RenderbufferStorage = LoadFunctionOrNull<RenderbufferStorageDelegate>("glRenderbufferStorageEXT");
                RenderbufferStorageMultisample = LoadFunctionOrNull<RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisampleEXT");
                GenerateMipmap = LoadFunctionOrNull<GenerateMipmapDelegate>("glGenerateMipmapEXT");
                BlitFramebuffer = LoadFunctionOrNull<BlitFramebufferDelegate>("glBlitFramebufferEXT");
                CheckFramebufferStatus = LoadFunctionOrNull<CheckFramebufferStatusDelegate>("glCheckFramebufferStatusEXT");
            }

            if (RenderbufferStorageMultisample == null)
            {                
                if (Extensions.Contains("GL_APPLE_framebuffer_multisample"))
                {
                    RenderbufferStorageMultisample = LoadFunctionOrNull<OGL.RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisampleAPPLE");
                    BlitFramebuffer = LoadFunctionOrNull<OGL.BlitFramebufferDelegate>("glResolveMultisampleFramebufferAPPLE");
                }
                else if (Extensions.Contains("GL_EXT_multisampled_render_to_texture"))
                {
                    RenderbufferStorageMultisample = LoadFunctionOrNull<OGL.RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisampleEXT");
                    FramebufferTexture2DMultiSample = LoadFunctionOrNull<OGL.FramebufferTexture2DMultiSampleDelegate>("glFramebufferTexture2DMultisampleEXT");
                }
                else if (Extensions.Contains("GL_IMG_multisampled_render_to_texture"))
                {
                    RenderbufferStorageMultisample = LoadFunctionOrNull<OGL.RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisampleIMG");
                    FramebufferTexture2DMultiSample = LoadFunctionOrNull<OGL.FramebufferTexture2DMultiSampleDelegate>("glFramebufferTexture2DMultisampleIMG");
                }
                else if (Extensions.Contains("GL_NV_framebuffer_multisample"))
                {
                    RenderbufferStorageMultisample = LoadFunctionOrNull<OGL.RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisampleNV");
                    BlitFramebuffer = LoadFunctionOrNull<OGL.BlitFramebufferDelegate>("glBlitFramebufferNV");
                }
            }

            if (BlendFuncSeparatei == null && Extensions.Contains("GL_ARB_draw_buffers_blend"))
                BlendFuncSeparatei = LoadFunctionOrNull<OGL.BlendFuncSeparateiDelegate>("BlendFuncSeparateiARB");

            if (BlendEquationSeparatei == null && Extensions.Contains("GL_ARB_draw_buffers_blend"))
                BlendEquationSeparatei = LoadFunctionOrNull<OGL.BlendEquationSeparateiDelegate>("BlendEquationSeparateiARB");

            if (Extensions.Contains("GL_ARB_draw_elements_base_vertex"))
                DrawElementsBaseVertex = LoadFunctionOrNull<DrawElementsBaseVertexDelegate>("glDrawElementsBaseVertex");
        }


        /* Helper Functions */

        internal void DepthRange(float min, float max)
        {
            if (BoundApi == RenderApi.ES)
                DepthRangef(min, max);
            else
                DepthRanged(min, max);
        }

        internal void Uniform1(int location, int value)
        {
            Uniform1i(location, value);
        }
        
        internal unsafe void Uniform4(int location, Vector4 value)
        {
            Uniform4fv(location, 1, &value);
        }

        internal void Uniform1(int location, float value)
        {
            Uniform1f(location, value);
        }

        internal unsafe void Uniform1(int location, int count, int* value)
        {
            Uniform1iv(location, count, value);
        }

        internal unsafe void Uniform1(int location, int count, float* value)
        {
            Uniform1fv(location, count, value);
        }

        internal unsafe void Uniform2(int location, int count, Vector2* value)
        {
            Uniform2fv(location, count, value);
        }

        internal unsafe void Uniform3(int location, int count, Vector3* value)
        {
            Uniform3fv(location, count, value);
        }

        internal unsafe void Uniform4(int location, int count, Vector4* value)
        {
            Uniform4fv(location, count, value);
        }

        internal unsafe void UniformMatrix2x2(int location, int count, bool transpose, float* value)
        {
            UniformMatrix2fv(location, count, transpose, value);
        }

        internal unsafe void UniformMatrix3x3(int location, int count, bool transpose, float* value)
        {
            UniformMatrix3fv(location, count, transpose, value);
        }

        internal unsafe void UniformMatrix4x4(int location, int count, bool transpose, Matrix* value)
        {
            UniformMatrix4fv(location, count, transpose, value);
        }

        internal string GetString(StringName name)
        {
            IntPtr pStr = GetStringInternal(name);
            return Marshal.PtrToStringAnsi(pStr);
        }

        internal string GetProgramInfoLog(int programId)
        {
            int length = 0;
            GetProgram(programId, GetProgramParameterName.LogLength, out length);
            StringBuilder sb = new StringBuilder(length, length);
            GetProgramInfoLogInternal(programId, length, IntPtr.Zero, sb);
            return sb.ToString();
        }

        internal string GetShaderInfoLog(int shaderId)
        {
            int length = 0;
            GetShader(shaderId, ShaderParameter.LogLength, out length);
            StringBuilder sb = new StringBuilder(length, length);
            GetShaderInfoLogInternal(shaderId, length, IntPtr.Zero, sb);
            return sb.ToString();
        }

        internal unsafe void ShaderSource(int shaderId, byte[] shaderBytecode)
        {
            int codeLength = shaderBytecode.Length;
            fixed (void* pData = shaderBytecode)
            {
                void** ppData = &pData;
                ShaderSourceInternal(shaderId, 1, new IntPtr(&pData), &codeLength);
            }
        }

        internal unsafe void GetShader(int shaderId, ShaderParameter name, out int result)
        {
            fixed (int* ptr = &result)
            {
                GetShaderiv(shaderId, (int)name, ptr);
            }
        }

        internal unsafe void GetProgram(int programId, GetProgramParameterName name, out int result)
        {
            fixed (int* ptr = &result)
            {
                GetProgramiv(programId, (int)name, ptr);
            }
        }

        internal unsafe void GetInteger(GetPName name, out int value)
        {
            fixed (int* ptr = &value)
            {
                GetIntegerv((int)name, ptr);
            }
        }

        internal unsafe void GetInteger(int name, out int value)
        {
            fixed (int* ptr = &value)
            {
                GetIntegerv(name, ptr);
            }
        }

        internal void TexParameter(TextureTarget target, TextureParameterName name, float value)
        {
            TexParameterf(target, name, value);
        }

        internal unsafe void TexParameter(TextureTarget target, TextureParameterName name, float[] values)
        {
            fixed (float* ptr = &values[0])
            {
                TexParameterfv(target, name, ptr);
            }
        }

        internal void TexParameter(TextureTarget target, TextureParameterName name, int value)
        {
            TexParameteri(target, name, value);
        }

        internal void GetTexImage<T>(TextureTarget target, int level, PixelFormat format, PixelType type, T[] pixels) where T : struct
        {
            GCHandle pixelsPtr = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                GetTexImageInternal(target, level, format, type, pixelsPtr.AddrOfPinnedObject());
            }
            finally
            {
                pixelsPtr.Free();
            }
        }

        internal void GetCompressedTexImage<T>(TextureTarget target, int level, T[] pixels) where T : struct
        {
            GCHandle pixelsPtr = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                GetCompressedTexImageInternal(target, level, pixelsPtr.AddrOfPinnedObject());
            }
            finally
            {
                pixelsPtr.Free();
            }
        }

        public void ReadPixels<T>(int x, int y, int width, int height, PixelFormat format, PixelType type, T[] data)
        {
            GCHandle dataPtr = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                ReadPixelsInternal(x, y, width, height, format, type, dataPtr.AddrOfPinnedObject());
            }
            finally
            {
                dataPtr.Free();
            }
        }


        [Conditional("DEBUG")]
        [DebuggerHidden]
        public void CheckGLError()
        {
            ErrorCode error = this.GetError();
            if (error == ErrorCode.NO_ERROR)
                return;

            string errorMsg = String.Format("GL_ERROR: {0} (0x{1:X4})", error, (int)error);
            //Console.WriteLine(errorMsg);
            throw new OpenGLException("GL.GetError() returned " + errorMsg);
        }

        [Conditional("DEBUG")]
        public void LogGLError(string location)
        {
            try
            {
                this.CheckGLError();
            }
            catch (OpenGLException ex)
            {
#if ANDROID
                // Todo: Add generic logging interface
                Android.Util.Log.Debug("KNI", "OpenGLException at " + location + " - " + ex.Message);
#else
                Debug.WriteLine("OpenGLException at " + location + " - " + ex.Message);
#endif
            }
        }

    }
}

