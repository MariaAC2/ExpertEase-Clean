using System.Text.Json.Serialization;
using ExpertEase.Application.Services;
using ExpertEase.Database;
using ExpertEase.Infrasructure.Services;
using ExpertEase.Repositories.Implementation;
using ExpertEase.Repositories.Interfaces;
using ExpertEase.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ExpertEase.Infrastructure.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Load connection string from appsettings.json
builder.Services.AddDbContext<WebAppDatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDb")));


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IRepository<WebAppDatabaseContext>, Repository<WebAppDatabaseContext>>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.Configure<JwtConfiguration>(
    builder.Configuration.GetSection("JwtConfiguration"));

builder.Services.AddSingleton(resolver =>
    resolver.GetRequiredService<IOptions<JwtConfiguration>>().Value);


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.MapControllers();
app.UseCors("AllowAll");

app.Run();