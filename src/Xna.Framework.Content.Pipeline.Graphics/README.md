# Xna.Framework.Content.Pipeline.Graphics

Graphics asset processing and compilation for models, textures, and effects.

## Overview

The `Xna.Framework.Content.Pipeline.Graphics` package provides importers and processors for graphics assets. It includes:

- **Model Processing**: 3D file import and model optimization
- **Texture Processing**: DDS, PNG, BMP, JPEG, TGA processing with mipmap generation and format conversion
- **Font Processing**: TrueType/OpenType font rasterization to texture atlases
- **Effect Compilation**: HLSL effect file compilation
- **Compression**: Support for GPU texture compression formats

## Installation

```bash
dotnet add package nkast.Xna.Framework.Content.Pipeline.Graphics
```

## Configuration

Add to your `.mgcb` project file:

```
#begin Models/character.fbx
/importer:FbxImporter
/processor:ModelProcessor
/processorParam:TextureFormat=NoChange
/processorParam:PremultiplyAlpha=True
/build:Models/character.fbx

#begin Textures/wall.png
/importer:TextureImporter
/processor:TextureProcessor
/processorParam:GenerateMipmaps=True
/processorParam:ColorKeyEnabled=False
/build:Textures/wall.png

#begin Fonts/Arial.spritefont
/importer:FontDescriptionImporter
/processor:FontDescriptionProcessor
/processorParam:PremultiplyAlpha=True
/build:Fonts/Arial.spritefont

#begin Effects/basic.fx
/importer:EffectImporter
/processor:EffectProcessor
/build:Effects/basic.fx
```

## Importers

- **TextureImporter**: Supports PNG, BMP, JPEG, TGA, DDS
- **EffectImporter**: Builds HLSL effects to platform-specific format
- **FontDescriptionImporter**: Processes system fonts
- **OpenAssetImporter**: 3D models
- **FbxImporter**: FBX format 3D models
- **XImporter**: DirectX X format 3D models

## Processors

- **TextureProcessor**: Mipmap generation, format conversion, compression
- **FontDescriptionProcessor**: Font atlas generation
- **FontTextureProcessor**: Pre-rasterized font support
- **ModelProcessor**: Material compilation, bone conversion, vertex optimization
- **EffectProcessor**: Effect compilation