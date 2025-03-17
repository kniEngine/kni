# Migrating from 3.14 to 4.0


## Migrating Framework

Edit your .csproj file and replace:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="3.14.9001" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="3.14.9001" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="3.14.9001" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="3.14.9001" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="3.14.9001" />
    <PackageReference Include="nkast.Xna.Framework.Input" Version="3.14.9001" />
    <PackageReference Include="nkast.Xna.Framework.Game" Version="3.14.9001" />
    <PackageReference Include="MonoGame.Framework.{Platform}.9000" Version="3.14.9001" />
```

to:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.Devices" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.Input" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.Game" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.Storage" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.XR" Version="4.0.9001" />
    <PackageReference Include="nkast.Kni.Platform.{Platform}" Version="4.0.9001" />
```

where {Platform} maps as follows.
 - Android - > Android.GL
 - BlazorGL - > Blazor.GL
 - Cardboard - > Cardboard.GL
 - DesktopGL - > SDL2.GL 
 - iOS -> iOS.GL
 - WindowsUniversal -> UAP.DX11
 - WindowsDX - > WinForms.DX11

For libraries, edit your .csproj file and replace:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="3.14.9001" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="3.14.9001" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="3.14.9001" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="3.14.9001" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="3.14.9001" />
    <PackageReference Include="nkast.Xna.Framework.Input" Version="3.14.9001" />
    <PackageReference Include="nkast.Xna.Framework.Game" Version="3.14.9001" />
```

to:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.Devices" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.Input" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.Game" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.Storage" Version="4.0.9001" />
    <PackageReference Include="nkast.Xna.Framework.XR" Version="4.0.9001" />
```

## Migrating Content Builder

Edit your .csproj file and replace:

```xml
  <ItemGroup>
    <PackageReference Include="nkast.Xna.Framework.Content.Pipeline.Builder" Version="3.14.9001" />
  </ItemGroup>
```

to:

```xml
  <ItemGroup>
    <PackageReference Include="nkast.Xna.Framework.Content.Pipeline.Builder" Version="4.0.9001" />
  </ItemGroup>
```

if your importers require Windows libraries (WinForms,WPF), use the 'nkast.Xna.Framework.Content.Pipeline.Builder.Windows' package.


### Migrating BlazorGL projects

Edit your .csproj file and replace:

```xml
  <ItemGroup Condition="'$(TargetFramework)' == 'net6.0'">
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="6.0.27" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="6.0.27" PrivateAssets="all" />
  </ItemGroup>
  <ItemGroup Condition=" '$(TargetFramework)' == 'net8.0' ">
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.7" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="8.0.7" PrivateAssets="all" />
  </ItemGroup>
```

with:

```xml
  <ItemGroup Condition=" '$(TargetFramework)' == 'net8.0' ">
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.11" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="8.0.11" PrivateAssets="all" />
  </ItemGroup>
```

Edit index.html file and replace:

```
    <script src="_content/nkast.Wasm.Dom/js/JSObject.8.0.2.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Window.8.0.2.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Document.8.0.2.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Media.8.0.2.js"></script>
    <script src="_content/nkast.Wasm.XHR/js/XHR.8.0.2.js"></script>
    <script src="_content/nkast.Wasm.Canvas/js/Canvas.8.0.2.js"></script>
    <script src="_content/nkast.Wasm.Canvas/js/CanvasGLContext.8.0.2.js"></script>
    <script src="_content/nkast.Wasm.Audio/js/Audio.8.0.2.js"></script>
```

with:

```
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

after this line:

```
    import { BrotliDecode } from './js/decode.min.js';
```

add

```
    import { BrotliDecode } from './js/decode.min.js';
    window.BrotliDecode = BrotliDecode;
```


### Migrating OculusVR projects

Edit your .csproj file and replace:

```xml
    <PackageReference Include="nkast.Xna.Framework.Oculus.OvrDX11" Version="3.14.9001" />
```

with:

```xml
    <PackageReference Include="nkast.Kni.Platform.WinForms.DX11.OculusOVR" Version="4.0.9001" />
```

In Program.cs, before creating the the Game instance,

```
    using (var game = new $ext_safeprojectname$Game())
        game.Run();
```

add:
```
    Microsoft.Xna.Platform.XR.XRFactory.RegisterXRFactory(new Microsoft.Xna.Platform.XR.LibOVR.ConcreteXRFactory());
    using (var game = new $ext_safeprojectname$Game())
        game.Run();
```

