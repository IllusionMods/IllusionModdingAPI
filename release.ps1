# Env setup ---------------
if ($PSScriptRoot -match '.+?\\bin\\?') {
    $dir = $PSScriptRoot + "\"
}
else {
    $dir = $PSScriptRoot + "\bin\"
}

$copy = $dir + "\copy\BepInEx\plugins" 

if ((Get-ChildItem -Path $dir -Filter *.dll).Length -gt 0)
{
    
    $pluginDir = $dir
}
else
{
    $pluginDir = $dir + "\BepInEx\plugins" 
}
Write-Information -MessageData ("Using " + $pluginDir + " as plugin directory")

New-Item -ItemType Directory -Force -Path ($dir + "\out\")

# Create releases ---------
function CreateZip ($pluginFile)
{
    Remove-Item -Force -Path ($dir + "\copy") -Recurse -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Force -Path $copy

    # the actual dll
    Copy-Item -Path $pluginFile.FullName -Destination $copy -Recurse -Force
    # the docs xml if it exists
    Copy-Item -Path ($pluginFile.DirectoryName + "\" + $pluginFile.BaseName + ".xml") -Destination $copy -Recurse -Force -ErrorAction Ignore

    # the replace removes .0 from the end of version up until it hits a non-0 or there are only 2 version parts remaining (e.g. v1.0 v1.0.1)
    $ver = (Get-ChildItem -Path ($copy) -Filter "*.dll" -Recurse -Force)[0].VersionInfo.FileVersion.ToString() -replace "^([\d+\.]+?\d+)[\.0]*$", '${1}'

    Compress-Archive -Path ($copy + "\..\") -Force -CompressionLevel "Optimal" -DestinationPath ($dir + "\out\" + $pluginFile.BaseName + "_" + "v" + $ver + ".zip")
}

foreach ($pluginFile in Get-ChildItem -Path $pluginDir -Filter *.dll) 
{
    try
    {
        CreateZip ($pluginFile)
    }
    catch 
    {
        # retry
        CreateZip ($pluginFile)
    }
}


Remove-Item -Force -Path ($dir + "\copy") -Recurse
