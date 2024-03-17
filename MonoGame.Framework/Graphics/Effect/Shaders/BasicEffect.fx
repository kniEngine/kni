//-----------------------------------------------------------------------------
// BasicEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "Macros.fxh"


Texture2D<float4> Texture : register(t0);
sampler TextureSampler : register(s0);


cbuffer Parameters : register(b0)
{
    float4 DiffuseColor             _vs(c0)  _ps(c1)  _cb(c0);
    float3 EmissiveColor            _vs(c1)  _ps(c2)  _cb(c1);
    float3 SpecularColor            _vs(c2)  _ps(c3)  _cb(c2);
    float  SpecularPower            _vs(c3)  _ps(c4)  _cb(c2.w);

    float3 DirLight0Direction       _vs(c4)  _ps(c5)  _cb(c3);
    float3 DirLight0DiffuseColor    _vs(c5)  _ps(c6)  _cb(c4);
    float3 DirLight0SpecularColor   _vs(c6)  _ps(c7)  _cb(c5);

    float3 DirLight1Direction       _vs(c7)  _ps(c8)  _cb(c6);
    float3 DirLight1DiffuseColor    _vs(c8)  _ps(c9)  _cb(c7);
    float3 DirLight1SpecularColor   _vs(c9)  _ps(c10) _cb(c8);

    float3 DirLight2Direction       _vs(c10) _ps(c11) _cb(c9);
    float3 DirLight2DiffuseColor    _vs(c11) _ps(c12) _cb(c10);
    float3 DirLight2SpecularColor   _vs(c12) _ps(c13) _cb(c11);

    float3 EyePosition              _vs(c13) _ps(c14) _cb(c12);

    float3 FogColor                          _ps(c0)  _cb(c13);
    float4 FogVector                _vs(c14)          _cb(c14);

    float4x4 World                  _vs(c19)          _cb(c15);
    float3x3 WorldInverseTranspose  _vs(c23)          _cb(c19);


    float4x4 WorldViewProj          _vs(c15)          _cb(c0);
};


#include "Structures.fxh"
#include "Common.fxh"
#include "Lighting.fxh"


// Vertex shader: basic.
VSOutputFog VSBasicFog(VSInput vin)
{
    VSOutputFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsFog;
    
    return vout;
}


// Vertex shader: no fog.
VSOutput VSBasic(VSInput vin)
{
    VSOutput vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    return vout;
}


