# KNI Effects Compiler (KNIFXC)

The KNIFXC tool is used to compile [DirectX Effect files](https://docs.microsoft.com/en-us/windows/win32/direct3d9/writing-an-effect) (shaders) for usage with KNI. It will compile shaders into fxo files ready to be consumed by the ```Effect``` class.

The KNIFXC uses the EffectProcessor to compile effects and extract them from the xnb file.

If you compile effects with KNIFXC you can load effects using the `Microsoft.Framework.Xna.Graphics.Effect` constructor that takes a byte array with the effect code.

Effects compiled with KNIFXC are not content files and can not be loaded by the `ContentManager`.

## Installation

KNIFXC can be installed as a [.NET tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools).
Make sure you have the .NET SDK installed. You can download it [here](https://dotnet.microsoft.com/download).

In a terminal run `dotnet tool install -g dotnet-knifxc` to install KNIFXC.

## Command Line

The command line options are:

```bat
KNIFXC <SourceFile> <OutputFile> [/Debug] [/Platform:<Windows,DesktopGL,Android>]
```

### Source File

The input effect file in typical FX format with samplers, techniques, and passes defined.  This parameter is required.

### Output File

The path to write the compiled effect to.  This parameter is required.

NOTE: The generated file is not an XNB file for use with the ContentManager.

If the `/Debug` flag is passed the resulting compiled effect file will contain extra debug information and the fewest possible optimizations.

### Platform Profile

The `/Platform` option defines the platform we're targeting with this effect file.  It can be one of the following:

- Windows
- DesktopGL
- Android

### Help

If you use `/?`, `/help`, or simply pass no parameters to KNIFXC you will get information about these command-line options.

## Runtime Use

The resulting compiled effect file can be used from your game code like so:

```csharp
byte[] bytecode = File.ReadAllBytes("mycompiled.fxo");
var effect = new Effect(bytecode);
```

This is how the stock effects (BasicEffect, DualTextureEffect, etc) are compiled and loaded.
