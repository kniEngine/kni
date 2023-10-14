//-----------------------------------------------------------------------------
// Macros.fxh
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


#define TECHNIQUE(name, vsname, psname ) \
	technique name { pass { VertexShader = compile vs_4_0_level_9_1 vsname (); PixelShader = compile ps_4_0_level_9_1 psname(); } }

#define TECHNIQUE_9_3(name, vsname, psname ) \
	technique name { pass { VertexShader = compile vs_4_0_level_9_3 vsname (); PixelShader = compile ps_4_0_level_9_3 psname(); } }


#ifdef __DIRECTX__


// HLSL Shader Model 4.0 macros (for targeting DirectX11)

#define BEGIN_CONSTANTS     cbuffer Parameters : register(b0) {
#define MATRIX_CONSTANTS
#define END_CONSTANTS       };

#define _vs(r)
#define _ps(r)
#define _cb(r)

#define DECLARE_TEXTURE(Name, index) \
    Texture2D<float4> Name : register(t##index); \
    sampler Name##Sampler : register(s##index) = sampler_state { Texture = <Name>; }

#define DECLARE_CUBEMAP(Name, index) \
    TextureCube<float4> Name : register(t##index); \
    sampler Name##Sampler : register(s##index) = sampler_state { Texture = <Name>; }

#define SAMPLE_TEXTURE(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)
#define SAMPLE_CUBEMAP(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)


#else


// HLSL Shader Model 2.0 & 3.0 macros (for targeting DirectX9/XNA or openGL/MojoShader)

#define BEGIN_CONSTANTS
#define MATRIX_CONSTANTS
#define END_CONSTANTS

#define _vs(r)  : register(vs, r)
#define _ps(r)  : register(ps, r)
#define _cb(r)

#define DECLARE_TEXTURE(Name, index) \
    Texture2D Name : register(t##index); \
    sampler2D Name##Sampler : register(s##index) = sampler_state { Texture = <Name>; };

#define DECLARE_CUBEMAP(Name, index) \
    TextureCube Name : register(t##index); \
    samplerCUBE Name##Sampler : register(s##index) = sampler_state { Texture = <Name>; };

#define SAMPLE_TEXTURE(Name, texCoord)  tex2D(Name##Sampler, texCoord)
#define SAMPLE_CUBEMAP(Name, texCoord)  texCUBE(Name##Sampler, texCoord)


#endif
