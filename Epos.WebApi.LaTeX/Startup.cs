using Epos.WebApi.LaTeX.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Epos.WebApi.LaTeX
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) {
            services.AddSingleton<ILaTeXService, LaTeXService>();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app) {
            app.UseMvc();
        }
    }
}
