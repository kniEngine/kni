# Xna.Framework.Input

Input device handling for keyboard, mouse, gamepad, touch, and joystick input.

## Overview

The `Xna.Framework.Input` package provides a unified interface for all input device types. It includes:

- **Mouse Input**: MouseState with position and button tracking, custom cursor support
- **GamePad Support**: Multi-gamepad input with triggers, thumbsticks, D-Pad, and buttons
- **Touch Input**: Multi-touch support with gesture recognition (tap, hold, double-tap, drag, flick, pinch)
- **Joystick Support**: Raw joystick axis and button access
- **XR Controllers**: Support for VR/XR hand controllers and input

## Installation

```bash
dotnet add package nkast.Xna.Framework.Input
```

## Quick Start

```csharp
using Microsoft.Xna.Framework.Input;

// Keyboard input
KeyboardState keyState = Keyboard.GetState();
if (keyState.IsKeyDown(Keys.W))
{
	// Move forward
}

// GamePad input
GamePadState padState = GamePad.GetState(PlayerIndex.One);
if (padState.IsConnected)
{
	Vector2 leftThumb = padState.ThumbSticks.Left;
	float rightTrigger = padState.Triggers.Right;
}

// Touch input with gestures
TouchPanel.EnableMouseTouchPoint = true;
TouchCollection touches = TouchPanel.GetState();
if (TouchPanel.IsGestureAvailable)
{
	GestureSample gesture = TouchPanel.ReadGesture();
	if (gesture.GestureType == GestureType.Tap)
	{
		// Handle tap
	}
}
```
