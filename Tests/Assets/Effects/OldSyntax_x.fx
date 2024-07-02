
sampler2D Sampler0 = sampler_state
{
};

sampler2D Sampler1 = sampler_state
{
};

float4 PS_Main(float2 uv : TEXCOORD0) : COLOR0
{
    return tex2D(Sampler0, uv) * tex2D(Sampler1, uv);
}

technique
{
    pass
    {
        PixelShader = compile ps_2_0 PS_Main();
    }
}