using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Configurations;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories;
using ExpertEase.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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

ConfigureAuthentication();
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        // .RequireAuthenticatedUser()
        .RequireClaim(ClaimTypes.NameIdentifier)
        .RequireClaim(ClaimTypes.Name)
        .RequireClaim(ClaimTypes.Email)
        .Build();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
//
// app.UseDefaultFiles();
// app.UseStaticFiles();
//
// app.UseRouting();
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

// ✅ Local Function: JWT Auth Setup
void ConfigureAuthentication()
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme; // This is to use the JWT token with the "Bearer" scheme
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        var jwtConfiguration =
            builder.Configuration.GetSection(nameof(JwtConfiguration))
                .Get<JwtConfiguration>(); // Here we use the JWT configuration from the application.json.

        if (jwtConfiguration == null)
        {
            throw new ApplicationException("The JWT configuration needs to be set!");
        }

        var key = Encoding.ASCII.GetBytes(jwtConfiguration.Key); // Use configured key to verify the JWT signature.
        options.TokenValidationParameters = new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true, // Validate the issuer claim in the JWT. 
            ValidateAudience = true, // Validate the audience claim in the JWT.
            ValidAudience = jwtConfiguration.Audience, // Sets the intended audience.
            ValidIssuer = jwtConfiguration.Issuer, // Sets the issuing authority.
            ClockSkew = TimeSpan
                .Zero // No clock skew is added, when the token expires it will immediately become unusable.
        };
        options.RequireHttpsMetadata = false;
        options.IncludeErrorDetails = true;
    });
}