using Microsoft.AspNetCore.Mvc;
using EmailService.Core;
using EmailService.Models;

namespace EmailService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailsController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly IUserService _userService;

    public EmailsController(IEmailService emailService, IUserService userService)
    {
        _emailService = emailService;
        _userService = userService;
    }

    [HttpPost("send")]
    public async Task<ActionResult<EmailResponse>> SendEmail([FromBody] SendEmailRequest request)
    {
        try
        {
            // Get API key from header
            var apiKey = Request.Headers["X-API-Key"].FirstOrDefault();
            if (string.IsNullOrEmpty(apiKey))
                return Unauthorized(new { error = "API Key required" });

            // Get user
            var user = await _userService.GetUserByApiKeyAsync(apiKey);
            if (user == null)
                return Unauthorized(new { error = "Invalid API Key" });

            // Check rate limit
            var canSend = await _userService.CheckRateLimitAsync(user.Id);
            if (!canSend)
                return StatusCode(429, new { error = "Daily rate limit exceeded" });

            // Send email
            var response = await _emailService.SendEmailAsync(request, user.Id);

            // Increment counter
            await _userService.IncrementEmailCountAsync(user.Id);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost("send-bulk")]
    public async Task<ActionResult<List<EmailResponse>>> SendBulkEmail([FromBody] SendBulkEmailRequest request)
    {
        try
        {
            var apiKey = Request.Headers["X-API-Key"].FirstOrDefault();
            if (string.IsNullOrEmpty(apiKey))
                return Unauthorized(new { error = "API Key required" });

            var user = await _userService.GetUserByApiKeyAsync(apiKey);
            if (user == null)
                return Unauthorized(new { error = "Invalid API Key" });

            var canSend = await _userService.CheckRateLimitAsync(user.Id);
            if (!canSend)
                return StatusCode(429, new { error = "Daily rate limit exceeded" });

            var responses = await _emailService.SendBulkEmailAsync(request, user.Id);

            await _userService.IncrementEmailCountAsync(user.Id);

            return Ok(responses);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Email>> GetEmail(Guid id)
    {
        var email = await _emailService.GetEmailByIdAsync(id);
        if (email == null)
            return NotFound(new { error = "Email not found" });

        return Ok(email);
    }

    [HttpGet]
    public async Task<ActionResult<List<Email>>> GetEmails([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var apiKey = Request.Headers["X-API-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey))
            return Unauthorized(new { error = "API Key required" });

        var user = await _userService.GetUserByApiKeyAsync(apiKey);
        if (user == null)
            return Unauthorized(new { error = "Invalid API Key" });

        var emails = await _emailService.GetEmailsByUserIdAsync(user.Id, page, pageSize);
        return Ok(emails);
    }
}