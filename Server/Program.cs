using System.Data.SQLite;
using Microsoft.EntityFrameworkCore;
using MiniTwit.Server;

var builder2 = new ConfigurationBuilder(); // Create a ConfigurationBuilder instance
builder2.AddEnvironmentVariables("ConnectionString"); // Load the connection string from the environment variable
var config = builder2.Build(); // Build the configuration object
var localPostgresConnectionString = "";
var connectionString = config["inDocker"] == "1" ? config["ConnectionString"] : localPostgresConnectionString;// Get the connection string from the configuration object
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<SQLiteConnection>(c => new SQLiteConnection("Data Source=../tmp/minitwit.db;Version=3;Journal mode=Wal"));
builder.Services.AddDbContext<TwitContext>(
    options =>
        options.UseSqlite("Data Source=../tmp/minitwit.db;"));
/* builder.Services.AddDbContext<TwitContext>(
options =>
    options.UseNpgsql(connectionString));
 */

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

public partial class Program { }