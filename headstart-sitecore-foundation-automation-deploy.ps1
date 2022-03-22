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
			New-Item -ItemType "directory" -Path $RootPackagesPath
			Write-Host "Creating the folder '$RootPackagesPath' - Completed";
		}
		if ((Test-Path $RootPackagesPath) -and (Test-Path $SitecoreFoundationSitecoreExtensionsPackages))
		{
			Write-Host "Synchronization of 'SitecoreFoundationSitecoreExtensionsPackages' to 'RootPackagesPath' - Started";
			Copy-Item -Path ("{0}\*" -f $SitecoreFoundationSitecoreExtensionsPackages) -Destination $RootPackagesPath -PassThru;
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

function GetBranch($Branch, $Repo) {
	git checkout $Branch;
	git fetch;
	git clean -f;
	git reset --hard;
	git pull;
	Write-Host "The branch: $Branch was selected for the $Repo repo.";
}

function Quit($Text) {
    Write-Warning $Text;
	cd $RootScriptPath;
    Break Script;
}

Write-Host "SitecoreFoundation-PackagesSynchronization-Autotmation - Started";
Set-ItemProperty 'HKLM:\System\CurrentControlSet\Control\FileSystem' -Name 'LongPathsEnabled' -value 1 -Force;
SitecoreFoundation-PackagesSynchronization-Autotmation;
Write-Host "SitecoreFoundation-PackagesSynchronization-Autotmation - Completed";
cd $RootScriptPath;