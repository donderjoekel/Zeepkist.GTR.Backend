using System.Text.Json;
using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;
using TNRD.Zeepkist.GTR.Backend.Authentication;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Backend.PreProcessors;
using TNRD.Zeepkist.GTR.Backend.Rabbit;
using TNRD.Zeepkist.GTR.Database;

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
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Source", "Backend")
                .MinimumLevel.Debug()
                .WriteTo.Seq(context.Configuration["Seq:Url"], apiKey: context.Configuration["Seq:Key"])
                .WriteTo.Console(LogEventLevel.Debug);
        });

        AddServices(builder);
        WebApplication app = builder.Build();
        ConfigureApp(app);

        using (IServiceScope scope = app.Services.CreateScope())
        {
            GTRContext db = scope.ServiceProvider.GetRequiredService<GTRContext>();
            await db.Database.MigrateAsync();
        }

        await app.RunAsync();
    }

    private static void AddServices(WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();
        builder.Services.AddMemoryCache();

        builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));
        builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("Rabbit"));

        builder.Services.AddNpgsql<GTRContext>(builder.Configuration["Database:ConnectionString"],
            options => { options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery); });

        builder.Host.UseConsoleLifetime(options => options.SuppressStatusMessages = true);

        builder.Services.AddHttpClient();
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
        builder.Services.AddFastEndpoints();
        builder.Services.AddJWTBearerAuth(GetJwtToken(builder));
        builder.Services.AddCors();
        builder.Services.AddSwaggerDoc(b => { b.Title = "Zeepkist GTR"; }, addJWTBearerAuth: false);

        builder.Services.AddSingleton<IRabbitPublisher, RabbitPublisher>();
        builder.Services.AddHostedService<RabbitHostedService>();
    }

    private static void ConfigureApp(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDefaultExceptionHandler();
        }

        app.UseHealthChecks("/healthcheck");

        app.UseForwardedHeaders(new ForwardedHeadersOptions() { ForwardedHeaders = ForwardedHeaders.All });

        app.UseCors(policyBuilder => policyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseFastEndpoints(options =>
        {
            options.Endpoints.Configurator = ep =>
            {
                ep.PreProcessors(Order.Before, new GenericGetRequestPreProcessor());
            };

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
}
