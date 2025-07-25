// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
	internal class EffectObject
	{
        public EffectObject()
        {
        }

		public enum RENDERSTATETYPE
        {
		    ZENABLE                   =   7,
		    FILLMODE                  =   8,
		    SHADEMODE                 =   9,
		    ZWRITEENABLE              =  14,
		    ALPHATESTENABLE           =  15,
		    LASTPIXEL                 =  16,
		    SRCBLEND                  =  19,
		    DESTBLEND                 =  20,
		    CULLMODE                  =  22,
		    ZFUNC                     =  23,
		    ALPHAREF                  =  24,
		    ALPHAFUNC                 =  25,
		    DITHERENABLE              =  26,
		    ALPHABLENDENABLE          =  27,
		    FOGENABLE                 =  28,
		    SPECULARENABLE            =  29,
		    FOGCOLOR                  =  34,
		    FOGTABLEMODE              =  35,
		    FOGSTART                  =  36,
		    FOGEND                    =  37,
		    FOGDENSITY                =  38,
		    RANGEFOGENABLE            =  48,
		    STENCILENABLE             =  52,
		    STENCILFAIL               =  53,
		    STENCILZFAIL              =  54,
		    STENCILPASS               =  55,
		    STENCILFUNC               =  56,
		    STENCILREF                =  57,
		    STENCILMASK               =  58,
		    STENCILWRITEMASK          =  59,
		    TEXTUREFACTOR             =  60,
		    WRAP0                     = 128,
		    WRAP1                     = 129,
		    WRAP2                     = 130,
		    WRAP3                     = 131,
		    WRAP4                     = 132,
		    WRAP5                     = 133,
		    WRAP6                     = 134,
		    WRAP7                     = 135,
		    CLIPPING                  = 136,
		    LIGHTING                  = 137,
		    AMBIENT                   = 139,
		    FOGVERTEXMODE             = 140,
		    COLORVERTEX               = 141,
		    LOCALVIEWER               = 142,
		    NORMALIZENORMALS          = 143,
		    DIFFUSEMATERIALSOURCE     = 145,
		    SPECULARMATERIALSOURCE    = 146,
		    AMBIENTMATERIALSOURCE     = 147,
		    EMISSIVEMATERIALSOURCE    = 148,
		    VERTEXBLEND               = 151,
		    CLIPPLANEENABLE           = 152,
		    POINTSIZE                 = 154,
		    POINTSIZE_MIN             = 155,
		    POINTSPRITEENABLE         = 156,
		    POINTSCALEENABLE          = 157,
		    POINTSCALE_A              = 158,
		    POINTSCALE_B              = 159,
		    POINTSCALE_C              = 160,
		    MULTISAMPLEANTIALIAS      = 161,
		    MULTISAMPLEMASK           = 162,
		    PATCHEDGESTYLE            = 163,
		    DEBUGMONITORTOKEN         = 165,
		    POINTSIZE_MAX             = 166,
		    INDEXEDVERTEXBLENDENABLE  = 167,
		    COLORWRITEENABLE          = 168,
		    TWEENFACTOR               = 170,
		    BLENDOP                   = 171,
		    POSITIONDEGREE            = 172,
		    NORMALDEGREE              = 173,
		    SCISSORTESTENABLE         = 174,
		    SLOPESCALEDEPTHBIAS       = 175,
		    ANTIALIASEDLINEENABLE     = 176,
		    MINTESSELLATIONLEVEL      = 178,
		    MAXTESSELLATIONLEVEL      = 179,
		    ADAPTIVETESS_X            = 180,
		    ADAPTIVETESS_Y            = 181,
		    ADAPTIVETESS_Z            = 182,
		    ADAPTIVETESS_W            = 183,
		    ENABLEADAPTIVETESSELLATION= 184,
		    TWOSIDEDSTENCILMODE       = 185,
		    CCW_STENCILFAIL           = 186,
		    CCW_STENCILZFAIL          = 187,
		    CCW_STENCILPASS           = 188,
		    CCW_STENCILFUNC           = 189,
		    COLORWRITEENABLE1         = 190,
		    COLORWRITEENABLE2         = 191,
		    COLORWRITEENABLE3         = 192,
		    BLENDFACTOR               = 193,
		    SRGBWRITEENABLE           = 194,
		    DEPTHBIAS                 = 195,
		    WRAP8                     = 198,
		    WRAP9                     = 199,
		    WRAP10                    = 200,
		    WRAP11                    = 201,
		    WRAP12                    = 202,
		    WRAP13                    = 203,
		    WRAP14                    = 204,
		    WRAP15                    = 205,
		    SEPARATEALPHABLENDENABLE  = 206,
		    SRCBLENDALPHA             = 207,
		    DESTBLENDALPHA            = 208,
		    BLENDOPALPHA              = 209,
		
		    FORCE_DWORD               = 0x7fffffff
		}

		public enum TEXTURESTAGESTATETYPE
        {
		    COLOROP               =  1,
		    COLORARG1             =  2,
		    COLORARG2             =  3,
		    ALPHAOP               =  4,
		    ALPHAARG1             =  5,
		    ALPHAARG2             =  6,
		    BUMPENVMAT00          =  7,
		    BUMPENVMAT01          =  8,
		    BUMPENVMAT10          =  9,
		    BUMPENVMAT11          = 10,
		    TEXCOORDINDEX         = 11,
		    BUMPENVLSCALE         = 22,
		    BUMPENVLOFFSET        = 23,
		    TEXTURETRANSFORMFLAGS = 24,
		    COLORARG0             = 26,
		    ALPHAARG0             = 27,
		    RESULTARG             = 28,
		    CONSTANT              = 32,
		
		    FORCE_DWORD           = 0x7fffffff
		}

		public enum TRANSFORMSTATETYPE
        {
		    VIEW            =  2,
		    PROJECTION      =  3,
		    TEXTURE0        = 16,
		    TEXTURE1        = 17,
		    TEXTURE2        = 18,
		    TEXTURE3        = 19,
		    TEXTURE4        = 20,
		    TEXTURE5        = 21,
		    TEXTURE6        = 22,
		    TEXTURE7        = 23,
			WORLD           = 256,
		    FORCE_DWORD     = 0x7fffffff
		}

		public const int PARAMETER_SHARED = 1;
		public const int PARAMETER_LITERAL = 2;
		public const int PARAMETER_ANNOTATION = 4;

		public enum PARAMETER_CLASS
		{
			SCALAR,
			VECTOR,
			MATRIX_ROWS,
			MATRIX_COLUMNS,
			OBJECT,
			STRUCT,
			FORCE_DWORD = 0x7fffffff,
		}

		public enum PARAMETER_TYPE
		{
			VOID,
			BOOL,
			INT,
			FLOAT,
			STRING,
			TEXTURE,
			TEXTURE1D,
			TEXTURE2D,
			TEXTURE3D,
			TEXTURECUBE,
			SAMPLER,
			SAMPLER1D,
			SAMPLER2D,
			SAMPLER3D,
			SAMPLERCUBE,
			PIXELSHADER,
			VERTEXSHADER,
			COMPUTESHADER,
			PIXELFRAGMENT,
			VERTEXFRAGMENT,
			UNSUPPORTED,
			FORCE_DWORD = 0x7fffffff,
		}

		enum SAMPLERSTATETYPE 
        {
		    ADDRESSU       = 1,
		    ADDRESSV       = 2,
		    ADDRESSW       = 3,
		    BORDERCOLOR    = 4,
		    MAGFILTER      = 5,
		    MINFILTER      = 6,
		    MIPFILTER      = 7,
		    MIPMAPLODBIAS  = 8,
		    MAXMIPLEVEL    = 9,
		    MAXANISOTROPY  = 10,
		    SRGBTEXTURE    = 11,
		    ELEMENTINDEX   = 12,
		    DMAPOFFSET     = 13,
		                                
		    FORCE_DWORD   = 0x7fffffff,
		};

		public enum STATE_CLASS
		{
		    LIGHTENABLE,
		    FVF,
		    LIGHT,
		    MATERIAL,
		    NPATCHMODE,
		    PIXELSHADER,
		    RENDERSTATE,
		    SETSAMPLER,
		    SAMPLERSTATE,
		    TEXTURE,
		    TEXTURESTAGE,
		    TRANSFORM,
		    VERTEXSHADER,
		    SHADERCONST,
		    COMPUTESHADER,
		    UNKNOWN,
		};

		public enum MATERIAL_TYPE
		{
		    DIFFUSE,
		    AMBIENT,
		    SPECULAR,
		    EMISSIVE,
		    POWER,
		};

		public enum LIGHT_TYPE
		{
		    TYPE,
		    DIFFUSE,
		    SPECULAR,
		    AMBIENT,
		    POSITION,
		    DIRECTION,
		    RANGE,
		    FALLOFF,
		    ATTENUATION0,
		    ATTENUATION1,
		    ATTENUATION2,
		    THETA,
		    PHI,
		};

		public enum SHADER_CONSTANT_TYPE
		{
		    VSFLOAT,
		    VSBOOL,
		    VSINT,
		    PSFLOAT,
		    PSBOOL,
		    PSINT,
		}

		public enum STATE_TYPE
		{
			CONSTANT,
			PARAMETER,
			EXPRESSION,
			EXPRESSIONINDEX,
		}

		public class EffectParameterContent
		{
			public string name;
			public string semantic;
			public object data;
			public PARAMETER_CLASS class_;
			public PARAMETER_TYPE  type;
			public uint rows;
			public uint columns;
			public uint element_count;
			public uint annotation_count = 0;
			public uint member_count;
			public uint flags = 0;
			public uint bytes = 0;

            public int bufferIndex = -1;
            public int bufferOffset = -1;

		    public EffectParameterContent[] annotation_handles = null;
			public EffectParameterContent[] member_handles;

            public override string ToString()
            {
                if (rows > 0 || columns > 0)
                    return string.Format("{0} {1}{2}x{3} {4} : cb{5},{6}", class_, type, rows, columns, name, bufferIndex, bufferOffset);
                else
                    return string.Format("{0} {1} {2}", class_, type, name);
            }
		}
		
		public class EffectStateContent
		{
			public uint operation;
			public uint index;
			public STATE_TYPE type;
			public EffectParameterContent parameter;
		}

		public class EffectSamplerContent
		{
		    public uint state_count = 0;
		    public EffectStateContent[] states = null;
		}
		
		public class EffectPassContent
		{
			public string name;
			public uint state_count;
		    public uint annotation_count = 0;

			public BlendState blendState;
			public DepthStencilState depthStencilState;
			public RasterizerState rasterizerState;

			public EffectStateContent[] states;
		    public EffectParameterContent[] annotation_handles = null;
		}

		public class EffectTechniqueContent
		{
			public string name;
			public uint pass_count;
		    public uint annotation_count = 0;

		    public EffectParameterContent[] annotation_handles = null;
			public EffectPassContent[] pass_handles;
		}

        public class state_info
		{
            public STATE_CLASS class_ { get; private set; }
            public uint op { get; private set; }
            public string name { get; private set; }

			public state_info(STATE_CLASS class_, uint op, string name) 
            {
				this.class_ = class_;
				this.op = op;
				this.name = name;
			}
		}

        /// <summary>
        /// The shared state definition table.
        /// </summary>
		public static readonly state_info[] state_table =
		{
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.ZENABLE, "ZENABLE"), /* 0x0 */
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.FILLMODE, "FILLMODE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.SHADEMODE, "SHADEMODE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.ZWRITEENABLE, "ZWRITEENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.ALPHATESTENABLE, "ALPHATESTENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.LASTPIXEL, "LASTPIXEL"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.SRCBLEND, "SRCBLEND"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.DESTBLEND, "DESTBLEND"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.CULLMODE, "CULLMODE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.ZFUNC, "ZFUNC"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.ALPHAREF, "ALPHAREF"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.ALPHAFUNC, "ALPHAFUNC"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.DITHERENABLE, "DITHERENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.ALPHABLENDENABLE, "ALPHABLENDENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.FOGENABLE, "FOGENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.SPECULARENABLE, "SPECULARENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.FOGCOLOR, "FOGCOLOR"), /* 0x10 */
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.FOGTABLEMODE, "FOGTABLEMODE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.FOGSTART, "FOGSTART"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.FOGEND, "FOGEND"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.FOGDENSITY, "FOGDENSITY"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.RANGEFOGENABLE, "RANGEFOGENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.STENCILENABLE, "STENCILENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.STENCILFAIL, "STENCILFAIL"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.STENCILZFAIL, "STENCILZFAIL"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.STENCILPASS, "STENCILPASS"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.STENCILFUNC, "STENCILFUNC"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.STENCILREF, "STENCILREF"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.STENCILMASK, "STENCILMASK"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.STENCILWRITEMASK, "STENCILWRITEMASK"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.TEXTUREFACTOR, "TEXTUREFACTOR"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.WRAP0, "WRAP0"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.WRAP1, "WRAP1"), /* 0x20 */
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.WRAP2, "WRAP2"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.WRAP3, "WRAP3"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.WRAP4, "WRAP4"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.WRAP5, "WRAP5"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.WRAP6, "WRAP6"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.WRAP7, "WRAP7"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.WRAP8, "WRAP8"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.WRAP9, "WRAP9"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.WRAP10, "WRAP10"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.WRAP11, "WRAP11"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.WRAP12, "WRAP12"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.WRAP13, "WRAP13"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.WRAP14, "WRAP14"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.WRAP15, "WRAP15"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.CLIPPING, "CLIPPING"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.LIGHTING, "LIGHTING"), /* 0x30 */
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.AMBIENT, "AMBIENT"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.FOGVERTEXMODE, "FOGVERTEXMODE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.COLORVERTEX, "COLORVERTEX"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.LOCALVIEWER, "LOCALVIEWER"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.NORMALIZENORMALS, "NORMALIZENORMALS"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.DIFFUSEMATERIALSOURCE, "DIFFUSEMATERIALSOURCE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.SPECULARMATERIALSOURCE, "SPECULARMATERIALSOURCE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.AMBIENTMATERIALSOURCE, "AMBIENTMATERIALSOURCE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.EMISSIVEMATERIALSOURCE, "EMISSIVEMATERIALSOURCE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.VERTEXBLEND, "VERTEXBLEND"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.CLIPPLANEENABLE, "CLIPPLANEENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.POINTSIZE, "POINTSIZE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.POINTSIZE_MIN, "POINTSIZE_MIN"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.POINTSIZE_MAX, "POINTSIZE_MAX"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.POINTSPRITEENABLE, "POINTSPRITEENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.POINTSCALEENABLE, "POINTSCALEENABLE"), /* 0x40 */
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.POINTSCALE_A, "POINTSCALE_A"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.POINTSCALE_B, "POINTSCALE_B"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.POINTSCALE_C, "POINTSCALE_C"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.MULTISAMPLEANTIALIAS, "MULTISAMPLEANTIALIAS"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.MULTISAMPLEMASK, "MULTISAMPLEMASK"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.PATCHEDGESTYLE, "PATCHEDGESTYLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.DEBUGMONITORTOKEN, "DEBUGMONITORTOKEN"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.INDEXEDVERTEXBLENDENABLE, "INDEXEDVERTEXBLENDENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.COLORWRITEENABLE, "COLORWRITEENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.TWEENFACTOR, "TWEENFACTOR"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.BLENDOP, "BLENDOP"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.POSITIONDEGREE, "POSITIONDEGREE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.NORMALDEGREE, "NORMALDEGREE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.SCISSORTESTENABLE, "SCISSORTESTENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.SLOPESCALEDEPTHBIAS, "SLOPESCALEDEPTHBIAS"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.ANTIALIASEDLINEENABLE, "ANTIALIASEDLINEENABLE"), /* 0x50 */
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.MINTESSELLATIONLEVEL, "MINTESSELLATIONLEVEL"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.MAXTESSELLATIONLEVEL, "MAXTESSELLATIONLEVEL"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.ADAPTIVETESS_X, "ADAPTIVETESS_X"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.ADAPTIVETESS_Y, "ADAPTIVETESS_Y"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.ADAPTIVETESS_Z, "ADAPTIVETESS_Z"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.ADAPTIVETESS_W, "ADAPTIVETESS_W"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.ENABLEADAPTIVETESSELLATION, "ENABLEADAPTIVETESSELLATION"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.TWOSIDEDSTENCILMODE, "TWOSIDEDSTENCILMODE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.CCW_STENCILFAIL, "CCW_STENCILFAIL"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.CCW_STENCILZFAIL, "CCW_STENCILZFAIL"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.CCW_STENCILPASS, "CCW_STENCILPASS"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.CCW_STENCILFUNC, "CCW_STENCILFUNC"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.COLORWRITEENABLE1, "COLORWRITEENABLE1"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.COLORWRITEENABLE2, "COLORWRITEENABLE2"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.COLORWRITEENABLE3, "COLORWRITEENABLE3"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.BLENDFACTOR, "BLENDFACTOR"), /* 0x60 */
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.SRGBWRITEENABLE, "SRGBWRITEENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.DEPTHBIAS, "DEPTHBIAS"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.SEPARATEALPHABLENDENABLE, "SEPARATEALPHABLENDENABLE"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.SRCBLENDALPHA, "SRCBLENDALPHA"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.DESTBLENDALPHA, "DESTBLENDALPHA"),
			new state_info(STATE_CLASS.RENDERSTATE, (uint)RENDERSTATETYPE.BLENDOPALPHA, "BLENDOPALPHA"),
			/* Texture stages */
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)TEXTURESTAGESTATETYPE.COLOROP, "COLOROP"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)TEXTURESTAGESTATETYPE.COLORARG0, "COLORARG0"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)TEXTURESTAGESTATETYPE.COLORARG1, "COLORARG1"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)TEXTURESTAGESTATETYPE.COLORARG2, "COLORARG2"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)TEXTURESTAGESTATETYPE.ALPHAOP, "ALPHAOP"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)TEXTURESTAGESTATETYPE.ALPHAARG0, "ALPHAARG0"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)TEXTURESTAGESTATETYPE.ALPHAARG1, "ALPHAARG1"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)TEXTURESTAGESTATETYPE.ALPHAARG2, "ALPHAARG2"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)TEXTURESTAGESTATETYPE.RESULTARG, "RESULTARG"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)TEXTURESTAGESTATETYPE.BUMPENVMAT00, "BUMPENVMAT00"), /* 0x70 */
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)TEXTURESTAGESTATETYPE.BUMPENVMAT01, "BUMPENVMAT01"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)TEXTURESTAGESTATETYPE.BUMPENVMAT10, "BUMPENVMAT10"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)TEXTURESTAGESTATETYPE.BUMPENVMAT11, "BUMPENVMAT11"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)TEXTURESTAGESTATETYPE.TEXCOORDINDEX, "TEXCOORDINDEX"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)TEXTURESTAGESTATETYPE.BUMPENVLSCALE, "BUMPENVLSCALE"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)TEXTURESTAGESTATETYPE.BUMPENVLOFFSET, "BUMPENVLOFFSET"),
			new state_info(STATE_CLASS.TEXTURESTAGE, (uint)TEXTURESTAGESTATETYPE.TEXTURETRANSFORMFLAGS, "TEXTURETRANSFORMFLAGS"),
			/* */
			new state_info(STATE_CLASS.UNKNOWN, 0, "UNKNOWN"),
			/* NPatchMode */
			new state_info(STATE_CLASS.NPATCHMODE, 0, "NPatchMode"),
			/* */
			new state_info(STATE_CLASS.UNKNOWN, 0, "UNKNOWN"),
			/* Transform */
			new state_info(STATE_CLASS.TRANSFORM, (uint)TRANSFORMSTATETYPE.PROJECTION, "PROJECTION"),
			new state_info(STATE_CLASS.TRANSFORM, (uint)TRANSFORMSTATETYPE.VIEW, "VIEW"),
			new state_info(STATE_CLASS.TRANSFORM, (uint)TRANSFORMSTATETYPE.WORLD, "WORLD"),
			new state_info(STATE_CLASS.TRANSFORM, (uint)TRANSFORMSTATETYPE.TEXTURE0, "TEXTURE0"),
			/* Material */
			new state_info(STATE_CLASS.MATERIAL, (uint)MATERIAL_TYPE.DIFFUSE, "MaterialDiffuse"),
			new state_info(STATE_CLASS.MATERIAL, (uint)MATERIAL_TYPE.AMBIENT, "MaterialAmbient"), /* 0x80 */
			new state_info(STATE_CLASS.MATERIAL, (uint)MATERIAL_TYPE.SPECULAR, "MaterialSpecular"),
			new state_info(STATE_CLASS.MATERIAL, (uint)MATERIAL_TYPE.EMISSIVE, "MaterialEmissive"),
			new state_info(STATE_CLASS.MATERIAL, (uint)MATERIAL_TYPE.POWER, "MaterialPower"),
			/* Light */
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.TYPE, "LightType"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.DIFFUSE, "LightDiffuse"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.SPECULAR, "LightSpecular"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.AMBIENT, "LightAmbient"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.POSITION, "LightPosition"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.DIRECTION, "LightDirection"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.RANGE, "LightRange"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.FALLOFF, "LightFallOff"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.ATTENUATION0, "LightAttenuation0"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.ATTENUATION1, "LightAttenuation1"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.ATTENUATION2, "LightAttenuation2"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.THETA, "LightTheta"),
			new state_info(STATE_CLASS.LIGHT, (uint)LIGHT_TYPE.PHI, "LightPhi"), /* 0x90 */
			/* Ligthenable */
			new state_info(STATE_CLASS.LIGHTENABLE, 0, "LightEnable"),
			/* Vertexshader */
			new state_info(STATE_CLASS.VERTEXSHADER, 0, "Vertexshader"),
			/* Pixelshader */
			new state_info(STATE_CLASS.PIXELSHADER, 0, "Pixelshader"),
            /* ComputerShader */
            new state_info(STATE_CLASS.COMPUTESHADER, 0, "ComputeShader"),
			/* Shader constants */
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstantF"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSBOOL, "VertexShaderConstantB"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSINT, "VertexShaderConstantI"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstant"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstant1"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstant2"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstant3"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.VSFLOAT, "VertexShaderConstant4"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstantF"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSBOOL, "PixelShaderConstantB"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSINT, "PixelShaderConstantI"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstant"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstant1"), /* 0xa0 */
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstant2"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstant3"),
			new state_info(STATE_CLASS.SHADERCONST, (uint)SHADER_CONSTANT_TYPE.PSFLOAT, "PixelShaderConstant4"),
			/* Texture */
			new state_info(STATE_CLASS.TEXTURE, 0, "Texture"),
			/* Sampler states */
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)SAMPLERSTATETYPE.ADDRESSU, "AddressU"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)SAMPLERSTATETYPE.ADDRESSV, "AddressV"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)SAMPLERSTATETYPE.ADDRESSW, "AddressW"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)SAMPLERSTATETYPE.BORDERCOLOR, "BorderColor"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)SAMPLERSTATETYPE.MAGFILTER, "MagFilter"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)SAMPLERSTATETYPE.MINFILTER, "MinFilter"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)SAMPLERSTATETYPE.MIPFILTER, "MipFilter"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)SAMPLERSTATETYPE.MIPMAPLODBIAS, "MipMapLodBias"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)SAMPLERSTATETYPE.MAXMIPLEVEL, "MaxMipLevel"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)SAMPLERSTATETYPE.MAXANISOTROPY, "MaxAnisotropy"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)SAMPLERSTATETYPE.SRGBTEXTURE, "SRGBTexture"),
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)SAMPLERSTATETYPE.ELEMENTINDEX, "ElementIndex"), /* 0xb0 */
			new state_info(STATE_CLASS.SAMPLERSTATE, (uint)SAMPLERSTATETYPE.DMAPOFFSET, "DMAPOffset"),
			/* Set sampler */
			new state_info(STATE_CLASS.SETSAMPLER, 0, "Sampler"),
		};

        static public EffectParameterClass ToXNAParameterClass( PARAMETER_CLASS class_ )
        {
			switch (class_) 
            {
			    case PARAMETER_CLASS.SCALAR:
				    return EffectParameterClass.Scalar;
			    case PARAMETER_CLASS.VECTOR:
				    return EffectParameterClass.Vector;
			    case PARAMETER_CLASS.MATRIX_ROWS:
			    case PARAMETER_CLASS.MATRIX_COLUMNS:
                    return EffectParameterClass.Matrix;
			    case PARAMETER_CLASS.OBJECT:
                    return EffectParameterClass.Object;
			    case PARAMETER_CLASS.STRUCT:
                    return EffectParameterClass.Struct;
			    default:
				    throw new NotImplementedException();
			}
        }

        static public EffectParameterType ToXNAParameterType(PARAMETER_TYPE type)
        {
			switch (type) 
            {
			    case PARAMETER_TYPE.BOOL:
                    return EffectParameterType.Bool;
			    case PARAMETER_TYPE.INT:
				    return EffectParameterType.Int32;
			    case PARAMETER_TYPE.FLOAT:
				    return EffectParameterType.Single;
			    case PARAMETER_TYPE.STRING:
				    return EffectParameterType.String;
			    case PARAMETER_TYPE.TEXTURE:
				    return EffectParameterType.Texture;
			    case PARAMETER_TYPE.TEXTURE1D:
				    return EffectParameterType.Texture1D;
			    case PARAMETER_TYPE.TEXTURE2D:
				    return EffectParameterType.Texture2D;
			    case PARAMETER_TYPE.TEXTURE3D:
				    return EffectParameterType.Texture3D;
			    case PARAMETER_TYPE.TEXTURECUBE:
				    return EffectParameterType.TextureCube;
                default:
                    throw new NotImplementedException();
			}
        }

        static internal VertexElementUsage ToXNAVertexElementUsage(MojoShader.Usage usage)
        {
            switch (usage)
            {
                case MojoShader.Usage.POSITION:
                    return VertexElementUsage.Position;
		        case MojoShader.Usage.BLENDWEIGHT:
                    return VertexElementUsage.BlendWeight;
                case MojoShader.Usage.BLENDINDICES:
                    return VertexElementUsage.BlendIndices;
		        case MojoShader.Usage.NORMAL:
                    return VertexElementUsage.Normal;
                case MojoShader.Usage.POINTSIZE:
                    return VertexElementUsage.PointSize;
                case MojoShader.Usage.TEXCOORD:
                    return VertexElementUsage.TextureCoordinate;
                case MojoShader.Usage.TANGENT:
                    return VertexElementUsage.Tangent;
                case MojoShader.Usage.BINORMAL:
                    return VertexElementUsage.Binormal;
                case MojoShader.Usage.TESSFACTOR:
                    return VertexElementUsage.TessellateFactor;
                case MojoShader.Usage.COLOR:
                    return VertexElementUsage.Color;
                case MojoShader.Usage.FOG:
                    return VertexElementUsage.Fog;
                case MojoShader.Usage.DEPTH:
                    return VertexElementUsage.Depth;
                case MojoShader.Usage.SAMPLE:
                    return VertexElementUsage.Sample;

                default:
                    throw new NotImplementedException();
            }
        }

        internal static int GetShaderIndex(STATE_CLASS type, EffectStateContent[] states)
        {
            foreach (EffectStateContent state in states)
            {
                state_info operation = state_table[state.operation];
                if (operation.class_ != type)
                    continue;

                if (state.type != STATE_TYPE.CONSTANT)
                    throw new NotSupportedException("We do not support shader expressions!");

                return (int)state.parameter.data;
            }

            return -1;
        }

        public EffectParameterContent[] Objects { get; private set; }

        public EffectParameterContent[] Parameters { get; internal set; }

        public EffectTechniqueContent[] Techniques { get; internal set; }

        public List<ShaderData> Shaders { get; internal set; }

        public List<ConstantBufferData> ConstantBuffers { get; internal set; }
	}
}

