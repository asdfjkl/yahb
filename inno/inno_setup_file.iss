; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "YAHB"
#define MyAppVersion "1.0.9.0"
#define MyAppPublisher "asdfjkl"
#define MyAppURL "https://github.com/asdfjkl/yahb"
#define MyAppExeName "yahb.exe"

#include "environment.iss"

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
begin
    if (CurStep = ssPostInstall) and IsTaskSelected('envPath')
    then EnvAddPath(ExpandConstant('{app}'));
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
    if CurUninstallStep = usPostUninstall
    then EnvRemovePath(ExpandConstant('{app}'));
end;

[Tasks]
Name: envPath; Description: "Add to PATH variable" 

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{828D92DF-53A9-4DC2-B60B-360DE739E493}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=D:\MyFiles\workspace\yahb_release\gpl-3.0.txt
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir=D:\MyFiles\workspace\yahb_release\inno_output
OutputBaseFilename=yahb_setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ChangesEnvironment=true

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "D:\MyFiles\workspace\yahb_release\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\MyFiles\workspace\yahb_release\AlphaFS.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\MyFiles\workspace\yahb_release\AlphaFS.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\MyFiles\workspace\yahb_release\AlphaVSS.Common.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\MyFiles\workspace\yahb_release\AlphaVSS.x64.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\MyFiles\workspace\yahb_release\AlphaVSS.x86.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\MyFiles\workspace\yahb_release\app.manifest"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\MyFiles\workspace\yahb_release\yahb.exe.config"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"