// Vertex shader: vertex color.
VSOutputFog VSBasicVcFog(VSInputVc vin)
{
    VSOutputFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsFog;
    
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: vertex color, no fog.
VSOutput VSBasicVc(VSInputVc vin)
{
    VSOutput vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: texture.
VSOutputTxFog VSBasicTxFog(VSInputTx vin)
{
    VSOutputTxFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: texture, no fog.
VSOutputTx VSBasicTx(VSInputTx vin)
{
    VSOutputTx vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: texture + vertex color.
VSOutputTxFog VSBasicTxVcFog(VSInputTxVc vin)
{
    VSOutputTxFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: texture + vertex color, no fog.
VSOutputTx VSBasicTxVc(VSInputTxVc vin)
{
    VSOutputTx vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord = vin.TexCoord;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: vertex lighting.
VSOutputFog VSBasicVertexLighting(VSInputNm vin)
{
    VSOutputFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 3);
    SetCommonVSOutputParamsFog;
    
    return vout;
}


// Vertex shader: vertex lighting + vertex color.
VSOutputFog VSBasicVertexLightingVc(VSInputNmVc vin)
{
    VSOutputFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 3);
    SetCommonVSOutputParamsFog;
    
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: vertex lighting + texture.
VSOutputTxFog VSBasicVertexLightingTx(VSInputNmTx vin)
{
    VSOutputTxFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 3);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: vertex lighting + texture + vertex color.
VSOutputTxFog VSBasicVertexLightingTxVc(VSInputNmTxVc vin)
{
    VSOutputTxFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 3);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: one light.
VSOutputFog VSBasicOneLight(VSInputNm vin)
{
    VSOutputFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 1);
    SetCommonVSOutputParamsFog;
    
    return vout;
}


// Vertex shader: one light + vertex color.
VSOutputFog VSBasicOneLightVc(VSInputNmVc vin)
{
    VSOutputFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 1);
    SetCommonVSOutputParamsFog;
    
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: one light + texture.
VSOutputTxFog VSBasicOneLightTx(VSInputNmTx vin)
{
    VSOutputTxFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 1);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: one light + texture + vertex color.
VSOutputTxFog VSBasicOneLightTxVc(VSInputNmTxVc vin)
{
    VSOutputTxFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 1);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: pixel lighting.
VSOutputPixelLighting VSBasicPixelLighting(VSInputNm vin)
{
    VSOutputPixelLighting vout;
    
    CommonVSOutputPixelLighting cout = ComputeCommonVSOutputPixelLighting(vin.Position, vin.Normal);
    SetCommonVSOutputParamsPixelLightingFog;

    vout.Diffuse = float4(1, 1, 1, DiffuseColor.a);
    
    return vout;
}


// Vertex shader: pixel lighting + vertex color.
VSOutputPixelLighting VSBasicPixelLightingVc(VSInputNmVc vin)
{
    VSOutputPixelLighting vout;
    
    CommonVSOutputPixelLighting cout = ComputeCommonVSOutputPixelLighting(vin.Position, vin.Normal);
    SetCommonVSOutputParamsPixelLightingFog;
    
    vout.Diffuse.rgb = vin.Color.rgb;
    vout.Diffuse.a = vin.Color.a * DiffuseColor.a;
    
    return vout;
}


// Vertex shader: pixel lighting + texture.
VSOutputPixelLightingTx VSBasicPixelLightingTx(VSInputNmTx vin)
{
    VSOutputPixelLightingTx vout;
    
    CommonVSOutputPixelLighting cout = ComputeCommonVSOutputPixelLighting(vin.Position, vin.Normal);
    SetCommonVSOutputParamsPixelLightingFog;
    
    vout.Diffuse = float4(1, 1, 1, DiffuseColor.a);
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: pixel lighting + texture + vertex color.
VSOutputPixelLightingTx VSBasicPixelLightingTxVc(VSInputNmTxVc vin)
{
    VSOutputPixelLightingTx vout;
    
    CommonVSOutputPixelLighting cout = ComputeCommonVSOutputPixelLighting(vin.Position, vin.Normal);
    SetCommonVSOutputParamsPixelLightingFog;
    
    vout.Diffuse.rgb = vin.Color.rgb;
    vout.Diffuse.a = vin.Color.a * DiffuseColor.a;
    vout.TexCoord = vin.TexCoord;
    
    return vout;
}


// Pixel shader: basic.
float4 PSBasicFog(VSOutputFog pin) : SV_Target0
{
    float4 color = pin.Diffuse;
    
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: no fog.
float4 PSBasic(VSOutput pin) : SV_Target0
{
    return pin.Diffuse;
}


// Pixel shader: texture.
float4 PSBasicTxFog(VSOutputTxFog pin) : SV_Target0
{
    float4 color = Texture.Sample(TextureSampler, pin.TexCoord) * pin.Diffuse;
    
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: texture, no fog.
float4 PSBasicTx(VSOutputTx pin) : SV_Target0
{
    return Texture.Sample(TextureSampler, pin.TexCoord) * pin.Diffuse;
}


// Pixel shader: vertex lighting.
float4 PSBasicVertexLightingFog(VSOutputFog pin) : SV_Target0
{
    float4 color = pin.Diffuse;

    AddSpecular(color, pin.Specular.rgb);
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: vertex lighting, no fog.
float4 PSBasicVertexLighting(VSOutputFog pin) : SV_Target0
{
    float4 color = pin.Diffuse;
    
    AddSpecular(color, pin.Specular.rgb);
    
    return color;
}


// Pixel shader: vertex lighting + texture.
float4 PSBasicVertexLightingTxFog(VSOutputTxFog pin) : SV_Target0
{
    float4 color = Texture.Sample(TextureSampler, pin.TexCoord) * pin.Diffuse;
    
    AddSpecular(color, pin.Specular.rgb);
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: vertex lighting + texture, no fog.
float4 PSBasicVertexLightingTx(VSOutputTxFog pin) : SV_Target0
{
    float4 color = Texture.Sample(TextureSampler, pin.TexCoord) * pin.Diffuse;
    
    AddSpecular(color, pin.Specular.rgb);
    
    return color;
}


// Pixel shader: pixel lighting.
float4 PSBasicPixelLighting(VSOutputPixelLighting pin) : SV_Target0
{
    float4 color = pin.Diffuse;

    float3 eyeVector = normalize(EyePosition - pin.PositionWS.xyz);
    float3 worldNormal = normalize(pin.NormalWS);
    
    ColorPair lightResult = ComputeLights(eyeVector, worldNormal, 3);

    color.rgb *= lightResult.Diffuse;
    
    AddSpecular(color, lightResult.Specular);
    ApplyFog(color, pin.PositionWS.w);
    
    return color;
}


// Pixel shader: pixel lighting + texture.
float4 PSBasicPixelLightingTx(VSOutputPixelLightingTx pin) : SV_Target0
{
    float4 color = Texture.Sample(TextureSampler, pin.TexCoord) * pin.Diffuse;
    
    float3 eyeVector = normalize(EyePosition - pin.PositionWS.xyz);
    float3 worldNormal = normalize(pin.NormalWS);
    
    ColorPair lightResult = ComputeLights(eyeVector, worldNormal, 3);
    
    color.rgb *= lightResult.Diffuse;

    AddSpecular(color, lightResult.Specular);
    ApplyFog(color, pin.PositionWS.w);
    
    return color;
}


// NOTE: The order of the techniques here are
// defined to match the indexing in BasicEffect.cs.

TECHNIQUE_SM2( BasicEffect,							VSBasic,		PSBasic );
TECHNIQUE_SM2( BasicEffect_Fog,						VSBasicFog,			PSBasicFog );
TECHNIQUE_SM2( BasicEffect_VertexColor,				VSBasicVc,		PSBasic );
TECHNIQUE_SM2( BasicEffect_VertexColor_Fog,			VSBasicVcFog,		PSBasicFog );

TECHNIQUE_SM2( BasicEffect_Texture,					VSBasicTx,		PSBasicTx );
TECHNIQUE_SM2( BasicEffect_Texture_Fog,				VSBasicTxFog,		PSBasicTxFog );
TECHNIQUE_SM2( BasicEffect_Texture_VertexColor,		VSBasicTxVc,	PSBasicTx );
TECHNIQUE_SM2( BasicEffect_Texture_VertexColor_Fog,	VSBasicTxVcFog,		PSBasicTxFog );

TECHNIQUE_SM2( BasicEffect_VertexLighting,							VSBasicVertexLighting,		PSBasicVertexLighting );
TECHNIQUE_SM2( BasicEffect_VertexLighting_Fog,						VSBasicVertexLighting,		PSBasicVertexLightingFog );
TECHNIQUE_SM2( BasicEffect_VertexLighting_VertexColor,				VSBasicVertexLightingVc,	PSBasicVertexLighting );
TECHNIQUE_SM2( BasicEffect_VertexLighting_VertexColor_Fog,			VSBasicVertexLightingVc,	PSBasicVertexLightingFog );

TECHNIQUE_SM2( BasicEffect_VertexLighting_Texture,					VSBasicVertexLightingTx,	PSBasicVertexLightingTx );
TECHNIQUE_SM2( BasicEffect_VertexLighting_Texture_Fog,				VSBasicVertexLightingTx,	PSBasicVertexLightingTxFog );
TECHNIQUE_SM2( BasicEffect_VertexLighting_Texture_VertexColor,		VSBasicVertexLightingTxVc,	PSBasicVertexLightingTx );
TECHNIQUE_SM2( BasicEffect_VertexLighting_Texture_VertexColor_Fog,	VSBasicVertexLightingTxVc,	PSBasicVertexLightingTxFog );

TECHNIQUE_SM2( BasicEffect_OneLight,							VSBasicOneLight,		PSBasicVertexLighting );
TECHNIQUE_SM2( BasicEffect_OneLight_Fog,						VSBasicOneLight,		PSBasicVertexLightingFog );
TECHNIQUE_SM2( BasicEffect_OneLight_VertexColor,				VSBasicOneLightVc,		PSBasicVertexLighting );
TECHNIQUE_SM2( BasicEffect_OneLight_VertexColor_Fog,			VSBasicOneLightVc,		PSBasicVertexLightingFog );

TECHNIQUE_SM2( BasicEffect_OneLight_Texture,					VSBasicOneLightTx,		PSBasicVertexLightingTx );
TECHNIQUE_SM2( BasicEffect_OneLight_Texture_Fog,				VSBasicOneLightTx,		PSBasicVertexLightingTxFog );
TECHNIQUE_SM2( BasicEffect_OneLight_Texture_VertexColor,		VSBasicOneLightTxVc,	PSBasicVertexLightingTx );
TECHNIQUE_SM2( BasicEffect_OneLight_Texture_VertexColor_Fog,	VSBasicOneLightTxVc,	PSBasicVertexLightingTxFog );

TECHNIQUE_SM2( BasicEffect_PixelLighting,							VSBasicPixelLighting,		PSBasicPixelLighting );
TECHNIQUE_SM2( BasicEffect_PixelLighting_Fog,						VSBasicPixelLighting,		PSBasicPixelLighting );
TECHNIQUE_SM2( BasicEffect_PixelLighting_VertexColor,				VSBasicPixelLightingVc,		PSBasicPixelLighting );
TECHNIQUE_SM2( BasicEffect_PixelLighting_VertexColor_Fog,			VSBasicPixelLightingVc,		PSBasicPixelLighting );

TECHNIQUE_SM2( BasicEffect_PixelLighting_Texture,					VSBasicPixelLightingTx,		PSBasicPixelLightingTx );
TECHNIQUE_SM2( BasicEffect_PixelLighting_Texture_Fog,				VSBasicPixelLightingTx,		PSBasicPixelLightingTx );
TECHNIQUE_SM2( BasicEffect_PixelLighting_Texture_VertexColor,		VSBasicPixelLightingTxVc,	PSBasicPixelLightingTx );
TECHNIQUE_SM2( BasicEffect_PixelLighting_Texture_VertexColor_Fog,	VSBasicPixelLightingTxVc,	PSBasicPixelLightingTx );
