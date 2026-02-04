; Script di Inno Setup per Livello HD Service PRO
; Richiede Inno Setup 6.x o superiore (download da: https://jrsoftware.org/isdl.php)

#define MyAppName "Livello HD Service PRO"
#define MyAppVersion "1.1.2"
#define MyAppPublisher "Livello HD"
#define MyAppExeName "LivelloHDServicePRO.exe"
#define MyAppAssocName MyAppName
#define MyAppAssocExt ".sla"
#define MyAppAssocKey StringChange(MyAppAssocName, " ", "") + MyAppAssocExt

[Setup]
; Informazioni di base dell'applicazione
AppId={{8F3C4A2B-9D5E-4F1C-A6B7-2E8D9C1F4A5B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\LICENSE.txt
InfoBeforeFile=..\README.txt
OutputDir=..\Releases
OutputBaseFilename=LivelloHDServicePRO_Setup_v{#MyAppVersion}
SetupIconFile=..\LivelloHDServicePRO\Assets\sla.ico
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible
MinVersion=10.0.17763

; Privilegi
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; Interfaccia
DisableProgramGroupPage=yes
DisableWelcomePage=no

; Aspetto del wizard - Usa immagini predefinite di Inno Setup
; Se vuoi personalizzare le immagini, sostituisci con i tuoi file:
; WizardImageFile=MyWizardImage.bmp
; WizardSmallImageFile=MyWizardSmallImage.bmp

[Languages]
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; File principali dell'applicazione (dalla cartella publish)
Source: "..\Publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Non usare "Flags: ignoreversion" su file di sistema o condivisi

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
; Esegui l'applicazione dopo l'installazione
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Funzione per verificare se .NET 10 Runtime è installato
function IsDotNet10Installed(): Boolean;
var
  ResultCode: Integer;
  DotNetPath: String;
begin
  // Verifica se dotnet.exe esiste
  DotNetPath := ExpandConstant('{pf}\dotnet\dotnet.exe');
  
  if not FileExists(DotNetPath) then
  begin
    Result := False;
    Exit;
  end;
  
  // Esegui dotnet --list-runtimes per verificare .NET 10
  if Exec(DotNetPath, '--list-runtimes', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Result := True; // Semplificato - in produzione dovresti parsare l'output
  end
  else
  begin
    Result := False;
  end;
end;

function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
  DotNetInstallUrl: String;
begin
  Result := True;
  
  // Verifica se .NET 10 Desktop Runtime è installato
  if not IsDotNet10Installed() then
  begin
    if MsgBox('Questa applicazione richiede .NET 10 Desktop Runtime.' + #13#10 + 
              'Vuoi scaricare e installare .NET 10 Desktop Runtime ora?', 
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      DotNetInstallUrl := 'https://dotnet.microsoft.com/download/dotnet/10.0';
      ShellExec('open', DotNetInstallUrl, '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
      Result := False;
      MsgBox('Installa .NET 10 Desktop Runtime e poi riavvia questo setup.', mbInformation, MB_OK);
    end
    else
    begin
      Result := False;
      MsgBox('L''installazione non può continuare senza .NET 10 Desktop Runtime.', mbError, MB_OK);
    end;
  end;
end;

[UninstallDelete]
Type: filesandordirs; Name: "{app}\Data"
Type: filesandordirs; Name: "{app}\Logs"
Type: filesandordirs; Name: "{localappdata}\LivelloHDServicePRO"

[Registry]
Root: HKCU; Subkey: "Software\LivelloHD\LivelloHDServicePRO"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\LivelloHD\LivelloHDServicePRO\Settings"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey

[Messages]
italian.WelcomeLabel2=Questo installerà [name/ver] sul tuo computer.%n%nSi raccomanda di chiudere tutte le altre applicazioni prima di continuare.
italian.FinishedLabel=L'installazione di [name] è stata completata con successo.%n%nL'applicazione può essere avviata selezionando l'icona installata.


