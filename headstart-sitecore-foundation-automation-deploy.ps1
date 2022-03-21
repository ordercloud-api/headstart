$RootScriptPath = Get-Location;
$SitecoreFoundationSitecoreExtensions = ("{0}\\src\Middleware\src\SitecoreExtensions\code\" -f $RootScriptPath);
$MachineName = $env:computername;
$MSBuildExe = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\msbuild.exe"

function buildVS
{
    param
    (
        [parameter(Mandatory=$true)]
        [String] $path,

        [parameter(Mandatory=$false)]
        [bool] $nuget = $true,
        
        [parameter(Mandatory=$false)]
        [bool] $clean = $true
    )
	
    process
    {
        if ($nuget) {
            Write-Host "Restoring NuGet packages" -foregroundcolor green
            nuget restore "$($path)"
        }

        if ($clean) {
            Write-Host "Cleaning $($path)" -foregroundcolor green
            & "$($MSBuildExe)" "$($path)" /t:Clean /m
        }

        Write-Host "Building $($path)" -foregroundcolor green
        & "$($MSBuildExe)" "$($path)" /t:Build /m
    }
}

function Upgrade-Current-Python-Version {
	try 
	{
		Write-Host "Building Upgrade-Current-Python-Version with Dependencies - Started.";
		$isChocolateyInstalled = powershell choco -v;
		
		if (-not($isChocolateyInstalled)) 
		{
			Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'));
		}
		else 
		{
			Write-Host "Chocolate version {$isChocolateyInstalled} is already installed.";
		}
		
		$python310FolderPath = "C:\Python310";
		if (-not(Test-Path $python310FolderPath))
		{				
			choco install -y --f python --version=3.10.0;
		}
		else 
		{
			Write-Host "The {$python310FolderPath} folder already exist.";
		}
		Write-Host "Building Upgrade-Current-Python-Version with Dependencies - Completed.";
	}
	catch 
	{
		Quit ("An Exception error occured in the Upgrade-Current-Python-Version method. {0}>" -f $Error[0]);
	}
}

function SitecoreFoundation-Initialization-Autotmation {
	try 
	{
		if ((Test-Path $SitecoreFoundationSitecoreExtensions))
		{
			Write-Host "Building Headstart Repo Solution with Dependencies - Started";
			if (-not(Test-Path $MSBuildExe))
			{
				Write-Host "Installing and Configuring Visual Studio Build Tools 2022 - Started";
				choco upgrade -y visualstudio2022-workload-vctools
				npm config set msvs_version 2022;
				Write-Host "Installing and Configuring Visual Studio Build Tools 2022 - Completed";
			}
			else
			{
				Write-Host "Visual Studio Build Tools 2022 already installed and configured ('$MSBuildExe')";
			}
			
			cd $SitecoreFoundationSitecoreExtensions;
			buildVS Sitecore.Foundation.SitecoreExtensions.sln;
			Write-Host "Building Headstart Repo Solution with Dependencies - Completed";
		}
		else
		{
			Write-Host "One of the following folders: '$SitecoreFoundationSitecoreExtensions' does not exist.";	
		}
	}
	catch 
	{
		Quit ("An Exception error occured in the SitecoreFoundation-Initialization-Autotmation. {0}" -f $Error[0]);
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

Write-Host "SitecoreFoundation-Initialization-Autotmation - Started";
Set-ItemProperty 'HKLM:\System\CurrentControlSet\Control\FileSystem' -Name 'LongPathsEnabled' -value 1 -Force;
Upgrade-Current-Python-Version;
SitecoreFoundation-Initialization-Autotmation;
Write-Host "SitecoreFoundation-Initialization-Autotmation - Completed";
cd $RootScriptPath;