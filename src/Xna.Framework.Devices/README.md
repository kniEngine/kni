# Xna.Framework.Devices

Device sensor and haptic feedback support.

## Overview

The `Xna.Framework.Devices` package provides access to device sensors and haptic feedback. It includes:

- **Accelerometer**: 3-axis acceleration measurement
- **Compass**: Magnetic heading and direction
- **Haptics**: Vibration and haptic feedback control

## Installation

```bash
dotnet add package nkast.Xna.Framework.Devices
```

## Quick Start

```csharp
using Microsoft.Xna.Framework.Devices;

// Accelerometer input
if (Accelerometer.IsSupported)
{
	Accelerometer.Initialize();
	AccelerometerReading reading = Accelerometer.GetState();
	Vector3 acceleration = reading.Acceleration;

	Accelerometer.ReadingChanged += (s, e) =>
	{
		// Handle real-time acceleration changes
	};
}

// Compass heading
if (Compass.IsSupported)
{
	Compass.Initialize();
	CompassReading reading = Compass.GetState();
	float heading = reading.Heading;
}

// Haptic feedback
Haptics.SetVibration(1.0f); // Full vibration
```
