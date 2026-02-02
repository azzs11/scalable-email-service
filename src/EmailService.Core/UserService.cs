using EmailService.Models;

namespace EmailService.Core;

public class UserService : IUserService
{
    private static readonly List<User> _users = new();

    // Create a default user for testing
    static UserService()
    {
        _users.Add(new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Email = "test@example.com",
            Name = "Test User",
            ApiKey = "test-api-key-12345",
            IsActive = true,
            DailyEmailLimit = 1000,
            EmailsSentToday = 0,
            CreatedAt = DateTime.UtcNow
        });
    }

    public Task<User?> GetUserByApiKeyAsync(string apiKey)
    {
        var user = _users.FirstOrDefault(u => u.ApiKey == apiKey && u.IsActive);
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByIdAsync(Guid userId)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        return Task.FromResult(user);
    }

    public Task<User> CreateUserAsync(string email, string name)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = name,
            ApiKey = Guid.NewGuid().ToString(),
            IsActive = true,
            DailyEmailLimit = 1000,
            EmailsSentToday = 0,
            CreatedAt = DateTime.UtcNow
        };

        _users.Add(user);
        return Task.FromResult(user);
    }

    public Task<bool> ValidateApiKeyAsync(string apiKey)
    {
        var user = _users.FirstOrDefault(u => u.ApiKey == apiKey);
        return Task.FromResult(user != null && user.IsActive);
    }

    public Task<bool> CheckRateLimitAsync(Guid userId)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return Task.FromResult(false);

        // Reset counter if it's a new day
        if (user.LimitResetDate == null || user.LimitResetDate.Value.Date < DateTime.UtcNow.Date)
        {
            user.EmailsSentToday = 0;
            user.LimitResetDate = DateTime.UtcNow.Date;
        }

        // Check if under limit
        return Task.FromResult(user.EmailsSentToday < user.DailyEmailLimit);
    }

    public Task IncrementEmailCountAsync(Guid userId)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.EmailsSentToday++;
            user.LastUsedAt = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }
}