using System.Text.Json.Serialization;
using ERP.API.Auth;
using ERP.Infrastructure;
using ERP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 🔐 CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            return;
        }

        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
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