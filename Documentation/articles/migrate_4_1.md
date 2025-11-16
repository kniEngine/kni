# Migrating from 4.1 to 4.2


## Migrating Framework

Edit your .csproj file and replace:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.Devices" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.Input" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.Game" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.Storage" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.XR" Version="4.1.9001" />
    <PackageReference Include="nkast.Kni.Platform.{Platform}." Version="4.1.9001" />
```

to:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.Devices" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.Input" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.Game" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.Storage" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.XR" Version="4.2.9001" />
    <PackageReference Include="nkast.Kni.Platform.{Platform}" Version="4.2.9001" />
```

For libraries, edit your .csproj file and replace:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.Devices" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.Input" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.Game" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.Storage" Version="4.1.9001" />
    <PackageReference Include="nkast.Xna.Framework.XR" Version="4.1.9001" />
```

to:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.Devices" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.Input" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.Game" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.Storage" Version="4.2.9001" />
    <PackageReference Include="nkast.Xna.Framework.XR" Version="4.2.9001" />
```

## Migrating Content Builder

Edit your .csproj file and replace:

```xml
  <ItemGroup>
    <PackageReference Include="nkast.Xna.Framework.Content.Pipeline.Builder" Version="4.1.9001" />
  </ItemGroup>
```

to:

```xml
  <ItemGroup>
    <PackageReference Include="nkast.Xna.Framework.Content.Pipeline.Builder" Version="4.2.9001" />
  </ItemGroup>
```

if your importers require Windows libraries (WinForms,WPF), use the 'nkast.Xna.Framework.Content.Pipeline.Builder.Windows' package.


### Migrating Blazor.GL projects

Edit your .csproj file and replace:

```xml
  <ItemGroup Condition=" '$(TargetFramework)' == 'net8.0' ">
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.11" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="8.0.11" PrivateAssets="all" />
  </ItemGroup>
```

with:

```xml
  <ItemGroup Condition=" '$(TargetFramework)' == 'net8.0' ">
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.17" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="8.0.17" PrivateAssets="all" />
  </ItemGroup>
```

Edit index.html file and replace:

```xml
    <script src="_content/nkast.Wasm.Dom/js/JSObject.8.0.5.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Window.8.0.5.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Document.8.0.5.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Navigator.8.0.5.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Gamepad.8.0.5.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Media.8.0.5.js"></script>
    <script src="_content/nkast.Wasm.XHR/js/XHR.8.0.5.js"></script>
    <script src="_content/nkast.Wasm.Canvas/js/Canvas.8.0.5.js"></script>
    <script src="_content/nkast.Wasm.Canvas/js/CanvasGLContext.8.0.5.js"></script>
    <script src="_content/nkast.Wasm.Audio/js/Audio.8.0.5.js"></script>
    <script src="_content/nkast.Wasm.XR/js/XR.8.0.5.js"></script>
```

with:

```xml
    <script src="_content/nkast.Wasm.JSInterop/js/JSObject.8.0.11.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Window.8.0.11.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Document.8.0.11.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Navigator.8.0.11.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Gamepad.8.0.11.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Media.8.0.11.js"></script>
    <script src="_content/nkast.Wasm.XHR/js/XHR.8.0.11.js"></script>
    <script src="_content/nkast.Wasm.Canvas/js/Canvas.8.0.11.js"></script>
    <script src="_content/nkast.Wasm.Canvas/js/CanvasGLContext.8.0.11.js"></script>
    <script src="_content/nkast.Wasm.Audio/js/Audio.8.0.11.js"></script>
    <script src="_content/nkast.Wasm.XR/js/XR.8.0.11.js"></script>
```

Create a new Blazor.GL project, and copy the files:
  \wwwroot\js\micProcessor.js 
  \wwwroot\js\streamProcessor.js


### Migrating Oculus.GL projects

Edit your Application.csproj file and after <TargetFramework>, add:

```xml
    <SupportedOSPlatformVersion>32.0</SupportedOSPlatformVersion>
```


## Migrating Effects

Edit your .fx file and replace the following defines:

 replace DEBUG with `__DEBUG__`  <br>
 replace MGFX with `__KNIFX__`  <br>
 replace HLSL and SM4 with `__DIRECTX__`  <br>
 replace GLSL , OPENGL and `__OPENGL__` with `__GL__` or (`__OPENGL__` || `__GLES__`)  <br>



