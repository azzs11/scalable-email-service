namespace EmailService.Models;

public class EmailLog
{
    public Guid Id { get; set; }
    public Guid EmailId { get; set; }
    public EmailLogEvent Event { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    // Navigation property
    public Email? Email { get; set; }
}

public enum EmailLogEvent
{
    Created = 0,
    Queued = 1,
    Sending = 2,
    Sent = 3,
    Delivered = 4,
    Opened = 5,
    Clicked = 6,
    Bounced = 7,
    Failed = 8,
    Complained = 9
}