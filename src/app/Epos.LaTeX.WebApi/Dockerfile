FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

# Copy sources
WORKDIR /build
COPY . .

# Build
RUN dotnet publish --configuration Release --output dist

# ---------------------------------------                          

FROM mcr.microsoft.com/dotnet/aspnet:6.0-jammy AS run-env

ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get update
RUN apt-get -y install texlive
RUN apt-get -y install texlive-lang-german
RUN apt-get -y install texlive-latex-extra
RUN apt-get -y install texlive-fonts-extra
RUN apt-get -y install imagemagick

WORKDIR /svc
COPY --from=build-env /build/dist ./

# Enable PDF conversion
COPY ./policy.xml /etc/ImageMagick-6

ENTRYPOINT ["dotnet", "Epos.LaTeX.WebApi.dll"]
