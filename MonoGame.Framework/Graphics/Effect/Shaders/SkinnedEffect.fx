//-----------------------------------------------------------------------------
// SkinnedEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "Macros.fxh"

#define SKINNED_EFFECT_MAX_BONES   72


DECLARE_TEXTURE(Texture, 0);


BEGIN_CONSTANTS

    float4 DiffuseColor                         _vs(c0)  _ps(c1)  _cb(c0);
    float3 EmissiveColor                        _vs(c1)  _ps(c2)  _cb(c1);
    float3 SpecularColor                        _vs(c2)  _ps(c3)  _cb(c2);
    float  SpecularPower                        _vs(c3)  _ps(c4)  _cb(c2.w);

    float3 DirLight0Direction                   _vs(c4)  _ps(c5)  _cb(c3);
    float3 DirLight0DiffuseColor                _vs(c5)  _ps(c6)  _cb(c4);
    float3 DirLight0SpecularColor               _vs(c6)  _ps(c7)  _cb(c5);

    float3 DirLight1Direction                   _vs(c7)  _ps(c8)  _cb(c6);
    float3 DirLight1DiffuseColor                _vs(c8)  _ps(c9)  _cb(c7);
    float3 DirLight1SpecularColor               _vs(c9)  _ps(c10) _cb(c8);

    float3 DirLight2Direction                   _vs(c10) _ps(c11) _cb(c9);
    float3 DirLight2DiffuseColor                _vs(c11) _ps(c12) _cb(c10);
    float3 DirLight2SpecularColor               _vs(c12) _ps(c13) _cb(c11);

    float3 EyePosition                          _vs(c13) _ps(c14) _cb(c12);

    float3 FogColor                                      _ps(c0)  _cb(c13);
    float4 FogVector                            _vs(c14)          _cb(c14);

    float4x4 World                              _vs(c19)          _cb(c15);
    float3x3 WorldInverseTranspose              _vs(c23)          _cb(c19);
    
    float4x3 Bones[SKINNED_EFFECT_MAX_BONES]    _vs(c26)          _cb(c22);

MATRIX_CONSTANTS

    float4x4 WorldViewProj                      _vs(c15)          _cb(c0);

END_CONSTANTS


#include "Structures.fxh"
#include "Common.fxh"
#include "Lighting.fxh"


void Skin(inout VSInputNmTxWeights vin, uniform int boneCount)
{
    float4x3 skinning = 0;

    [unroll]
    for (int i = 0; i < boneCount; i++)
    {
        skinning += Bones[vin.Indices[i]] * vin.Weights[i];
    }

    vin.Position.xyz = mul(vin.Position, skinning);
    vin.Normal = mul(vin.Normal, (float3x3)skinning);
}


// Vertex shader: vertex lighting, one bone.
VSOutputTxFog VSSkinnedVertexLightingOneBone(VSInputNmTxWeights vin)
{
    VSOutputTxFog vout;
    
    Skin(vin, 1);
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 3);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: vertex lighting, two bones.
VSOutputTxFog VSSkinnedVertexLightingTwoBones(VSInputNmTxWeights vin)
{
    VSOutputTxFog vout;
    
    Skin(vin, 2);
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 3);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: vertex lighting, four bones.
VSOutputTxFog VSSkinnedVertexLightingFourBones(VSInputNmTxWeights vin)
{
    VSOutputTxFog vout;
    
    Skin(vin, 4);
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 3);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: one light, one bone.
VSOutputTxFog VSSkinnedOneLightOneBone(VSInputNmTxWeights vin)
{
    VSOutputTxFog vout;
    
    Skin(vin, 1);

    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 1);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: one light, two bones.
VSOutputTxFog VSSkinnedOneLightTwoBones(VSInputNmTxWeights vin)
{
    VSOutputTxFog vout;
    
    Skin(vin, 2);

    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 1);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: one light, four bones.
VSOutputTxFog VSSkinnedOneLightFourBones(VSInputNmTxWeights vin)
{
    VSOutputTxFog vout;
    
    Skin(vin, 4);

    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 1);
    SetCommonVSOutputParamsFog;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: pixel lighting, one bone.
