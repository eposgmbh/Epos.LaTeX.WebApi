FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env

# Copy sources
WORKDIR /build
COPY . .

# Build
RUN dotnet publish --configuration Release --output dist

# ---------------------------------------                          

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS run-env

RUN apt-get update && \
    apt-get -y install texlive && \
    apt-get -y install texlive-lang-german && \
    apt-get -y install texlive-latex-extra && \
    apt-get -y install texlive-fonts-extra && \
    apt-get -y install imagemagick

WORKDIR /svc
COPY --from=build-env /build/dist ./

# Enable PDF conversion
COPY ./policy.xml /etc/ImageMagick-6

ENTRYPOINT ["dotnet", "Epos.LaTeX.WebApi.dll"]
