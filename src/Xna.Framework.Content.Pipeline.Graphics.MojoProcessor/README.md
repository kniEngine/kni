# Xna.Framework.Content.Pipeline.Graphics.MojoProcessor

HLSL effect compilation for cross-platform graphics support using MojoShader.

## Overview

The `Xna.Framework.Content.Pipeline.Graphics.MojoProcessor` package provides HLSL effect compilation with cross-platform support for DirectX 11, OpenGL, and mobile platforms. It includes multi-target shader compilation, preprocessor support, constant buffer optimization, and sampler state validation.

## Installation

```bash
dotnet add package nkast.Xna.Framework.Content.Pipeline.Graphics.MojoProcessor
```

## Configuration

Add to your `.mgcb` project file:

```
#begin Effects/basic.fx
/importer:EffectImporter
/processor:MojoEffectProcessor
/processorParam:DebugMode=False
/build:Effects/basic.fx
```

## Processors

- **MojoEffectProcessor**: Compiles HLSL effects using MojoShader with cross-platform bytecode generation, preprocessor support, and constant buffer optimization
- **EffectProcessor**: Standard effect processor with platform-specific compilation
