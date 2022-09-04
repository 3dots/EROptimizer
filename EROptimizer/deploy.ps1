
$iisWebsiteName = "Test"
$iisRoot = "C:\inetpub\wwwroot\"
$websitePath = $iisRoot + $iisWebsiteName

$remoteCredential = Get-Credential

try 
{
    $localPublishDirWildcard = ".\bin\Release\net6.0\publish\*"
    Remove-Item  $localPublishDirWildcard -Recurse
    dotnet publish .\EROptimizer.csproj --configuration Release -p:PublishProfile=FolderProfile
    
    cd .\ClientApp
    ng build --configuration production
    
    cd ..
    New-Item -Path ".\bin\Release\net6.0\publish\ClientApp" -Name "dist" -ItemType "directory"
    Copy-Item -Path ".\ClientApp\dist\my-app\*" -Destination ".\bin\Release\net6.0\publish\ClientApp\dist" -Recurse
    
    $remoteSession = New-PSSession -ComputerName 3dot-IIS -UseSSL -Credential $remoteCredential
    $dontReplace = @("web.config", "appsettings.json", "ScrapeWiki.dll.config")
    Invoke-Command -Session $remoteSession {
        Stop-IISSite $using:iisWebsiteName -Confirm: $false
        Stop-WebAppPool $using:iisWebsiteName
        Start-Sleep -s 2
        if (-not ((Get-WebAppPoolState $using:iisWebsiteName).Value -eq "Stopped")) {
            throw ("AppPool " + $iisWebsiteName + " failed to stop.")
        }
    
        Remove-Item $using:websitePath -Recurse -Exclude $using:dontReplace
    }
    Copy-Item $localPublishDirWildcard -ToSession $remoteSession -Destination $websitePath -Recurse -Exclude $dontReplace
    
    Invoke-Command -Session $remoteSession { 
        Start-WebAppPool $using:iisWebsiteName
        Start-IISSite $using:iisWebsiteName
    }
    
    Remove-PSSession -Session $remoteSession
    
    Read-Host -Prompt "Press Enter to exit"
}
catch 
{
    Write-Error $_.Exception.ToString()
    Read-Host -Prompt "The above error occurred. Press Enter to exit."
}

