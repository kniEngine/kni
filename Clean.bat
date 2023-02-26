
for /d /r . %%d in (bin,obj,bak) do @if exist "%%d" rd /s /q "%%d"

rd /s /q "Artifacts"

cd Templates
del /s /q *.zip
cd ..

pause