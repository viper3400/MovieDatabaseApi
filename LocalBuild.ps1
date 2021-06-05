# Get config file
$BuildSettingsFile = "LocalBuild.json"
$BuildConfig = Get-Content -Raw -Path $BuildSettingsFile | ConvertFrom-Json

# Get config settings
$ProjectPath = $BuildConfig.ProjectPath
$ProjectFile = $BuildConfig.ProjectFile
$OutputPath = $BuildConfig.DeploymentPath
$Version = $BuildConfig.Version
$BuildNumber = $BuildConfig.BuildNumber
$BuildConfiguration = $BuildConfig.BuildConfiuration

#Increase BuildNumber
$BuildNumber = $BuildNumber + 1

# Pepare build vairables
$ProjectFilePath = Join-Path $ProjectPath $ProjectFile
$VersionNumber = $Version + "." + $BuildNumber


$VersionOutputPathWin = $OutputPath + "_win-x64_" + $VersionNumber
$VersionOutputPathLinux = $OutputPath + "_linux-x64_" + $VersionNumber

# Build
dotnet.exe publish $ProjectFilePath -c $BuildConfiguration -r win-x64 -p:PublishSingleFile=false -p:Version=$VersionNumber -o $VersionOutputPathWin
dotnet.exe publish $ProjectFilePath -c $BuildConfiguration -r linux-x64 -p:PublishSingleFile=false -p:Version=$VersionNumber -o $VersionOutputPathLinux


# Write new build number to configFile
$BuildConfig.BuildNumber = $BuildNumber
$BuildConfig | ConvertTo-Json -Depth 100 | Out-File $BuildSettingsFile