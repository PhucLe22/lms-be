using Lms.Api.Data;
using Lms.Api.DTOs.Auth;
using Lms.Api.Entities;
using Lms.Api.Services;
using Lms.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lms.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "SuperSecretKeyForTestingPurposesOnly1234567890",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiryInHours"] = "1"
            })
            .Build();

        var emailService = new Mock<IEmailService>();
        var logger = new Mock<ILogger<AuthService>>();
        _sut = new AuthService(_db, config, emailService.Object, logger.Object);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task RegisterAsync_Success_ReturnsTokenAndUserInfo()
    {
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Password123"
        };

        var result = await _sut.RegisterAsync(dto);

        Assert.NotEmpty(result.Token);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("Test User", result.FullName);
        Assert.Equal("Student", result.Role);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperation()
    {
        _db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Existing",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = "Student",
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var dto = new RegisterDto
        {
            FullName = "New User",
            Email = "test@example.com",
            Password = "Password123"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.RegisterAsync(dto));
    }

    [Fact]
    public async Task RegisterAsync_PasswordIsHashed()
    {
        var dto = new RegisterDto
        {
            FullName = "Test User",
            Email = "hash@example.com",
            Password = "Password123"
        };

        await _sut.RegisterAsync(dto);

        var user = await _db.Users.FirstAsync(u => u.Email == "hash@example.com");
        Assert.NotEqual("Password123", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password123", user.PasswordHash));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        _db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Login User",
            Email = "login@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Role = "Student",
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var dto = new LoginDto { Email = "login@example.com", Password = "Password123" };

        var result = await _sut.LoginAsync(dto);

        Assert.NotEmpty(result.Token);
        Assert.Equal("login@example.com", result.Email);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        _db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Login User",
            Email = "wrong@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Role = "Student",
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var dto = new LoginDto { Email = "wrong@example.com", Password = "WrongPassword" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_NonExistentEmail_ThrowsUnauthorized()
    {
        var dto = new LoginDto { Email = "nobody@example.com", Password = "Password123" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.LoginAsync(dto));
    }

    [Fact]
    public async Task GetMeAsync_ExistingUser_ReturnsUserDto()
    {
        var userId = Guid.NewGuid();
        _db.Users.Add(new User
        {
            Id = userId,
            FullName = "Me User",
            Email = "me@example.com",
            PasswordHash = "hash",
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GetMeAsync(userId);

        Assert.Equal("Me User", result.FullName);
        Assert.Equal("me@example.com", result.Email);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task GetMeAsync_NonExistentUser_ThrowsKeyNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetMeAsync(Guid.NewGuid()));
    }
}
