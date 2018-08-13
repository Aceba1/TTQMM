#define Name "QModManager"
#define Version "2.0-InstallerTest4"
#define Publisher "the QModManager team"
#define URL "https://github.com/QModManager"

#define PreRelease true
#define InstallerTest true

#define SubnauticaGUID '{52CC87AA-645D-40FB-8411-510142191678}'
#define TerraTechGUID '{53D64B81-BFF9-47E3-A599-66C18ED14B71}'

[Setup]
#if InstallerTest == false
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
Compression=lzma
DefaultDirName=.
DirExistsWarning=no
DisableDirPage=no
DisableProgramGroupPage=yes
DisableWelcomePage=no
EnableDirDoesntExistWarning=yes
InfoBeforeFile=Info.txt
OutputBaseFilename=QModManager_Setup
OutputDir=.
PrivilegesRequired=admin
RestartApplications=yes
SetupIconFile=..\Assets\icon.ico
SolidCompression=yes
UninstallDisplayIcon=..\Assets\icon.ico
UninstallDisplayName={#Name}
UsePreviousAppDir=no
UsePreviousLanguage=no
WizardImageFile=..\Assets\Placeholder.bmp
WizardSmallImageFile=..\Assets\SmallImage.bmp

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "0Harmony.dll"; DestDir: "{app}\TerraTechWin64_Data\Managed"; Flags: IgnoreVersion
Source: "Mono.Cecil.dll"; DestDir: "{app}\TerraTechWin64_Data\Managed"; Flags: IgnoreVersion
Source: "QModInstaller.dll"; DestDir: "{app}\TerraTechWin64_Data\Managed"; Flags: IgnoreVersion
Source: "QModManager.exe"; DestDir: "{app}\TerraTechWin64_Data\Managed"; Flags: IgnoreVersion;

[Run]
Filename: {app}\Subnautica_Data\Managed\QModManager.exe; Parameters: "Type=Install,Game=Subnautica"; Check: IsSubnautica
Filename: {app}\TerraTechWin64_Data\Managed\QModManager.exe; Parameters: "Type=Install,Game=TerraTech"; Check: IsTerraTech

[UninstallRun]
Filename: {app}\Subnautica_Data\Managed\QModManager.exe; Parameters: "Type=Uninstall,Game=Subnautica"; Check: IsSubnautica
Filename: {app}\TerraTechWin64_Data\Managed\QModManager.exe; Parameters: "Type=Uninstall,Game=TerraTech"; Check: IsTerraTech

[Messages]
BeveledLabel={#Name} {#Version}
WizardPassword=Warning
PasswordLabel1=Please read the following important information before continuing.
PasswordLabel3=You are trying to install a pre-release version of QModManager.%nPre-releases are unstable and might contain bugs.%nWe are not responsible for any crashes or world corruptions that might occur.%n%nPlease type 'YES' (without quotes) to continue with the installation.
PasswordEditLabel=Consent:
WizardSelectDir=Select install location
SelectDirLabel3=Please select the install folder of the game.
SelectDirBrowseLabel=If this is correct, click Next. If you need to select a different install folder, click Browse.
WizardSelectComponents=Review Install
SelectComponentsDesc=
SelectComponentsLabel2=Cannot install in this folder
#if InstallerTest == true
  WizardReady=Installer test
  ReadyLabel1=This is just an installer test
  ReadyLabel2a=As this is just a dummy prototype for the installer, you cannot actually install it. Thank you for trying it out!
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
// Game detect

var SN_Already: Boolean;

function IsSubnautica: Boolean;
var
  app: String;
begin
  app := ExpandConstant('{app}')
  //Result := true
  //Exit
  if (FileExists(app + '\Subnautica.exe')) then
  begin
    if (FileExists(app + '\Subnautica_Data\Managed\Assembly-CSharp.dll')) then
    begin
      Result := true
      if SN_Already = false then
      begin
        Log('[GAME-DETECT] Subnautica is installed')
        SN_Already := true;
      end
    end
  end
  else
  begin
    Result := false
    if SN_Already = false then
    begin
      Log('[GAME-DETECT] Subnautica is not installed')
      SN_Already := true;
    end
  end
end;

var TT_Already: Boolean;

function IsTerraTech: Boolean;
var
  app: String;
begin
  app := ExpandConstant('{app}')
  //Result := true
  //Exit
  if FileExists(app + '\TerraTechWin64.exe') then
  begin
    if (FileExists(app + '\TerraTechWin64_Data\Managed\Assembly-CSharp.dll')) then
    begin
      Result := true
      if TT_Already = false then
      begin
        Log('[GAME-DETECT] TerraTech is installed')
        TT_Already := true;
      end
    end
  end
  else
  begin
    Result := false
    if TT_Already = false then
    begin
      Log('[GAME-DETECT] TerraTech is not installed')
      TT_Already := true;
    end
  end
end;

function CurPageChanged_SelectComponents(CurPageID: Integer): Boolean;
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
        WizardForm.ComponentsList.Checked[Index] := true
        WizardForm.SelectComponentsLabel.Caption := 'Install for Subnautica'
        Log('[COMPONENTS] "Install for Subnautica" component checked')
      end
    end;
    Index := WizardForm.ComponentsList.Items.IndexOf('Install for TerraTech')
    if Index <> -1 then
    begin
      if IsTerraTech then
      begin
        WizardForm.ComponentsList.Checked[Index] := true
        WizardForm.SelectComponentsLabel.Caption := 'Install for TerraTech'
        Log('[COMPONENTS] "Install for TerraTech" component checked')
      end
    end
  end
end;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Get install path from registry

function GetDir(folder: String; name: String): String;
var
I : Integer;
P : Integer;
steamInstallPath : String;
configFile : String;
fileLines: TArrayOfString;
begin
  steamInstallPath := 'not found'
  RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\WOW6432Node\Valve\Steam', 'InstallPath', steamInstallPath)
  if (FileExists(steamInstallPath + '\steamapps\common\' + folder + '\' + name + '.exe')) and (FileExists(steamInstallPath + '\steamapps\common\' + folder + '\' + name + '\Assembly-CSharp.dll')) then
  begin
    Result := steamInstallPath + '\steamapps\common\' + folder
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
            if (FileExists(steamInstallPath + '\steamapps\common\' + folder + '\' + name + '.exe')) and (FileExists(steamInstallPath + '\steamapps\common\' + folder + '\' + name + '\Assembly-CSharp.dll')) then
            begin
              Result := steamInstallPath + '\steamapps\common\' + folder
              Exit
            end
          end
        end
      end
    end
  end;
  Result := ''
end;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Auto-complete check boxes

var ACLabel: TLabel;
var SubnauticaButton: TNewRadioButton;
var TerraTechButton: TNewRadioButton;

procedure TerraTechButtonOnClick(Sender: TObject);
begin
  WizardForm.DirEdit.Text := GetDir('TerraTech', 'TerraTechWin64')
end;

procedure SubnauticaButtonOnClick(Sender: TObject);
begin
  WizardForm.DirEdit.Text := GetDir('Subnautica', 'Subnautica')
end;

function InitializeWizard_AddButtons(): Boolean;
begin
  ACLabel := TLabel.Create(WizardForm)
  with ACLabel do
  begin
    Parent := WizardForm
    Caption := 'Auto-complete path for:'
    Left := WizardForm.BackButton.Left - 360
    Top := WizardForm.BackButton.Top - 8
  end;
  Log('[UI] Added auto-complete label')

  SubnauticaButton := TNewRadioButton.Create(WizardForm)
  with SubnauticaButton do
  begin
    Parent := WizardForm
    Caption := 'Subnautica'
    OnClick := @SubnauticaButtonOnClick
    Left := WizardForm.BackButton.Left - 244
    Top := WizardForm.BackButton.Top + 10
    Height := WizardForm.BackButton.Height
  end;
  Log('[UI] Added auto-complete button for Subnautica')
  
  TerraTechButton := TNewRadioButton.Create(WizardForm)
  with TerraTechButton do
  begin
    Parent := WizardForm
    Caption := 'TerraTech'
    OnClick := @TerraTechButtonOnClick
    Left := WizardForm.BackButton.Left - 122
    Top := WizardForm.BackButton.Top + 10
    Height := WizardForm.BackButton.Height
  end;
  Log('[UI] Added auto-complete button for TerraTech')
end;

function CurPageChanged_AddButtons(CurPageID: Integer): Boolean;
begin
  if CurPageID = wpSelectDir then
  begin
    WizardForm.DirEdit.Text := ''
    if (GetDir('Subnautica', 'Subnautica') = '') and (SubnauticaButton.Enabled = true) then
    begin
      SubnauticaButton.Enabled := false
      Log('[UI] Disabled auto-complete button for Subnautica')
    end;
    if (GetDir('TerraTech', 'TerraTechWin64') = '') and (TerraTechButton.Enabled = true) then
    begin
      TerraTechButton.Enabled := false
      Log('[UI] Disabled auto-complete button for TerraTech')
    end;
    if SubnauticaButton.Enabled and not TerraTechButton.Enabled then
    begin
      WizardForm.DirEdit.Text := GetDir('Subnautica', 'Subnautica')
    end
    else if TerraTechButton.Enabled and not SubnauticaButton.Enabled then
    begin
      WizardForm.DirEdit.Text := GetDir('TerraTech', 'TerraTechWin64')
    end
  end;
  SubnauticaButton.Visible := CurPageID = wpSelectDir
  TerraTechButton.Visible := CurPageID = wpSelectDir
  ACLabel.Visible := CurPageID = wpSelectDir
end;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// GUID stuff

var appIsSet: Boolean;

function GetGUID(def: String): String;
begin
  if not appIsSet then
  begin
    Result := ''
    Log('[GUID] Returned empty app id at startup. This is normal')
    Exit
  end;
  if IsSubnautica then
  begin
    Result := '{52CC87AA-645D-40FB-8411-510142191678}'
    Log('[GUID] Returned app id for Subnautica')
    Exit
  end;
  if IsTerraTech then
  begin
    Result := '{53D64B81-BFF9-47E3-A599-66C18ED14B71}'
    Log('[GUID] Returned app id for TerraTech');
    Exit
  end
end;

function InitializeSetup(): Boolean;
begin
  appIsSet := false
  Result := true
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  if CurPageID = wpSelectComponents then
  begin
    appIsSet := true
  end;
  Result := true
end;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Installer test

#if InstallerTest == true
  var LastValue_DisableInstall: Boolean;

  function CurPageChanged_DisableInstall(CurPageID: Integer): Boolean;
  begin
    if CurPageID = wpReady then
    begin
      WizardForm.NextButton.Enabled := false
      LastValue_DisableInstall := false
      Log('[INSTALLER-TEST] Next button disabled, this is just an dummy installer prototype')
    end
    else if LastValue_DisableInstall = false then
    begin
      WizardForm.NextButton.Enabled := true
      LastValue_DisableInstall := true
      Log('[INSTALLER-TEST] Next button enabled, page changed')
    end
  end;
#endif

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Password stuff

#if PreRelease == true
  var PasswordEditOnChangePrev: TNotifyEvent;
  var LastValue_PreRelease: Boolean;

  function CurPageChanged__(CurPageID: Integer): Boolean;
  begin
    if CurPageID = wpPassword then
    begin
      WizardForm.PasswordEdit.Password := false;
      WizardForm.NextButton.Enabled := false;
      LastValue_PreRelease := false;
      Log('[PRE-RELEASE] Next button disabled, need pre-release consent')
    end
  end;

  procedure PasswordEditOnChange(Sender: TObject);
  begin
    if (LowerCase(WizardForm.PasswordEdit.Text) = 'yes') then
    begin
      WizardForm.NextButton.Enabled := true
      LastValue_PreRelease := true
      Log('[PRE-RELEASE] Next button enabled, consent granted')
    end
    else if (LastValue_PreRelease = true) and not (WizardForm.PasswordEdit.Text = '') then
    begin
      WizardForm.NextButton.Enabled := false
      LastValue_PreRelease := false
      Log('[PRE-RELEASE] Next button disabled, consent changed')
    end
  end;

  function InitializeWizard_(): Boolean;
  begin
    PasswordEditOnChangePrev := WizardForm.PasswordEdit.OnChange
    WizardForm.PasswordEdit.OnChange := @PasswordEditOnChange
    Log('[EVENTS] Added password on change event')
  end;

  function CheckPassword(Password: String): Boolean;
  begin
    if LowerCase(Password) = 'yes' then
    begin
      Result := true
    end
  end;
#endif

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Button disable

var TypesComboOnChangePrev: TNotifyEvent;

procedure ComponentsListCheckChanges;
begin
  WizardForm.NextButton.Enabled := (WizardSelectedComponents(false) <> '')
  Log('[FOLDER-CHECK] Next button disabled, cannot install in this folder')
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

procedure InitializeWizard;
begin
  WizardForm.ComponentsList.OnClickCheck := @ComponentsListClickCheck
  TypesComboOnChangePrev := WizardForm.TypesCombo.OnChange
  WizardForm.TypesCombo.OnChange := @TypesComboOnChange
  Log('[EVENTS] Added components list check event')
  #if PreRelease == true
    InitializeWizard_()
  #endif
  InitializeWizard_AddButtons;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  #if InstallerTest == true
    CurPageChanged_DisableInstall(CurPageID)
  #endif
  #if PreRelease == true
    CurPageChanged__(CurPageID)
  #endif
  CurPageChanged_SelectComponents(CurPageID)
  CurPageChanged_AddButtons(CurPageID)
  if CurPageID = wpSelectComponents then
  begin
    ComponentsListCheckChanges
  end
end;