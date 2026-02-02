namespace EmailService.Models;

public class Email
{
    public Guid Id { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public EmailStatus Status { get; set; } = EmailStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; } = 0;
    public Guid UserId { get; set; }
    
    // Navigation property
    public User? User { get; set; }
    public List<EmailLog> Logs { get; set; } = new();
}

public enum EmailStatus
{
    Pending = 0,
    Queued = 1,
    Sending = 2,
    Sent = 3,
    Delivered = 4,
    Failed = 5,
    Bounced = 6
}