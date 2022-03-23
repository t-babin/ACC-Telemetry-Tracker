using AccTelemetryTracker.Api.Middleware;
using AccTelemetryTracker.Datastore;
using AccTelemetryTracker.Datastore.Models;
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
    {
        lc.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning);
        lc.WriteTo.Console()
        .ReadFrom.Configuration(ctx.Configuration);
    });

    // Add services to the container.
    builder.Services.AddControllers();

    var frontendUrl = builder.Configuration.GetValue<string>("FRONTEND_URL");
    Log.Information($"Read variable FRONTEND_URL: [{frontendUrl}]");

    var databaseHost = builder.Configuration.GetValue<string>("DATABASE_HOST");
    Log.Information($"Read variable DATABASE_HOST: [{databaseHost}]");

    var databasePort = builder.Configuration.GetValue<string>("DATABASE_PORT");
    Log.Information($"Read variable DATABASE_PORT: [{databasePort}]");

    var databaseName = builder.Configuration.GetValue<string>("DATABASE_NAME");
    Log.Information($"Read variable DATABASE_NAME: [{databaseName}]");

    var databaseUser = builder.Configuration.GetValue<string>("DATABASE_USER");
    Log.Information($"Read variable DATABASE_USER: [{databaseUser}]");

    var databasePassword = builder.Configuration.GetValue<string>("DATABASE_PASSWORD");
    Log.Information($"Read variable DATABASE_PASSWORD: [{databasePassword}]");

    var sqliteDatabase = builder.Configuration.GetValue<string>("SQLITE_DATABASE");
    Log.Information($"Read variable SQLITE_DATABASE: [{sqliteDatabase}]");

    var adminUsersString = builder.Configuration.GetValue<string>("ADMIN_USERS");
    Log.Information($"Read variable ADMIN_USERS: [{adminUsersString}]");

    var adminUsers = adminUsersString.Split(",");

    if (!adminUsers.Any())
    {
        throw new KeyNotFoundException("At least one discord user ID in the ADMIN_USERS variable must be provided");
    }
    else if (!adminUsers.Select(u => long.TryParse(u, out _)).All(u => u == true))
    {
        throw new KeyNotFoundException("At least one valid discord user ID in the ADMIN_USERS variable must be provided");
    }
    else
    {
        adminUsers = adminUsers.Distinct().ToArray();
    }

    if (new [] { databaseHost, databaseName, databaseUser, databasePassword }.Any(s => string.IsNullOrEmpty(s)))
    {
        Log.Information("Connecting to a local sqlite database");
        builder.Services.AddDbContext<AccTelemetryTrackerContext>(x =>
        {
            x.UseSqlite($"Data Source={(string.IsNullOrEmpty(sqliteDatabase) ? "acc.db" : sqliteDatabase)}");
        });
    }
    else
    {
        try
        {
            Log.Information("Connecting to a dockerized database");
            var connectionString = $"server={databaseHost};{(string.IsNullOrEmpty(databasePort) ? "" : $"port={databasePort};")}database={databaseName};user={databaseUser};password={databasePassword};";
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
    builder.Services.AddSingleton<IDiscordNotifier, DiscordNotifier>();
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
                    .WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
    });

    var app = builder.Build();
    
    var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AccTelemetryTrackerContext>();
    if (!string.IsNullOrEmpty(sqliteDatabase))
    {
        await db.Database.EnsureCreatedAsync();
    }
    var existingAdmins = await db.Users.Where(u => adminUsers.Contains(u.Id.ToString())).ToListAsync();
    if (existingAdmins.Any())
    {
        Log.Information($"Found [{existingAdmins.Count}] admin users. Ensuring their roles are 'admin' and they are activated...");
        foreach (var admin in existingAdmins)
        {
            Log.Information($"Updating admin [{admin.Id.ToString()}]");
            admin.Role = "admin";
            admin.IsValid = true;
        }
        await db.SaveChangesAsync();
    }
    else
    {
        Log.Information($"Adding [{adminUsers.Length}] admin users");
        db.Users.AddRange(adminUsers.Select(u => new User { Id = long.Parse(u), IsValid = true, Role = "admin", SignupDate = DateTime.Now }));
        await db.SaveChangesAsync();
    }

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
