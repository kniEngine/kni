# Migrating from 3.10.9001 to 3.11.9001


## Migrating Framework

Edit your .csproj file and replace:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="3.10.9001" />
    <PackageReference Include="MonoGame.Framework.{Platform}.9000" Version="3.10.9001" />
```

to:

```xml   
    <PackageReference Include="nkast.Xna.Framework" Version="3.11.9001" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="3.11.9001" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="3.11.9001" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="3.11.9001" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="3.11.9001" />
    <PackageReference Include="MonoGame.Framework.{Platform}.9000" Version="3.11.9001" />
```

## Migrating Content Builder

Edit your .csproj file and remove:

```xml
  <Import Project="$(MSBuildExtensionsPath)\MonoGame\v3.0\Kni.Content.Builder.targets" />
```

Then add:

```xml
  <ItemGroup>
    <PackageReference Include="nkast.Xna.Framework.Content.Pipeline.Builder" Version="3.11.9001" />
  </ItemGroup>
```

if your importers require Windows libraries (WinForms,WPF), use the 'nkast.Xna.Framework.Content.Pipeline.Builder.Windows' package.

### Migrating BlazorGL projects

Edit your .csproj file and replace:

```xml
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="6.0.11" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="6.0.11" PrivateAssets="all" />
```

with:

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

Edit index.html file and replace:

```
    <script src="_content/nkast.Wasm.Dom/js/JSObject.6.0.5.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Window.6.0.5.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Document.6.0.5.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Media.6.0.5.js"></script>
    <script src="_content/nkast.Wasm.XHR/js/XHR.6.0.5.js"></script>
    <script src="_content/nkast.Wasm.Canvas/js/Canvas.6.0.5.js"></script>
    <script src="_content/nkast.Wasm.Canvas/js/CanvasGLContext.6.0.5.js"></script>
    <script src="_content/nkast.Wasm.Audio/js/Audio.6.0.5.js"></script>
```

with:

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

