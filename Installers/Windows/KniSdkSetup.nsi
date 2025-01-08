SetCompressor /SOLID /FINAL lzma

!include "header.nsh"
!include "LogicLib.nsh"
!define APPNAME "KNI"

;Include Modern UI

!include "Sections.nsh"
!include "MUI2.nsh"
!include "InstallOptions.nsh"

!define MUI_ICON "${FrameworkPath}\Kni.ico"

!define MUI_UNICON "${FrameworkPath}\Kni.ico"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

;Interface Configuration

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "${FrameworkPath}\Kni.bmp"
!define MUI_ABORTWARNING

!define MUI_WELCOMEFINISHPAGE_BITMAP "${FrameworkPath}\panel.bmp"
;Languages

!insertmacro MUI_PAGE_WELCOME

!insertmacro MUI_PAGE_LICENSE "..\..\License.txt"

!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES

Page Custom SponsorPage SponsorPageLeave

!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"

 
Name '${APPNAME} SDK ${INSTALLERVERSION}'
OutFile 'KniSdkSetup.exe'
InstallDir '$PROGRAMFILES\${APPNAME}\v${VERSION}'
VIProductVersion "${INSTALLERVERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "${APPNAME} SDK"
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "Kni framework"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "${INSTALLERVERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductVersion" "${INSTALLERVERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "${APPNAME} SDK Installer"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "Copyright © Kni framework"

!macro VS_ASSOCIATE_EDITOR TOOLNAME VSVERSION EXT TOOLPATH
  WriteRegStr   HKCU 'Software\Microsoft\VisualStudio\${VSVERSION}\Default Editors\${EXT}' 'Custom' '${TOOLNAME}'
  WriteRegDWORD HKCU 'Software\Microsoft\VisualStudio\${VSVERSION}\Default Editors\${EXT}' 'Type' 0x00000002
  WriteRegStr   HKCU 'Software\Microsoft\VisualStudio\${VSVERSION}\Default Editors\${EXT}\${TOOLNAME}' '' '${TOOLPATH}'
  WriteRegStr   HKCU 'Software\Microsoft\VisualStudio\${VSVERSION}\Default Editors\${EXT}\${TOOLNAME}' 'Arguments' ''
!macroend

!macro APP_ASSOCIATE EXT FILECLASS DESCRIPTION ICON COMMANDTEXT COMMAND
  WriteRegStr HKCR ".${EXT}" "" "${FILECLASS}" 
  WriteRegStr HKCR "${FILECLASS}" "" `${DESCRIPTION}`
  WriteRegStr HKCR "${FILECLASS}\DefaultIcon" "" `${ICON}`
  WriteRegStr HKCR "${FILECLASS}\shell" "" "open"
  WriteRegStr HKCR "${FILECLASS}\shell\open" "" `${COMMANDTEXT}`
  WriteRegStr HKCR "${FILECLASS}\shell\open\command" "" `${COMMAND}`
!macroend

;--------------------------------

