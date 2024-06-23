// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

float4 VS_Main(float4 position : POSITION0) : SV_Position0
{
	return float4(1, 2, 3, 4);
}

float4 PS_Main(float4 position : SV_Position) : COLOR0
{
	return 1;
}

technique
{
    pass
    {
        VertexShader = compile vs_2_0 VS_Main();
        PixelShader = compile ps_2_0 PS_Main();
    }
}

#if defined(INVALID_SYNTAX)
Foo;
#endif

#if MACRO_DEFINE_TEST != 3
Bar;
#endif
