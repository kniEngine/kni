//-----------------------------------------------------------------------------
// SpriteEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "Macros.fxh"


Texture2D<float4> Texture : register(t0);
sampler TextureSampler : register(s0);


cbuffer Parameters : register(b0)
{

    float4x4 MatrixTransform    _vs(c0) _cb(c0);
};


struct VSOutputFog
{
	float4 position		: SV_Position;
	float4 color		: COLOR0;
    float2 texCoord		: TEXCOORD0;
};

VSOutputFog SpriteVertexShader(	float4 position	: POSITION0,
								float4 color	: COLOR0,
								float2 texCoord	: TEXCOORD0)
{
	VSOutputFog output;
    output.position = mul(position, MatrixTransform);
	output.color = color;
	output.texCoord = texCoord;
	return output;
}


float4 SpritePixelShader(VSOutputFog input) : SV_Target0
{
    return Texture.Sample(TextureSampler, input.texCoord) * input.color;
}

TECHNIQUE_SM2( SpriteBatch, SpriteVertexShader, SpritePixelShader );
