using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ci.Extension.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using SimpleWeb.Models;
using TwentyTwenty.Storage;
using TwentyTwenty.Storage.Azure;
using TwentyTwenty.Storage.Local;

namespace SimpleWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
        }

        public IWebHostEnvironment WebHostEnvironment { get; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
            services.AddHealthChecks();

            var storageType = Configuration.GetValue<StorageType>("Storage:Type");
            switch (storageType)
            {
                case StorageType.Azure:
                    var azureConnStr = Configuration.GetValue<string>("Storage:Azure:ConnectionString");
                    if (azureConnStr.IsNullOrWhiteSpace())
                        throw new ArgumentNullException(nameof(azureConnStr));
                    services.AddSingleton<IStorageProvider, AzureStorageProvider>(provider =>
                        new AzureStorageProvider(new AzureProviderOptions() { ConnectionString = azureConnStr }));
                    break;
                default:
                    services.AddSingleton<IStorageProvider, LocalStorageProvider>(provider =>
                        new LocalStorageProvider(Path.Combine(WebHostEnvironment.WebRootPath, "")));
                    break;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    ResponseWriter = WriteResponse
                });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        /// <summary>
        /// Health endpoint to json
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <remarks>https://docs.microsoft.com/zh-tw/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-5.0#customize-output</remarks>
        private static Task WriteResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var options = new JsonWriterOptions
            {
                Indented = true
            };

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, options))
            {
                writer.WriteStartObject();
                writer.WriteString("status", result.Status.ToString());
                writer.WriteStartObject("results");
                foreach (var entry in result.Entries)
                {
                    writer.WriteStartObject(entry.Key);
                    writer.WriteString("status", entry.Value.Status.ToString());
                    writer.WriteString("description", entry.Value.Description);
                    writer.WriteStartObject("data");
                    foreach (var item in entry.Value.Data)
                    {
                        writer.WritePropertyName(item.Key);
                        JsonSerializer.Serialize(
                            writer, item.Value, item.Value?.GetType() ??
                                                typeof(object));
                    }
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            var json = Encoding.UTF8.GetString(stream.ToArray());

            return context.Response.WriteAsync(json);
        }
    }
}
