; File commented by AlexejheroYTB

; Throws an error if the version used to compile this script is not 5.6.1 unicode
#if VER < EncodeVer(5, 6, 1)
  #error A newer version of Inno Setup is required to compile this script (5.6.1 unicode)
#endif
#if VER > EncodeVer(5, 6, 1)
  #error An older version of Inno Setup is required to compile this script (5.6.1 unicode)
#endif
#if !Defined(UNICODE)
  #error An unicode version of Inno Setup is required to compile this script (5.6.1 unicode)
#endif

; Defines some variables
#define Name "QModManager" ; The name of the installer/program (the game name will be added after it)
#define Version "2.0-InstallerTest6" ; The version of the installer/program (should be the same as in the app)
#define Publisher "the QModManager team" ; The authors of the installer/program
#define URL "https://github.com/QModManager" ; The link to the repo

; Defines special flags that change the way the installer behaves
#define PreRelease true ; If this is true, a window will appear, letting the user know that this version is a pre-relese
#define InstallerTest true ; If this is true, the app won't be able to be installed
#define FastTravel true ; This will hide the pre-release window even if its condition is set to true

; Different versions for different games have different guids
; DO NOT CHANGE UNDER ANY CIRCUMSTANCES
#define SubnauticaGUID '{52CC87AA-645D-40FB-8411-510142191678}'
#define TerraTechGUID '{53D64B81-BFF9-47E3-A599-66C18ED14B71}'

; Overrides the previous pre-release condition if fast travel is enabled
#if FastTravel == true
  #define PreRelease false
#endif

[Setup]
#if InstallerTest == false ; Disables installing the app on network paths (if it isn't and installer test)
  AllowNetworkDrive=no
  AllowUNCPath=no
#else ; Allows installing the app at the base of a drive (only if it's an installer test)
  AllowRootDirectory=yes
