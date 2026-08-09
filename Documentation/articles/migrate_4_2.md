# Migrating from 4.2 to 4.3


## Migrating Framework

Edit your .csproj file and replace:

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

to:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.Devices" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.Input" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.Game" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.Storage" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.XR" Version="4.3.9001" />
    <PackageReference Include="nkast.Kni.Platform.{Platform}" Version="4.3.9001" />
```

For libraries, edit your .csproj file and replace:

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

to:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.Devices" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.Input" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.Game" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.Storage" Version="4.3.9001" />
    <PackageReference Include="nkast.Xna.Framework.XR" Version="4.3.9001" />
```

## Migrating Content Builder

Edit your .csproj file and replace:

```xml
  <ItemGroup>
    <PackageReference Include="nkast.Xna.Framework.Content.Pipeline.Builder" Version="4.2.9001" />
  </ItemGroup>
```

to:

```xml
  <ItemGroup>
    <PackageReference Include="nkast.Xna.Framework.Content.Pipeline.Builder" Version="4.3.9001" />
  </ItemGroup>
```

if your importers require Windows libraries (WinForms,WPF), use the 'nkast.Xna.Framework.Content.Pipeline.Builder.Windows' package.


### Migrating Blazor.GL projects

Edit your .csproj file and replace:

```xml
  <ItemGroup Condition=" '$(TargetFramework)' == 'net8.0' ">
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.17" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="8.0.17" PrivateAssets="all" />
  </ItemGroup>
```

with:

```xml
  <ItemGroup Condition=" '$(TargetFramework)' == 'net8.0' ">
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.29" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="8.0.29" PrivateAssets="all" />
  </ItemGroup>
  <ItemGroup Condition=" '$(TargetFramework)' == 'net10.0' ">
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="10.0.10" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="10.0.10" PrivateAssets="all" />
  </ItemGroup>
```

Edit index.html file and replace:

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

with:

```xml
    <script src="_content/nkast.Wasm.JSInterop/js/JSObject.10.0.3.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Window.10.0.3.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Document.10.0.3.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Navigator.10.0.3.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Gamepad.10.0.3.js"></script>
    <script src="_content/nkast.Wasm.Dom/js/Media.10.0.3.js"></script>
    <script src="_content/nkast.Wasm.XHR/js/XHR.10.0.3.js"></script>
    <script src="_content/nkast.Wasm.Canvas/js/Canvas.10.0.3.js"></script>
    <script src="_content/nkast.Wasm.Canvas/js/CanvasGLContext.10.0.3.js"></script>
    <script src="_content/nkast.Wasm.Audio/js/Audio.10.0.3.js"></script>
    <script src="_content/nkast.Wasm.XR/js/XR.10.0.3.js"></script>
```

Create a new Blazor.GL project, and replace the old streamProcessor file with the new one:
  Delete \wwwroot\js\streamProcessor.js 
  Copy \wwwroot\js\streamProcessor2.js from the new project


### Migrating Android and Oculus.GL projects

Edit your Application.csproj file and replace:

```xml
    <TargetFramework>net8.0-android</TargetFramework>
```

with:

```xml
    <TargetFramework>net10.0-android</TargetFramework>
    <AndroidEnableMarshalMethods>false</AndroidEnableMarshalMethods>
```

For Oculus.GL projects, edit AndroidManifest.xml and replace:

```xml
    <uses-sdk android:minSdkVersion="32" android:targetSdkVersion="32" />
```

with:

```xml
    <uses-sdk android:minSdkVersion="32" android:targetSdkVersion="34" />
    <AndroidEnableMarshalMethods>false</AndroidEnableMarshalMethods>
```


