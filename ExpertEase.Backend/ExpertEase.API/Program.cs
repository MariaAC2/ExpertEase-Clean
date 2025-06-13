using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Configurations;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Firebase.FirestoreRepository;
using ExpertEase.Infrastructure.Middlewares;
using ExpertEase.Infrastructure.Realtime;
using ExpertEase.Infrastructure.Repositories;
using ExpertEase.Infrastructure.Services;
using ExpertEase.Infrastructure.Workers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using ReviewService = ExpertEase.Infrastructure.Services.ReviewService;
using StripeConfiguration = Stripe.StripeConfiguration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var firebasePath = configuration["Firebase:CredentialsPath"];

    if (string.IsNullOrEmpty(firebasePath) || !System.IO.File.Exists(firebasePath))
    {
        throw new InvalidOperationException("Firebase credentials path is not set or the file doesn't exist.");
    }

    var credential = GoogleCredential.FromFile(firebasePath);
    var firestoreBuilder = new FirestoreClientBuilder
    {
        Credential = credential
    };

    var client = firestoreBuilder.Build();
    return FirestoreDb.Create("expertease-1b005", client);
});


builder.Services.AddDbContext<WebAppDatabaseContext>(o => 
    o.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDb")));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
});
builder.Services.Configure<MailConfiguration>(builder.Configuration.GetSection(nameof(MailConfiguration)));
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection(nameof(StripeConfiguration)));
builder.Services.AddScoped<IRepository<WebAppDatabaseContext>, Repository<WebAppDatabaseContext>>();
builder.Services.AddScoped<IFirestoreRepository, FirestoreRepository>();
builder.Services.AddScoped<ILoginService, LoginService>()
    .AddScoped<IUserService, UserService>()
    .AddScoped<ISpecialistProfileService, SpecialistProfileService>()
    .AddScoped<IRequestService, RequestService>()
    .AddScoped<IReplyService, ReplyService>()
    .AddScoped<ITransactionSummaryGenerator, TransactionSummaryGenerator>()
    .AddScoped<ICategoryService, CategoryService>()
    .AddScoped<IMailService, MailService>()
    .AddScoped<ISpecialistService, SpecialistService>()
    .AddScoped<IConversationService, ConversationService>()
    .AddScoped<IServiceTaskService, ServiceTaskService>()
    .AddScoped<IReviewService, ReviewService>()
    .AddScoped<IMessageService, MessageService>()
    .AddScoped<IFirebaseStorageService, FirebaseStorageService>()
    .AddScoped<IPhotoService, PhotoService>()
    .AddScoped<IStripeAccountService, StripeAccountService>()
    .AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IConversationNotifier, ConversationNotifier>();
builder.Services.AddSingleton<IMessageUpdateQueue, MessageUpdateWorker>();
builder.Services.AddMemoryCache();


builder.Services.AddHostedService<InitializerWorker>()
    .AddHostedService<MessageUpdateWorker>();

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
        .RequireClaim(ClaimTypes.Role)
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
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ConversationHub>("/hubs/conversations");

app.MapControllers();
app.MapFallbackToFile("browser/index.html");

await app.RunAsync();

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
            throw new InvalidOperationException("The JWT configuration needs to be set!");
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