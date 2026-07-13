# Xna.Framework.Graphics

GPU graphics rendering and shader support for the KNI game framework.

## Overview

The `Xna.Framework.Graphics` package provides a complete graphics API abstraction layer. It includes:

- **Core Rendering**: GraphicsDevice, SpriteBatch, Model and mesh rendering
- **Textures & Buffers**: Texture2D, Texture3D, TextureCube, VertexBuffer, IndexBuffer
- **Render Targets**: RenderTarget2D, RenderTarget3D, RenderTargetCube with MSAA support
- **Render States**: BlendState, DepthStencilState, RasterizerState, SamplerState
- **Effects & Shaders**: Effect system, built-in effects (BasicEffect, SkinnedEffect, etc.)
- **Content Loading**: Automatic deserialization of compiled graphics assets
- **Packed Vector Formats**: Optimized in-memory color formats

## Installation

```bash
dotnet add package nkast.Xna.Framework.Graphics
```

## Quick Start

```csharp
using Microsoft.Xna.Framework.Graphics;

// Initialize graphics device (see Xna.Framework.Game)
GraphicsDevice graphics = new GraphicsDevice(/* ... */);

// Load textures
texture = contentManager.Load<Texture2D>("Textures/myTexture");

// Create a sprite batch
SpriteBatch spriteBatch = new SpriteBatch(graphics);

// Render a frame
graphics.Clear(Color.Black);
spriteBatch.Begin();
spriteBatch.Draw(texture, Vector2.Zero, Color.White);
spriteBatch.End();
```
