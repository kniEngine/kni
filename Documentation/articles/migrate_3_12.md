# Migrating from 3.12 to 3.13


## Migrating Framework

Edit your .csproj file and replace:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="3.12.9002" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="3.12.9002" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="3.12.9002" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="3.12.9002" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="3.12.9002" />
    <PackageReference Include="nkast.Xna.Framework.Input" Version="3.12.9001" />
    <PackageReference Include="nkast.Xna.Framework.Game" Version="3.12.9001" />
    <PackageReference Include="MonoGame.Framework.{Platform}.9000" Version="3.12.9002" />
```

to:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="3.13.9001" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="3.13.9001" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="3.13.9001" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="3.13.9001" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="3.13.9001" />
    <PackageReference Include="nkast.Xna.Framework.Input" Version="3.13.9001" />
    <PackageReference Include="nkast.Xna.Framework.Game" Version="3.13.9001" />
    <PackageReference Include="MonoGame.Framework.{Platform}.9000" Version="3.13.9001" />
```

For libraries, edit your .csproj file and replace:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="3.12.9002" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="3.12.9002" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="3.12.9002" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="3.12.9002" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="3.12.9002" />
    <PackageReference Include="nkast.Xna.Framework.Input" Version="3.12.9001" />
    <PackageReference Include="nkast.Xna.Framework.Game" Version="3.12.9001" />
```

to:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="3.13.9001" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="3.13.9001" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="3.13.9001" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="3.13.9001" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="3.13.9001" />
    <PackageReference Include="nkast.Xna.Framework.Input" Version="3.13.9001" />
    <PackageReference Include="nkast.Xna.Framework.Game" Version="3.13.9001" />
```

## Migrating Content Builder

Edit your .csproj file and replace:

```xml
  <ItemGroup>
    <PackageReference Include="nkast.Xna.Framework.Content.Pipeline.Builder" Version="3.12.9002" />
  </ItemGroup>
```

to:

```xml
  <ItemGroup>
    <PackageReference Include="nkast.Xna.Framework.Content.Pipeline.Builder" Version="3.13.9001" />
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
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.2" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="8.0.2" PrivateAssets="all" />
  </ItemGroup>
```

with:

```xml
  <ItemGroup Condition="'$(TargetFramework)' == 'net6.0'">
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="6.0.32" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="6.0.32" PrivateAssets="all" />
  </ItemGroup>
  <ItemGroup Condition=" '$(TargetFramework)' == 'net8.0' ">
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.7" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="8.0.7" PrivateAssets="all" />
  </ItemGroup>
```

Edit index.html file and replace:

```
    <script src="_content/nkast.Wasm.Dom/js/JSObject.8.0.0.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Window.8.0.0.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Document.8.0.0.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Media.8.0.0.js"></script>
    <script src="_content/nkast.Wasm.XHR/js/XHR.8.0.0.js"></script>
    <script src="_content/nkast.Wasm.Canvas/js/Canvas.8.0.0.js"></script>
    <script src="_content/nkast.Wasm.Canvas/js/CanvasGLContext.8.0.0.js"></script>
    <script src="_content/nkast.Wasm.Audio/js/Audio.8.0.0.js"></script>
```

with:

```
    <script src="_content/nkast.Wasm.Dom/js/JSObject.8.0.1.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Window.8.0.1.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Document.8.0.1.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Navigator.8.0.1.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Gamepad.8.0.1.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Media.8.0.1.js"></script>
    <script src="_content/nkast.Wasm.XHR/js/XHR.8.0.1.js"></script>
    <script src="_content/nkast.Wasm.Canvas/js/Canvas.8.0.1.js"></script>
    <script src="_content/nkast.Wasm.Canvas/js/CanvasGLContext.8.0.1.js"></script>
    <script src="_content/nkast.Wasm.Audio/js/Audio.8.0.1.js"></script>
```

### Migrating OculusVR projects

Edit your .csproj file and replace:

```xml
    <PackageReference Include="nkast.Xna.Framework.Oculus.OvrDX11" Version="3.12.9001" />
```

with:

```xml
    <PackageReference Include="nkast.Xna.Framework.Oculus.OvrDX11" Version="3.13.9002" />
```
