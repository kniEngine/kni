# Custom Effects

A core element of Microsoft XNA is the effect system which is used for all rendering.

For KNI we are supporting stock and custom effects for DirectX HLSL, desktop GLSL, mobile GLSL.
There currently is no effect system or shader language that supports all the platforms we require, forcing us to build a new custom effect system.

## MGFX

MGFX is KNI's "Effect" runtime and tools which with the following core goals:

* Support a similar technique, passes, shaders structure as Microsoft FX files.
* Have a textual format for ease of editing.
* Have a compiled and optimized binary format for runtime use.
* Be cross-platform and support multiple shader languages and bytecodes.
* Easy to extend for future platforms and features.

## Stock Effects

KNI has the following effects built-in and fully supported on current platforms:

* BasicEffect
* AlphaTestEffect
* DualTextureEffect
* EnvironmentMapEffect
* SkinnedEffect
* SpriteEffect

Under the hood these effects use the same system and tools as one would for a custom Effect.  The source of these effects can be found in the ['MonoGame.Framework\Graphics\Effect\Shaders'](https://github.com/kniEngine/kni/tree/main/MonoGame.Framework/Graphics/Effect/Shaders) folder.

## Custom Effects

To use a custom effect with KNI you must do one of the following:

* Run the effect file through the [content processor](~/articles/tools/mgcb.md) for loading via the [`ContentManager`](xref:Microsoft.Xna.Framework.Content.ContentManager) (Recommended).
* Process your effect file with the [KNIFXC tool](~/articles/tools/knifxc.md) and load them yourself at runtime.

## Effect Writing Tips

These are some tips for writing or converting effects for use with MonoGame.

| The supported shader models when targeting DX are the following:|
|---|
|  * `vs_4_0_level_9_1` and `ps_4_0_level_9_1` (`Reach` `GraphicsProfile`)|
|  * `vs_4_0_level_9_3` and `ps_4_0_level_9_3` (`HiDef` `GraphicsProfile`)|
|  * `vs_4_0` and `ps_4_0` (requires `FL10_0` `GraphicsProfile` at runtime)|
|  * `vs_4_1` and `ps_4_1` (requires `FL10_1` `GraphicsProfile` at runtime)|
|  * `vs_5_0` and `ps_5_0` (requires `FL11_0` `GraphicsProfile` at runtime)|
---
|When targeting GL platforms we automatically translate FX files to GLSL using a library called [MojoShader](http://icculus.org/mojoshader/).  The supported feature levels are the following:|
|---|
|  * `vs_4_0_level_9_1` and `ps_4_0_level_9_1` (`Reach` `GraphicsProfile`)|
|  * `vs_4_0_level_9_3` and `ps_4_0_level_9_3` (`HiDef` `GraphicsProfile`)|
|  * `vs_2_0` and `ps_2_0` (`Reach` `GraphicsProfile`)|
|  * `vs_3_0` and `ps_3_0` (`HiDef` `GraphicsProfile`)|
---
|You can use preprocessor checks to add conditional code or compilation depending on defined symbols. MonoGame defines the following symbols when compiling effects:|
|---|
|  * `MGFX`                        |
|  * `__DEBUG__` when building with the Debug flag |
|  * `__DIRECTX__` when targeting DirectX |
|  * `__OPENGL__` when targeting OpenGL |
|  * `__MOJOSHADER__` when building with MojoShader, targeting OpenGL |
---


Custom symbols can be defined from the [MGCB Editor](~/articles/tools/mgcb_editor.md) or via [KNIFXC](~/articles/tools/knifxc.md).

* Make sure the pixel shaders inputs **exactly match** the vertex shader outputs so the parameters are passed in the correct registers. The parameters need to have the same size and order. Omitting parameters might not break compilation, but can cause unexpected results.
* Note that on GL platforms default values on Effect parameters do not work.  Either set the parameter from code or use a real constant like a #define.
* The effect compiler is aggressive about removing unused parameters, be sure the parameters you are setting are actually used.
* Preshaders are not supported.
* If you think you have found a bug porting a shader, [please let us know](https://github.com/MonoGame/MonoGame/issues).
