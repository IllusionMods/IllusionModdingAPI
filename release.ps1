$array = @("KKAPI", "ECAPI", "AIAPI", "PHAPI", "HS2API")

if ($PSScriptRoot -match '.+?\\bin\\?') {
    $dir = $PSScriptRoot + "\"
}
else {
    $dir = $PSScriptRoot + "\bin\"
}

$out = $dir + "BepInEx\plugins\" 
New-Item -ItemType Directory -Force -Path $out

New-Item -ItemType Directory -Force -Path ($dir + "out\")

function CreateZip ($element)
{
    Remove-Item -Force -Path ($out + "*")
    New-Item -ItemType Directory -Force -Path $out

    Copy-Item -Path ($dir + $element + ".dll") -Destination $out
    Copy-Item -Path ($dir + $element + ".xml") -Destination $out

    $ver = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dir + $element + ".dll").FileVersion.ToString()

    Compress-Archive -Path ($dir + "BepInEx") -Force -CompressionLevel "Optimal" -DestinationPath ($dir + "out\" + $element + "_" + $ver + ".zip")
}

foreach ($element in $array) 
{
    try
    {
        CreateZip ($element)
    }
    catch 
    {
        # retry
        CreateZip ($element)
    }
}

Remove-Item -Force -Path ($out + "*")
Remove-Item -Force -Path ($dir + "BepInEx") -Recurse