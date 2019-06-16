Push-Location -Path ..
dotnet pack --configuration Release --output ./bin/nupkg -p:PackAsTool=true -p:ToolCommandName=epos-latex
dotnet tool install --global --add-source ./bin/nupkg Epos.LaTeX.CmdLine
Remove-Item -Confirm:$False -Recurse ./bin/nupkg
Pop-Location
