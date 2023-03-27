
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();
    builder.Configuration.AddEnvironmentVariables(prefix: "connection_string");
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration));
    if (builder.Environment.IsDevelopment())
    {
        Console.WriteLine("Starting Development database");
        builder.Services.AddDbContext<TwitContext>(options => options.UseInMemoryDatabase(databaseName: "SlimTwit"));

    }
    else
    {
        builder.Services.AddDbContext<TwitContext>(options =>
            options.UseNpgsql(Environment.GetEnvironmentVariable("connection_string")));
    }

    builder.Services.AddSingleton<PrometheusMetrics>();
    var app = builder.Build();

// Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TwitContext>();
    if (!context.Database.IsInMemory() && context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
        context.Database.EnsureCreated();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseHttpMetrics();
    app.UseAuthorization();

    app.MapControllers();

    app.UseCors();

    app.MapMetrics();
    app.UseSerilogRequestLogging();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly!");
}
finally
{
    Log.CloseAndFlush();
}


