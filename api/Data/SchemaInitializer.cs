using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Models;
using Restaurant.Api.Services;

namespace Restaurant.Api.Data;

public static class SchemaInitializer
{
    public static void EnsureUpdated(RestaurantDbContext db, string initialUsername, string initialPassword)
    {
        db.Database.EnsureCreated();
        EnsureInitialUser(db, initialUsername, initialPassword);
    }

    private static void EnsureInitialUser(RestaurantDbContext db, string initialUsername, string initialPassword)
    {
        var existingInitialUser = db.Users.SingleOrDefault(user => user.Username == initialUsername);
        if (existingInitialUser is not null)
        {
            if (PasswordHasher.Verify("admin", existingInitialUser.PasswordHash, existingInitialUser.PasswordSalt))
            {
                var (updatedHash, updatedSalt) = PasswordHasher.HashPassword(initialPassword);
                existingInitialUser.PasswordHash = updatedHash;
                existingInitialUser.PasswordSalt = updatedSalt;
                db.SaveChanges();
            }

            return;
        }

        if (db.Users.Any())
        {
            return;
        }

        var (hash, salt) = PasswordHasher.HashPassword(initialPassword);
        db.Users.Add(new User
        {
            Username = initialUsername,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }
}
