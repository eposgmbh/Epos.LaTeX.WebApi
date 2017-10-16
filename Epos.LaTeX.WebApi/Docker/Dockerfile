FROM eposgmbh/aspnetcore-texlive:2.0
WORKDIR /app
COPY . .
WORKDIR Publish
EXPOSE 81
ENTRYPOINT ["dotnet", "Epos.Blog.LaTeX.dll"]
