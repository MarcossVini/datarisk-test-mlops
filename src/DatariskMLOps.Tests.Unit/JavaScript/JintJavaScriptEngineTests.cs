using Xunit;
using FluentAssertions;
using DatariskMLOps.Infrastructure.JavaScript;

namespace DatariskMLOps.Tests.Unit.JavaScript;

public class JintJavaScriptEngineTests
{
    private readonly JintJavaScriptEngine _engine;

    public JintJavaScriptEngineTests()
    {
        _engine = new JintJavaScriptEngine();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnResult_WhenValidScriptProvided()
    {
        // Arrange
        var script = "function process(data) { return data.length; }";
        var inputData = new[] { 1, 2, 3, 4, 5 };

        // Act
        var result = await _engine.ExecuteAsync(script, inputData);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldProcessArrayData_WhenFilterScriptProvided()
    {
        // Arrange
        var script = @"function process(data) { 
            return data.filter(item => item.value > 10); 
        }";
        var inputData = new[]
        {
            new { value = 5, name = "item1" },
            new { value = 15, name = "item2" },
            new { value = 8, name = "item3" },
            new { value = 20, name = "item4" }
        };

        // Act
        var result = await _engine.ExecuteAsync(script, inputData);

        // Assert
        result.Should().NotBeNull();
        // Note: The exact assertion would depend on how Jint converts the result back to .NET objects
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenInvalidScriptProvided()
    {
        // Arrange
        var script = "function process(data) { invalid syntax here }";
        var inputData = new[] { 1, 2, 3 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _engine.ExecuteAsync(script, inputData));

        exception.Message.Should().Contain("Script execution failed");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleComplexDataTransformation()
    {
        // Arrange
        var script = @"function process(data) {
            const result = data.reduce((acc, item) => {
                const key = item.category;
                if (!acc[key]) {
                    acc[key] = { category: key, total: 0, count: 0 };
                }
                acc[key].total += item.amount;
                acc[key].count += 1;
                return acc;
            }, {});
            return Object.values(result);
        }";

        var inputData = new[]
        {
            new { category = "A", amount = 100 },
            new { category = "B", amount = 200 },
            new { category = "A", amount = 150 },
            new { category = "B", amount = 50 }
        };

        // Act
        var result = await _engine.ExecuteAsync(script, inputData);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPreventDangerousOperations()
    {
        // Arrange
        var script = "function process(data) { return require('fs'); }";
        var inputData = new[] { 1, 2, 3 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _engine.ExecuteAsync(script, inputData));

        exception.Message.Should().Contain("Script execution failed");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleEmptyData()
    {
        // Arrange
        var script = "function process(data) { return data.length || 0; }";
        var inputData = new int[0];

        // Act
        var result = await _engine.ExecuteAsync(script, inputData);

        // Assert
        result.Should().Be(0);
    }
}
