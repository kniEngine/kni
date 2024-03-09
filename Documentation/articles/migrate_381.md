# Migrating from MonoGame 3.8.1 to KNI 3.11.9001


## Migrating Framework

Edit your .csproj file of the main project and replace:

```xml
    <PackageReference Include="MonoGame.Framework.{Platform}" Version="3.8.1.303" />
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

where {Platform} is Windows, DesktopGL, Android, etc.

For libraries, edit your .csproj file and replace:

```xml    
    <PackageReference Include="MonoGame.Framework.DesktopGL" Version="3.8.1.303" PrivateAssets="All" />
```

to:

```xml   
    <PackageReference Include="nkast.Xna.Framework" Version="3.11.9001" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="3.11.9001" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="3.11.9001" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="3.11.9001" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="3.11.9001" />
    <PackageReference Include="nkast.Xna.Framework.Ref" Version="3.11.9001" PrivateAssets="All" />
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
    <PackageReference Include="nkast.Xna.Framework.Content.Pipeline.Builder" Version="3.11.9001" />
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


## Migrating Effects (optional)

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
