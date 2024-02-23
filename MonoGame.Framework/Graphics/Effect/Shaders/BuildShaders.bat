@echo off
setlocal

SET KNIFXC="..\..\..\..\Tools\EffectCompiler\bin\Windows\AnyCPU\Release\net8.0\KNIFXC.exe"

@for /f %%f IN ('dir /b *.fx') do (

  echo %%~nf.fx
  
  call %KNIFXC% %%~nf.fx ..\Resources\%%~nf.dx11.fxo /Platform:Windows

  call %KNIFXC% %%~nf.fx ..\Resources\%%~nf.ogl.fxo /Platform:Android

)

endlocal
pause
