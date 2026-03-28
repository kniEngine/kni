# KNI .NET Templates (`nkast.Kni.Templates`)

This package provides KNI project templates for `dotnet new`.

## Install

```bash
dotnet new install nkast.Kni.Templates
```

## Uninstall

```bash
dotnet new uninstall nkast.Kni.Templates
```

## List installed templates

```bash
dotnet new list kni
```

## Usage

After installation, create a project from one of the installed KNI templates:

```bash
dotnet new <template-short-name> -n ProjectName
```

## Available templates

### Project templates

| Short name | Template | Platform | Graphics backend | Audio backend |
|---|---|---|---|---|
| `kni-android-gl` | KNI Android Game Project | Android | GL ES | OpenAL |
| `kni-blazor-gl` | KNI Web Game Project (.net8) | Blazor WebAssembly | WebGL | WebAudio |
| `kni-ios-gl` | KNI iOS Game Project | iOS | GL ES | OpenAL |
| `kni-oculus-gl` | KNI Oculus Game Project | Oculus | OpenGL | OpenAL |
| `kni-sdl2-gl` | KNI Cross Platform Desktop Game Project (.net8) | SDL2 (Windows/Linux/macOS) | OpenGL | OpenAL |
| `kni-sdl2-gl-netframework` | KNI Cross Platform Desktop Game Project (.NET Framework) | SDL2 (Windows/Linux/macOS) | OpenGL | OpenAL |
| `kni-winforms-dx11` | KNI Windows Game Project (.net8) | WinForms | DirectX11 | XAudio |
| `kni-winforms-dx11-netframework` | KNI Windows Game Project (.NET Framework) | WinForms | DirectX11 | XAudio |
| `kni-uap-core-dx11-netframework` | KNI Windows 10 Universal (Core) Game Project (.uap10) | UWP Core | DirectX11 | XAudio |
| `kni-uap-xaml-dx11-netframework` | KNI Windows 10 Universal (XAML) Game Project (.uap10) | UWP XAML | DirectX11 | XAudio |
| `kni-multiplatform` | KNI Multiplatform Game Project (.net8) | Android/Blazor/SDL2/WinForms | GLES/WebGL/OpenGL/DirectX11 | OpenAL/WebAudio/XAudio |
| `kni-multiplatform-netframework` | KNI Multiplatform Game Project (.NET Framework) | SDL2/WinForms/UWP | DirectX11/OpenGL | XAudio/OpenAL |
| `kni-ContentPipelineExtension` | KNI Content Pipeline Extension Project (.NetStandard) | - | - | - |

### Item templates

| Short name | Template |
|---|---|
| `KniDrawableGameComponent` | KNI DrawableGameComponent Item |
| `KniGameComponent` | KNI GameComponent Item |

## What is included

This package includes templates for KNI game projects and related project setups available under the `Templates/dotnetTemplates/content` folder.

## Links

- Repository: https://github.com/kniEngine/kni
- Issues: https://github.com/kniEngine/kni/issues
- Discussions: https://github.com/kniEngine/kni/discussions
