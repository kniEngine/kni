# Xna.Framework.Audio

Audio playback, sound effects, and music for KNI games.

## Overview

The `Xna.Framework.Audio` package provides audio playback capabilities with support for both immediate sound effects and streaming audio. It includes:

- **Sound Effects**: SoundEffect and SoundEffectInstance
- **3D Audio**: AudioEmitter and AudioListener for spatial audio simulation
- **Microphone Input**: Audio capture from microphone devices

## Installation

```bash
dotnet add package nkast.Xna.Framework.Audio
```

## Quick Start

```csharp
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

// Load and play a sound effect
SoundEffect effect = contentManager.Load<SoundEffect>("Sounds/mySoundEffect");
effect.Play();

// Play a sound with control
SoundEffectInstance instance = effect.CreateInstance();
instance.Volume = 0.5f;
instance.Play();

// 3D audio
AudioEmitter emitter = new AudioEmitter { Position = soundPosition };
AudioListener listener = new AudioListener { Position = cameraPosition };
instance.Apply3D(listener, emitter);
```
