dotnet pack src\Xna.Framework\Xna.Framework.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release
dotnet pack src\Xna.Framework.Design\Xna.Framework.Design.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release

"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/nkast.Xna.Framework.Content.Pipeline.nuspec			-OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.10.9001.0  -Properties Configuration=Release
"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/nkast.Xna.Framework.Content.Pipeline.Audio.nuspec		-OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.10.9001.0  -Properties Configuration=Release
"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/nkast.Xna.Framework.Content.Pipeline.Graphics.nuspec	-OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.10.9001.0  -Properties Configuration=Release
"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/nkast.Xna.Framework.Content.Pipeline.Media.nuspec		-OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.10.9001.0  -Properties Configuration=Release

"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/MonoGame.Framework.WindowsDX.nuspec		-OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.10.9001.0  -Properties Configuration=Release
"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/MonoGame.Framework.DesktopGL.nuspec		-OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.10.9001.0  -Properties Configuration=Release
"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/MonoGame.Framework.Android.nuspec			-OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.10.9001.0  -Properties Configuration=Release
"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/MonoGame.Framework.iOS.nuspec				-OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.10.9001.0  -Properties Configuration=Release
"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/MonoGame.Framework.WindowsUniversal.nuspec	-OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.10.9001.0  -Properties Configuration=Release

dotnet pack XNA.Framework.Blazor.sln --output NuGetPackages\Output\ /t:Build /p:Configuration=Release 
dotnet pack MonoGame.Framework\Xna.Framework.Ref.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release 
dotnet pack MonoGame.Framework\XNA.Framework.Oculus.OvrDX11.csproj --output NuGetPackages\Output\ /t:Build /p:Configuration=Release 

"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/MonoGame.Framework.Cardboard.nuspec        -OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.10.9001.0  -Properties Configuration=Release

@pause
