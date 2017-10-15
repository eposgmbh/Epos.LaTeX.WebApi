using System;
using Epos.WebApi.LaTeX.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Epos.WebApi.LaTeX.Controllers
{
    [Route("api/[controller]")]
    public class LaTeXController : Controller
    {
        private readonly ILaTeXService myLaTeXService;
        private readonly ILogger<LaTeXController> myLogger;

        public LaTeXController(ILaTeXService laTeXService, ILogger<LaTeXController> logger) {
            myLaTeXService = laTeXService;
            myLogger = logger;
        }

        // api/latex
        [HttpPost]
        public LaTeXServiceResponse Get([FromBody] LaTeXServiceRequest request) {
            try {
                return myLaTeXService.GetPng(request);
            } catch (Exception theException) {
                myLogger.LogCritical(theException.ToString());
                return new LaTeXServiceResponse { IsSuccessful = false, ErrorMessage = theException.Message };
            }
        }
    }
}
