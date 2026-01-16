namespace Bjd9.Servers.Http;

/// <summary>
/// HTTPリクエスト
/// </summary>
public class HttpRequest
{
    /// <summary>HTTPメソッド（GET, POST等）</summary>
    public string Method { get; set; } = "GET";

    /// <summary>リクエストパス</summary>
    public string Path { get; set; } = "/";

    /// <summary>HTTPバージョン</summary>
    public string Version { get; set; } = "HTTP/1.1";

    /// <summary>HTTPヘッダー</summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>リクエストボディ</summary>
    public string? Body { get; set; }

    /// <summary>
    /// リクエスト行をパースする
    /// </summary>
    public static HttpRequest Parse(string requestLine)
    {
        var parts = requestLine.Split(' ', 3);
        if (parts.Length < 3)
        {
            throw new FormatException("Invalid HTTP request line");
        }

        return new HttpRequest
        {
            Method = parts[0],
            Path = parts[1],
            Version = parts[2]
        };
    }
}
