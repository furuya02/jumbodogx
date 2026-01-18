using Jdx.Servers.Http;
using Xunit;

namespace Jdx.Servers.Http.Tests;

public class HttpRequestTests
{
    [Fact]
    public void Parse_WithValidRequestLine_ParsesCorrectly()
    {
        // Arrange
        var requestLine = "GET /index.html HTTP/1.1";

        // Act
        var request = HttpRequest.Parse(requestLine);

        // Assert
        Assert.Equal("GET", request.Method);
        Assert.Equal("/index.html", request.Path);
        Assert.Equal("HTTP/1.1", request.Version);
    }

    [Fact]
    public void Parse_WithInvalidRequestLine_ThrowsFormatException()
    {
        // Arrange
        var requestLine = "GET /index.html";

        // Act & Assert
        Assert.Throws<FormatException>(() => HttpRequest.Parse(requestLine));
    }

    [Fact]
    public void ParseFull_WithHeaders_ParsesHeadersCorrectly()
    {
        // Arrange
        var lines = new[]
        {
            "GET /index.html HTTP/1.1",
            "Host: localhost",
            "User-Agent: TestClient/1.0",
            "Accept: text/html",
            ""
        };

        // Act
        var request = HttpRequest.ParseFull(lines);

        // Assert
        Assert.Equal("GET", request.Method);
        Assert.Equal("/index.html", request.Path);
        Assert.Equal(3, request.Headers.Count);
        Assert.Equal("localhost", request.Headers["Host"]);
        Assert.Equal("TestClient/1.0", request.Headers["User-Agent"]);
        Assert.Equal("text/html", request.Headers["Accept"]);
    }

    [Fact]
    public void ParseFull_WithQueryString_ParsesQueryCorrectly()
    {
        // Arrange
        var lines = new[]
        {
            "GET /search?q=test&lang=en HTTP/1.1",
            "Host: localhost",
            ""
        };

        // Act
        var request = HttpRequest.ParseFull(lines);

        // Assert
        Assert.Equal("/search", request.Path);
        Assert.Equal("q=test&lang=en", request.QueryString);
        Assert.Equal(2, request.Query.Count);
        Assert.Equal("test", request.Query["q"]);
        Assert.Equal("en", request.Query["lang"]);
    }

    [Fact]
    public void ParseFull_WithEncodedQueryString_DecodesCorrectly()
    {
        // Arrange
        var lines = new[]
        {
            "GET /search?q=hello%20world&name=John%20Doe HTTP/1.1",
            "Host: localhost",
            ""
        };

        // Act
        var request = HttpRequest.ParseFull(lines);

        // Assert
        Assert.Equal("hello world", request.Query["q"]);
        Assert.Equal("John Doe", request.Query["name"]);
    }

    [Fact]
    public void ParseFull_WithEmptyQueryValue_ParsesCorrectly()
    {
        // Arrange
        var lines = new[]
        {
            "GET /page?flag&option= HTTP/1.1",
            "Host: localhost",
            ""
        };

        // Act
        var request = HttpRequest.ParseFull(lines);

        // Assert
        Assert.Equal("", request.Query["flag"]);
        Assert.Equal("", request.Query["option"]);
    }

    [Fact]
    public void ParseFull_WithEmptyLines_ThrowsFormatException()
    {
        // Arrange
        var lines = Array.Empty<string>();

        // Act & Assert
        Assert.Throws<FormatException>(() => HttpRequest.ParseFull(lines));
    }

    [Fact]
    public void ParseFull_WithPostMethod_ParsesCorrectly()
    {
        // Arrange
        var lines = new[]
        {
            "POST /submit HTTP/1.1",
            "Host: localhost",
            "Content-Type: application/x-www-form-urlencoded",
            "Content-Length: 15",
            ""
        };

        // Act
        var request = HttpRequest.ParseFull(lines);

        // Assert
        Assert.Equal("POST", request.Method);
        Assert.Equal("/submit", request.Path);
        Assert.Equal("application/x-www-form-urlencoded", request.Headers["Content-Type"]);
    }

    [Fact]
    public void ParseFull_WithHeadersContainingColons_ParsesCorrectly()
    {
        // Arrange
        var lines = new[]
        {
            "GET / HTTP/1.1",
            "Host: localhost:8080",
            "Custom-Header: value:with:colons",
            ""
        };

        // Act
        var request = HttpRequest.ParseFull(lines);

        // Assert
        Assert.Equal("localhost:8080", request.Headers["Host"]);
        Assert.Equal("value:with:colons", request.Headers["Custom-Header"]);
    }
}
