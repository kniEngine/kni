//-----------------------------------------------------------------------------
// Macros.fxh
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


#define TECHNIQUE_SM2(name, vsname, psname ) \
	technique name { pass { \
                            VertexShader = compile vs_4_0_level_9_1 vsname(); \
                            PixelShader  = compile ps_4_0_level_9_1 psname(); \
                          } }

#define TECHNIQUE_SM3(name, vsname, psname ) \
	technique name { pass { \
                            VertexShader = compile vs_4_0_level_9_3 vsname(); \
                            PixelShader = compile ps_4_0_level_9_3 psname(); \
                          } }


#ifdef MGFX


// HLSL Shader Model 4.0 macros (for targeting DirectX11)

#define _vs(r)
#define _ps(r)
#define _cb(r)

#else


// HLSL Shader Model 2.0 & 3.0 macros (for targeting DirectX9/XNA or openGL/MojoShader)

#define _vs(r)  : register(vs, r)
#define _ps(r)  : register(ps, r)
#define _cb(r)

#endif
