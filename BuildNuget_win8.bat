"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/MonoGame.Framework.Windows8.nuspec         -OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.8.9100.0  -Properties Configuration=Release
"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/MonoGame.Framework.WindowsPhone8.nuspec	-OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.8.9100.0  -Properties Configuration=Release
"C:\Program Files (x86)\NuGet3\nuget.exe" pack NuGetPackages/MonoGame.Framework.WindowsPhone81.nuspec	-OutputDirectory NuGetPackages\Output\  -BasePath .  -Version 3.8.9100.0  -Properties Configuration=Release

@pause