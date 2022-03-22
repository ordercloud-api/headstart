$RootScriptPath = Get-Location;
$RootPackagesPath = ("{0}\packages" -f $RootScriptPath);;
$MachineName = $env:computername;
$MSBuildExe = "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\msbuild.exe"

function SitecoreFoundation-PackagesSynchronization-Autotmation {
	try 
	{
		if (-not(Test-Path $RootPackagesPath))
		{
			New-Item -ItemType "directory" -Path $RootPackagesPath;
		}
		
		if ((Test-Path $RootPackagesPath))
		{
			Write-Host "Synchronization of 'RootPackagesPath' for missing packages - Started";
			cd $RootPackagesPath;
			Install-Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform -Version 3.6.0 
			Write-Host "Synchronization of 'RootPackagesPath' for missing packages  - Completed";
		}
		else
		{
			Write-Host "One of the following folders: '$RootPackagesPath' does not exist.";	
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