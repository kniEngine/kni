@echo off
setlocal

SET KNIFXC="..\..\..\..\Tools\EffectCompiler\bin\Windows\AnyCPU\Release\net8.0\KNIFXC.exe"

@for /f %%f IN ('dir /b *.fx') do (

  echo %%~nf.fx
  
  call %KNIFXC% %%~nf.fx ..\Resources\%%~nf.dx11.fxo /Backend:DirectX11

  call %KNIFXC% %%~nf.fx ..\Resources\%%~nf.ogl.fxo /Backend:OpenGL
    
  call %KNIFXC% %%~nf.fx ..\Resources\%%~nf.gles.fxo /Backend:GLES

)

endlocal
pause
