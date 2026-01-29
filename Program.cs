using System.Text;
using AuthApi.Services;
using AuthApi.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;


DotNetEnv.Env.Load();
var connectionString = Environment.GetEnvironmentVariable("MongoDb__ConnectionString");
var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
var smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        )
    };
});
// Mongo settings
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings")
);

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value
);

builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<JwtService>();

builder.Services.AddSingleton<EmailService>();
builder.Services.AddHttpClient<CaptchaService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin-allow-popups";
    await next();
});

app.UseCors("AllowFrontend");
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
