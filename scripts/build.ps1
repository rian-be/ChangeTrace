<#
.SYNOPSIS
    Build & Publish ChangeTrace for multiple runtimes (interactive)
#>

$Project = Join-Path $PSScriptRoot "..\ChangeTrace.csproj"
$Configuration = "Release"
$OutputDir = Join-Path $PSScriptRoot "..\publish"

$AllRuntimes = @("win-x64", "linux-x64", "osx-x64", "linux-arm64", "osx-arm64")
$SelectedRuntimes = @()
$SelfContained = $false

$PublishTimes = @{}

function Write-Color($Text, $Color="White") {
    Write-Host $Text -ForegroundColor $Color
}

function Clean-Output
{
    Write-Color "Cleaning publish directory..." "Yellow"
    if (Test-Path $OutputDir)
    {
        Remove-Item -Recurse -Force $OutputDir
    }
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

function Prompt-SelfContain
 {
    Write-Host "Select build type:"
    Write-Host " 1) Framework-dependent (uses installed .NET) [default]"
    Write-Host " 2) Self-contained (bundles .NET runtime)"
    $choice = Read-Host "Choice [1/2]"
    if ($choice -eq "2")
    {
        $Global:SelfContained = $true
        Write-Color "Selected: Self-contained" "Cyan"
    } else {
        Write-Color "Selected: Framework-dependent" "Cyan"
    }
}

function Prompt-Runtime
 {
    Write-Host "Select runtime(s) to publish (comma separated, default: all):"
    for ($i=0; $i -lt $AllRuntimes.Count; $i++)
    {
        Write-Host " $($i+1)) $($AllRuntimes[$i])"
    }
    $input = Read-Host "Enter numbers (e.g., 1,3,5)"
    if ([string]::IsNullOrEmpty($input)) {
        $Global:SelectedRuntimes = $AllRuntimes
    } else 
    {
        $nums = $input -split ","
        foreach ($n in $nums)
        {
            $index = [int]$n - 1
            $Global:SelectedRuntimes += $AllRuntimes[$index]
        }
    }
    Write-Color ("Selected runtimes: " + ($SelectedRuntimes -join ", ")) "Cyan"
}

Write-Color "==============================" "Green"
Write-Color "ChangeTrace Build & Publish Script" "Green"
Write-Color "==============================" "Green"

$cleanChoice = Read-Host "Do you want to clean publish directory first? (y/N)"
if ($cleanChoice -match "^[Yy]") { Clean-Output }

Prompt-SelfContained
Prompt-Runtimes

$BuildStart = Get-Date
Write-Color "Building project..." "Blue"
dotnet build $Project -c $Configuration
$BuildEnd = Get-Date
$BuildTime = ($BuildEnd - $BuildStart).TotalSeconds

foreach ($runtime in $SelectedRuntimes)
{
    Write-Color "Publishing for $runtime..." "Cyan"
    $startTime = Get-Date
    dotnet publish $Project -c $Configuration -r $runtime --self-contained:$SelfContained -o (Join-Path $OutputDir $runtime)
    $endTime = Get-Date
    $PublishTimes[$runtime] = [math]::Round(($endTime - $startTime).TotalSeconds, 1)
}

Write-Color "==============================" "Green"
Write-Color "Build & Publish Summary" "Green"
Write-Color "==============================" "Green"
Write-Host "Project: $Project"
Write-Host "Configuration: $Configuration"
Write-Host ("Build time: {0}s" -f [math]::Round($BuildTime,1))
Write-Host ("Build type: {0}" -f ($(if ($SelfContained) {"Self-contained"} else {"Framework-dependent"})))
Write-Host ""
Write-Host "Published runtimes:"
foreach ($runtime in $SelectedRuntimes) {
    Write-Host ("  - {0} -> {1} (Time: {2}s)" -f $runtime, (Join-Path $OutputDir $runtime), $PublishTimes[$runtime])
}
Write-Color "All done! Published artifacts are in $OutputDir" "Green"
