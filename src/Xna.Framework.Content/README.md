# Xna.Framework.Content

Runtime asset loading and content management for KNI games.

## Overview

The `Xna.Framework.Content` package provides the runtime content loading system for compiled game assets. It includes:

- **ContentManager**: Asset loading and caching with RootDirectory support
- **Content Readers**: Type-specific readers for deserializing compiled content
- **Title Container**: Abstraction for game asset storage locations
- **ResourceContentManager**: Load content from .resx resource files
- **Type Reader Support**: Extensible reader system for custom types

## Installation

```bash
dotnet add package nkast.Xna.Framework.Content
```

## Quick Start

```csharp
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

// Initialize content manager (typically done in Game.LoadContent)
ContentManager contentManager = new ContentManager(serviceProvider, "Content");

// Load compiled assets
Texture2D texture = contentManager.Load<Texture2D>("Textures/player");
SpriteFont spriteFont = contentManager.Load<SpriteFont>("Fonts/arial");
Effect effect = contentManager.Load<Effect>("Effects/basic");

// Clean up all loaded content
contentManager.Dispose();
```

## Notes

- This package loads pre-compiled content created with the Content Pipeline tools
- All assets must be built using **Xna.Framework.Content.Pipeline** before runtime
