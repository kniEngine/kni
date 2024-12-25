set username=username

"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework                           3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Design                    3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Content                   3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Audio                     3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Graphics                  3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Media                     3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Input                     3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Game                      3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Devices                   3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Storage                   3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.XR                        3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive

"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Content.Pipeline          3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Content.Pipeline.Audio    3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Content.Pipeline.Graphics 3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Content.Pipeline.Media    3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive

"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Content.Pipeline.Builder  3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages"   -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Content.Pipeline.Builder.Windows  3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages"   -NonInteractive

"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Platform.Blazor.GL                  3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Platform.WinForms.DX11              3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Platform.UAP.DX11                   3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Platform.SDL2.GL                    3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Platform.Android.GL                 3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive

"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Platform.WinForms.DX11.OculusOVR    3.14.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive


"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.3.14.9001.nupkg                           -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Design.3.14.9001.nupkg                    -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Content.3.14.9001.nupkg                   -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Audio.3.14.9001.nupkg                     -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Graphics.3.14.9001.nupkg                  -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Media.3.14.9001.nupkg                     -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Input.3.14.9001.nupkg                     -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Game.3.14.9001.nupkg                      -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Devices.3.14.9001.nupkg                   -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Storage.3.14.9001.nupkg                   -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.XR.3.14.9001.nupkg                        -Source "C:\Users\%username%\.nuget\localPackages"

"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Content.Pipeline.3.14.9001.nupkg          -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Content.Pipeline.Audio.3.14.9001.nupkg    -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Content.Pipeline.Graphics.3.14.9001.nupkg -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Content.Pipeline.Media.3.14.9001.nupkg    -Source "C:\Users\%username%\.nuget\localPackages"

"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Content.Pipeline.Builder.3.14.9001.nupkg  -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Content.Pipeline.Builder.Windows.3.14.9001.nupkg -Source "C:\Users\%username%\.nuget\localPackages"

"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Platform.Blazor.GL.3.14.9001.nupkg             -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Platform.WinForms.DX11.3.14.9001.nupkg         -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Platform.UAP.DX11.3.14.9001.nupkg              -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Platform.SDL2.GL.3.14.9001.nupkg               -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Platform.Android.GL.3.14.9001.nupkg            -Source "C:\Users\%username%\.nuget\localPackages"

"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Platform.WinForms.DX11.OculusOVR.3.14.9001.nupkg -Source "C:\Users\%username%\.nuget\localPackages"


@pause