VSOutputPixelLightingTx VSSkinnedPixelLightingOneBone(VSInputNmTxWeights vin)
{
    VSOutputPixelLightingTx vout;
    
    Skin(vin, 1);

    CommonVSOutputPixelLighting cout = ComputeCommonVSOutputPixelLighting(vin.Position, vin.Normal);
    SetCommonVSOutputParamsPixelLightingFog;
    
    vout.Diffuse = float4(1, 1, 1, DiffuseColor.a);
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: pixel lighting, two bones.
VSOutputPixelLightingTx VSSkinnedPixelLightingTwoBones(VSInputNmTxWeights vin)
{
    VSOutputPixelLightingTx vout;
    
    Skin(vin, 2);

    CommonVSOutputPixelLighting cout = ComputeCommonVSOutputPixelLighting(vin.Position, vin.Normal);
    SetCommonVSOutputParamsPixelLightingFog;
    
    vout.Diffuse = float4(1, 1, 1, DiffuseColor.a);
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: pixel lighting, four bones.
VSOutputPixelLightingTx VSSkinnedPixelLightingFourBones(VSInputNmTxWeights vin)
{
    VSOutputPixelLightingTx vout;
    
    Skin(vin, 4);

    CommonVSOutputPixelLighting cout = ComputeCommonVSOutputPixelLighting(vin.Position, vin.Normal);
    SetCommonVSOutputParamsPixelLightingFog;
    
    vout.Diffuse = float4(1, 1, 1, DiffuseColor.a);
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Pixel shader: vertex lighting.
float4 PSSkinnedVertexLightingFog(VSOutputTxFog pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
    
    AddSpecular(color, pin.Specular.rgb);
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: vertex lighting, no fog.
float4 PSSkinnedVertexLighting(VSOutputTxFog pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
    
    AddSpecular(color, pin.Specular.rgb);
    
    return color;
}


// Pixel shader: pixel lighting.
float4 PSSkinnedPixelLighting(VSOutputPixelLightingTx pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
    
    float3 eyeVector = normalize(EyePosition - pin.PositionWS.xyz);
    float3 worldNormal = normalize(pin.NormalWS);
    
    ColorPair lightResult = ComputeLights(eyeVector, worldNormal, 3);
    
    color.rgb *= lightResult.Diffuse;

    AddSpecular(color, lightResult.Specular);
    ApplyFog(color, pin.PositionWS.w);
    
    return color;
}


// NOTE: The order of the techniques here are
// defined to match the indexing in SkinnedEffect.cs.

TECHNIQUE( SkinnedEffect_VertexLighting_OneBone,		VSSkinnedVertexLightingOneBone,		PSSkinnedVertexLighting );
TECHNIQUE( SkinnedEffect_VertexLighting_OneBone_Fog,	VSSkinnedVertexLightingOneBone,		PSSkinnedVertexLightingFog );
TECHNIQUE( SkinnedEffect_VertexLighting_TwoBone,		VSSkinnedVertexLightingTwoBones,	PSSkinnedVertexLighting );
TECHNIQUE( SkinnedEffect_VertexLighting_TwoBone_Fog,	VSSkinnedVertexLightingTwoBones,	PSSkinnedVertexLightingFog );
TECHNIQUE( SkinnedEffect_VertexLighting_FourBone,		VSSkinnedVertexLightingFourBones,	PSSkinnedVertexLighting );
TECHNIQUE( SkinnedEffect_VertexLighting_FourBone_Fog,	VSSkinnedVertexLightingFourBones,	PSSkinnedVertexLightingFog );

TECHNIQUE( SkinnedEffect_OneLight_OneBone,			VSSkinnedOneLightOneBone,	PSSkinnedVertexLighting );
TECHNIQUE( SkinnedEffect_OneLight_OneBone_Fog,		VSSkinnedOneLightOneBone,	PSSkinnedVertexLightingFog );
TECHNIQUE( SkinnedEffect_OneLight_TwoBone,			VSSkinnedOneLightTwoBones,	PSSkinnedVertexLighting );
TECHNIQUE( SkinnedEffect_OneLight_TwoBone_Fog,		VSSkinnedOneLightTwoBones,	PSSkinnedVertexLightingFog );
TECHNIQUE( SkinnedEffect_OneLight_FourBone,			VSSkinnedOneLightFourBones,	PSSkinnedVertexLighting );
TECHNIQUE( SkinnedEffect_OneLight_FourBone_Fog,		VSSkinnedOneLightFourBones,	PSSkinnedVertexLightingFog );

TECHNIQUE( SkinnedEffect_PixelLighting_OneBone,			VSSkinnedPixelLightingOneBone,		PSSkinnedPixelLighting );
TECHNIQUE( SkinnedEffect_PixelLighting_OneBone_Fog,		VSSkinnedPixelLightingOneBone,		PSSkinnedPixelLighting );
TECHNIQUE( SkinnedEffect_PixelLighting_TwoBone,			VSSkinnedPixelLightingTwoBones,		PSSkinnedPixelLighting );
TECHNIQUE( SkinnedEffect_PixelLighting_TwoBone_Fog,		VSSkinnedPixelLightingTwoBones,		PSSkinnedPixelLighting );
TECHNIQUE( SkinnedEffect_PixelLighting_FourBone,		VSSkinnedPixelLightingFourBones,	PSSkinnedPixelLighting );
TECHNIQUE( SkinnedEffect_PixelLighting_FourBone_Fog,	VSSkinnedPixelLightingFourBones,	PSSkinnedPixelLighting );