; The stuff to install
Section "Kni Core Components" CoreComponents ;No components page, name is not important
  SectionIn RO
  
  ; Install the Kni tools to a single shared folder.
  SetOutPath '$INSTDIR\Tools'
  File /r '..\..\Tools\EffectCompiler\bin\Windows\AnyCPU\Release\net8.0\*.exe'
  File /r '..\..\Tools\EffectCompiler\bin\Windows\AnyCPU\Release\net8.0\*.runtimeconfig.json'
  File /r '..\..\Tools\EffectCompiler\bin\Windows\AnyCPU\Release\net8.0\*.dll'
  File /r '..\..\Tools\MonoGame.Content.Builder\bin\Windows\AnyCPU\Release\net8.0-windows\*.exe'
  File /r '..\..\Tools\MonoGame.Content.Builder\bin\Windows\AnyCPU\Release\net8.0-windows\*.runtimeconfig.json'
  File /r '..\..\Tools\MonoGame.Content.Builder\bin\Windows\AnyCPU\Release\net8.0-windows\*.dll'
  File /r '..\..\Tools\Content.Pipeline.Editor.WinForms\bin\AnyCPU\Release\net8.0-windows\Templates'
  File /r '..\..\Tools\Content.Pipeline.Editor.WinForms\bin\AnyCPU\Release\net8.0-windows\runtimes'
  File /r '..\..\Tools\Content.Pipeline.Editor.WinForms\bin\AnyCPU\Release\net8.0-windows\PipelineEditor.exe'
  File /r '..\..\Tools\Content.Pipeline.Editor.WinForms\bin\AnyCPU\Release\net8.0-windows\PipelineEditor.runtimeconfig.json'
  File /r '..\..\Tools\Content.Pipeline.Editor.WinForms\bin\AnyCPU\Release\net8.0-windows\PipelineEditor.dll'
  File '..\..\Artifacts\Xna.Framework.Design\Release\netstandard2.0\*.dll'

  ; Associate .mgcb files open in the Pipeline tool.
  !insertmacro VS_ASSOCIATE_EDITOR 'MonoGame Pipeline' '15.0' 'mgcb' '$INSTDIR\Tools\PipelineEditor.exe'
  !insertmacro APP_ASSOCIATE 'mgcb' 'MonoGame.ContentBuilderFile' 'A MonoGame content builder project.' '$INSTDIR\Tools\PipelineEditor.exe,0' 'Open with PipelineEditor' '$INSTDIR\Tools\PipelineEditor.exe "%1"'

  ; Install the assemblies for all the platforms we can 
  ; target from a Windows desktop system.

  ; Install framework packages
  SetOutPath '$INSTDIR\Packages\'
  File '..\..\Artifacts\Packages\*.*'
  
  ; Install framework Assemblies
  SetOutPath '$INSTDIR\Assemblies\Framework\net40'
  File '..\..\Artifacts\Xna.Framework\Release\net40\*.*'
  File '..\..\Artifacts\Xna.Framework.Design\Release\net40\*.*'
  File '..\..\Artifacts\Xna.Framework.Content\Release\net40\*.*'
  File '..\..\Artifacts\Xna.Framework.Graphics\Release\net40\*.*'
  File '..\..\Artifacts\Xna.Framework.Audio\Release\net40\*.*'
  File '..\..\Artifacts\Xna.Framework.Media\Release\net40\*.*'
  File '..\..\Artifacts\Xna.Framework.Input\Release\net40\*.*'
  File '..\..\Artifacts\Xna.Framework.Game\Release\net40\*.*'
  SetOutPath '$INSTDIR\Assemblies\Framework\netstandard2.0'
  File '..\..\Artifacts\Xna.Framework\Release\netstandard2.0\*.*'
  File '..\..\Artifacts\Xna.Framework.Design\Release\netstandard2.0\*.*'
  File '..\..\Artifacts\Xna.Framework.Content\Release\netstandard2.0\*.*'
  File '..\..\Artifacts\Xna.Framework.Graphics\Release\netstandard2.0\*.*'
  File '..\..\Artifacts\Xna.Framework.Audio\Release\netstandard2.0\*.*'
  File '..\..\Artifacts\Xna.Framework.Media\Release\netstandard2.0\*.*'
  File '..\..\Artifacts\Xna.Framework.Input\Release\netstandard2.0\*.*'
  File '..\..\Artifacts\Xna.Framework.Game\Release\netstandard2.0\*.*'
  File '..\..\Artifacts\Xna.Framework.Devices\Release\netstandard2.0\*.*'
  File '..\..\Artifacts\Xna.Framework.Storage\Release\netstandard2.0\*.*'
    
  
  ; Install Reference Assemblies
  SetOutPath '$INSTDIR\Assemblies\Ref\net40'
  File '..\..\Artifacts\Platforms\Ref\Release\net40\*.*'
  SetOutPath '$INSTDIR\Assemblies\Ref\netstandard2.0'
  File '..\..\Artifacts\Platforms\Ref\Release\netstandard2.0\*.*'
  
  ; Install Android Assemblies
  SetOutPath '$INSTDIR\Assemblies\Android'
  File '..\..\Platforms\bin\Android\AnyCPU\Release\*.dll'
  File '..\..\Platforms\bin\Android\AnyCPU\Release\*.xml'
  
  ; Install DesktopGL Assemblies
  SetOutPath '$INSTDIR\Assemblies\DesktopGL'
  File /nonfatal '..\..\Artifacts\Platforms\DesktopGL\Release\net40\*.dll'
  File /nonfatal '..\..\Artifacts\Platforms\DesktopGL\Release\net40\*.xml'
  File '..\..\ThirdParty\Dependencies\SDL\MacOS\Universal\libSDL2.dylib'
  File '..\..\ThirdParty\Dependencies\openal-soft\MacOS\Universal\libopenal.1.dylib'
  File '..\..\ThirdParty\Dependencies\MonoGame.Framework.dll.config'
  
  ; Install x86 DesktopGL Dependencies
  SetOutPath '$INSTDIR\Assemblies\DesktopGL\x86'
  File '..\..\ThirdParty\Dependencies\SDL\Windows\x86\SDL2.dll'
  File '..\..\ThirdParty\Dependencies\openal-soft\Windows\x86\soft_oal.dll'
  File '..\..\ThirdParty\Dependencies\openal-soft\Linux\x86\libopenal.so.1'
  
  ; Install x64 DesktopGL Dependencies
  SetOutPath '$INSTDIR\Assemblies\DesktopGL\x64'
  File '..\..\ThirdParty\Dependencies\SDL\Windows\x64\SDL2.dll'
  File '..\..\ThirdParty\Dependencies\openal-soft\Windows\x64\soft_oal.dll'
  File '..\..\ThirdParty\Dependencies\SDL\Linux\x64\libSDL2-2.0.so.0'
  File '..\..\ThirdParty\Dependencies\openal-soft\Linux\x64\libopenal.so.1'
  
  ; Install Windows Desktop DirectX Assemblies
  SetOutPath '$INSTDIR\Assemblies\Windows'
  File '..\..\Artifacts\Platforms\WindowsDX\Release\net40\*.dll'
  File '..\..\Artifacts\Platforms\WindowsDX\Release\net40\*.xml'

  ; Install Windows 10 UAP Assemblies
  SetOutPath '$INSTDIR\Assemblies\WindowsUniversal'
  ;File '..\..\Platforms\bin\WindowsUniversal\Release\uap10.0\*.dll'
  ;File '..\..\Platforms\bin\WindowsUniversal\Release\uap10.0\*.xml' 
  File '..\..\Platforms\bin\WindowsUniversal\AnyCPU\Release\*.dll'
  File '..\..\Platforms\bin\WindowsUniversal\AnyCPU\Release\*.xml'

  ; Install iOS Assemblies
  IfFileExists `$PROGRAMFILES\MSBuild\Xamarin\iOS\*.*` InstalliOSAssemblies SkipiOSAssemblies
  InstalliOSAssemblies:
  ;SetOutPath '$INSTDIR\Assemblies\iOS'
  ;File '..\..\Artifacts\Platforms\iOS\Release\xamarinios10\*.dll'
  ;File '..\..\Artifacts\Platforms\iOS\Release\xamarinios10\*.xml'
  SkipiOSAssemblies:

  WriteRegStr HKLM 'SOFTWARE\Microsoft\MonoAndroid\v2.3\AssemblyFoldersEx\${APPNAME} for Android' '' '$INSTDIR\Assemblies\Android'
  WriteRegStr HKLM 'SOFTWARE\Microsoft\MonoTouch\v1.0\AssemblyFoldersEx\${APPNAME} for iOS' '' '$INSTDIR\Assemblies\iOS'

  IfFileExists $WINDIR\SYSWOW64\*.* Is64bit Is32bit
  Is32bit:
    GOTO End32Bitvs64BitCheck
  Is64bit:
    WriteRegStr HKLM 'SOFTWARE\Wow6432Node\Microsoft\MonoTouch\v1.0\AssemblyFoldersEx\${APPNAME} for iOS' '' '$INSTDIR\Assemblies\iOS'

  End32Bitvs64BitCheck:
  ; Add remote programs
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'DisplayName' '${APPNAME} SDK'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'DisplayVersion' '${INSTALLERVERSION}'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'DisplayIcon' '$INSTDIR\Kni.ico'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'InstallLocation' '$INSTDIR\'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'Publisher' 'Kni framework'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}' 'UninstallString' '$INSTDIR\uninstall.exe'


  SetOutPath '$INSTDIR'
  File '..\Kni.ico'

  ; Uninstaller
  WriteUninstaller "uninstall.exe"


