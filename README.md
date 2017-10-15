# Epos.WebApi.LaTeX
ASP.NET Core Web API to convert LaTeX to a PNG Image

POST raw JSON (application/json) body to http://[host]:81/api/latex

```javascript
{
    LaTeX: "a^2 + b^2 = c^2",
    TextColor: "777777" // HTML "#"-Format
}
```
