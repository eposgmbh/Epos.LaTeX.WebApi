# Epos.LaTeX.WebApi

![Build Status](https://eposgmbh.visualstudio.com/_apis/public/build/definitions/f8efcc28-0cef-4500-a9e4-7b6d4c7f3c3d/6/badge)

(Build and deployment to [Dockerhub](https://hub.docker.com/r/eposgmbh/latex-webapi/) is automated with
[Visual Studio Team Services](https://www.visualstudio.com/team-services).)

## Description

`Epos.LaTeX.WebApi` is an ASP.NET Core Web API to convert LaTeX to a PNG Image.

## Installation

Pull the latest Docker image and run it:

```docker
docker pull eposgmbh/latex-webapi:latest
docker run -d --rm -p 81:81 --name latex-webapi eposgmbh/latex-webapi
```

## Usage

POST raw JSON (application/json) body to `http://[host]:81/api/latex`.

```javascript
{
    laTeX: "a^2 + b^2 = c^2",
    textColor: "777777" // HTML "#"-Format
}
```

You get back:

```
{
    "isSuccessful": true,
    "errorMessage": null,
    "pngImageData": "iVBORw0KGgoAAAANSUhEUgAABZ..."
}
```

`pngImageData` is a Base64-encoded PMG-Image. [Json.NET](https://www.newtonsoft.com/json) deserializes this into a `byte[]` array.

## Contributing

You can enter an issue on Github [here](https://github.com/eposgmbh/Epos.LaTeX.WebApi/issues). I will work on
your issue when I have time. Pull Requests are possible too.

## License

MIT License

Copyright (c) 2017 eposgmbh

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
