$array = @("KKAPI", "ECAPI", "AIAPI")

if ($PSScriptRoot -match '.+?\\bin\\?') {
    $dir = $PSScriptRoot + "\"
}
else {
    $dir = $PSScriptRoot + "\bin\"
}

$out = $dir + "BepInEx\plugins\" 

$ver = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dir + $array[0] + ".dll").FileVersion.ToString()


New-Item -ItemType Directory -Force -Path ($dir + "out\")

foreach ($element in $array) {

Remove-Item -Force -Path ($out + "*") -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $out

Copy-Item -Path ($dir + $element + ".dll") -Destination $out
Copy-Item -Path ($dir + $element + ".xml") -Destination $out -ErrorAction SilentlyContinue

Compress-Archive -Path ($dir + "BepInEx") -Force -CompressionLevel "Optimal" -DestinationPath ($dir + "out\" + $element + "_" + $ver + ".zip")

}

Remove-Item -Force -Path ($out + "*")
Remove-Item -Force -Path ($dir + "BepInEx") -Recurse