#endif
; Makes the install path appear on the Ready to Install page
AlwaysShowDirOnReadyPage=yes
; Fixes an issue with the previous version where 'not found' would appear at the end of the path
AppendDefaultDirName=no
; The GUID of the app (different for different games)
AppId=StringToGUID({code:GetGUID})
; The app name
; TODO: Append game name to the end
AppName={#Name}
; Authors of the app
AppPublisher={#Publisher}
; URLs that will appear on the information page of the app in the Add or Remove Programs page
AppPublisherURL={#URL}
AppSupportURL={#URL}
AppUpdatesURL={#URL}
; Display name of the app in the Add or Remove Programs page
; TODO: Append game name to the end
AppVerName={#Name} {#Version}
; Sets the version of the app
AppVersion={#Version}
; How the installer compresses the required files
Compression=lzma
; The default directory name (this is not used, but it needs to have a value)
DefaultDirName=.
; Disables directory exists warnings
DirExistsWarning=no
; Forces the choose install path page to appear
DisableDirPage=no
; Disables a page that is not used
DisableProgramGroupPage=yes
; Enables the welcome page
DisableWelcomePage=no
; Enables directory doesn't exist warnings
; TODO: Create own pop-up for customization
EnableDirDoesntExistWarning=yes
; Shows information before installing
InfoBeforeFile=Info.txt
; The output file name
OutputBaseFilename=QModManager_Setup
; The output directory
OutputDir=.
; The application needs administrator access in case the user has steam in Program Files x86 (which is protected)
PrivilegesRequired=admin
; Restarts closed applications after install
RestartApplications=yes
; Icon file
SetupIconFile=..\Assets\icon.ico
; Changes compression, smaller size
SolidCompression=yes
; Uninstall icon file
UninstallDisplayIcon=..\Assets\icon.ico
; Uninstall app name
; TODO: Append game name at the end
UninstallDisplayName={#Name}
; Disables the usage of previous settings (when updating) because the GUID is generated too late for them to work
UsePreviousAppDir=no
UsePreviousLanguage=no
; Images that appear in the installer
WizardImageFile=..\Assets\Placeholder.bmp
WizardSmallImageFile=..\Assets\SmallImage.bmp

[Languages]
; Uses default messages when not overriden
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; Required files
Source: "0Harmony.dll"; DestDir: "{app}\TerraTechWin64_Data\Managed"; Flags: IgnoreVersion
Source: "Mono.Cecil.dll"; DestDir: "{app}\TerraTechWin64_Data\Managed"; Flags: IgnoreVersion
Source: "QModInstaller.dll"; DestDir: "{app}\TerraTechWin64_Data\Managed"; Flags: IgnoreVersion
Source: "QModManager.exe"; DestDir: "{app}\TerraTechWin64_Data\Managed"; Flags: IgnoreVersion

[Run]
; On install, run executable based on condition
Filename: {app}\Subnautica_Data\Managed\QModManager.exe; Parameters: "Type=Install,Game=Subnautica"; Check: IsSubnautica
Filename: {app}\TerraTechWin64_Data\Managed\QModManager.exe; Parameters: "Type=Install,Game=TerraTech"; Check: IsTerraTech

[UninstallRun]
; On uninstall, run executable based on condition
Filename: {app}\Subnautica_Data\Managed\QModManager.exe; Parameters: "Type=Uninstall,Game=Subnautica"; Check: IsSubnautica
Filename: {app}\TerraTechWin64_Data\Managed\QModManager.exe; Parameters: "Type=Uninstall,Game=TerraTech"; Check: IsTerraTech

[Messages]
; The text that appears in the bottom-left, on the line of the box
BeveledLabel={#Name} {#Version}
; The installer isn't password-protected, but the feature is used for the pre-release warning if the condition is set to true
WizardPassword=Warning
PasswordLabel1=Please read the following important information before continuing.
PasswordLabel3=You are trying to install a pre-release version of QModManager.%nPre-releases are unstable and might contain bugs.%nWe are not responsible for any crashes or world corruptions that might occur.%n%nPlease type 'YES' (without quotes) to continue with the installation.
PasswordEditLabel=Consent:
; The text that appears on the Select install location page
WizardSelectDir=Select install location
SelectDirLabel3=Please select the install folder of the game.
SelectDirBrowseLabel=If this is correct, click Next. If you need to select a different install folder, click Browse.
; The installer doesn't use components, but the feature is used for displaying the install type
WizardSelectComponents=Review Install
SelectComponentsDesc=
SelectComponentsLabel2=
; Changes the text on the Ready to Install page if the installer is a test
#if InstallerTest == true
  WizardReady=Installer test
  ReadyLabel1=This is just an installer test
  ReadyLabel2a=As this is just a dummy prototype for the installer, you cannot actually install it.%nThank you for trying it out!
#endif
; The message that appears when the user tries to cancel the install
ExitSetupMessage=Setup is not complete. If you exit now, {#Name} will not be installed.%nExit Setup?

[Types]
; Used to disable the three Full, Compact and Custom types
Name: "select"; Description: "QModManager"; Flags: IsCustom

[Components]
; Adds read-only components that are only used for displaying
Name: "qmm"; Description: "QModManager"; Flags: fixed; Types: select

Name: "qmm\sn"; Description: "Install for Subnautica"; Flags: exclusive fixed
Name: "qmm\tt"; Description: "Install for TerraTech"; Flags: exclusive fixed

[Code]

// Scroll all the way to the bottom for syntax basics

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Game detect

var SN_Already: Boolean; // True if a message has already been outputted to the console saying that Subnautica is (not) installed in the current folder, false otherwise

function IsSubnautica: Boolean; // Checks if Subnautica is installed in the current folder
var
  app: String;
begin
  try
    app := ExpandConstant('{app}') // Saves the app variable so it doesn't need to be extended every time
  except // If an exception is thrown (the app variable is not defined) <<THIS SHOULD NEVER HAPPEN>>
    Log('[GAME-DETECT] ERROR: "{app}" variable not defined!')
    Result := false // Returns false
    Exit
  end;
  if (FileExists(app + '\Subnautica.exe')) and (FileExists(app + '\Subnautica_Data\Managed\Assembly-CSharp.dll')) then // If Subnautica-specific files exist
  begin
    if SN_Already = false then // If the message hasn't already been logged
    begin
      Log('[GAME-DETECT] Subnautica is installed')
      SN_Already := true;
    end;
    Result := true // Returns true
    Exit
  end
  else
  begin
    if SN_Already = false then // If the message hasn't already been logged
    begin
      Log('[GAME-DETECT] Subnautica is not installed')
      SN_Already := true;
    end;
    Result := false // Returns false
    Exit
  end
end;

var TT_Already: Boolean; // True if a message has already been outputted to the console saying that TerraTech is (not) installed in the current folder, false otherwise

function IsTerraTech: Boolean; // Checks if Subnautica is installed in the current folder
var
  app: String;
begin
  try
    app := ExpandConstant('{app}') // Saves the app variable so it doesn't need to be extended every time
  except // If an exception is thrown (the app variable is not defined) <<THIS SHOULD NEVER HAPPEN>>
    Log('[GAME-DETECT] ERROR: "{app}" variable not defined!')
    Result := false // Returns false
    Exit
  end;
  if FileExists(app + '\TerraTechWin64.exe') then
  begin
    if (FileExists(app + '\TerraTechWin64_Data\Managed\Assembly-CSharp.dll')) then // If TerraTech-specific files exist
    begin
      Result := true // Returns true
      if TT_Already = false then // If the message hasn't already been logged
      begin
        Log('[GAME-DETECT] TerraTech is installed')
        TT_Already := true;
      end
    end
  end
  else
  begin
    Result := false // Returns false
    if TT_Already = false then // If the message hasn't already been logged
    begin
      Log('[GAME-DETECT] TerraTech is not installed')
      TT_Already := true;
    end
  end
end;

function CurPageChanged_SelectComponents(CurPageID: Integer): Boolean; // Executes whenever the page is changed
var
  Index: Integer;
  app: String;
begin
  if CurPageID = wpSelectComponents then // If the page is Select components (aka Review install)
  begin
    try
      app := ExpandConstant('{app}')
    except
      app := 'null'
    end;
    if IsSubnautica and IsTerraTech then // If multiple games detected (This should never happen in theory)
    begin
      WizardForm.SelectComponentsLabel.Caption := 'Multiple games detected in the same folder, cannot install'
      Log('[COMPONENTS] Multiple games detected in this folder! (' + app + ')')
      Exit
    end;
    if not IsSubnautica and not IsTerraTech then // If no games are detected
    begin
      WizardForm.SelectComponentsLabel.Caption := 'No game detected in this folder, cannot install'
      Log('[COMPONENTS] No games detected in this folder! (' + app + ')')
      Exit
    end;
    Index := WizardForm.ComponentsList.Items.IndexOf('Install for Subnautica') // Gets the index of the component
    if Index <> -1 then // If the component exists (it should)
    begin
      if IsSubnautica then
      begin
        WizardForm.ComponentsList.Checked[Index] := true // Checks it
        WizardForm.SelectComponentsLabel.Caption := 'Install QModManager for Subnautica' // Changes the description
        Log('[COMPONENTS] "Install for Subnautica" component checked')
      end
    end;
    Index := WizardForm.ComponentsList.Items.IndexOf('Install for TerraTech')
    if Index <> -1 then
    begin
      if IsTerraTech then
      begin
        WizardForm.ComponentsList.Checked[Index] := true
        WizardForm.SelectComponentsLabel.Caption := 'Install QModManager for TerraTech'
        Log('[COMPONENTS] "Install for TerraTech" component checked')
      end
    end
  end
end;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Get install path from registry

var Output: TStringList;

function GetDir(folder: String; name: String): String;
var
I : Integer;
P : Integer;
steamInstallPath : String;
configFile : String;
fileLines: TArrayOfString;
temp: Integer;
begin
  steamInstallPath := 'Steam install location not found in registry' // Sets a dummy value
  RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\WOW6432Node\Valve\Steam', 'InstallPath', steamInstallPath) // Gets the install path of steam from the registry
  if (FileExists(steamInstallPath + '\steamapps\common\' + folder + '\' + name + '.exe')) and (FileExists(steamInstallPath + '\steamapps\common\' + folder + '\' + name + '_Data\Managed\Assembly-CSharp.dll')) then // If game files exist
  begin
    if Output.IndexOf(folder) = -1 then // If the game hasn't already been logged
    begin
      Log('[GET-DIR] Game "' + folder + '" found in base steam folder (' + steamInstallPath + ')')
      Output.Add(folder) // Adds it to the array, essentially marking it as logged
    end;
    Result := steamInstallPath + '\steamapps\common\' + folder
    Exit
  end
  else // If the game files DON'T exist
  begin
    configFile := steamInstallPath + '\config\config.vdf' // Gets the path to the steam config file
    if FileExists(configFile) then // If the config file exists
    begin
      // Does some very complicated stuff to get other install folders
      if LoadStringsFromFile(configFile, FileLines) then 
      begin
        for I := 0 to GetArrayLength(FileLines) - 1 do
        begin
          P := Pos('BaseInstallFolder_', FileLines[I])
          if P > 0 then
          begin
            steamInstallPath := Copy(FileLines[I], P + 23, Length(FileLines[i]) - P - 23)
            if (FileExists(steamInstallPath + '\steamapps\common\' + folder + '\' + name + '.exe')) and (FileExists(steamInstallPath + '\steamapps\common\' + folder + '\' + name + '_Data\Managed\Assembly-CSharp.dll')) then // If the folder is correct
            begin
              if Output.IndexOf(folder) = -1 then // If it hasn't already been logged
              begin
                Log('[GET-DIR] Game "' + folder + '" found in alternate steam install location (' + steamInstallPath + ')')
                Output.Add(folder)
              end;
              Result := steamInstallPath + '\steamapps\common\' + folder
              Exit
            end
          end
        end
      end
    end
  end;
  if Output.IndexOf(folder) = -1 then
  begin
    Log('[GET-DIR] Game "' + folder + '" not found on steam')
    Output.Add(folder)
  end;
  Result := 'x' // Returns dummy value (before it was an empty string, but that would conflict with other stuff, so I changed it)
  Exit
end;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Auto-complete check boxes

var ACLabel: TLabel; // "Auto-complete path for:" label
var SubnauticaButton: TNewRadioButton;
var TerraTechButton: TNewRadioButton;

procedure SubnauticaButtonOnClick(Sender: TObject);
begin
  WizardForm.DirEdit.Text := GetDir('Subnautica', 'Subnautica')
  Log('[BUTTONS] Setting path to Subnautica folder')
end;

procedure TerraTechButtonOnClick(Sender: TObject);
begin
  WizardForm.DirEdit.Text := GetDir('TerraTech', 'TerraTechWin64')
  Log('[BUTTONS] Setting path to TerraTech folder')
end;

function InitializeWizard_AddButtons(): Boolean; // Is called when the wizard gets initialized
begin
  ACLabel := TLabel.Create(WizardForm) // Create
  with ACLabel do // Set properties
  begin
    Parent := WizardForm
    Caption := 'Auto-complete path for:'
    Left := WizardForm.BackButton.Left - 360
    Top := WizardForm.BackButton.Top - 8
  end;
  Log('[BUTTONS] Created label')

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
  Log('[BUTTONS] Created button for Subnautica')
  
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
  Log('[BUTTONS] Created button for TerraTech')
end;

function CurPageChanged_AddButtons(CurPageID: Integer): Boolean; // Is called whenever the page is changed
var
  LastPage: Boolean;
begin
  if CurPageID = wpSelectDir then // If the page is select install path
  begin
    WizardForm.DirEdit.Text := '' // Sets the install path to an empty string
    if (GetDir('Subnautica', 'Subnautica') = 'x') and (SubnauticaButton.Enabled = true) then // If Subnautica isn't found
    begin
      SubnauticaButton.Enabled := false
      Log('[BUTTONS] Disabled button for Subnautica')
    end;
    if (GetDir('TerraTech', 'TerraTechWin64') = 'x') and (TerraTechButton.Enabled = true) then // If TerraTech isn't found
    begin
      TerraTechButton.Enabled := false
      Log('[BUTTONS] Disabled button for TerraTech')
    end;
    
    if SubnauticaButton.Enabled and not TerraTechButton.Enabled then // If only Subnautica is found
    begin
      WizardForm.DirEdit.Text := GetDir('Subnautica', 'Subnautica') // Sets path to Subnautica install location
      SubnauticaButton.Checked := true
      Log('[BUTTONS] Automatically set path to Subnautica folder')
    end
    else if TerraTechButton.Enabled and not SubnauticaButton.Enabled then
    begin
      WizardForm.DirEdit.Text := GetDir('TerraTech', 'TerraTech/Win64') // Sets path to Subnautica install location
      TerraTechButton.Checked := true
      Log('[BUTTONS] Automatically set path to TerraTech folder')
    end;
    Log('[BUTTONS] Page changed, buttons visible')
    LastPage := true // LastPage is a boolean variable which is used just to stop the Page changed message to appear multiple times
  end
  else
  begin
    if LastPage = true then
    begin
      Log('[BUTTONS] Page changed, buttons hidden')
      LastPage := false
    end
  end;
  SubnauticaButton.Visible := CurPageID = wpSelectDir // Enables or disables the buttons
  TerraTechButton.Visible := CurPageID = wpSelectDir
  ACLabel.Visible := CurPageID = wpSelectDir
end;

var DirEditOnChangePrev: TNotifyEvent;

procedure DirEditOnChange(Sender: TObject);
begin
  if LowerCase(WizardForm.DirEdit.Text) = LowerCase(GetDir('Subnautica', 'Subnautica')) then // If the Subnautica path is typed manually
  begin
    if not SubnauticaButton.Checked then // Don't log the message if the button is already checked
    begin
      Log('[BUTTONS] Path for Subnautica automatically detected, checked button')
    end;
    SubnauticaButton.Checked := true // Check the button
  end
  else if LowerCase(WizardForm.DirEdit.Text) = LowerCase(GetDir('TerraTech', 'TerraTechWin64')) then // Same for TerraTech
  begin
    if not TerraTechButton.Checked then
    begin
      Log('[BUTTONS] Path for TerraTech automatically detected, checked button')
    end;
    TerraTechButton.Checked := true
  end
  else // If the path doesn't match any of the known ones, disable buttons
  begin
    SubnauticaButton.Checked := false;
    TerraTechButton.Checked := false;
  end 
end;

function InitializeWizard_DirOnChange(): Boolean; // Overrides the DirEdit.OnChange event
begin
  DirEditOnChangePrev := WizardForm.DirEdit.OnChange
  WizardForm.DirEdit.OnChange := @DirEditOnChange
  Log('[EVENTS] Added directory on change event')
end;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// GUID stuff

var appIsSet: Boolean; // True if {app} has a value, false otherwise

function GetGUID(def: String): String;
begin
  if not appIsSet then // The installer tries to get the GUID at startup to use previous options such as install path or install settings. As QModManager's GUID is defined AFTER the path is selected, it doesn't need to provide a value
  begin
    Result := ''
    Log('[GUID] Returned empty app id at startup. This is normal')
    Exit
  end; // The rest is self-explanatory. A different GUID is provided based on selected install location
  if IsSubnautica then
  begin
    Result := '{52CC87AA-645D-40FB-8411-510142191678}'
    Log('[GUID] Returned app id for Subnautica version')
    Exit
  end;
  if IsTerraTech then
  begin
    Result := '{53D64B81-BFF9-47E3-A599-66C18ED14B71}'
    Log('[GUID] Returned app id for TerraTech version');
    Exit
  end
end;

// Called when the app launches. If returns false, cancel install
// Same as InitializeWizard
// TODO: Move and split event functions
function InitializeSetup(): Boolean;
begin
  appIsSet := false // Sets a default value
  Result := true
end;

// Called whenever the Next button is clicked. If returns false, cancel click
// Same as CurPageChanged ONLY IF THE PAGE IS CHANGED BY CLICKING THE BUTTON. If the page is changed thru script, this doesn't get called
// TODO: Move and split event functions
function NextButtonClick(CurPageID: Integer): Boolean;
begin
  if CurPageID = wpSelectComponents then // If the path has been selected, it means that the {app} variable is defined
  begin
    appIsSet := true // Set it to true
  end;
  Result := true
end;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Installer test

// This code only gets compiled if the InstallerTest flag is set to true

#if InstallerTest == true
  var LastValue_DisableInstall: Boolean; // Some boolean values to avoid logging the same message twice in a row
  var LastValue_EnableInstall: Boolean;

  function CurPageChanged_DisableInstall(CurPageID: Integer): Boolean;
  begin
    if CurPageID = wpReady then
    begin
      WizardForm.NextButton.Enabled := false
      LastValue_DisableInstall := false
      LastValue_EnableInstall := true
      Log('[INSTALLER-TEST] Next button disabled, this is just an dummy installer prototype')
    end
    else if LastValue_DisableInstall = false then
    begin
      if LastValue_EnableInstall = true then
      begin
        WizardForm.NextButton.Enabled := true
        LastValue_DisableInstall := true
        LastValue_EnableInstall := false
        Log('[INSTALLER-TEST] Next button enabled, page changed')
      end
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
  if WizardSelectedComponents(false) <> '' then
  begin
    Log('[FOLDER-CHECK] Next button disabled, cannot install in this folder')
  end
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
  InitializeWizard_AddButtons
  InitializeWizard_DirOnChange
  Output := TStringList.Create
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

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Syntax tutorial

#if false ; This prevents the code from compiling and outputting errors. You shouldn't do this :P

// Defining a variable
var <name>: <type>;
// <name>: Can be any value, just don't use any pre-defined ones
// <type>: The most usual ones are: String, Integer, Boolean

#endif