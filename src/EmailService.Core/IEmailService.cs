using EmailService.Models;

namespace EmailService.Core;

public interface IEmailService
{
    Task<EmailResponse> SendEmailAsync(SendEmailRequest request, Guid userId);
    Task<List<EmailResponse>> SendBulkEmailAsync(SendBulkEmailRequest request, Guid userId);
    Task<Email?> GetEmailByIdAsync(Guid emailId);
    Task<List<Email>> GetEmailsByUserIdAsync(Guid userId, int page = 1, int pageSize = 50);
    Task<bool> UpdateEmailStatusAsync(Guid emailId, EmailStatus status, string? errorMessage = null);
}