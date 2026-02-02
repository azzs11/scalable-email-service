using EmailService.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddSingleton<IEmailService, EmailService.Core.EmailService>();
builder.Services.AddSingleton<IUserService, UserService>();

var app = builder.Build();

// Simple middleware for API key authentication
app.Use(async (context, next) =>
{
    // Skip auth for health check
    if (context.Request.Path == "/health")
    {
        await next();
        return;
    }

    var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
    
    if (string.IsNullOrEmpty(apiKey))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(new { error = "API Key is required in X-API-Key header" });
        return;
    }

    var userService = context.RequestServices.GetRequiredService<IUserService>();
    var isValid = await userService.ValidateApiKeyAsync(apiKey);

    if (!isValid)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(new { error = "Invalid API Key" });
        return;
    }

    await next();
});

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "healthy", 
    timestamp = DateTime.UtcNow 
}));

app.Run();