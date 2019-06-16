using Epos.LaTeX.WebApi.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Epos.LaTeX.WebApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc();
            services.AddControllers();
            services.AddResponseCaching();

            services.AddSingleton<ILaTeXService, LaTeXService>();
        }

        public void Configure(IApplicationBuilder app) {
            app.UseRouting();
            app.UseResponseCaching();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
