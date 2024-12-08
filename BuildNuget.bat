dotnet pack src\Xna.Framework\Xna.Framework.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release
dotnet pack src\Xna.Framework.Content\Xna.Framework.Content.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release
dotnet pack src\Xna.Framework.Graphics\Xna.Framework.Graphics.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release
dotnet pack src\Xna.Framework.Audio\Xna.Framework.Audio.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release
dotnet pack src\Xna.Framework.Media\Xna.Framework.Media.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release
dotnet pack src\Xna.Framework.Input\Xna.Framework.Input.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release
dotnet pack src\Xna.Framework.Game\Xna.Framework.Game.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release
dotnet pack src\Xna.Framework.Devices\Xna.Framework.Devices.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release
dotnet pack src\Xna.Framework.Storage\Xna.Framework.Storage.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release
dotnet pack src\Xna.Framework.XR\Xna.Framework.XR.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release
dotnet pack src\Xna.Framework.Design\Xna.Framework.Design.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release

dotnet pack src\Xna.Framework.Content.Pipeline\Xna.Framework.Content.Pipeline.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release
dotnet pack src\Xna.Framework.Content.Pipeline.Audio\Xna.Framework.Content.Pipeline.Audio.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release
dotnet pack src\Xna.Framework.Content.Pipeline.Graphics\Xna.Framework.Content.Pipeline.Graphics.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release
dotnet pack src\Xna.Framework.Content.Pipeline.Media\Xna.Framework.Content.Pipeline.Media.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release

"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/Content.Pipeline.Builder.nuspec	        -OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.14.9001  -Properties Configuration=Release
"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/Content.Pipeline.Builder.Windows.nuspec	-OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.14.9001  -Properties Configuration=Release

"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/MonoGame.Framework.Android.nuspec			-OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.14.9001.0  -Properties Configuration=Release
"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/MonoGame.Framework.iOS.nuspec				-OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.14.9001.0  -Properties Configuration=Release
"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/MonoGame.Framework.WindowsUniversal.nuspec	-OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.14.9001.0  -Properties Configuration=Release

dotnet pack Platforms\Xna.Framework.WindowsDX11.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release
dotnet pack Platforms\Xna.Framework.DesktopGL.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release
dotnet pack XNA.Framework.Blazor.sln --output NuGetPackages\Output\ /t:Build /p:Configuration=Release 
dotnet pack Platforms\Xna.Framework.Ref.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release 
dotnet pack Platforms\XNA.Framework.Oculus.OvrDX11.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release 

dotnet pack Platforms\Xna.Framework.Cardboard.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release

@pause
