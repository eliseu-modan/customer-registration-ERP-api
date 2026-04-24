namespace ERP.Application.DTOs.Auth;

public record LoginResponse(string Token, string Name, string Username, string Role);
