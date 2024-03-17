//-----------------------------------------------------------------------------
// DualTextureEffect_Fog.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "Macros.fxh"


Texture2D<float4> Texture : register(t0);
sampler TextureSampler : register(s0);
Texture2D<float4> Texture2 : register(t1);
sampler Texture2Sampler : register(s1);


cbuffer Parameters : register(b0)
{
    float4 DiffuseColor     _vs(c0) _cb(c0);
    float3 FogColor         _ps(c0) _cb(c1);
    float4 FogVector        _vs(c5) _cb(c2);


    float4x4 WorldViewProj  _vs(c1) _cb(c0);
};


#include "Structures.fxh"
#include "Common.fxh"


// Vertex shader: basic.
VSOutputTx2Fog VSDualTextureFog(VSInputTx2 vin)
{
    VSOutputTx2Fog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;
    vout.TexCoord2 = vin.TexCoord2;

    return vout;
}


// Vertex shader: no fog.
VSOutputTx2 VSDualTexture(VSInputTx2 vin)
{
    VSOutputTx2 vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord = vin.TexCoord;
    vout.TexCoord2 = vin.TexCoord2;

    return vout;
}


// Vertex shader: vertex color.
VSOutputTx2Fog VSDualTextureVcFog(VSInputTx2Vc vin)
{
    VSOutputTx2Fog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;
    vout.TexCoord2 = vin.TexCoord2;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: vertex color, no fog.
VSOutputTx2 VSDualTextureVc(VSInputTx2Vc vin)
{
    VSOutputTx2 vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord = vin.TexCoord;
    vout.TexCoord2 = vin.TexCoord2;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Pixel shader: basic.
float4 PSDualTextureFog(VSOutputTx2Fog pin) : SV_Target0
{
    float4 color = Texture.Sample(TextureSampler, pin.TexCoord);
    float4 overlay = Texture2.Sample(Texture2Sampler, pin.TexCoord2);

    color.rgb *= 2;    
    color *= overlay * pin.Diffuse;
    
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: no fog.
float4 PSDualTexture(VSOutputTx2 pin) : SV_Target0
{
    float4 color = Texture.Sample(TextureSampler, pin.TexCoord);
    float4 overlay = Texture2.Sample(Texture2Sampler, pin.TexCoord2);
    
    color.rgb *= 2;    
    color *= overlay * pin.Diffuse;
    
    return color;
}


// NOTE: The order of the techniques here are
// defined to match the indexing in DualTextureEffect_Fog.cs.

TECHNIQUE_SM2( DualTextureEffect,					VSDualTexture,		PSDualTexture );
TECHNIQUE_SM2( DualTextureEffect_Fog,				VSDualTextureFog,		PSDualTextureFog );
TECHNIQUE_SM2( DualTextureEffect_VertexColor,		VSDualTextureVc,	PSDualTexture );
TECHNIQUE_SM2( DualTextureEffect_VertexColor_Fog,	VSDualTextureVcFog,		PSDualTextureFog );
