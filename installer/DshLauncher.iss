#ifndef AppVersion
  #define AppVersion "0.1.0"
#endif
#ifndef RuntimeIdentifier
  #define RuntimeIdentifier "win-x64"
#endif

#if RuntimeIdentifier == "win-arm64"
  #define AllowedArchitecture "arm64"
#else
  #define AllowedArchitecture "x64compatible"
#endif

[Setup]
AppId={{E8DFB365-3BF8-47AD-83FD-1CFD29BB5D8E}
AppName=DSH Launcher
AppVersion={#AppVersion}
AppPublisher=aowen233
AppPublisherURL=https://github.com/
AppSupportURL=https://github.com/
AppUpdatesURL=https://github.com/
DefaultDirName={localappdata}\Programs\DSH Launcher
DefaultGroupName=DSH Launcher
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
MinVersion=10.0.17763
ArchitecturesAllowed={#AllowedArchitecture}
OutputDir=..\artifacts\installer
OutputBaseFilename=DSH-Launcher-{#AppVersion}-{#RuntimeIdentifier}-unsigned-preview
Compression=lzma2/normal
SolidCompression=yes
WizardStyle=modern
UninstallDisplayName=DSH Launcher
CloseApplications=yes
RestartApplications=no
SetupLogging=yes
LicenseFile=..\LICENSE
SetupIconFile=..\assets\dsh-launcher.ico

[Languages]
Name: "chinesesimplified"; MessagesFile: "translations\ChineseSimplified.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "快捷方式："

[Files]
Source: "..\artifacts\publish\{#RuntimeIdentifier}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\THIRD_PARTY_NOTICES.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\PRIVACY.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\DSH Launcher"; Filename: "{app}\DshLauncher.exe"
Name: "{autodesktop}\DSH Launcher"; Filename: "{app}\DshLauncher.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\DshLauncher.exe"; Description: "启动 DSH Launcher"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{app}\DshLauncher.exe"; Parameters: "--stop"; Flags: runhidden waituntilterminated; RunOnceId: "StopDshLauncher"

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\DshLauncher"
