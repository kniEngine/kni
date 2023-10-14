#ifdef __MOJOSHADER__
#define DECLARE_TEXTURE(Name, index) \
    Texture2D Name : register(t##index); \
    sampler2D Name##Sampler : register(s##index) = sampler_state { Texture = <Name>; };
#define DECLARE_CUBEMAP(Name, index) \
    TextureCube Name : register(t##index); \
    samplerCUBE Name##Sampler : register(s##index) = sampler_state { Texture = <Name>; };
#define SAMPLE_TEXTURE(Name, texCoord)  tex2D(Name##Sampler, texCoord)
#define SAMPLE_CUBEMAP(Name, texCoord)  texCUBE(Name##Sampler, texCoord)
#else
#define DECLARE_TEXTURE(Name, index) \
    Texture2D<float4> Name : register(t##index); \
    sampler Name##Sampler : register(s##index) = sampler_state { Texture = <Name>; }
#define DECLARE_CUBEMAP(Name, index) \
    TextureCube<float4> Name : register(t##index); \
    sampler Name##Sampler : register(s##index) = sampler_state { Texture = <Name>; }
#define SAMPLE_TEXTURE(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)
#define SAMPLE_CUBEMAP(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)
#endif


DECLARE_TEXTURE(SpriteTexture, 0);

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return SAMPLE_TEXTURE(SpriteTexture, input.TextureCoordinates) * input.Color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile ps_4_0_level_9_1 MainPS();
	}
};