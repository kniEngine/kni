# Xna.Framework

Core mathematical and foundational types for the KNI game framework.

## Overview

The `Xna.Framework` package provides essential mathematical primitives and base types. It includes:

- **Mathematical Types**: Vector2, Vector3, Vector4, Matrix, Quaternion, Complex
- **Geometric Shapes**: BoundingBox, BoundingSphere, BoundingFrustum, Rectangle, Point, Ray, Plane
- **Animation Support**: Curve, CurveKey, CurveKeyCollection, Pose2D, Pose3D
- **Framework Dispatcher**: Event loop management

## Installation

```bash
dotnet add package nkast.Xna.Framework
```

## Quick Start

```csharp
using Microsoft.Xna.Framework;

// Create vectors
Vector3 pos = new Vector3(10, 20, 30);
Vector3 direction = Vector3.Normalize(pos);

// Work with matrices
Matrix world = Matrix.Identity;
world.Translation = pos;

// Use bounding volumes
BoundingSphere sphere = new BoundingSphere(pos, 5f);
ContainmentType containment = sphere.Contains(pos);
```
