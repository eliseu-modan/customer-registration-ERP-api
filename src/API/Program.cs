using System.Text.Json.Serialization;
using ERP.API.Auth;
using ERP.Infrastructure;
using ERP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
ValidateJwtConfiguration(builder.Configuration);

// 🔐 CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("https://customer-registration-erp.vercel.app")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


// 🧱 Infraestrutura (DB)
builder.Services.AddInfrastructure(builder.Configuration);

// 🔐 Auth
builder.Services
    .AddAuthentication("Bearer")
    .AddScheme<AuthenticationSchemeOptions, JwtAuthenticationHandler>("Bearer", _ => { });

builder.Services.AddAuthorization();

// 🎯 Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// 📄 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "ERP API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Informe o token JWT."
    });

    options.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 🌐 PORTA DO RAILWAY (CRÍTICO)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

Console.WriteLine($"🚀 API iniciando na porta {port}...");

// 🗄️ BANCO (NÃO BLOQUEIA STARTUP)
_ = Task.Run(async () =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Console.WriteLine("🔌 Rodando migrations...");
        await dbContext.Database.MigrateAsync();

        var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
         Console.WriteLine($"DATABASE_URL: {dbUrl}");

        if (await dbContext.Database.CanConnectAsync())
        {
            Console.WriteLine("🌱 Rodando seed...");
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();
        }

        Console.WriteLine("✅ Banco pronto");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro no banco: {ex.Message}");
    }
});

// 🧪 Swagger (dev)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔐 Middlewares
app.UseCors("Frontend");

// ⚠️ Em cloud pode quebrar, então deixa comentado no começo
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// ❤️ Healthcheck (Railway usa isso)
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

// 🎯 Controllers
app.MapControllers();

app.Run();

static void ValidateJwtConfiguration(IConfiguration configuration)
{
    var issuer = configuration["Jwt:Issuer"];
    var audience = configuration["Jwt:Audience"];
    var secretKey = configuration["Jwt:SecretKey"];

    Console.WriteLine($"JWT issuer: {issuer ?? "(null)"}");
    Console.WriteLine($"JWT audience: {audience ?? "(null)"}");
    Console.WriteLine($"JWT secret length: {secretKey?.Length ?? 0}");

    if (string.IsNullOrWhiteSpace(issuer))
    {
        throw new InvalidOperationException("Jwt:Issuer deve ser configurada.");
    }

    if (string.IsNullOrWhiteSpace(audience))
    {
        throw new InvalidOperationException("Jwt:Audience deve ser configurada.");
    }

    if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
    {
        throw new InvalidOperationException("Jwt:SecretKey deve ser configurada com pelo menos 32 caracteres.");
    }
}
