using Microsoft.EntityFrameworkCore;
using MiniTwit.Server;
using System.Data.SQLite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<SQLiteConnection>(c => new SQLiteConnection("Data Source=../tmp/minitwit.db;Version=3;Journal mode=Wal"));
builder.Services.AddDbContext<TwitContext>(
    options =>
        options.UseSqlite("Data Source=../tmp/minitwit.db;"));
var url = "https://localhost:5000";
// Create the host factory with the App class as parameter and the
// url we are going to use.
using var hostFactory = new WebTestingHostFactory<AssemblyClassLocator>();

// Override host configuration to mock stuff if required.
hostFactory.WithWebHostBuilder(builder =>
  {
      // Setup the url to use.
      builder.UseUrls(url);
      // Replace or add services if needed.
      builder.ConfigureServices(services =>
      {
          // services.AddTransient<....>();
      });
      // Replace or add configuration if needed.
      builder.ConfigureAppConfiguration((app, conf) =>
      {
          // conf.AddJsonFile("appsettings.Test.json");
      });
  })
  // Create the host using the CreateDefaultClient method.
  .CreateDefaultClient();


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

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
public class AssemblyClassLocator
{ }

public partial class Program { }