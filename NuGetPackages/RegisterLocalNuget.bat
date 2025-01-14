set username=username

"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework                           4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Design                    4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Content                   4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Audio                     4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Graphics                  4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Media                     4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Input                     4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Game                      4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Devices                   4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Storage                   4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.XR                        4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive

"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Content.Pipeline          4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Content.Pipeline.Audio    4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Content.Pipeline.Graphics 4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Content.Pipeline.Media    4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive

"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Content.Pipeline.Builder  4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages"   -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Xna.Framework.Content.Pipeline.Builder.Windows  4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages"   -NonInteractive

"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Kni.Platform.Blazor.GL                  4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Kni.Platform.WinForms.DX11              4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Kni.Platform.UAP.DX11                   4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Kni.Platform.SDL2.GL                    4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Kni.Platform.Android.GL                 4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive
"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Kni.Platform.Oculus.GL                  4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive

"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.Kni.Platform.WinForms.DX11.OculusOVR    4.0.9001 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive


"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.4.0.9001.nupkg                           -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Design.4.0.9001.nupkg                    -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Content.4.0.9001.nupkg                   -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Audio.4.0.9001.nupkg                     -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Graphics.4.0.9001.nupkg                  -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Media.4.0.9001.nupkg                     -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Input.4.0.9001.nupkg                     -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Game.4.0.9001.nupkg                      -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Devices.4.0.9001.nupkg                   -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Storage.4.0.9001.nupkg                   -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.XR.4.0.9001.nupkg                        -Source "C:\Users\%username%\.nuget\localPackages"

"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Content.Pipeline.4.0.9001.nupkg          -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Content.Pipeline.Audio.4.0.9001.nupkg    -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Content.Pipeline.Graphics.4.0.9001.nupkg -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Content.Pipeline.Media.4.0.9001.nupkg    -Source "C:\Users\%username%\.nuget\localPackages"

"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Content.Pipeline.Builder.4.0.9001.nupkg  -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Xna.Framework.Content.Pipeline.Builder.Windows.4.0.9001.nupkg -Source "C:\Users\%username%\.nuget\localPackages"

"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Kni.Platform.Blazor.GL.4.0.9001.nupkg             -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Kni.Platform.WinForms.DX11.4.0.9001.nupkg         -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Kni.Platform.UAP.DX11.4.0.9001.nupkg              -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Kni.Platform.SDL2.GL.4.0.9001.nupkg               -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Kni.Platform.Android.GL.4.0.9001.nupkg            -Source "C:\Users\%username%\.nuget\localPackages"
"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Kni.Platform.Oculus.GL.4.0.9001.nupkg             -Source "C:\Users\%username%\.nuget\localPackages"

"C:\Program Files (x86)\nuget\nuget.exe" add output\nkast.Kni.Platform.WinForms.DX11.OculusOVR.4.0.9001.nupkg -Source "C:\Users\%username%\.nuget\localPackages"


@pause