#define Name "QModManager"
#define Version "2.0-InstallerTest"
#define Publisher "the QModManager team"
#define URL "https://github.com/QModManager"

#define SubnauticaGUID '{52CC87AA-645D-40FB-8411-510142191678}'
#define TerraTechGUID '{53D64B81-BFF9-47E3-A599-66C18ED14B71}'

#define PreRelease True
#define InstallerTest True

[Setup]
#if InstallerTest == False
  AllowNetworkDrive=no
  AllowUNCPath=no
#else
  AllowRootDirectory=yes
#endif
AlwaysShowDirOnReadyPage=yes
AppendDefaultDirName=no
AppId=StringToGUID({code:GetGUID})
AppName={#Name}
AppPublisher={#Publisher}
AppPublisherURL={#URL}
AppSupportURL={#URL}
AppUpdatesURL={#URL}
AppVerName={#Name} {#Version}
AppVersion={#Version}
CloseApplications=yes
Compression=lzma
DefaultDirName=C:/
DirExistsWarning=no
DisableDirPage=no
DisableProgramGroupPage=yes
DisableWelcomePage=no
EnableDirDoesntExistWarning=yes
InfoBeforeFile=Info.txt
OutputBaseFilename=QModManager_Setup
OutputDir=.
#if PreRelease == True
  Password=YES
#endif
PrivilegesRequired=admin
RestartApplications=yes
SetupIconFile=..\Assets\icon.ico
SolidCompression=yes
UninstallDisplayIcon=..\Assets\icon.ico
UninstallDisplayName={#Name}
UsePreviousAppDir=no
UsePreviousLanguage=no
WizardImageFile=..\Assets\LargeImage.bmp
WizardSmallImageFile=..\Assets\SmallImage.bmp

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "0Harmony.dll"; DestDir: "{app}\TerraTechWin64_Data\Managed"; Flags: IgnoreVersion
Source: "Mono.Cecil.dll"; DestDir: "{app}\TerraTechWin64_Data\Managed"; Flags: IgnoreVersion
Source: "QModInstaller.dll"; DestDir: "{app}\TerraTechWin64_Data\Managed"; Flags: IgnoreVersion
Source: "QModManager.exe"; DestDir: "{app}\TerraTechWin64_Data\Managed"; Flags: IgnoreVersion

[Run]
Filename: "{app}\Subnautica_Data\Managed\QModManager.exe"; Parameters: """-i"" ""Game=Subnautica"""; Check: IsSubnautica
Filename: "{app}\TerraTechWin64_Data\Managed\QModManager.exe"; Parameters: """-i"" ""Game=TerraTech"""; Check: IsTerraTech

[UninstallRun]
Filename: "{app}\Subnautica_Data\Managed\QModManager.exe"; Parameters: """-u"" ""Game=Subnautica"""; Check: IsSubnautica
Filename: "{app}\TerraTechWin64_Data\Managed\QModManager.exe"; Parameters: """-u"" ""Game=TerraTech"""; Check: IsTerraTech

[Messages]
BeveledLabel={#Name} {#Version}
WizardPassword=Warning
PasswordLabel1=Please read the following important information before continuing.
PasswordLabel3=You are trying to install a pre-release version of QModManager.%nPre-releases are unstable and might contain bugs.%nWe are not responsible for any crashes or world corruptions that might occur.%n%nPlease type 'YES' (without quotes) to continue with the installation.
PasswordEditLabel=Consent:
IncorrectPassword=You're not funny
WizardSelectDir=Select install location
SelectDirLabel3=Please select the install folder of the game.
SelectDirBrowseLabel=If this is correct, click Next. If you need to select a different install folder, click Browse.
WizardSelectComponents=Review install
SelectComponentsDesc=
SelectComponentsLabel2=Cannot install in this folder
#if InstallerTest == True
  WizardReady=Installer test
  ReadyLabel1=This is just an installer test
  ReadyLabel2a=As this is just a test for the installer, you cannot actually install it. Thank you for trying it out!
#endif  
ExitSetupMessage=Setup is not complete. If you exit now, {#Name} will not be installed.%nExit Setup?

[Types]
Name: "select"; Description: "QModManager"; Flags: IsCustom

[Components]
Name: "qmm"; Description: "QModManager"; Flags: fixed; Types: select

Name: "qmm\sn"; Description: "Install for Subnautica"; Flags: exclusive fixed
Name: "qmm\tt"; Description: "Install for TerraTech"; Flags: exclusive fixed

[Code]

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Installer test

#if InstallerTest == True
  function CurPageChanged_DisableReady(CurPageID: Integer): Boolean;
  begin
    if CurPageID = wpReady then
    begin
        WizardForm.NextButton.Enabled := False;
    end
  end;
#endif

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Game detect

function IsSubnautica: Boolean;
var
  app: String;
begin
  app := ExpandConstant('{app}')
  //Result := True
  //Exit
  if (FileExists(app + '\Subnautica.exe')) then
  begin
    if (FileExists(app + '\Subnautica_Data\Managed\Assembly-CSharp.dll')) then
    begin
      Result := True
    end
  end
  else
  begin
    Result := False
  end;
end;

function IsTerraTech: Boolean;
var
  app: String;
begin
  app := ExpandConstant('{app}')
  //Result := True
  //Exit
  if FileExists(app + '\TerraTechWin64.exe') then
  begin
    if (FileExists(app + '\TerraTechWin64_Data\Managed\Assembly-CSharp.dll')) then
    begin
      Result := True
    end
  end
  else
  begin
    Result := False
  end;
end;

function CurPageChanged_(CurPageID: Integer): Boolean;
var
  Index: Integer;
begin
  if CurPageID = wpSelectComponents then
  begin
    Index := WizardForm.ComponentsList.Items.IndexOf('Install for Subnautica')
    if Index <> -1 then
    begin
      if IsSubnautica then
      begin
        WizardForm.ComponentsList.Checked[Index] := True
        WizardForm.SelectComponentsLabel.Caption := 'Install for Subnautica'
      end
    end;
    Index := WizardForm.ComponentsList.Items.IndexOf('Install for TerraTech')
    if Index <> -1 then
    begin
      if IsTerraTech then
      begin
        WizardForm.ComponentsList.Checked[Index] := True
        WizardForm.SelectComponentsLabel.Caption := 'Install for TerraTech'
      end
    end
  end;
end;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Password stuff

#if PreRelease == True
  var PasswordEditOnChangePrev: TNotifyEvent;

  function CurPageChanged__(CurPageID: Integer): Boolean;
  begin
    if CurPageID = wpPassword then
    begin
      WizardForm.PasswordEdit.Password := False;
      WizardForm.NextButton.Enabled := False;
    end
  end;

  procedure PasswordEditOnChange(Sender: TObject);
  begin
    if (LowerCase(WizardForm.PasswordEdit.Text) = 'yes') or (LowerCase(WizardForm.PasswordEdit.Text) = 'no') then
    begin
      WizardForm.NextButton.Enabled := True
    end
    else
    begin
      WizardForm.NextButton.Enabled := False
    end
  end;

  function InitializeWizard_(): Boolean;
  begin
    PasswordEditOnChangePrev := WizardForm.PasswordEdit.OnChange
    WizardForm.PasswordEdit.OnChange := @PasswordEditOnChange
  end;

  function CheckPassword(Password: String): Boolean;
  begin
    if LowerCase(Password) = 'yes' then
    begin
      Result := True
    end
  end;
#endif

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// GUID stuff

var appIsSet: Boolean;

function GetGUID(def: String): String;
begin
  if not appIsSet then
  begin
    Result := ''
    Exit
  end;
  if IsSubnautica then
  begin
    Result := '{52CC87AA-645D-40FB-8411-510142191678}'
    Exit
  end;
  if IsTerraTech then
  begin
    Result := '{53D64B81-BFF9-47E3-A599-66C18ED14B71}'
    Exit
  end
end;

function InitializeSetup(): Boolean;
begin
  appIsSet := False
  Result := True
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  if CurPageID = wpSelectComponents then
  begin
    appIsSet := True
  end;
  Result := True
end;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Button disable

var TypesComboOnChangePrev: TNotifyEvent;

procedure ComponentsListCheckChanges;
begin
  WizardForm.NextButton.Enabled := (WizardSelectedComponents(False) <> '')
end;

procedure ComponentsListClickCheck(Sender: TObject);
begin
  ComponentsListCheckChanges
end;

procedure TypesComboOnChange(Sender: TObject);
begin
  TypesComboOnChangePrev(Sender)
  ComponentsListCheckChanges
end;

procedure InitializeWizard();
begin
  WizardForm.ComponentsList.OnClickCheck := @ComponentsListClickCheck
  TypesComboOnChangePrev := WizardForm.TypesCombo.OnChange
  WizardForm.TypesCombo.OnChange := @TypesComboOnChange
  #if PreRelease == True
    InitializeWizard_()
  #endif
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  #if InstallerTest == True
    CurPageChanged_DisableReady(CurPageID)
  #endif
  #if PreRelease == True
    CurPageChanged__(CurPageID)
  #endif
  CurPageChanged_(CurPageID)
  if CurPageID = wpSelectComponents then
  begin
    ComponentsListCheckChanges
  end
end;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Obsolete

function GetDefaultDir(def: String): String;
var
I : Integer;
P : Integer;
steamInstallPath : String;
configFile : String;
fileLines: TArrayOfString;
begin
  steamInstallPath := 'not found'
  if RegQueryStringValue( HKEY_LOCAL_MACHINE, 'SOFTWARE\WOW6432Node\Valve\Steam', 'InstallPath', steamInstallPath ) then
  begin
  end;
  if FileExists(steamInstallPath + '\steamapps\common\TerraTech\TerraTechWin64.exe') then
  begin
    Result := steamInstallPath + '\steamapps\common\TerraTech'
    Exit
  end
  else
  begin
    configFile := steamInstallPath + '\config\config.vdf'
    if FileExists(configFile) then
    begin
      if LoadStringsFromFile(configFile, FileLines) then
      begin
        for I := 0 to GetArrayLength(FileLines) - 1 do
        begin
          P := Pos('BaseInstallFolder_', FileLines[I])
          if P > 0 then
          begin
            steamInstallPath := Copy(FileLines[I], P + 23, Length(FileLines[i]) - P - 23)
            if FileExists(steamInstallPath + '\steamapps\common\TerraTech\TerraTechWin64.exe') then
            begin
              Result := steamInstallPath + '\steamapps\common\TerraTech'
              Exit
            end
          end
        end
      end
    end
  end;
  Result := 'C:\';
end;