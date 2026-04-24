using ERP.Application.Abstractions;
using ERP.Application.Services;
using ERP.Infrastructure.Integrations;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Repositories;
using ERP.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        var connectionString = ConnectionStringResolver.Resolve(configuration);

        services.AddDbContext<AppDbContext>(options =>
            options
                .UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                })
                .ConfigureWarnings(warnings =>
                {
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning);
                }));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<DatabaseSeeder>();

        services.AddHttpClient<ICepLookupService, ViaCepService>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalApis:ViaCepBaseUrl"] ?? "https://viacep.com.br/ws/");
        });

        services.AddScoped<AuthService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<ProductService>();
        services.AddScoped<OrderService>();
        services.AddScoped<DashboardService>();

        return services;
    }
}
