
Texture2D Texture0;
Texture2D Texture1;

sampler2D Sampler0 = sampler_state
{
    Texture = (Texture0);
};

sampler2D Sampler1 = sampler_state
{
    Texture = (Texture1);
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