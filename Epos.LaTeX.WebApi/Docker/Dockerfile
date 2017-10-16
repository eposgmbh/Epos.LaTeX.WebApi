FROM microsoft/aspnetcore:2.0
RUN \
    apt-get update && \
    apt-get -y install texlive && \
    apt-get -y install texlive-lang-german && \
    apt-get -y install texlive-latex-extra && \
    apt-get -y install imagemagick
WORKDIR /app
COPY . .
WORKDIR Publish
EXPOSE 81
ENTRYPOINT ["dotnet", "Epos.Blog.LaTeX.dll"]
