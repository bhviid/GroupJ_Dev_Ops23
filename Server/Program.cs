using Microsoft.EntityFrameworkCore;
using MiniTwit.Shared;
using Microsoft.EntityFrameworkCore.InMemory;
    using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();


try
    {


        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddEnvironmentVariables(prefix: "connection_string");
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration));

        if (builder.Environment.IsDevelopment())
        {
            Console.WriteLine("Starting Development database");
            builder.Services.AddDbContext<TwitContext>(options =>
                options.UseInMemoryDatabase(databaseName: "SlimTwit"));

        }
        else
        {
            builder.Services.AddDbContext<TwitContext>(options =>
                options.UseNpgsql(Environment.GetEnvironmentVariable("connection_string")));
        }

        var app = builder.Build();


// Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TwitContext>();
        if (!context.Database.IsInMemory() && context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }

        context.Database.EnsureCreated();

        app.UseHttpsRedirection();

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");
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



    public partial class Program { }