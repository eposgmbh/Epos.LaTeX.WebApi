FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env

# Copy sources
WORKDIR /build
COPY . .

# Build
RUN dotnet publish --configuration Release --output dist

# ---------------------------------------                          

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0

RUN \
    apt-get update && \
    apt-get -y install texlive=2018.20190227-2 && \
    apt-get -y install texlive-lang-german=2018.20190227-2 && \
    apt-get -y install texlive-latex-extra=2018.20190227-2 && \
    apt-get -y install imagemagick=8:6.9.10.23+dfsg-2.1

WORKDIR /svc
COPY --from=build-env /build/dist ./

EXPOSE 80
ENV PORT=80

ENTRYPOINT ["dotnet", "Epos.LaTeX.WebApi.dll"]
