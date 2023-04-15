using System.Net.Http.Headers;
using System.Text.Json;
using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Serilog;
using SteamWebAPI2.Utilities;
using TNRD.Zeepkist.GTR.Backend.Authentication;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Options;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Backend.Google;
using TNRD.Zeepkist.GTR.Backend.Steam;

namespace TNRD.Zeepkist.GTR.Backend;

internal class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.AddServerHeader = false;
            options.AllowSynchronousIO = false;
        });

        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration
                .WriteTo.Console()
                .MinimumLevel.Debug();
        });

        AddServices(builder);

        WebApplication app = builder.Build();

        ConfigureApp(app);

        await app.RunAsync();
    }

    private static void AddServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));
        builder.Services.Configure<DirectusOptions>(builder.Configuration.GetSection("Directus"));
        builder.Services.Configure<SteamOptions>(builder.Configuration.GetSection("Steam"));
        builder.Services.Configure<GoogleOptions>(builder.Configuration.GetSection("Google"));

        builder.Host.UseConsoleLifetime(options => options.SuppressStatusMessages = true);

        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
        builder.Services.AddFastEndpoints();
        builder.Services.AddJWTBearerAuth(GetJwtToken(builder));
        builder.Services.AddCors();
        builder.Services.AddSwaggerDoc(b => { b.Title = "Zeepkist GTR"; }, addJWTBearerAuth: false);

        builder.Services.AddHttpClient("directus",
            (provider, client) =>
            {
                DirectusOptions options = provider.GetRequiredService<IOptions<DirectusOptions>>().Value;

                string baseUrl = $"http://{options.BaseUrl}:{options.Port}";

                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", options.Token);
            });

        builder.Services.AddSingleton<IDirectusClient, DirectusClient>();
        builder.Services.AddSingleton<IGoogleUploadService, CloudStorageUploadService>();

        builder.Services.AddSingleton<SteamWebInterfaceFactory>(provider =>
        {
            SteamOptions steamOptions = provider.GetRequiredService<IOptions<SteamOptions>>().Value;
            return new SteamWebInterfaceFactory(steamOptions.Token);
        });
    }

    private static string GetJwtToken(WebApplicationBuilder builder)
    {
        IConfigurationSection section = builder.Configuration.GetSection("Auth");
        if (section == null)
            throw new NullReferenceException("Auth section is null");

        AuthOptions authOptions = section.Get<AuthOptions>();
        if (authOptions == null)
            throw new NullReferenceException("AuthOptions is null");

        if (string.IsNullOrEmpty(authOptions.SigningKey))
            throw new NullReferenceException("SigningKey is null or empty");

        return authOptions.SigningKey;
    }

    private static void ConfigureApp(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDefaultExceptionHandler();
        }

        app.UseForwardedHeaders(new ForwardedHeadersOptions() { ForwardedHeaders = ForwardedHeaders.All });

        app.UseCors(policyBuilder => policyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseFastEndpoints(options =>
        {
            options.Errors.ResponseBuilder = (errors, _, _) => errors.ToResponse();
            options.Errors.StatusCode = StatusCodes.Status422UnprocessableEntity;
            options.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        app.UseOpenApi();
        app.UseSwaggerUi3(x =>
        {
            x.ConfigureDefaults();
            x.Path = string.Empty;
            x.DocumentTitle = "Zeepkist GTR";
        });
    }
}