# Migrating from 3.11 to 3.12


## Migrating Framework

Edit your .csproj file and replace:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="3.11.9002" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="3.11.9002" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="3.11.9002" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="3.11.9002" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="3.11.9002" />
    <PackageReference Include="MonoGame.Framework.{Platform}.9000" Version="3.11.9002" />
```

to:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="3.12.9001" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="3.12.9001" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="3.12.9001" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="3.12.9001" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="3.12.9001" />
    <PackageReference Include="nkast.Xna.Framework.Input" Version="3.12.9001" />
    <PackageReference Include="nkast.Xna.Framework.Game" Version="3.12.9001" />
    <PackageReference Include="MonoGame.Framework.{Platform}.9000" Version="3.12.9001" />
```

For libraries, edit your .csproj file and replace:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="3.11.9002" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="3.11.9002" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="3.11.9002" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="3.11.9002" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="3.11.9002" />
    <PackageReference Include="nkast.Xna.Framework.Ref" Version="3.11.9002" PrivateAssets="All" />
```

to:

```xml
    <PackageReference Include="nkast.Xna.Framework" Version="3.12.9001" />
    <PackageReference Include="nkast.Xna.Framework.Content" Version="3.12.9001" />
    <PackageReference Include="nkast.Xna.Framework.Graphics" Version="3.12.9001" />
    <PackageReference Include="nkast.Xna.Framework.Audio" Version="3.12.9001" />
    <PackageReference Include="nkast.Xna.Framework.Media" Version="3.12.9001" />
    <PackageReference Include="nkast.Xna.Framework.Input" Version="3.12.9001" />
    <PackageReference Include="nkast.Xna.Framework.Game" Version="3.12.9001" />
```

## Migrating Content Builder

Edit your .csproj file and replace:

```xml
  <ItemGroup>
    <PackageReference Include="nkast.Xna.Framework.Content.Pipeline.Builder" Version="3.11.9002" />
  </ItemGroup>
```

to:

```xml
  <ItemGroup>
    <PackageReference Include="nkast.Xna.Framework.Content.Pipeline.Builder" Version="3.12.9001" />
  </ItemGroup>
```

if your importers require Windows libraries (WinForms,WPF), use the 'nkast.Xna.Framework.Content.Pipeline.Builder.Windows' package.

