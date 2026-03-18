using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Lms.IntegrationTests.Controllers;

public class SimpleCrudTests : IClassFixture<LmsWebApplicationFactory>, IDisposable{
    private readonly LmsWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public SimpleCrudTests(LmsWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient();

        // Reset database before each test
        _factory.ResetDatabaseAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task CreateCourse_Student_ReturnsForbidden()
    {
        // Arrange
        var studentToken = await CreateAndLoginStudent();
        var studentClient = _factory.CreateClient();
        studentClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", studentToken);

        var createRequest = new
        {
            Title = "Student Course",
            Description = "Should not be allowed",
            Level = "Beginner"
        };

        // Act
        var response = await studentClient.PostAsJsonAsync("/api/courses", createRequest);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized, 
            $"Expected Forbidden or Unauthorized but got {response.StatusCode}");
        
        _output.WriteLine($"Student create course: {response.StatusCode}");
    }

    [Fact]
    public async Task UnauthenticatedUser_CannotAccess_ProtectedEndpoints()
    {
        // Act - Try to access protected endpoint without token
        var response = await _client.GetAsync("/api/enrollments/my-courses");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        _output.WriteLine($"Unauthenticated access: {response.StatusCode}");
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

    private async Task<string> CreateAndLoginStudent()
    {
        var client = _factory.CreateClient();
        
        // Register student
        var registerRequest = new
        {
            Email = "student@test.com",
            Password = "Student@123",
            FullName = "Test Student"
        };

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        if (!registerResponse.IsSuccessStatusCode)
        {
            var errorContent = await registerResponse.Content.ReadAsStringAsync();
            _output.WriteLine($"Student registration failed: {registerResponse.StatusCode} - {errorContent}");
            return string.Empty;
        }

        // Login student
        var loginRequest = new
        {
            Email = "student@test.com",
            Password = "Student@123"
        };

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var responseJson = await loginResponse.Content.ReadAsStringAsync();
        
        return ExtractTokenFromJson(responseJson);
    }

    private string ExtractTokenFromJson(string json)
    {
        // Simple token extraction - look for "token" property
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("token", out var tokenElement))
            {
                return tokenElement.GetString() ?? string.Empty;
            }
        }
        catch (JsonException)
        {
            _output.WriteLine($"Failed to parse JSON: {json}");
        }
        return string.Empty;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
