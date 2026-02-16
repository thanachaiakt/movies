using System.Security.Claims;
using server.Services;

namespace server.Middleware;

public class UserActivityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserActivityMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public UserActivityMiddleware(RequestDelegate next, ILogger<UserActivityMiddleware> logger, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Process the request first
        await _next(context);

        // Only track activity for authenticated users on successful requests
        if (context.User.Identity?.IsAuthenticated == true && 
            context.Response.StatusCode >= 200 && 
            context.Response.StatusCode < 300)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    // Record activity asynchronously (fire and forget)
                    // Create a new scope to avoid using disposed DbContext
                    var activityTask = Task.Run(async () =>
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                        await authService.RecordUserActivityAsync(userId).ConfigureAwait(false);
                    });

                    _ = activityTask.ContinueWith(t =>
                    {
                        _logger.LogWarning(t.Exception,
                            "Failed to record user activity for user: {UserId}", userId);
                    }, TaskContinuationOptions.OnlyOnFaulted);
                }
                catch (Exception ex)
                {
                    // Don't fail the request if activity tracking fails
                    _logger.LogWarning(ex, 
                        "Failed to initiate activity tracking for user: {UserId}", userId);
                }
            }
        }
    }
}
