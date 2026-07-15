# Xna.Framework.XR

Virtual Reality (VR) and Augmented Reality (AR) support.

## Overview

The `Xna.Framework.XR` package provides VR/AR device integration for immersive experiences. It includes:

- **XR Device Support**: Headset and controller abstraction with session lifecycle control
- **Head Tracking**: Position and orientation of VR headset
- **Hand Tracking**: Hand and controller state monitoring

## Installation

```bash
dotnet add package nkast.Xna.Framework.XR
```

## Quick Start

```csharp
using Microsoft.Xna.Framework.XR;

// initialize XR device
xrDevice = new XRDevice("myVRGame", this.Services);
xrDevice.BeginSessionAsync(XRSessionMode.VR);
xrDevice.TrackFloorLevelAsync(true);

// Get VR Controllers state
GamePadState touchControllerState = TouchController.GetState(TouchControllerType.Touch);

// Draw on XR headset.
if (xrDevice.DeviceState == XRDeviceState.Enabled)
{
    xrDevice.BeginFrame();

	HeadsetState headsetState = xrDevice.GetHeadsetState();

	foreach (XREye eye in xrDevice.GetEyes())
    {
	    RenderTarget2D rt = xrDevice.GetEyeRenderTarget(eye);
		GraphicsDevice.SetRenderTarget(rt);
		GraphicsDevice.Clear(Color.CornflowerBlue);
		
		// Draw your scene here
		Matrix view = headsetState.GetEyeView(eye);
        Matrix projection = xrDevice.CreateProjection(eye, 0.05f, 1000);

		// Resolve and submit eye rendertarget.
        GraphicsDevice.SetRenderTarget(null);
        xrDevice.CommitRenderTarget(eye, rt);
    }

	xrDevice.EndFrame();
}
```
