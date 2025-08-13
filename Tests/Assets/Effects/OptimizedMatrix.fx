
float4x4 ColorMatrix =
{
    11, 21, 31, 41,
    12, 22, 32, 42,
    13, 23, 33, 43,
    14, 24, 34, 44
};

float4 PS_Main(float4 inPosition : SV_Position,
               float4 inColor : COLOR0
              ) : COLOR0
{    
    float4 color = inColor;
    
    color.r = color.r * ColorMatrix._11;
    color.g = color.g * ColorMatrix._12;
    color.b = color.b * ColorMatrix._13;
    //color.a = color.a * ColorMatrix._14;
    
    return color;
}

technique
{
    pass
    {
        PixelShader = compile ps_2_0 PS_Main();
    }
}