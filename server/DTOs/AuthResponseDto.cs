namespace server.DTOs;

/// <summary>
/// Public response sent to the client (no tokens — they go in HTTP-only cookies).
/// </summary>
public class AuthResponseDto
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

/// <summary>
/// Internal result used by the controller to set cookies before sending the response.
/// </summary>
public class AuthTokenResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationMinutes { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
