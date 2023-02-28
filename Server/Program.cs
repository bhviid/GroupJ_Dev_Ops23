using System.Data.SQLite;
using Microsoft.EntityFrameworkCore;
using MiniTwit.Server;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables(prefix: "connection_string");
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<TwitContext>(options => options.UseNpgsql(Environment.GetEnvironmentVariable("connection_string")));
/* 
SQLite debug
builder.Services.AddSingleton<SQLiteConnection>(c => new SQLiteConnection("Data Source=../tmp/minitwit.db;Version=3;Journal mode=Wal"));
builder.Services.AddDbContext<TwitContext>(
    options =>
        options.UseSqlite("Data Source=../tmp/minitwit.db;")); */

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
if (context.Database.GetPendingMigrations().Any())
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

app.Run();

public partial class Program { }