Set-Location $PSScriptRoot
Remove-Item -Recurse -Force Publish

Set-Location ..
dotnet publish --configuration Release --output Docker/Publish

Set-Location Docker

docker rmi --force eposgmbh/latex-webapi
docker build --tag eposgmbh/latex-webapi .
