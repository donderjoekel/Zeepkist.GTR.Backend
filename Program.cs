﻿using System.Runtime.InteropServices;
using System.Text;
using ByteSizeLib;
using Docker.DotNet;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Refit;
using Serilog;
using Serilog.Enrichers.ShortTypeName;
using Serilog.Events;
using Serilog.Filters;
using SteamWebAPI2.Utilities;
using TNRD.Zeepkist.GTR.Backend;
using TNRD.Zeepkist.GTR.Backend.Authentication;
using TNRD.Zeepkist.GTR.Backend.DataStore;
using TNRD.Zeepkist.GTR.Backend.Favorites;
using TNRD.Zeepkist.GTR.Backend.Hashing;
using TNRD.Zeepkist.GTR.Backend.Jobs;
using TNRD.Zeepkist.GTR.Backend.Jwt;
using TNRD.Zeepkist.GTR.Backend.Levels;
using TNRD.Zeepkist.GTR.Backend.Levels.Items;
using TNRD.Zeepkist.GTR.Backend.Levels.Metadata;
using TNRD.Zeepkist.GTR.Backend.Levels.Points;
using TNRD.Zeepkist.GTR.Backend.Levels.Requests;
using TNRD.Zeepkist.GTR.Backend.Logging;
using TNRD.Zeepkist.GTR.Backend.Media;
using TNRD.Zeepkist.GTR.Backend.Media.Jobs;
using TNRD.Zeepkist.GTR.Backend.Middleware;
using TNRD.Zeepkist.GTR.Backend.PersonalBests;
using TNRD.Zeepkist.GTR.Backend.Records;
using TNRD.Zeepkist.GTR.Backend.RemoteStorage;
using TNRD.Zeepkist.GTR.Backend.Steam;
using TNRD.Zeepkist.GTR.Backend.Upvotes;
using TNRD.Zeepkist.GTR.Backend.Users;
using TNRD.Zeepkist.GTR.Backend.Users.Points;
using TNRD.Zeepkist.GTR.Backend.Versioning;
using TNRD.Zeepkist.GTR.Backend.Workshop;
using TNRD.Zeepkist.GTR.Backend.WorldRecords;
using TNRD.Zeepkist.GTR.Backend.Zeeplevel;
using TNRD.Zeepkist.GTR.Database.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, provider, configuration) =>
    {
        LoggerOptions options = provider.GetRequiredService<IOptions<LoggerOptions>>().Value;

        configuration
            .Enrich.FromLogContext()
            .WriteTo.OpenObserve(
                options.Url,
                options.Organization,
                options.Login,
                options.Token)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Information)
            .Filter.ByExcluding(Matching.FromSource("Microsoft.EntityFrameworkCore"))
            .MinimumLevel.Information();
    });

builder.Logging.AddFilter("System.Net.Http.HttpClient.*.LogicalHandler", LogLevel.Warning);
builder.Logging.AddFilter("System.Net.Http.HttpClient.*.ClientHandler", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.*", LogLevel.Warning);

// Add services to the container.

builder.Services.AddAuthentication(
        x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
    .AddJwtBearer(
        x =>
        {
            x.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidIssuer = "https://api.zeepkist-gtr.com",
                ValidAudience = "https://api.zeepkist-gtr.com",
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration.GetSection(JwtOptions.Key)[nameof(JwtOptions.Token)])),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };
        });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHangfire(
    configuration =>
    {
        configuration
            // .UsePostgreSqlStorage(
            //     conf => { conf.UseNpgsqlConnection(builder.Configuration["Database:ConnectionString"]); },
            //     new PostgreSqlStorageOptions()
            //     {
            //         InvisibilityTimeout = TimeSpan.FromDays(1),
            //     })
            .UseMemoryStorage(
                new MemoryStorageOptions() // TODO: Change to external storage
                {
                    FetchNextJobTimeout = TimeSpan.FromDays(1)
                })
            .UseSerilogLogProvider();
    });
builder.Services.AddHangfireServer();

builder.Services.AddNpgsql<GtarrContext>(
    builder.Configuration["Database:ConnectionString"],
    options => { options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery); });

builder.Services.AddHostedService<Bootstrapper>();

builder.Services.AddSingleton<IJobScheduler, JobScheduler>();
builder.Services.AddSingleton<IDockerClient, DockerClient>(
    provider => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient()
        : new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient());
builder.Services.AddSingleton<IHashService, HashService>();
builder.Services.AddSingleton<IWorkshopService, WorkshopService>();
builder.Services.AddSingleton<IZeeplevelService, ZeeplevelService>();

builder.Services.AddScoped<IDatabase, Database>();

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();

builder.Services.AddScoped<IFavoritesRepository, FavoritesRepository>();
builder.Services.AddScoped<IFavoritesService, FavoritesService>();

builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddScoped<ILevelItemsRepository, LevelItemsRepository>();
builder.Services.AddScoped<ILevelItemsService, LevelItemsService>();

builder.Services.AddScoped<ILevelMetadataRepository, LevelMetadataRepository>();
builder.Services.AddScoped<ILevelMetadataService, LevelMetadataService>();

builder.Services.AddScoped<ILevelPointsRepository, LevelPointsRepository>();
builder.Services.AddScoped<ILevelPointsService, LevelPointsService>();

builder.Services.AddScoped<ILevelRequestsRepository, LevelRequestsRepository>();
builder.Services.AddScoped<ILevelRequestsService, LevelRequestsService>();

builder.Services.AddScoped<ILevelRepository, LevelRepository>();
builder.Services.AddScoped<ILevelService, LevelService>();

builder.Services.AddScoped<IMediaRepository, MediaRepository>();
builder.Services.AddScoped<IMediaService, MediaService>();

builder.Services.AddScoped<IPersonalBestsService, PersonalBestsService>();
builder.Services.AddScoped<IPersonalBestsDailyRepository, PersonalBestsDailyRepository>();
builder.Services.AddScoped<IPersonalBestsGlobalRepository, PersonalBestsGlobalRepository>();
builder.Services.AddScoped<IPersonalBestsMonthlyRepository, PersonalBestsMonthlyRepository>();
builder.Services.AddScoped<IPersonalBestsQuarterlyRepository, PersonalBestsQuarterlyRepository>();
builder.Services.AddScoped<IPersonalBestsWeeklyRepository, PersonalBestsWeeklyRepository>();
builder.Services.AddScoped<IPersonalBestsYearlyRepository, PersonalBestsYearlyRepository>();

builder.Services.AddScoped<IRecordsRepository, RecordsRepository>();
builder.Services.AddScoped<IRecordsService, RecordsService>();

// builder.Services.AddScoped<IRemoteStorageService, GoogleCloudStorageService>();
builder.Services.AddScoped<IRemoteStorageService, WasabiStorageService>();

builder.Services.AddScoped<ISteamService, SteamService>();
builder.Services.AddScoped<ISteamWebInterfaceFactory, SteamWebInterfaceFactory>(
    provider =>
    {
        SteamOptions options = provider.GetRequiredService<IOptions<SteamOptions>>().Value;
        return new SteamWebInterfaceFactory(options.ApiKey);
    });

builder.Services.AddScoped<IUpvotesRepository, UpvotesRepository>();
builder.Services.AddScoped<IUpvotesService, UpvotesService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IUserPointsRepository, UserPointsRepository>();
builder.Services.AddScoped<IUserPointsService, UserPointsService>();

builder.Services.AddScoped<IVersionService, VersionService>();
builder.Services.AddScoped<IVersionRepository, VersionRepository>();

builder.Services.AddScoped<IWorldRecordsService, WorldRecordsService>();
builder.Services.AddScoped<IWorldRecordsDailyRepository, WorldRecordsDailyRepository>();
builder.Services.AddScoped<IWorldRecordsGlobalRepository, WorldRecordsGlobalRepository>();
builder.Services.AddScoped<IWorldRecordsMonthlyRepository, WorldRecordsMonthlyRepository>();
builder.Services.AddScoped<IWorldRecordsQuarterlyRepository, WorldRecordsQuarterlyRepository>();
builder.Services.AddScoped<IWorldRecordsWeeklyRepository, WorldRecordsWeeklyRepository>();
builder.Services.AddScoped<IWorldRecordsYearlyRepository, WorldRecordsYearlyRepository>();

builder.Services.AddTransient<UploadMediaJob>();

builder.Services.Configure<AuthenticationOptions>(builder.Configuration.GetSection(AuthenticationOptions.Key));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.Key));
builder.Services.Configure<GoogleCloudStorageOptions>(builder.Configuration.GetSection(GoogleCloudStorageOptions.Key));
builder.Services.Configure<SteamOptions>(builder.Configuration.GetSection(SteamOptions.Key));
builder.Services.Configure<WasabiStorageOptions>(builder.Configuration.GetSection(WasabiStorageOptions.Key));
builder.Services.Configure<WorkshopOptions>(builder.Configuration.GetSection(WorkshopOptions.Key));
builder.Services.Configure<LoggerOptions>(builder.Configuration.GetSection(LoggerOptions.Key));

builder.Services.AddRefitClient<IPublishedFileServiceApi>(
        provider => new RefitSettings()
        {
            ContentSerializer = new NewtonsoftJsonContentSerializer()
        })
    .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://api.steampowered.com/IPublishedFileService/"));

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<LogBadRequestMiddleware>();

app.UseForwardedHeaders(
    new ForwardedHeadersOptions()
    {
        ForwardedHeaders = ForwardedHeaders.All
    });

app.UseCors(
    cors => cors
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

app.UseHttpsRedirection();

app.UseHangfireDashboard();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
