using System.Text.Json.Serialization;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Configurations;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories;
using ExpertEase.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WebAppDatabaseContext>(o => 
    o.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDb")));

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// builder.WebHost.UseUrls("http://0.0.0.0:5000");

var app = builder.Build();

app.UseCors("AllowAll");
//
// app.UseDefaultFiles();
// app.UseStaticFiles();
//
// app.UseRouting();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

// app.UseEndpoints(endpoints =>
// {
//     endpoints.MapControllers();
// });
app.MapControllers();
// app.MapFallbackToFile("index.html");

app.Run();