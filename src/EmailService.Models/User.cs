namespace EmailService.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
    public int DailyEmailLimit { get; set; } = 1000;
    public int EmailsSentToday { get; set; } = 0;
    public DateTime? LimitResetDate { get; set; }
    
    // Navigation property
    public List<Email> Emails { get; set; } = new();
}