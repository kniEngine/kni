# Migrating from 4.0 to 4.1


## Migrating Framework

Edit your .csproj file and replace:

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
    <PackageReference Include="nkast.Kni.Platform.{Platform}." Version="4.0.9001" />
```

to:

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
    <PackageReference Include="nkast.Kni.Platform.{Platform}" Version="4.1.9001" />
```

For libraries, edit your .csproj file and replace:

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

to:

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

## Migrating Content Builder

Edit your .csproj file and replace:

```xml
  <ItemGroup>
    <PackageReference Include="nkast.Xna.Framework.Content.Pipeline.Builder" Version="4.0.9001" />
  </ItemGroup>
```

to:

```xml
  <ItemGroup>
    <PackageReference Include="nkast.Xna.Framework.Content.Pipeline.Builder" Version="4.1.9001" />
  </ItemGroup>
```

if your importers require Windows libraries (WinForms,WPF), use the 'nkast.Xna.Framework.Content.Pipeline.Builder.Windows' package.


### Migrating Blazor.GL projects


Edit /wwwroot/css/app.css file and replace:

```
#theCanvas 
{
    position: fixed;
    top: 0px;
    right: 0px;
    bottom: 0px;
    left: 0px;
}
```

with:

```
 #theCanvas 
{
    position: fixed;
    top: 0px;
    right: 0px;
    bottom: 0px;
    left: 0px;

    /* Disable text highlighting and magnifying glass on iPhone/webkit */
    -webkit-user-select: none;
}
```

### Migrating Oculus.GL projects

Edit your AndroidManifest.xml file and add:

```xml
	<uses-sdk android:minSdkVersion="32" android:targetSdkVersion="32" />
```


