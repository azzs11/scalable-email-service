using EmailService.Models;

namespace EmailService.Core;

public class EmailService : IEmailService
{
    // We'll add a repository later for database access
    // For now, we'll use an in-memory list for demonstration
    private static readonly List<Email> _emails = new();
    private static readonly List<EmailLog> _emailLogs = new();

    public async Task<EmailResponse> SendEmailAsync(SendEmailRequest request, Guid userId)
    {
        // Validate request
        if (string.IsNullOrWhiteSpace(request.To))
            throw new ArgumentException("Recipient email is required", nameof(request.To));

        if (string.IsNullOrWhiteSpace(request.Subject))
            throw new ArgumentException("Subject is required", nameof(request.Subject));

        // Create email entity
        var email = new Email
        {
            Id = Guid.NewGuid(),
            From = "noreply@emailservice.com", // This will come from config later
            To = request.To,
            Cc = request.Cc,
            Bcc = request.Bcc,
            Subject = request.Subject,
            Body = request.Body,
            IsHtml = request.IsHtml,
            Status = EmailStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        // Save to "database" (in-memory for now)
        _emails.Add(email);

        // Create log entry
        var log = new EmailLog
        {
            Id = Guid.NewGuid(),
            EmailId = email.Id,
            Event = EmailLogEvent.Created,
            Timestamp = DateTime.UtcNow,
            Details = "Email created and queued for sending"
        };
        _emailLogs.Add(log);

        // Simulate async operation
        await Task.CompletedTask;

        return new EmailResponse
        {
            Id = email.Id,
            Status = email.Status.ToString(),
            Message = "Email queued successfully",
            CreatedAt = email.CreatedAt
        };
    }

    public async Task<List<EmailResponse>> SendBulkEmailAsync(SendBulkEmailRequest request, Guid userId)
    {
        var responses = new List<EmailResponse>();

        foreach (var recipient in request.To)
        {
            var singleRequest = new SendEmailRequest
            {
                To = recipient,
                Subject = request.Subject,
                Body = request.Body,
                IsHtml = request.IsHtml
            };

            var response = await SendEmailAsync(singleRequest, userId);
            responses.Add(response);
        }

        return responses;
    }

    public Task<Email?> GetEmailByIdAsync(Guid emailId)
    {
        var email = _emails.FirstOrDefault(e => e.Id == emailId);
        return Task.FromResult(email);
    }

    public Task<List<Email>> GetEmailsByUserIdAsync(Guid userId, int page = 1, int pageSize = 50)
    {
        var emails = _emails
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(emails);
    }

    public Task<bool> UpdateEmailStatusAsync(Guid emailId, EmailStatus status, string? errorMessage = null)
    {
        var email = _emails.FirstOrDefault(e => e.Id == emailId);
        if (email == null)
            return Task.FromResult(false);

        email.Status = status;
        email.ErrorMessage = errorMessage;

        if (status == EmailStatus.Sent)
            email.SentAt = DateTime.UtcNow;
        else if (status == EmailStatus.Delivered)
            email.DeliveredAt = DateTime.UtcNow;

        // Create log entry
        var log = new EmailLog
        {
            Id = Guid.NewGuid(),
            EmailId = email.Id,
            Event = MapStatusToEvent(status),
            Timestamp = DateTime.UtcNow,
            Details = errorMessage ?? $"Status updated to {status}"
        };
        _emailLogs.Add(log);

        return Task.FromResult(true);
    }

    private static EmailLogEvent MapStatusToEvent(EmailStatus status)
    {
        return status switch
        {
            EmailStatus.Queued => EmailLogEvent.Queued,
            EmailStatus.Sending => EmailLogEvent.Sending,
            EmailStatus.Sent => EmailLogEvent.Sent,
            EmailStatus.Delivered => EmailLogEvent.Delivered,
            EmailStatus.Failed => EmailLogEvent.Failed,
            EmailStatus.Bounced => EmailLogEvent.Bounced,
            _ => EmailLogEvent.Created
        };
    }
}