using System.Text;
using Jdx.Servers.Http;
using Xunit;

namespace Jdx.Servers.Http.Tests;

public class HttpResponseTests
{
    [Fact]
    public void Ok_CreatesOkResponse()
    {
        // Act
        var response = HttpResponse.Ok("<html>Test</html>");

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("OK", response.StatusText);
        Assert.Equal("<html>Test</html>", response.Body);
        Assert.Equal("text/html; charset=utf-8", response.Headers["Content-Type"]);
    }

    [Fact]
    public void NotFound_Creates404Response()
    {
        // Act
        var response = HttpResponse.NotFound();

        // Assert
        Assert.Equal(404, response.StatusCode);
        Assert.Equal("Not Found", response.StatusText);
        Assert.Contains("404 Not Found", response.Body);
    }

    [Fact]
    public void ToBytes_WithTextBody_ReturnsCorrectBytes()
    {
        // Arrange
        var response = new HttpResponse
        {
            StatusCode = 200,
            StatusText = "OK",
            Body = "Hello World"
        };

        // Act
        var bytes = response.ToBytes();
        var text = Encoding.UTF8.GetString(bytes);

        // Assert
        Assert.Contains("HTTP/1.1 200 OK", text);
        Assert.Contains("Content-Length: 11", text);
        Assert.Contains("Content-Type: text/html; charset=utf-8", text);
        Assert.EndsWith("Hello World", text);
    }

    [Fact]
    public void ToBytes_WithBinaryBody_ReturnsCorrectBytes()
    {
        // Arrange
        var binaryData = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
        var response = new HttpResponse
        {
            StatusCode = 200,
            StatusText = "OK",
            BodyBytes = binaryData
        };

        // Act
        var bytes = response.ToBytes();

        // Assert
        Assert.True(bytes.Length > binaryData.Length);
        Assert.Equal(binaryData[0], bytes[^4]);
        Assert.Equal(binaryData[1], bytes[^3]);
        Assert.Equal(binaryData[2], bytes[^2]);
        Assert.Equal(binaryData[3], bytes[^1]);
    }

    [Fact]
    public void ToBytes_WithCustomHeaders_IncludesHeaders()
    {
        // Arrange
        var response = new HttpResponse
        {
            StatusCode = 200,
            StatusText = "OK",
            Body = "Test",
            Headers = new Dictionary<string, string>
            {
                ["Custom-Header"] = "CustomValue",
                ["X-Test"] = "TestValue"
            }
        };

        // Act
        var bytes = response.ToBytes();
        var text = Encoding.UTF8.GetString(bytes);

        // Assert
        Assert.Contains("Custom-Header: CustomValue", text);
        Assert.Contains("X-Test: TestValue", text);
    }

    [Fact]
    public void ToBytes_AutomaticallyAddsContentLength()
    {
        // Arrange
        var response = new HttpResponse
        {
            StatusCode = 200,
            StatusText = "OK",
            Body = "Test Body"
        };

        // Act
        var bytes = response.ToBytes();
        var text = Encoding.UTF8.GetString(bytes);

        // Assert
        Assert.Contains("Content-Length: 9", text);
    }

    [Fact]
    public void ToBytes_AutomaticallyAddsContentType()
    {
        // Arrange
        var response = new HttpResponse
        {
            StatusCode = 200,
            StatusText = "OK",
            Body = "Test"
        };

        // Act
        var bytes = response.ToBytes();
        var text = Encoding.UTF8.GetString(bytes);

        // Assert
        Assert.Contains("Content-Type: text/html; charset=utf-8", text);
    }

    [Fact]
    public void Ok_WithCustomContentType_UsesCustomType()
    {
        // Act
        var response = HttpResponse.Ok("{\"test\":\"value\"}", "application/json");

        // Assert
        Assert.Equal("application/json", response.Headers["Content-Type"]);
    }

    [Fact]
    public async Task SendAsync_WithStream_WritesToStream()
    {
        // Arrange
        var response = HttpResponse.Ok("Test Response");
        using var memoryStream = new MemoryStream();

        // Act
        await response.SendAsync(memoryStream, CancellationToken.None);

        // Assert
        var text = Encoding.UTF8.GetString(memoryStream.ToArray());
        Assert.Contains("HTTP/1.1 200 OK", text);
        Assert.Contains("Test Response", text);
    }

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Act
        var response = new HttpResponse();

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("OK", response.StatusText);
        Assert.Equal("", response.Body);
        Assert.NotNull(response.Headers);
    }
}
