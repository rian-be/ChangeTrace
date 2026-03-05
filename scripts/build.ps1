<#
.SYNOPSIS
    Build & Publish ChangeTrace for multiple runtimes (interactive)
#>

param()

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

function Clean-Output {
    Write-Color "Cleaning publish directory..." "Yellow"
    if (Test-Path $OutputDir) {
        Remove-Item -Recurse -Force $OutputDir
    }
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

function Prompt-SelfContained {
    Write-Host "Select build type:"
    Write-Host " 1) Framework-dependent (uses installed .NET) [default]"
    Write-Host " 2) Self-contained (bundles .NET runtime)"
    $choice = Read-Host "Choice [1/2]"
    if ($choice -eq "2") {
        $Global:SelfContained = $true
        Write-Color "Selected: Self-contained" "Cyan"
    } else {
        Write-Color "Selected: Framework-dependent" "Cyan"
    }
}

function Prompt-Runtimes {
    Write-Host "Select runtime(s) to publish (comma separated, default: all):"
    for ($i = 0; $i -lt $AllRuntimes.Count; $i++) {
        Write-Host " $($i+1)) $($AllRuntimes[$i])"
    }
    $input = Read-Host "Enter numbers (e.g., 1,3,5)"
    if ([string]::IsNullOrEmpty($input)) {
        $Global:SelectedRuntimes = $AllRuntimes
    } else {
        $nums = $input -split ","
        foreach ($n in $nums) {
            $index = [int]$n - 1
            if ($index -ge 0 -and $index -lt $AllRuntimes.Count) {
                $Global:SelectedRuntimes += $AllRuntimes[$index]
            } else {
                Write-Color "Invalid runtime index: $n" "Red"
                exit 1
            }
        }
    }
    Write-Color ("Selected runtimes: " + ($SelectedRuntimes -join ", ")) "Cyan"
}

Write-Color "==============================" "Green"
Write-Color "ChangeTrace Build & Publish Script" "Green"
Write-Color "==============================" "Green"

$cleanChoice = Read-Host "Do you want to clean publish directory first? (y/N)"
if ($cleanChoice -match "^[Yy]") { Clean-Output } else { New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null }

Prompt-SelfContained
Prompt-Runtimes

Write-Color "`nRestoring packages..." "Blue"
dotnet restore $Project

Write-Color "`nBuilding project..." "Blue"
$BuildStart = Get-Date
dotnet build $Project -c $Configuration
$BuildEnd = Get-Date
$BuildTime = ($BuildEnd - $BuildStart).TotalSeconds

Write-Color "`nStarting parallel publish..." "Blue"

$jobs = @()
foreach ($runtime in $SelectedRuntimes) {
    $runtimeCopy = $runtime
    $jobs += Start-Job -ScriptBlock {
        param($Project, $Configuration, $OutputDir, $SelfContained, $Runtime)
        $startTime = Get-Date
        $logFile = Join-Path $OutputDir "$Runtime.log"
        dotnet publish $Project -c $Configuration -r $Runtime --self-contained:$SelfContained --no-build -o (Join-Path $OutputDir $Runtime) *> $logFile
        $endTime = Get-Date
        $timeSec = [math]::Round(($endTime - $startTime).TotalSeconds,1)
        $size = (Get-ChildItem -Recurse (Join-Path $OutputDir $Runtime) | Measure-Object -Property Length -Sum).Sum

        [PSCustomObject]@{
            Runtime = $Runtime
            Time = $timeSec
            SizeMB = [math]::Round($size/1MB,1)
        }
    } -ArgumentList $Project, $Configuration, $OutputDir, $SelfContained, $runtimeCopy
}


$results = $jobs | Wait-Job | Receive-Job
$jobs | Remove-Job

foreach ($r in $results) {
    $PublishTimes[$r.Runtime] = "$($r.Time)s, $($r.SizeMB)MB"
}

Write-Color "`n==============================" "Green"
Write-Color "Build & Publish Summary" "Green"
Write-Color "==============================" "Green"

Write-Host "Project: $Project"
Write-Host "Configuration: $Configuration"
Write-Host ("Build time: {0}s" -f [math]::Round($BuildTime,1))
Write-Host ("Build type: {0}" -f ($(if ($SelfContained) {"Self-contained"} else {"Framework-dependent"})))
Write-Host ""
Write-Host "Published runtimes:"
foreach ($runtime in $SelectedRuntimes) {
    Write-Host ("  - {0} -> {1} (Time, Size: {2})" -f $runtime, (Join-Path $OutputDir $runtime), $PublishTimes[$runtime])
}

Write-Color "All done! Published artifacts are in $OutputDir" "Green"