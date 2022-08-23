
Remove-Item ".\bin\Release\net6.0\publish\*" -Recurse
dotnet publish .\EROptimizer.csproj --configuration Release -p:PublishProfile=FolderProfile

cd .\ClientApp

ng build --configuration production

cd ..

New-Item -Path ".\bin\Release\net6.0\publish\ClientApp" -Name "dist" -ItemType "directory"

Copy-Item -Path ".\ClientApp\dist\my-app\*" -Destination ".\bin\Release\net6.0\publish\ClientApp\dist" -Recurse

Read-Host -Prompt "Press Enter to exit"