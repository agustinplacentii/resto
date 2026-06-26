using Restaurant.Api.Dtos;

namespace Restaurant.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<(LoginResponse? User, string? Error)> RegisterAsync(LoginRequest request);
}
