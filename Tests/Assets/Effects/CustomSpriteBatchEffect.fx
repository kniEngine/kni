// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.


Texture2D SourceTexture;
Texture2D OtherTexture;

sampler2D SourceSampler = sampler_state
{
    Texture = (SourceTexture);
};

sampler2D OtherSampler = sampler_state
{
    Texture = (OtherTexture);
};

float4 PS_Main(float2 uv : TEXCOORD0) : COLOR0
{
    return tex2D(SourceSampler, uv) + tex2D(OtherSampler, uv);
}

technique
{
    pass
    {
        PixelShader = compile ps_4_0_level_9_1 PS_Main();
    }
}