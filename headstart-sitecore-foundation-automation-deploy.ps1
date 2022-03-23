$RootScriptPath = Get-Location;
$RootPackagesPath = ("{0}\src\Middleware\packages" -f $RootScriptPath);;
$SitecoreFoundationSitecoreExtensions = ("{0}\src\Middleware\src\SitecoreExtensions\code" -f $RootScriptPath);
$SitecoreFoundationSitecoreExtensionsPackages = ("{0}\packages" -f $SitecoreFoundationSitecoreExtensions);
$MachineName = $env:computername;
$MSBuildExe = "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\msbuild.exe"

function SitecoreFoundation-PackagesSynchronization-Autotmation {
	try 
	{
		if (-not(Test-Path $RootPackagesPath))
		{
			Write-Host "Creating the folder '$RootPackagesPath' - Started";
			New-Item -ItemType "directory" -Path $RootPackagesPath;
			Write-Host "Creating the folder '$RootPackagesPath' - Completed";
		}
		if ((Test-Path $RootPackagesPath) -and (Test-Path $SitecoreFoundationSitecoreExtensionsPackages))
		{
			Write-Host "Synchronization of 'SitecoreFoundationSitecoreExtensionsPackages' to 'RootPackagesPath' - Started";
			Copy-Item -Path ("{0}\*" -f $SitecoreFoundationSitecoreExtensionsPackages) -Destination $RootPackagesPath -Force -Recurse;
			Write-Host "Synchronization of 'SitecoreFoundationSitecoreExtensionsPackages' to 'RootPackagesPath' - Completed";
		}
		else
		{
			Write-Host "One of the following folders: '$SitecoreFoundationSitecoreExtensionsPackages'; '$RootPackagesPath' does not exist.";	
		}
	}
	catch 
	{
		Quit ("An Exception error occured in the SitecoreFoundation-PackagesSynchronization-Autotmation. {0}" -f $Error[0]);
	}
}

function GetDotNet4FrameworkVersion($releaseKey) 
{
	$releaseVersion = "";
	
	if ($releaseKey >= 528040) 
	{
		$releaseVersion = "4.8";
	}
    if ($releaseKey >= 461808)
	{
		$releaseVersion = "4.7.2";
	}
    if ($releaseKey >= 461308)
	{
		$releaseVersion = "4.7.1";
	}
    if ($releaseKey >= 460798)
	{
		$releaseVersion = "4.7";
	}
    if ($releaseKey >= 394802)
	{
		$releaseVersion = "4.6.2";
	}
    if ($releaseKey >= 394254)
	{
		$releaseVersion = "4.6.1";
	}
    if ($releaseKey >= 393295)
	{
		$releaseVersion = "4.6";
	}
    if ($releaseKey >= 379893)
	{
		$releaseVersion = "4.5.2";
	}
    if ($releaseKey >= 378675)
	{
		$releaseVersion = "4.5.1";
	}
    if ($releaseKey >= 378389)
	{
		$releaseVersion = "4.5";
	}
    Write-Host "The current .NET 4 version installed is '$releaseVersion'.";
	return $releaseVersion;
}

function Quit($Text) {
    Write-Warning $Text;
	cd $RootScriptPath;
    Break Script;
}

Write-Host "SitecoreFoundation-PackagesSynchronization-Autotmation - Started";
Set-ItemProperty 'HKLM:\System\CurrentControlSet\Control\FileSystem' -Name 'LongPathsEnabled' -value 1 -Force;
$releaseKey = (Get-ItemProperty "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full").Release;
if (GetDotNet4FrameworkVersion($releaseKey) -ne "4.8") 
{
	choco install dotnetfx;
	choco install netfx-4.8-devpack;
}
SitecoreFoundation-PackagesSynchronization-Autotmation;
Write-Host "SitecoreFoundation-PackagesSynchronization-Autotmation - Completed";
cd $RootScriptPath;