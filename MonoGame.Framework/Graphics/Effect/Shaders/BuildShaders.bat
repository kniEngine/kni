@echo off
setlocal

SET MGFXC="..\..\..\..\Tools\MonoGame.Effect.Compiler\bin\Windows\AnyCPU\Release\2mgfx.exe"

@for /f %%f IN ('dir /b *.fx') do (

  echo %%~nf.fx
  
  call %MGFXC% %%~nf.fx ..\Resources\%%~nf.dx11.fxo /Platform:Windows

  call %MGFXC% %%~nf.fx ..\Resources\%%~nf.gl.fxo /Platform:Android

)

endlocal
pause
