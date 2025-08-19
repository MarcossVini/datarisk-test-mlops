using Xunit;
using Moq;
using FluentAssertions;
using DatariskMLOps.Domain.Entities;
using DatariskMLOps.Domain.Interfaces;
using DatariskMLOps.Domain.Services;

namespace DatariskMLOps.Tests.Unit.Services;

public class ScriptServiceTests
{
    private readonly Mock<IScriptRepository> _scriptRepositoryMock;
    private readonly ScriptService _scriptService;

    public ScriptServiceTests()
    {
        _scriptRepositoryMock = new Mock<IScriptRepository>();
        _scriptService = new ScriptService(_scriptRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnScript_WhenValidInputProvided()
    {
        // Arrange
        var name = "Test Script";
        var content = "function test() { return 'hello'; }";
        var description = "Test description";

        var expectedScript = new Script
        {
            Id = Guid.NewGuid(),
            Name = name,
            Content = content,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _scriptRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Script>()))
            .ReturnsAsync(expectedScript);

        // Act
        var result = await _scriptService.CreateAsync(name, content, description);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(name);
        result.Content.Should().Be(content);
        result.Description.Should().Be(description);

        _scriptRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Script>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnScript_WhenScriptExists()
    {
        // Arrange
        var scriptId = Guid.NewGuid();
        var expectedScript = new Script
        {
            Id = scriptId,
            Name = "Test Script",
            Content = "function test() { return 'hello'; }",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _scriptRepositoryMock
            .Setup(x => x.GetByIdAsync(scriptId))
            .ReturnsAsync(expectedScript);

        // Act
        var result = await _scriptService.GetByIdAsync(scriptId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(scriptId);
        result.Name.Should().Be("Test Script");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenScriptDoesNotExist()
    {
        // Arrange
        var scriptId = Guid.NewGuid();

        _scriptRepositoryMock
            .Setup(x => x.GetByIdAsync(scriptId))
            .ReturnsAsync((Script?)null);

        // Act
        var result = await _scriptService.GetByIdAsync(scriptId);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(0, 10, 1, 10)] // page 0 should become 1
    [InlineData(-1, 10, 1, 10)] // negative page should become 1
    [InlineData(1, 0, 1, 10)] // size 0 should become 10
    [InlineData(1, 101, 1, 10)] // size > 100 should become 10
    [InlineData(1, -1, 1, 10)] // negative size should become 10
    public async Task GetAllAsync_ShouldNormalizeParameters(int inputPage, int inputSize, int expectedPage, int expectedSize)
    {
        // Arrange
        var scripts = new List<Script>();
        _scriptRepositoryMock
            .Setup(x => x.GetAllAsync(expectedPage, expectedSize))
            .ReturnsAsync(scripts);

        // Act
        await _scriptService.GetAllAsync(inputPage, inputSize);

        // Assert
        _scriptRepositoryMock.Verify(x => x.GetAllAsync(expectedPage, expectedSize), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowArgumentException_WhenScriptDoesNotExist()
    {
        // Arrange
        var scriptId = Guid.NewGuid();

        _scriptRepositoryMock
            .Setup(x => x.ExistsAsync(scriptId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _scriptService.DeleteAsync(scriptId));
        exception.Message.Should().Contain("Script not found");
        exception.ParamName.Should().Be("id");

        _scriptRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