SectionEnd

Section "VS012 Redistributables (x64)" VS2012Redist

  SetOutPath "$INSTDIR"
  File "..\..\ThirdParty\VCRedist\vcredist2012_x64.exe"
  ExecWait '"$INSTDIR\vcredist2012_x64.exe"  /passive /norestart'
  
SectionEnd

Section "VS2022 Templates" VS2022

  IfFileExists `$DOCUMENTS\Visual Studio 2022\*.*` InstallTemplates CannotInstallTemplates
  InstallTemplates:
    SetOutPath "$DOCUMENTS\Visual Studio 2022\Templates\ProjectTemplates\Visual C#\KNI"
    File /r '..\..\Templates\VisualStudio2022\ProjectTemplates\*.zip'
    SetOutPath "$DOCUMENTS\Visual Studio 2022\Templates\ItemTemplates\Visual C#\KNI"
    File /r '..\..\Templates\VisualStudio2022\ItemTemplates\*.zip'
    GOTO EndTemplates
  CannotInstallTemplates:
    DetailPrint "Visual Studio 2022 not found"
  EndTemplates:

SectionEnd

; Optional section (can be disabled by the user)
Section "Start Menu Shortcuts" Menu
	CreateDirectory $SMPROGRAMS\${APPNAME}
	SetOutPath "$INSTDIR"
	CreateShortCut "$SMPROGRAMS\${APPNAME}\Uninstall MonoGame.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
	SetOutPath "$INSTDIR\Tools"
	CreateShortCut "$SMPROGRAMS\${APPNAME}\Kni PipelineEditor.lnk" "$INSTDIR\Tools\PipelineEditor.exe" "" "$INSTDIR\Tools\PipelineEditor.exe" 0
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Website.url" "InternetShortcut" "URL" "http://www.monogame.net"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Website.url" "InternetShortcut" "IconFile" "$INSTDIR\Kni.ico"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Website.url" "InternetShortcut" "IconIndex" "0"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Documentation.url" "InternetShortcut" "URL" "http://www.monogame.net/documentation"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Documentation.url" "InternetShortcut" "IconFile" "$INSTDIR\Kni.ico"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Documentation.url" "InternetShortcut" "IconIndex" "0"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Support.url" "InternetShortcut" "URL" "http://community.monogame.net/"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Support.url" "InternetShortcut" "IconFile" "$INSTDIR\Kni.ico"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Support.url" "InternetShortcut" "IconIndex" "0"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Bug Reports.url" "InternetShortcut" "URL" "https://github.com/kniEngine/kni/issues"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Bug Reports.url" "InternetShortcut" "IconFile" "$INSTDIR\Kni.ico"
	WriteINIStr "$SMPROGRAMS\${APPNAME}\MonoGame Bug Reports.url" "InternetShortcut" "IconIndex" "0"

