param
(
    [string]$SQLServerNameInstance = (Read-Host -prompt "Database Server (Default is $($env:computername))"),
    [string]$SQLAIRPORTVISIONDB    = (Read-Host -prompt "Database (Default is EclipsXDB)"),
    [string]$CONFIGUSER            = (Read-Host -prompt "Database User"),
    [string]$CONFIGPWD             = (Read-Host -prompt "Database Password"),
    [string]$AUTHAPIINSTALLFOLDER  = (Read-Host -prompt "Installation Folder.  A single letter will simply change the drive. (Default is C:\Program Files\Com-Net\AirportVision REST API\Authentication)"),
	[string]$RootInstallsPath      = "$PSScriptRoot\..",
    [string]$InstallerPath          = "$PSScriptRoot\..\Modules\API"
)

Import-Module Files

if(!$SQLServerNameInstance) {
	$SQLServerNameInstance = $env:computername
}

if(!$SQLAIRPORTVISIONDB) {
	$SQLAIRPORTVISIONDB = "EclipsXDB"
}

if(!$AUTHAPIINSTALLFOLDER) {
	$AUTHAPIINSTALLFOLDER = "C:\Program Files\Com-Net\AirportVision REST API\Authentication"
}

if($AUTHAPIINSTALLFOLDER.Length -eq 1) {
	$AUTHAPIINSTALLFOLDER = $AUTHAPIINSTALLFOLDER + ":\Program Files\Com-Net\AirportVision REST API\Authentication"
}

$ScriptPath = "$RootInstallsPath\Scripts\PS_Modules\ScriptLibrary"
."$ScriptPath\LogFunctions.ps1"
."$ScriptPath\UtilityFunctions.ps1"
."$ScriptPath\ComNet-MSILibrary.ps1"


$InstallComponent = 'AirportVision Authentication REST Installer'

$VersionsFile = "$InstallerPath\versions.txt"
$LogFile = "$InstallerPath\AirportVision Authentication REST Installer.install.log"

if($InstallComponent -ne '')
{
	$Properties = "/qb",
				"P.TOKENMINUTES=`"30`"",
				"P.SQLCONNSERVER=`"$SQLServerNameInstance`"",
				"P.SQLAIRPORTVISIONDB=`"$SQLAIRPORTVISIONDB`"",
				"P.CONFIGUSER=`"$CONFIGUSER`"",
				"P.CONFIGPWD=`"$CONFIGPWD`"",
				"APVAUTHINSTALLFOLDER=`"$AUTHAPIINSTALLFOLDER`"",
				"/l*v ""$InstallerPath\AirportVision Authentication REST Installer.install.log`""

	Run-APVMSI "$InstallComponent" "$Properties" "$InstallerPath" "$LogFile" "$VersionsFile"

	Import-Module WebAdministration
	$AUTHAPIINSTALLFOLDER = $AUTHAPIINSTALLFOLDER.TrimEnd('\')

	$PARENTFOLDEROBJ = (Get-Item $AUTHAPIINSTALLFOLDER -ErrorAction SilentlyContinue)
	
	#If this is null, and if the installer didn't fail prior to this, it means the app was installed
	#   to another foler.
	if($PARENTFOLDEROBJ) {
		$PARENTFOLDER = $PARENTFOLDEROBJ.Parent.FullName
	
		if(!(Test-Path 'IIS:\Sites\Default Web Site\apvREST')) {
			New-Item -Path 'IIS:\Sites\Default Web Site\apvREST' -physicalPath "$PARENTFOLDER" -type VirtualDirectory -Force -ErrorAction SilentlyContinue
		}

		if(!(Get-WebApplication -Site 'Default Web Site' -Name 'apvREST\AuthAPI')) {
			New-Item -Path 'IIS:\Sites\Default Web Site\apvREST\AuthAPI' -physicalPath "$AUTHAPIINSTALLFOLDER" -type Application -Force -ErrorAction SilentlyContinue
		}

		if(!(Test-Path 'IIS:\AppPools\DotNetCORE')) {
			New-Item -Path 'IIS:\AppPools\DotNetCORE' -Force -ErrorAction SilentlyContinue
		}

		Set-ItemProperty -Path IIS:\AppPools\DotNetCORE -Name managedRuntimeVersion -Value ''
		Set-ItemProperty -Path 'IIS:\Sites\Default Web Site\apvREST\AuthAPI' -Name applicationPool -Value 'DotNetCORE' 

		Set-WebConfigurationProperty -filter /system.webServer/security/authentication/windowsAuthentication -name enabled -value true -PSPath IIS:\ -location 'Default Web Site/apvREST/AuthAPI'
		Set-WebConfigurationProperty -filter /system.webServer/security/authentication/anonymousAuthentication -name enabled -value true -PSPath IIS:\ -location 'Default Web Site/apvREST/AuthAPI'
	}  else {
		Write-Host 'WARNING: The Authentication was already installed in another location.  It was not moved.  Uninstall and remove IIS applications before rerunning this script.'	
	}
}

