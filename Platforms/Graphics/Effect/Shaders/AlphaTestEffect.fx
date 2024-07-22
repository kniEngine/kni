//-----------------------------------------------------------------------------
// AlphaTestEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "Macros.fxh"


Texture2D<float4> Texture : register(t0);
sampler TextureSampler : register(s0);


cbuffer Parameters : register(b0)
{
    float4 DiffuseColor     _vs(c0) _cb(c0);
    float4 AlphaTest        _ps(c0) _cb(c1);
    float3 FogColor         _ps(c1) _cb(c2);
    float4 FogVector        _vs(c5) _cb(c3);


    float4x4 WorldViewProj  _vs(c1) _cb(c0);
};


#include "Structures.fxh"
#include "Common.fxh"


// Vertex shader: basic.
VSOutputTxFog VSAlphaTestFog(VSInputTx vin)
{
    VSOutputTxFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: no fog.
VSOutputTx VSAlphaTest(VSInputTx vin)
{
    VSOutputTx vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: vertex color.
VSOutputTxFog VSAlphaTestVcFog(VSInputTxVc vin)
{
    VSOutputTxFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: vertex color, no fog.
VSOutputTx VSAlphaTestVc(VSInputTxVc vin)
{
    VSOutputTx vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord = vin.TexCoord;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Pixel shader: less/greater compare function.
float4 PSAlphaTestLtGtFog(VSOutputTxFog pin) : SV_Target0
{
    float4 color = Texture.Sample(TextureSampler, pin.TexCoord) * pin.Diffuse;

    clip((color.a < AlphaTest.x) ? AlphaTest.z : AlphaTest.w);

    ApplyFog(color, pin.Specular.w);

    return color;
}


// Pixel shader: less/greater compare function, no fog.
float4 PSAlphaTestLtGt(VSOutputTx pin) : SV_Target0
{
    float4 color = Texture.Sample(TextureSampler, pin.TexCoord) * pin.Diffuse;
    
    clip((color.a < AlphaTest.x) ? AlphaTest.z : AlphaTest.w);

    return color;
}


// Pixel shader: equal/notequal compare function.
float4 PSAlphaTestEqNeFog(VSOutputTxFog pin) : SV_Target0
{
    float4 color = Texture.Sample(TextureSampler, pin.TexCoord) * pin.Diffuse;
    
    clip((abs(color.a - AlphaTest.x) < AlphaTest.y) ? AlphaTest.z : AlphaTest.w);

    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: equal/notequal compare function, no fog.
float4 PSAlphaTestEqNe(VSOutputTx pin) : SV_Target0
{
    float4 color = Texture.Sample(TextureSampler, pin.TexCoord) * pin.Diffuse;
    
    clip((abs(color.a - AlphaTest.x) < AlphaTest.y) ? AlphaTest.z : AlphaTest.w);

    return color;
}


// NOTE: The order of the techniques here are
// defined to match the indexing in AlphaTestEffect.cs.

TECHNIQUE_SM2(AlphaTestEffect_LTGT, VSAlphaTest, PSAlphaTestLtGt);
TECHNIQUE_SM2(AlphaTestEffect_LTGT_Fog, VSAlphaTestFog, PSAlphaTestLtGtFog);
TECHNIQUE_SM2(AlphaTestEffect_LTGT_VertexColor, VSAlphaTestVc, PSAlphaTestLtGt);
TECHNIQUE_SM2(AlphaTestEffect_LTGT_VertexColor_Fog, VSAlphaTestVcFog, PSAlphaTestLtGtFog);

TECHNIQUE_SM2(AlphaTestEffect_EQNE, VSAlphaTest, PSAlphaTestEqNe);
TECHNIQUE_SM2(AlphaTestEffect_EQNE_Fog, VSAlphaTestFog, PSAlphaTestEqNeFog);
TECHNIQUE_SM2(AlphaTestEffect_EQNE_VertexColor, VSAlphaTestVc, PSAlphaTestEqNe);
TECHNIQUE_SM2(AlphaTestEffect_EQNE_VertexColor_Fog, VSAlphaTestVcFog, PSAlphaTestEqNeFog);


