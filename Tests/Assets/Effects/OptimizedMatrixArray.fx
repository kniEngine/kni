
float4x4 ColorsMatrix[2] =
{
    {
        11, 21, 31, 41,
        12, 22, 32, 42,
        13, 23, 33, 43,
        14, 24, 34, 44
    },
    {
        111, 121, 131, 141,
        112, 122, 132, 142,
        113, 123, 133, 143,
        114, 124, 134, 144
    }
};

float4 PS_Main(float4 inPosition : SV_Position,
               float4 inColor : COLOR0
              ) : COLOR0
{    
    float4 color = inColor;
    
    for (int i = 0; i < 2; ++i)
    {    
        color.r = color.r * ColorsMatrix[i]._11;
        color.g = color.g * ColorsMatrix[i]._12;
        color.b = color.b * ColorsMatrix[i]._13;
        //color.a = color.a * ColorsMatrix[i]._14;
    }
    
    return color;
}

technique
{
    pass
    {
        PixelShader = compile ps_2_0 PS_Main();
    }
}