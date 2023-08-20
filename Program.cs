using System.Text.Json;
using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.AspNetCore;
using Serilog;
using Serilog.Events;
using SteamWebAPI2.Utilities;
using TNRD.Zeepkist.GTR.Backend.Authentication;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Backend.Google;
using TNRD.Zeepkist.GTR.Backend.Jobs;
using TNRD.Zeepkist.GTR.Backend.Rabbit;
using TNRD.Zeepkist.GTR.Backend.Redis;
using TNRD.Zeepkist.GTR.Backend.Steam;
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
                .MinimumLevel.Debug()
                .WriteTo.Seq(context.Configuration["Seq:Url"], apiKey: context.Configuration["Seq:Key"])
                .WriteTo.Console(LogEventLevel.Debug);
        });

        AddServices(builder);
        WebApplication app = builder.Build();
        ConfigureApp(app);

        await app.RunAsync();
    }

    private static void AddServices(WebApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache();

        builder.Services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();
            CreateAndAddJob<CalculateRankingJob>(q, "CalculateRanking", "0 0 0/12 ? * * *", true);
            CreateAndAddJob<CalculateWorldRecordsJob>(q, "CalculateWorldRecords", "0 0/15 * ? * * *", false);
            CreateAndAddJob<CalculateHotLevelsJob>(q, "CalculateHotLevels", "0 0/15 * ? * * *", true);
            CreateAndAddJob<CalculatePopularLevelsJob>(q, "CalculatePopularLevels", "0 0 * ? * * *", true);
        });

        builder.Services.AddQuartzServer(options =>
        {
            options.AwaitApplicationStarted = true;
            options.WaitForJobsToComplete = true;
        });

        builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));
        builder.Services.Configure<SteamOptions>(builder.Configuration.GetSection("Steam"));
        builder.Services.Configure<GoogleOptions>(builder.Configuration.GetSection("Google"));
        builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("Rabbit"));
        builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));

        builder.Services.AddNpgsql<GTRContext>(builder.Configuration["Database:ConnectionString"]);

        builder.Host.UseConsoleLifetime(options => options.SuppressStatusMessages = true);

        builder.Services.AddHttpClient();
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
        builder.Services.AddFastEndpoints();
        builder.Services.AddJWTBearerAuth(GetJwtToken(builder));
        builder.Services.AddCors();
        builder.Services.AddSwaggerDoc(b => { b.Title = "Zeepkist GTR"; }, addJWTBearerAuth: false);

        builder.Services.AddSingleton<IGoogleUploadService, CloudStorageUploadService>();
        builder.Services.AddSingleton<SteamWebInterfaceFactory>(provider =>
        {
            SteamOptions steamOptions = provider.GetRequiredService<IOptions<SteamOptions>>().Value;
            return new SteamWebInterfaceFactory(steamOptions.Token);
        });

        builder.Services.AddSingleton<IRabbitPublisher, RabbitPublisher>();
        builder.Services.AddHostedService<RabbitHostedService>();
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

    private static void CreateAndAddJob<TJob>(
        IServiceCollectionQuartzConfigurator q,
        string name,
        string cronSchedule,
        bool runAtStartup
    )
        where TJob : IJob
    {
        JobKey key = new JobKey($"{name}Job");
        q.AddJob<TJob>(opts => opts.WithIdentity(key));

        q.AddTrigger(opts =>
        {
            opts.ForJob(key)
                .WithIdentity($"{name}Job-Trigger")
                .WithCronSchedule(cronSchedule);
        });

        if (runAtStartup)
        {
            q.AddTrigger(opts =>
            {
                opts.ForJob(key)
                    .WithIdentity($"{name}Job-OnStartup")
                    .StartNow();
            });
        }
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
