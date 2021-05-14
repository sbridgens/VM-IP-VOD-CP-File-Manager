; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "VM IPVod CP File Manager Service"
#define MyAppVersion "1.0.0.8"
#define MyAppPublisher "SCH Tech Ltd"
#define MyAppURL "simon@schtech.co.uk"
#define MyAppExeName "VM IP VOD CP File Manager.exe"
#define MyFileName "VM_IPVOD_CP_FileManager_Setup"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={code:GetGuid}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={commonpf}\{#MyAppName}
DefaultGroupName={#MyAppName}
UsePreviousAppDir=no 
UsePreviousLanguage=no
AllowNoIcons=yes
OutputBaseFilename={#MyFileName}_{#MyAppVersion}
Compression=lzma
SolidCompression=yes
UninstallDisplayName={code:GetDisplayName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]

Source: "C:\Users\simon\Dropbox\scripts\C#\VM IP VOD CP File Manager\VM IP VOD CP File Manager\bin\Release\net472\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"


[Code]

var
  UserInputsPage: TInputQueryWizardPage;

function GetServiceName(Param: string): string;
begin
  Result := UserInputsPage.Values[0];
end;

function GetDisplayName(Param: string): string;
begin
  Result := UserInputsPage.Values[1];
end;

procedure InitializeWizard;
begin
  UserInputsPage :=
    CreateInputQueryPage(wpWelcome,
      'Service Information', 'Provide App Name, Service name and Service Display name',
      'Please specify the following information, then click Next.');
  UserInputsPage.Add('Service Name (no spaces):', False);
  UserInputsPage.Add('Display Name:', False);
end;

function CoCreateGuid(var Guid:TGuid):integer;
external 'CoCreateGuid@ole32.dll stdcall';

function inttohex(l:longword; digits:integer):string;
var hexchars:string;
begin
 hexchars:='0123456789ABCDEF';
 setlength(result,digits);
 while (digits>0) do begin
  result[digits]:=hexchars[l mod 16+1];
  l:=l div 16;
  digits:=digits-1;
 end;
end;

function GetGuid(dummy:string):string;
var Guid:TGuid;
begin
  if CoCreateGuid(Guid)=0 then begin
  result:='{'+IntToHex(Guid.D1,8)+'-'+
           IntToHex(Guid.D2,4)+'-'+
           IntToHex(Guid.D3,4)+'-'+
           IntToHex(Guid.D4[0],2)+IntToHex(Guid.D4[1],2)+'-'+
           IntToHex(Guid.D4[2],2)+IntToHex(Guid.D4[3],2)+
           IntToHex(Guid.D4[4],2)+IntToHex(Guid.D4[5],2)+
           IntToHex(Guid.D4[6],2)+IntToHex(Guid.D4[7],2)+
           '}';
  end else
    result:='{00000000-0000-0000-0000-000000000000}';
end;



[Run]
Filename: {sys}\sc.exe; Parameters: " create ""{code:GetServiceName}"" binPath= ""{app}\{#MyAppExeName}"" DisplayName= ""{code:GetDisplayName}"" start= auto" ; Flags: runhidden 


[UninstallRun]
Filename: {sys}\sc.exe; Parameters: "stop {code:GetServiceName}" ; Flags: runhidden; RunOnceId: {code:GetGuid};
Filename: {sys}\sc.exe; Parameters: "delete {code:GetServiceName}" ; Flags: runhidden; RunOnceId: {code:GetGuid};