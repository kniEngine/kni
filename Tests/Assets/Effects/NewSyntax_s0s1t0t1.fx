
Texture2D Texture0;
Texture2D Texture1;

sampler Sampler0 = sampler_state
{
    MAGFILTER = POINT;
    MINFILTER = POINT;
    MIPFILTER = POINT;
};

sampler Sampler1 = sampler_state
{
    MAGFILTER = POINT;
    MINFILTER = POINT;
    MIPFILTER = POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

float4 PS_Main(float2 uv : TEXCOORD0) : COLOR0
{
    return Texture0.Sample(Sampler0, uv) * Texture1.Sample(Sampler1, uv);
}

technique
{
    pass
    {
        PixelShader = compile ps_2_0 PS_Main();
    }
}