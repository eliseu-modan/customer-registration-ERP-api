using ERP.Domain.Entities;

namespace ERP.Application.Abstractions;

public interface ITokenService
{
    string Generate(User user);
}