SectionEnd

LangString CoreComponentsDesc ${LANG_ENGLISH} "Install the Runtimes and the MSBuild extensions for MonoGame"
LangString VS2012RedistDesc ${LANG_ENGLISH} "Install the VS2012 Redistributables (x64)"
LangString VS2022Desc ${LANG_ENGLISH} "Install the project templates for Visual Studio 2022"
LangString MenuDesc ${LANG_ENGLISH} "Add a link to the MonoGame website to your start menu"

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${CoreComponents} $(CoreComponentsDesc)
  !insertmacro MUI_DESCRIPTION_TEXT ${VS2012Redist} $(VS2012RedistDesc)
  !insertmacro MUI_DESCRIPTION_TEXT ${NugetPackages} $(NugetPackagesDesc)
  !insertmacro MUI_DESCRIPTION_TEXT ${VS2022} $(VS2022Desc)
  !insertmacro MUI_DESCRIPTION_TEXT ${Menu} $(MenuDesc)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

Function SponsorPage
  ReserveFile "SponsorPage.ini"
  !insertmacro INSTALLOPTIONS_EXTRACT "SponsorPage.ini"
  !insertmacro INSTALLOPTIONS_DISPLAY "SponsorPage.ini"
FunctionEnd
 
Function SponsorPageLeave
  # Find out which field event called us. 0 = Next button called us.
  !insertmacro INSTALLOPTIONS_READ $R0 "SponsorPage.ini" "Settings" "State"
  ${If} $R0 == 3 # Field 3.
    ExecShell "open" "https://github.com/sponsors/nkast"
    abort
  ${EndIf}
