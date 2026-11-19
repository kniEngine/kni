//-----------------------------------------------------------------------------
// EnvironmentMapEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "Macros.fxh"


Texture2D<float4> Texture : register(t0);
sampler TextureSampler : register(s0);
TextureCube<float4> EnvironmentMap : register(t1);
sampler EnvironmentMapSampler : register(s1);


cbuffer Parameters : register(b0)
{
    float3 EnvironmentMapSpecular   _ps(c0)  _cb(c0);
    float  FresnelFactor            _vs(c0)  _cb(c0.w);
    float  EnvironmentMapAmount     _vs(c1)  _cb(c2.w);

    float4 DiffuseColor             _vs(c2)  _cb(c1);
    float3 EmissiveColor            _vs(c3)  _cb(c2);

    float3 DirLight0Direction       _vs(c4)  _cb(c3);
    float3 DirLight0DiffuseColor    _vs(c5)  _cb(c4);

    float3 DirLight1Direction       _vs(c6)  _cb(c5);
    float3 DirLight1DiffuseColor    _vs(c7)  _cb(c6);

    float3 DirLight2Direction       _vs(c8)  _cb(c7);
    float3 DirLight2DiffuseColor    _vs(c9)  _cb(c8);

    float3 EyePosition              _vs(c10) _cb(c9);

    float3 FogColor                 _ps(c1)  _cb(c10);
    float4 FogVector                _vs(c11) _cb(c11);

    float4x4 World                  _vs(c16) _cb(c12);
    float3x3 WorldInverseTranspose  _vs(c19) _cb(c16);


    float4x4 WorldViewProj          _vs(c12) _cb(c0);
};


// We don't use these parameters, but Lighting.fxh won't compile without them.
#define SpecularPower           0
#define SpecularColor           float3(0, 0, 0)
#define DirLight0SpecularColor  float3(0, 0, 0)
#define DirLight1SpecularColor  float3(0, 0, 0)
#define DirLight2SpecularColor  float3(0, 0, 0)


#include "Structures.fxh"
#include "Common.fxh"
#include "Lighting.fxh"


float ComputeFresnelFactor(float3 eyeVector, float3 worldNormal)
{
    float viewAngle = dot(eyeVector, worldNormal);
    
    return pow(max(1 - abs(viewAngle), 0), FresnelFactor) * EnvironmentMapAmount;
}


VSOutputTxEnvMap ComputeEnvMapVSOutput(VSInputNmTx vin, uniform bool useFresnel, uniform int numLights)
{
    VSOutputTxEnvMap vout;
    
    float4 pos_ws = mul(vin.Position, World);
    float3 eyeVector = normalize(EyePosition - pos_ws.xyz);
    float3 worldNormal = normalize(mul(vin.Normal, WorldInverseTranspose));

    ColorPair lightResult = ComputeLights(eyeVector, worldNormal, numLights);
    
    vout.PositionPS = mul(vin.Position, WorldViewProj);
    vout.Diffuse = float4(lightResult.Diffuse, DiffuseColor.a);
    
    if (useFresnel)
        vout.Specular.rgb = ComputeFresnelFactor(eyeVector, worldNormal);
    else
        vout.Specular.rgb = EnvironmentMapAmount;
    
    vout.Specular.a = ComputeFogFactor(vin.Position);
    vout.TexCoord = vin.TexCoord;
    vout.EnvCoord = reflect(-eyeVector, worldNormal);

    return vout;
}


// Vertex shader: basic.
VSOutputTxEnvMap VSEnvMap(VSInputNmTx vin)
{
    return ComputeEnvMapVSOutput(vin, false, 3);
}


// Vertex shader: fresnel.
VSOutputTxEnvMap VSEnvMapFresnel(VSInputNmTx vin)
{
    return ComputeEnvMapVSOutput(vin, true, 3);
}


// Vertex shader: one light.
VSOutputTxEnvMap VSEnvMapOneLight(VSInputNmTx vin)
{
    return ComputeEnvMapVSOutput(vin, false, 1);
}


// Vertex shader: one light, fresnel.
VSOutputTxEnvMap VSEnvMapOneLightFresnel(VSInputNmTx vin)
{
    return ComputeEnvMapVSOutput(vin, true, 1);
}


