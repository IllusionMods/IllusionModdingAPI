$dir = $PSScriptRoot + "\bin\"
$out = $dir + "BepInEx\plugins\" 

$ver = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dir +"KKAPI.dll").FileVersion.ToString()


New-Item -ItemType Directory -Force -Path ($dir + "out\")

$array = @("KKAPI", "ECAPI", "AIAPI")

foreach ($element in $array) {

Remove-Item -Force -Path ($out + "*") -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $out

Copy-Item -Path ($dir + $element + ".dll") -Destination $out
Copy-Item -Path ($dir + $element + ".xml") -Destination $out

Compress-Archive -Path ($dir + "BepInEx") -Force -CompressionLevel "Optimal" -DestinationPath ($dir + "out\" + $element + "_" + $ver + ".zip")

}

Remove-Item -Force -Path ($out + "*")
Remove-Item -Force -Path ($dir + "BepInEx") -Recurse