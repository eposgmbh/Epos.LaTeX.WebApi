# Epos.LaTeX.WebApi

[![Dockerhub](https://img.shields.io/docker/pulls/eposgmbh/latex-service.svg)](https://hub.docker.com/r/eposgmbh/latex-service/)

## Description

`Epos.LaTeX.WebApi` is an ASP.NET Core Web API to convert LaTeX to a PNG Image.

## Installation

Build the Docker image (Linux) and run it:

```bash
cd ./docker
docker-compose -f docker-compose.yaml build
docker run --detach --rm --publish 80:5000 --name latex-service eposgmbh/latex-service:latest
```

> **Hint:** The Web API can only be made to work on Linux, because it needs TeX Live and ImageMagick installed. The
> simplest way is to run the Linux Docker image, see above.

## Usage

Convert raw JSON to BASE64 (see <https://www.base64encode.org/>) and send a GET request to `http://localhost/api/latex/{BASE64}`.

```javascript
{
    "laTeX": "a^2 + b^2 = c^2",
    "textColor": "777777", // HTML "#"-Format
    "pageColor": "111111"  // or "transparent" for a transparent background 
}
```

You get back a PNG image.

## Contributing

You can enter an issue on Github [here](https://github.com/eposgmbh/Epos.LaTeX.WebApi/issues). I will work on
your issue when I have time. Pull Requests are possible too.

## License

MIT License

Copyright (c) 2021 eposgmbh

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
