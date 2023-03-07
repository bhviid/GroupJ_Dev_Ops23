var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TwitContext>(options => options.UseNpgsql(Environment.GetEnvironmentVariable("connection_string")));

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
app.Run();