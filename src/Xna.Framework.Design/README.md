# Xna.Framework.Design

Design-time support for Xna.Framework types.

## Overview

The `Xna.Framework.Design` package provides type converters that enable proper serialization and editing of Xna.Framework types. It includes:

- **Type Converters**: ColorConverter, Vector2Converter, Vector3Converter, Vector4Converter
- **Designer Support**: Enables XAML and Form designers to work with XNA types
- **Serialization**: Proper conversion between types and their string representations

## Installation

```bash
dotnet add package nkast.Xna.Framework.Design
```

## Usage

This package is typically only needed at design-time in Editor Tools. It's automatically discovered by the designer framework.

```csharp
using Microsoft.Xna.Framework.Design;

// Type converters are automatically used by .net
// when editing properties of type Color, Vector2, etc.
// Example in XAML or property editors:
// Background="#FF0000" or "Red"
// Offset="10,20"
```
