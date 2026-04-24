using ERP.Application.Abstractions;
using ERP.Domain.Entities;
using ERP.Domain.Enums;

namespace ERP.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseSeeder(AppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!_context.Users.Any())
        {
            _context.Users.AddRange(
                new User
                {
                    Name = "Administrador",
                    Username = "admin",
                    PasswordHash = _passwordHasher.Hash("admin123"),
                    Role = UserRole.Admin
                },
                new User
                {
                    Name = "Funcionário",
                    Username = "funcionario",
                    PasswordHash = _passwordHasher.Hash("func123"),
                    Role = UserRole.Employee
                });
        }

        if (!_context.Products.Any())
        {
            _context.Products.AddRange(
                new Product { Name = "Notebook Dell Inspiron", Sku = "NOTE-001", Price = 3500m },
                new Product { Name = "Monitor LG 24", Sku = "MONI-002", Price = 899.90m },
                new Product { Name = "Teclado Mecânico", Sku = "TECL-003", Price = 249.90m },
                new Product { Name = "Mouse Sem Fio", Sku = "MOUS-004", Price = 129.90m },
                new Product { Name = "Headset USB", Sku = "HEAD-005", Price = 199.90m });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
