cd $PSScriptRoot
Remove-Item -Recurse -Force Publish

cd ..
dotnet publish --configuration Release --output Docker/Publish

cd Docker

docker rmi -f eposgmbh/blog-latex
docker rmi -f eposgmbh/blog-latex:1.0
docker build . -t eposgmbh/blog-latex:1.0 -t eposgmbh/blog-latex
