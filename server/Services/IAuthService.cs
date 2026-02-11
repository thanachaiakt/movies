using server.DTOs;

namespace server.Services;

public interface IAuthService
{
    Task<AuthTokenResult> RegisterAsync(RegisterDto dto);
    Task<AuthTokenResult> LoginAsync(LoginDto dto);
    Task<AuthTokenResult> RefreshTokenAsync(string accessToken, string refreshToken);
    Task LogoutAsync(string userId, string refreshToken);
    Task<UserDto> GetCurrentUserAsync(string userId);
}
