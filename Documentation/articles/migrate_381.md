# Migrating from MonoGame 3.8.1 to KNI 4.2


## Migrating Framework

Edit your .csproj file of the main project and replace:

```xml
    <PackageReference Include="MonoGame.Framework.{Platform}" Version="3.8.1.303" />
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

where {Platform} maps as follows.
 - Android - > Android.GL
 - DesktopGL - > SDL2.GL 
 - iOS -> iOS.GL
 - WindowsUniversal -> UAP.DX11
 - WindowsDX - > WinForms.DX11

For libraries, edit your .csproj file and replace:

```xml
    <PackageReference Include="MonoGame.Framework.DesktopGL" Version="3.8.1.303" PrivateAssets="All" />
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

### Migrating Framework (Android)

Edit your Activity1.cs file and replace:

```xml
  ScreenOrientation = ScreenOrientation.FullUser,
```

To:

```xml
  ScreenOrientation = ScreenOrientation.FullSensor,
```


## Migrating Content Builder

Edit your .csproj file and add:

```xml
  <PropertyGroup>
    <KniPlatform>{Platform}</KniPlatform>
  </PropertyGroup>
```

Where {Platform} is Windows, DesktopGL, Android, etc.

Then replace:

```xml
    <PackageReference Include="MonoGame.Content.Builder.Task" Version="3.8.1.303" />
```

With:

```xml
    <PackageReference Include="nkast.Xna.Framework.Content.Pipeline.Builder" Version="4.2.9001" />
```

Then rename 'MonoGameContentReference':

```xml
    <MonoGameContentReference Include="Content\Content.mgcb">
```

With 'KniContentReference':

```xml
     <KniContentReference Include="Content\Content.mgcb">
```



if your importers require Windows libraries (WinForms,WPF), use the 'nkast.Xna.Framework.Content.Pipeline.Builder.Windows' package.


## Migrating Effects

Edit your .fx file and rename 'VS_SHADERMODEL' and 'PS_SHADERMODEL':

```
    pass Pass0
	{   
		VertexShader = compile VS_SHADERMODEL VSMethod();
		PixelShader  = compile PS_SHADERMODEL PSMethod();
	}
```

With 'vs_4_0_level_9_1' and 'ps_4_0_level_9_1':

``` 
    pass Pass0
	{   
		VertexShader = compile vs_4_0_level_9_1 VSMethod();
		PixelShader  = compile ps_4_0_level_9_1 PSMethod();
	}
```

Then remove:

```
#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif
```

Replace the following defines:

 DEBUG with `__DEBUG__`  <br>
 MGFX with `__KNIFX__`  <br>
 HLSL and SM4 with `__DIRECTX__`  <br>
 GLSL and OPENGL with `__GL__` or (`__OPENGL__` || `__GLES__`)  <br>

## Trimming (optional)

### Enable Trimming (Android)

Edit your .csproj file and add:

```xml
  <PropertyGroup>
    <IsTrimmable>True</IsTrimmable>
	<TrimMode>partial</TrimMode>	
  </PropertyGroup>
```

### Enable Trimming and Aot (DesktopGL)

Edit your .csproj file and upgrade TargetFramework from net6.0 to net8.0.
Then add:

```xml
  <PropertyGroup>
    <PublishTrimmed>True</PublishTrimmed>
    <PublishAot>True</PublishAot>	
  </PropertyGroup>
```
