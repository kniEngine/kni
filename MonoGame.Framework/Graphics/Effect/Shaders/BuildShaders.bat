@echo off
setlocal

SET MGFXC="..\..\..\..\Artifacts\MonoGame.Effect.Compiler\Release\mgfxc.exe"

@for /f %%f IN ('dir /b *.fx') do (

  echo %%~nf.fx
  
  call %MGFXC% %%~nf.fx ..\Resources\%%~nf.ogl.mgfxo /Profile:OpenGL

  call %MGFXC% %%~nf.fx ..\Resources\%%~nf.dx11.mgfxo /Profile:DirectX_11

)

endlocal
pause
