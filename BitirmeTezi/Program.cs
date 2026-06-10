using Microsoft.EntityFrameworkCore;
using BitirmeTezi.Data;
using BitirmeTezi.Interface;
using BitirmeTezi.Repository;
using BitirmeTezi.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BitirmeTezi.Service;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("[WARNING] DefaultConnection string is null or empty!");
}
else
{
    Console.WriteLine("[INFO] DefaultConnection string is found and not empty.");
}
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connectionString));
// -------------------------------------------------------------
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutterWebDev", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<GeminiService>();
builder.Services.AddHttpClient<GroqService>();
builder.Services.AddScoped<UserService>();
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings")
);

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<TokenService>();

builder.WebHost.UseUrls(
    "http://0.0.0.0:5260",
    "https://0.0.0.0:7260"
);


builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<FirebaseAdminService>();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])
            )
        };
    });

// ── Firebase Admin SDK Initialization ─────────────────────────────────────
// Set FIREBASE_SERVICE_ACCOUNT_PATH env variable to the path of your
// Firebase service account JSON. Never commit the JSON file.
var firebaseServiceAccountPath = Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_PATH");
if (!string.IsNullOrEmpty(firebaseServiceAccountPath) && File.Exists(firebaseServiceAccountPath))
{
    if (FirebaseApp.DefaultInstance == null)
    {
        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(firebaseServiceAccountPath)
        });
    }
}
else
{
    Console.WriteLine("[WARNING] FIREBASE_SERVICE_ACCOUNT_PATH is not set or file does not exist. " +
        "Firebase Admin SDK will not be initialized. Firebase endpoints will return 500.");
}
// ──────────────────────────────────────────────────────────────────────────

var app = builder.Build();
app.UseCors("AllowFlutterWebDev");


await Xabe.FFmpeg.Downloader.FFmpegDownloader.GetLatestVersion(Xabe.FFmpeg.Downloader.FFmpegVersion.Official);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
if (builder.Environment.IsDevelopment())
{
    HttpClientHandler clientHandler = new HttpClientHandler();
    clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();