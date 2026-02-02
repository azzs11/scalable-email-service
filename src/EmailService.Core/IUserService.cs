using EmailService.Models;

namespace EmailService.Core;

public interface IUserService
{
    Task<User?> GetUserByApiKeyAsync(string apiKey);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User> CreateUserAsync(string email, string name);
    Task<bool> ValidateApiKeyAsync(string apiKey);
    Task<bool> CheckRateLimitAsync(Guid userId);
    Task IncrementEmailCountAsync(Guid userId);
}