// Pixel shader: basic.
float4 PSEnvMapFog(VSOutputTxEnvMap pin) : SV_Target0
{
    float4 color = Texture.Sample(TextureSampler, pin.TexCoord) * pin.Diffuse;
    float4 envmap = EnvironmentMap.Sample(EnvironmentMapSampler, pin.EnvCoord) * color.a;

    color.rgb = lerp(color.rgb, envmap.rgb, pin.Specular.rgb);
    
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: no fog.
float4 PSEnvMap(VSOutputTxEnvMap pin) : SV_Target0
{
    float4 color = Texture.Sample(TextureSampler, pin.TexCoord) * pin.Diffuse;
    float4 envmap = EnvironmentMap.Sample(EnvironmentMapSampler, pin.EnvCoord) * color.a;

    color.rgb = lerp(color.rgb, envmap.rgb, pin.Specular.rgb);
    
    return color;
}


// Pixel shader: specular.
float4 PSEnvMapSpecularFog(VSOutputTxEnvMap pin) : SV_Target0
{
    float4 color = Texture.Sample(TextureSampler, pin.TexCoord) * pin.Diffuse;
    float4 envmap = EnvironmentMap.Sample(EnvironmentMapSampler, pin.EnvCoord) * color.a;

    color.rgb = lerp(color.rgb, envmap.rgb, pin.Specular.rgb);
    color.rgb += EnvironmentMapSpecular * envmap.a;
    
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: specular, no fog.
float4 PSEnvMapSpecular(VSOutputTxEnvMap pin) : SV_Target0
{
    float4 color = Texture.Sample(TextureSampler, pin.TexCoord) * pin.Diffuse;
    float4 envmap = EnvironmentMap.Sample(EnvironmentMapSampler, pin.EnvCoord) * color.a;

    color.rgb = lerp(color.rgb, envmap.rgb, pin.Specular.rgb);
    color.rgb += EnvironmentMapSpecular * envmap.a;
    
    return color;
}


// NOTE: The order of the shader indices here are
// defined to match the indexing in EnvironmentMapEffect.cs.

VertexShader VSArray[4] =
{
    compile vs_4_0_level_9_1 VSEnvMap(),
    compile vs_4_0_level_9_1 VSEnvMapFresnel(),
    compile vs_4_0_level_9_1 VSEnvMapOneLight(),
    compile vs_4_0_level_9_1 VSEnvMapOneLightFresnel(),
};

PixelShader PSArray[4] =
{
    compile ps_4_0_level_9_1 PSEnvMap(),
    compile ps_4_0_level_9_1 PSEnvMapFog(),
    compile ps_4_0_level_9_1 PSEnvMapSpecular(),
    compile ps_4_0_level_9_1 PSEnvMapSpecularFog(),
};

int VSIndices[16] =
{
    0,   // EnvironmentMapEffect
    0,   // EnvironmentMapEffect_Fog
    1,   // EnvironmentMapEffect_Fresnel
    1,   // EnvironmentMapEffect_Fresnel_Fog
    0,   // EnvironmentMapEffect_Specular
    0,   // EnvironmentMapEffect_Specular_Fog
    1,   // EnvironmentMapEffect_Fresnel_Specular
    1,   // EnvironmentMapEffect_Fresnel_Specular_Fog
    2,   // EnvironmentMapEffect_OneLight
    2,   // EnvironmentMapEffect_OneLight_Fog
    3,   // EnvironmentMapEffect_OneLight_Fresnel
    3,   // EnvironmentMapEffect_OneLight_Fresnel_Fog
    2,   // EnvironmentMapEffect_OneLight_Specular
    2,   // EnvironmentMapEffect_OneLight_Specular_Fog
    3,   // EnvironmentMapEffect_OneLight_Fresnel_Specular
    3,   // EnvironmentMapEffect_OneLight_Fresnel_Specular_Fog
};

int PSIndices[16] =
{
    0,   // EnvironmentMapEffect
    1,   // EnvironmentMapEffect_Fog
    0,   // EnvironmentMapEffect_Fresnel
    1,   // EnvironmentMapEffect_Fresnel_Fog
    2,   // EnvironmentMapEffect_Specular
    3,   // EnvironmentMapEffect_Specular_Fog
    2,   // EnvironmentMapEffect_Fresnel_Specular
    3,   // EnvironmentMapEffect_Fresnel_Specular_Fog
    0,   // EnvironmentMapEffect_OneLight
    1,   // EnvironmentMapEffect_OneLight_Fog
    0,   // EnvironmentMapEffect_OneLight_Fresnel
    1,   // EnvironmentMapEffect_OneLight_Fresnel_Fog
    2,   // EnvironmentMapEffect_OneLight_Specular
    3,   // EnvironmentMapEffect_OneLight_Specular_Fog
    2,   // EnvironmentMapEffect_OneLight_Fresnel_Specular
    3,   // EnvironmentMapEffect_OneLight_Fresnel_Specular_Fog
};

int ShaderIndex = 0;

Technique EnvironmentMapEffect
{
    Pass
    {
        VertexShader = (VSArray[VSIndices[ShaderIndex]]);
        PixelShader  = (PSArray[PSIndices[ShaderIndex]]);
    }
}
