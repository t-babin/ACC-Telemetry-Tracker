using AccTelemetryTracker.Api.Middleware;
using AccTelemetryTracker.Datastore;
using AccTelemetryTracker.Logic;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up application");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => 
        lc.WriteTo.Console()
        .ReadFrom.Configuration(ctx.Configuration));

    // Add services to the container.
    builder.Services.AddControllers();

    var frontendUrl = builder.Configuration.GetValue<string>("FRONTEND_URL");
    var databaseHost = builder.Configuration.GetValue<string>("DATABASE_HOST");
    var databaseName = builder.Configuration.GetValue<string>("DATABASE_NAME");
    var databaseUser = builder.Configuration.GetValue<string>("DATABASE_USER");
    var databasePassword = builder.Configuration.GetValue<string>("DATABASE_PASSWORD");
    var sqliteDatabase = builder.Configuration.GetValue<string>("SQLITE_DATABASE");

    if (new [] { databaseHost, databaseName, databaseUser, databasePassword }.Any(s => string.IsNullOrEmpty(s)))
    {
        Log.Information("Connecting to a local sqlite database");
        builder.Services.AddDbContext<AccTelemetryTrackerContext>(x =>
        {
            x.UseSqlite($"Data Source={sqliteDatabase}");
        });
    }
    else
    {
        try
        {
            Log.Information("Connecting to a dockerized database");
            var connectionString = $"server={databaseHost};database={databaseName};user={databaseUser};password={databasePassword};";
            // var version = ServerVersion.AutoDetect(connectionString);
            builder.Services.AddDbContext<AccTelemetryTrackerContext>(x => x.UseMySql(connectionString, new MySqlServerVersion(new Version(5, 7, 34)), options =>
                options.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null))
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .LogTo(
                    filter: (eventId, level) => eventId.Id == CoreEventId.ExecutionStrategyRetrying,
                    logger: (eventData) =>
                    {
                        var retryEventData = eventData as ExecutionStrategyEventData;
                        var exceptions = retryEventData!.ExceptionsEncountered;
                        Console.WriteLine($"Retry #{exceptions.Count} with delay {retryEventData.Delay} due to error: {exceptions.Last().Message}");
                    }));
        }
        catch (Exception ex)
        {
            Log.Error(ex, ex.Message);
        }
    }

    var _clientId = builder.Configuration.GetValue<string>("DISCORD_CLIENT_ID");
    var _clientSecret = builder.Configuration.GetValue<string>("DISCORD_CLIENT_SECRET");

    if (string.IsNullOrEmpty(_clientId))
    {
        Log.Error("The discord client ID is not provided in the configuration");
        throw new KeyNotFoundException("The discord client ID is not provided in the configuration as the environment variable DISCORD_CLIENT_ID");
    }

    if (string.IsNullOrEmpty(_clientSecret))
    {
        Log.Error("The discord client secret is not provided in the configuration");
        throw new KeyNotFoundException("The discord client secret is not provided in the configuration as the environment variable DISCORD_CLIENT_SECRET");
    }

    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    builder.Services.AddSingleton<IMotecParser, MotecParser>();
    builder.Services.AddSingleton<IParserLogic, ParserLogic>();
    builder.Services.AddScoped<IAuditRepository, AuditRepository>();

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
            builder =>
            {
                builder
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
    });

    var app = builder.Build();
    

    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseRouting();
    app.UseCors();
    app.UseMiddleware<CookieAuthenticationMiddleware>();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
