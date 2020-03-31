using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CoronaVirusApi.BackgroundServices;
using CoronaVirusApi.BackgroundServices.Config;
using CoronaVirusApi.Config;
using CoronaVirusApi.HttpServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CoronaVirusApi
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
      services.AddApplicationInsightsTelemetry();

      services.AddControllers()
                .AddJsonOptions(options =>
                {
                  options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });

      services.AddResponseCaching();

      services.AddSingleton<DataStorage>();

      services.Configure<ServiceConfig>(Configuration.GetSection("ServiceConfig"));
      services.Configure<AzureStorageConfig>(Configuration.GetSection("AzureStorage"));

      services.AddHttpClient<OpenDataHttpService>();

      services.AddHostedService<UpdateDataBackgroundService>();



      JsonConvert.DefaultSettings = () => new JsonSerializerSettings
      {
        Formatting = Formatting.Indented,
        ContractResolver = new DefaultContractResolver(),
        DateFormatString = "o",
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
      };
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseHsts();

        app.UseResponseCaching();
      }

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        endpoints.MapGet("/", async context =>
        {
          context.Response.ContentType = "text/html";
          await WriteToBodyStart(context);
          await context.Response.WriteAsync($@"
<h1 style='margin-bottom: -15px;'>Corona Virus API</h1>
<sub>another project by <a href='https://lazy-developer.xyz/' target='_blank'>the lazy developer</a></sub>
<p>
  This project is hosted on GitHub at
  <a href='https://github.com/Gordon-Beeming' target='_blank'>GordonBeeming</a>
  /
  <a href='https://github.com/Gordon-Beeming/CoronaVirusApi' target='_blank'>CoronaVirusApi</a>
<br/>
    Latest Publish: v{typeof(Program).Assembly.GetName().Version}
</p>
");
          foreach (var dataSource in endpoints.DataSources)
          {
            foreach (var endpoint in dataSource.Endpoints)
            {
              if (endpoint is RouteEndpoint routeEndpoint)
              {
                var method = "UNKNOWN";
                HttpMethodMetadata? httpMethod = (HttpMethodMetadata)endpoint.Metadata.FirstOrDefault(o => o is HttpMethodMetadata);
                if (httpMethod != null && httpMethod.HttpMethods.Count > 0)
                {
                  method = httpMethod.HttpMethods[0];
                }
                if (routeEndpoint.RoutePattern.RawText == "/")
                {
                  continue;
                }
                var id = Guid.NewGuid().ToString("N");
                await context.Response.WriteAsync($@"
<h4>{method} <a href='/{routeEndpoint.RoutePattern.RawText.ToLowerInvariant()}'>/{routeEndpoint.RoutePattern.RawText.ToLowerInvariant()}</a><br/>
    <button id='btnShow{id}' onclick='document.getElementById(""pre{id}"").style = ""display: block;"";document.getElementById(""btnShow{id}"").style = ""display: none;"";document.getElementById(""btnHide{id}"").style = ""display: block;"";return false;'>show the deets</button>
    <button id='btnHide{id}' style='display:none;' onclick='document.getElementById(""pre{id}"").style = ""display: none;"";document.getElementById(""btnHide{id}"").style = ""display: none;"";document.getElementById(""btnShow{id}"").style = ""display: block;"";return false;'>hide the deets</button>
</h4>
<pre id='pre{id}' style='display:none;'>{JsonConvert.SerializeObject(routeEndpoint.RoutePattern, Formatting.Indented)}</pre>
");
              }
            }
          }
          await WriteFromBodyFinish(context);
        });
      });

      //AddStaticFilesLastToThePipeline(app, env);

      var zaCulture = (CultureInfo)CultureInfo.GetCultureInfo("en-za").Clone();
      var zaCultureNumberFormat = (NumberFormatInfo)zaCulture.NumberFormat.Clone();
      zaCultureNumberFormat.CurrencySymbol = "R";
      zaCultureNumberFormat.CurrencyDecimalSeparator = ".";
      zaCultureNumberFormat.NumberDecimalSeparator = ".";
      zaCulture.NumberFormat = zaCultureNumberFormat;
      CultureInfo.DefaultThreadCurrentCulture = zaCulture;
      CultureInfo.DefaultThreadCurrentUICulture = zaCulture;
    }

    private static async Task WriteToBodyStart(HttpContext context) =>
      await context.Response.WriteAsync(@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8' />
  <title>Corona Virus API</title>
  <style type='text/css'>
    body{
      font-family:'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    }
  </style>
</head>
<body>
");

    private async Task WriteFromBodyFinish(HttpContext context) =>
      await context.Response.WriteAsync(@"
</body>
</html>
");

    private static void AddStaticFilesLastToThePipeline(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseDefaultFiles();
      if (env.IsDevelopment())
      {
        app.UseStaticFiles();
      }
      else
      {
        app.UseStaticFiles(new StaticFileOptions
        {
          OnPrepareResponse = ctx =>
          {
            const int durationInSeconds = 60 * 60 * 6;
            ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] =
                          "public,max-age=" + durationInSeconds;
          }
        });
      }
    }
  }
}
