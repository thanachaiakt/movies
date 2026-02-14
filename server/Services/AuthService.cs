using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using server.Data;
using server.DTOs;
using server.Models;

namespace server.Services;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationMinutes { get; set; } = 30;
}

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        AppDbContext context,
        IOptions<JwtSettings> jwtSettings,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthTokenResult> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", dto.Email);
            throw new InvalidOperationException("User with this email already exists.");
        }

        var user = _mapper.Map<ApplicationUser>(dto);
        user.FullName = dto.FullName;

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Registration failed for {Email}: {Errors}", dto.Email, errors);
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        _logger.LogInformation("User registered successfully: {Email}", dto.Email);
        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthTokenResult> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            _logger.LogWarning("Login attempt with non-existent email: {Email}", dto.Email);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Failed login attempt for user: {Email}", dto.Email);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        _logger.LogInformation("User logged in successfully: {Email}", dto.Email);
        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthTokenResult> RefreshTokenAsync(string refreshToken)
    {
        // Find the refresh token in the database to get the userId
        var storedToken = await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken
                && !rt.IsRevoked
                && rt.ExpiresAt > DateTime.UtcNow);

        if (storedToken == null)
        {
            // Check if token was already used (possible replay attack)
            var revokedToken = await _context.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.IsRevoked);
            
            if (revokedToken != null)
            {
                _logger.LogWarning(
                    "Refresh token reuse detected! User: {UserId}, Token created: {CreatedAt}",
                    revokedToken.UserId, revokedToken.CreatedAt);
                
                // Security: Revoke ALL tokens for this user due to potential compromise
                var userTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == revokedToken.UserId && !rt.IsRevoked)
                    .ToListAsync();
                
                foreach (var token in userTokens)
                {
                    token.IsRevoked = true;
                }
                
                await _context.SaveChangesAsync();
                _logger.LogWarning(
                    "All refresh tokens revoked for user {UserId} due to token reuse",
                    revokedToken.UserId);
            }
            
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        // Check refresh count limit
        if (storedToken.RefreshCount >= storedToken.MaxRefreshCount)
        {
            _logger.LogWarning(
                "Max refresh count reached for user: {UserId}, Count: {Count}/{Max}",
                storedToken.UserId, storedToken.RefreshCount, storedToken.MaxRefreshCount);
            
            storedToken.IsRevoked = true;
            _context.RefreshTokens.Update(storedToken);
            await _context.SaveChangesAsync();
            
            throw new UnauthorizedAccessException(
                "Session expired due to inactivity. Please login again.");
        }

        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        if (user == null)
        {
            _logger.LogError("Refresh token exists but user not found: {UserId}", storedToken.UserId);
            throw new UnauthorizedAccessException("User not found.");
        }

        // Token rotation: Revoke the old refresh token
        storedToken.IsRevoked = true;
        _context.RefreshTokens.Update(storedToken);
        await _context.SaveChangesAsync();

        // Clear any tracked entities before generating new tokens
        _context.ChangeTracker.Clear();

        _logger.LogInformation(
            "Refresh token rotated successfully for user: {Email}, Count: {Count}/{Max}",
            user.Email, storedToken.RefreshCount + 1, storedToken.MaxRefreshCount);
        
        return await GenerateAuthResponseAsync(user, storedToken);
    }

    public async Task LogoutAsync(string userId, string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken
                && rt.UserId == userId
                && !rt.IsRevoked);

        if (storedToken != null)
        {
            storedToken.IsRevoked = true;
            _context.RefreshTokens.Update(storedToken);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            
            _logger.LogInformation("User logged out successfully: {UserId}", userId);
        }
    }

    public async Task<UserDto> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        return _mapper.Map<UserDto>(user);
    }

    public async Task RecordUserActivityAsync(string userId)
    {
        // Find active (non-revoked, non-expired) refresh tokens for this user
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId 
                && !rt.IsRevoked 
                && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        if (!activeTokens.Any())
            return; // No active tokens to update

        var now = DateTime.UtcNow;
        var activityThreshold = TimeSpan.FromHours(1); // Only record if >1 hour since last activity

        foreach (var token in activeTokens)
        {
            // Check if enough time has passed since last activity
            if (now - token.LastActivityAt < activityThreshold)
                continue;

            token.LastActivityAt = now;

            // Check if token is close to expiration and can be extended
            var timeUntilExpiry = token.ExpiresAt - now;
            var extensionThreshold = TimeSpan.FromDays(2); // Extend if <2 days remaining

            if (timeUntilExpiry < extensionThreshold && 
                token.ActivityExtensionCount < token.MaxActivityExtensions)
            {
                // Reset refresh count (user is actively using the system)
                var oldRefreshCount = token.RefreshCount;
                token.RefreshCount = 0;
                
                // Extend token expiration by 7 days
                token.ExpiresAt = token.ExpiresAt.AddDays(7);
                token.ActivityExtensionCount++;

                _logger.LogInformation(
                    "Token extended for active user: {UserId}, Extension: {Count}/{Max}, " +
                    "Refresh count reset from {OldCount} to 0, New expiry: {Expiry}",
                    userId, token.ActivityExtensionCount, token.MaxActivityExtensions, 
                    oldRefreshCount, token.ExpiresAt);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task<AuthTokenResult> GenerateAuthResponseAsync(
        ApplicationUser user, 
        RefreshToken? oldToken = null)
    {
        var accessToken = GenerateJwtToken(user);
        var refreshToken = await GenerateAndStoreRefreshTokenAsync(user, oldToken);

        return new AuthTokenResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpirationMinutes = _jwtSettings.AccessTokenExpirationMinutes,
            RefreshTokenExpirationMinutes = _jwtSettings.RefreshTokenExpirationMinutes,
            Email = user.Email!,
            FullName = user.FullName
        };
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateAndStoreRefreshTokenAsync(
        ApplicationUser user, 
        RefreshToken? oldToken = null)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var refreshTokenString = Convert.ToBase64String(randomBytes);

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpirationMinutes),
            IsRevoked = false,
            // Carry forward tracking data from old token
            RefreshCount = oldToken != null ? oldToken.RefreshCount + 1 : 0,
            MaxRefreshCount = oldToken?.MaxRefreshCount ?? 672, // Default: 7 days * 96 refreshes/day
            LastActivityAt = oldToken?.LastActivityAt ?? DateTime.UtcNow,
            ActivityExtensionCount = oldToken?.ActivityExtensionCount ?? 0,
            MaxActivityExtensions = oldToken?.MaxActivityExtensions ?? 10
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshTokenString;
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ValidateLifetime = false // Allow reading expired tokens
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token.");
        }

        return principal;
    }
}
