using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Lms.IntegrationTests.Controllers;

public class DockerAuthTests : IClassFixture<LmsWebApplicationFactory>, IDisposable
{
    private readonly LmsWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public DockerAuthTests(LmsWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient();

        // Reset database before each test
        _factory.ResetDatabaseAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Register_ValidUser_ReturnsSuccess()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "test@example.com",
            Password = "Test@123",
            FullName = "Test User"
        };

        var json = JsonSerializer.Serialize(registerRequest);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        Assert.Contains("test@example.com", responseJson);
        Assert.Contains("Test User", responseJson);
        Assert.Contains("token", responseJson);
        
        _output.WriteLine($"Register Response: {responseJson}");
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsSuccess()
    {
        // Arrange - Register user first
        var registerRequest = new
        {
            Email = "login@example.com",
            Password = "Test@123",
            FullName = "Login User"
        };

        var json = JsonSerializer.Serialize(registerRequest);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        await _client.PostAsync("/api/auth/register", content);

        var loginRequest = new
        {
            Email = "login@example.com",
            Password = "Test@123"
        };

        var loginJson = JsonSerializer.Serialize(loginRequest);
        var loginContent = new StringContent(loginJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", loginContent);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        Assert.Contains("login@example.com", responseJson);
        Assert.Contains("token", responseJson);
        
        _output.WriteLine($"Login Response: {responseJson}");
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
        
        _output.WriteLine($"Health Check Response: {content}");
    }

    [Fact]
    public async Task GetCourses_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/courses");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("[]", content); // Should be empty array
        
        _output.WriteLine($"Courses Response: {content}");
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
