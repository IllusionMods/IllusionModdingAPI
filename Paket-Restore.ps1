New-Item -Force -ItemType directory -Path ".paket" | Out-Null

if(!(Test-Path ".paket/paket.exe"))
{
    try
    {
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        $json = (Invoke-WebRequest -Uri "https://api.github.com/repos/fsprojects/Paket/releases" | ConvertFrom-Json)
        $asset = ($json[0].assets | Where-Object name -eq paket.bootstrapper.exe)[0];
        Invoke-WebRequest $asset.browser_download_url -OutFile ".paket/paket.exe"
        Write-Output "paket.exe downloaded"
    }
    catch
    {
        Write-Output "Download failed"
    }
}

.\.paket\paket.exe restore