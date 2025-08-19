using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using DatariskMLOps.API.DTOs;
using DatariskMLOps.Infrastructure.Data;

namespace DatariskMLOps.Tests.Integration.Controllers;

public class ScriptsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ScriptsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateScript_ShouldReturnCreated_WhenValidRequestProvided()
    {
        // Arrange
        var request = new CreateScriptRequest
        {
            Name = "Test Script",
            Content = "function process(data) { return data.length; }",
            Description = "Test description"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/scripts", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var script = JsonSerializer.Deserialize<ScriptDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        script.Should().NotBeNull();
        script!.Name.Should().Be(request.Name);
        script.Content.Should().Be(request.Content);
        script.Description.Should().Be(request.Description);
    }

    [Fact]
    public async Task CreateScript_ShouldReturnBadRequest_WhenInvalidRequestProvided()
    {
        // Arrange
        var request = new CreateScriptRequest
        {
            Name = "", // Invalid: empty name
            Content = "function process(data) { return data.length; }",
            Description = "Test description"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/scripts", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetScript_ShouldReturnNotFound_WhenScriptDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/scripts/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetScripts_ShouldReturnOk_WithEmptyList_WhenNoScriptsExist()
    {
        // Act
        var response = await _client.GetAsync("/api/scripts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var scripts = JsonSerializer.Deserialize<List<ScriptDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        scripts.Should().NotBeNull();
        scripts.Should().BeEmpty();
    }
}