FunctionEnd

Function checkVS2012Redist
 ; TODO: check if VS2012 Redisttributables are installed 
FunctionEnd

Function checkVS2022
IfFileExists `$DOCUMENTS\Visual Studio 2022\*.*` end disable
  disable:
	 SectionSetFlags ${VS2022} $0
  end:
FunctionEnd

Function .onInit 
  IntOp $0 $0 | ${SF_RO}
  call checkVS2012Redist
  Call checkVS2022
  IntOp $0 ${SF_SELECTED} | ${SF_RO}
  SectionSetFlags ${core_id} $0
FunctionEnd

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  DeleteRegKey HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\MonoAndroid\v2.3\AssemblyFoldersEx\${APPNAME} for Android'
  DeleteRegKey HKLM 'SOFTWARE\Microsoft\MonoTouch\v1.0\AssemblyFoldersEx\${APPNAME} for iOS'

  ;DeleteRegKey HKCU 'Software\Microsoft\VisualStudio\12.0\Default Editors\mgcb'

  DeleteRegKey HKCR '.mgcb'
  DeleteRegKey HKCR 'MonoGame.ContentBuilderFile'

  IfFileExists $WINDIR\SYSWOW64\*.* Is64bit Is32bit
  Is32bit:
    GOTO End32Bitvs64BitCheck
  Is64bit:
    DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\MonoAndroid\v2.3\AssemblyFoldersEx\${APPNAME} for Android'
	DeleteRegKey HKLM 'SOFTWARE\Wow6432Node\Microsoft\MonoTouch\v1.0\AssemblyFoldersEx\${APPNAME} for iOS'


  End32Bitvs64BitCheck:

  ReadRegStr $0 HKLM 'SOFTWARE\Wow6432Node\Xamarin\MonoDevelop' "Path"
  ${If} $0 == "" ; check on 32 bit machines just in case
  ReadRegStr $0 HKLM 'SOFTWARE\Xamarin\MonoDevelop' "Path"
  ${EndIf}

  ${If} $0 == ""
  ${Else}
  RMDir /r "$0\AddIns\MonoDevelop.MonoGame"
  ${EndIf}
  
  RMDir /r "$DOCUMENTS\Visual Studio 2022\Templates\ProjectTemplates\Visual C#\KNI"
  RMDir /r "$DOCUMENTS\Visual Studio 2022\Templates\ItemTemplates\Visual C#\KNI"
  RMDir /r "$SMPROGRAMS\${APPNAME}"

  Delete "$INSTDIR\Uninstall.exe"
  RMDir /r "$INSTDIR"

SectionEnd

