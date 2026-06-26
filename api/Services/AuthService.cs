using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.Dtos;
using Restaurant.Api.Models;

namespace Restaurant.Api.Services;

public class AuthService(RestaurantDbContext db) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var username = request.Username.Trim();
        if (username.Length == 0 || request.Password.Length == 0)
        {
            return null;
        }

        var user = await db.Users.SingleOrDefaultAsync(item => item.Username == username);
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return null;
        }

        return new LoginResponse(user.Id, user.Username);
    }

    public async Task<(LoginResponse? User, string? Error)> RegisterAsync(LoginRequest request)
    {
        var username = request.Username.Trim();
        if (username.Length < 3)
        {
            return (null, "El usuario tiene que tener al menos 3 caracteres.");
        }

        if (request.Password.Length < 6)
        {
            return (null, "La contrasena tiene que tener al menos 6 caracteres.");
        }

        var exists = await db.Users.AnyAsync(item => item.Username == username);
        if (exists)
        {
            return (null, "Ese usuario ya existe.");
        }

        var (hash, salt) = PasswordHasher.HashPassword(request.Password);
        var user = new User
        {
            Username = username,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (new LoginResponse(user.Id, user.Username), null);
    }
}
