cd $PSScriptRoot
Remove-Item -Recurse -Force Publish

cd ..
dotnet publish --configuration Release --output Docker/Publish

cd Docker

docker rmi -f eposgmbh/latex-webapi
docker build . -t eposgmbh/latex-webapi
