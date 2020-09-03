using System;
using System.IO;
using Epos.LaTeX.WebApi.Services;
using LettuceEncrypt;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Epos.LaTeX.WebApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) {
            if (Environment.GetEnvironmentVariable("LE") != null) {
                services
                    .AddLettuceEncrypt()
                    .PersistDataToDirectory(new DirectoryInfo("/lettuce-encrypt"), pfxPassword: null);
            }

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
