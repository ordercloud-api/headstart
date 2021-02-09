using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ordercloud.integrations.library
{
    public static class OrderCloudIntegrationsConfigureWebExtensions
    {
        public static IApplicationBuilder OrderCloudIntegrationsConfigureWebApp(this IApplicationBuilder app, IWebHostEnvironment env, string corsPolicyName)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMiddleware<GlobalExceptionHandler>();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(corsPolicyName ?? Environment.GetEnvironmentVariable("CORS_POLICY"));
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            return app;
        }
    }
}