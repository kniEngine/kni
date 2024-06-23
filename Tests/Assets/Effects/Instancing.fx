// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

float4x4 View;
float4x4 Projection;

struct VSInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float2 TexCoord : TEXCOORD0;
    float4 Position : SV_Position;
};

struct PSInput
{
    float2 TexCoord : TEXCOORD0;
};

VSOutput VS(VSInput input, float4x4 worldTransposed : BLENDWEIGHT)
{
    VSOutput output = (VSOutput)0;
    
    float4x4 world = transpose(worldTransposed);
    float4 positionWorld = mul(input.Position, world);
    float4 positionView = mul(positionWorld, View);
    output.Position = mul(positionView, Projection);
    
    output.TexCoord = input.TexCoord;
    
    return output;
}

float4 PS(PSInput input) : COLOR0
{
    return float4(input.TexCoord.xy, 0, 1);
}

technique
{
    pass
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}
