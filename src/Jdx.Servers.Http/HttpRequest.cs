namespace Jdx.Servers.Http;

/// <summary>
/// HTTPリクエスト
/// </summary>
public class HttpRequest
{
    /// <summary>HTTPメソッド（GET, POST等）</summary>
    public string Method { get; set; } = "GET";

    /// <summary>リクエストパス（クエリ文字列を含まない）</summary>
    public string Path { get; set; } = "/";

    /// <summary>HTTPバージョン</summary>
    public string Version { get; set; } = "HTTP/1.1";

    /// <summary>HTTPヘッダー</summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>クエリ文字列（例: "key=value&foo=bar"）</summary>
    public string? QueryString { get; set; }

    /// <summary>パースされたクエリパラメータ</summary>
    public Dictionary<string, string> Query { get; set; } = new();

    /// <summary>リクエストボディ</summary>
    public string? Body { get; set; }

    /// <summary>
    /// リクエスト行をパースする（基本）
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

    /// <summary>
    /// HTTPリクエスト全体をパースする（ヘッダー、クエリ文字列含む）
    /// </summary>
    public static HttpRequest ParseFull(string[] lines)
    {
        if (lines.Length == 0)
        {
            throw new FormatException("Empty HTTP request");
        }

        // リクエスト行をパース
        var request = Parse(lines[0]);

        // URI + QueryString分離
        var uriParts = request.Path.Split('?', 2);
        request.Path = uriParts[0];
        if (uriParts.Length > 1)
        {
            request.QueryString = uriParts[1];
            request.Query = ParseQueryString(uriParts[1]);
        }

        // ヘッダー解析
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                // 空行はヘッダーの終わり
                break;
            }

            var colonIndex = lines[i].IndexOf(':');
            if (colonIndex > 0)
            {
                var key = lines[i].Substring(0, colonIndex).Trim();
                var value = lines[i].Substring(colonIndex + 1).Trim();
                request.Headers[key] = value;
            }
        }

        return request;
    }

    /// <summary>
    /// クエリ文字列をパースする（例: "key=value&foo=bar"）
    /// </summary>
    private static Dictionary<string, string> ParseQueryString(string queryString)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(queryString))
        {
            return result;
        }

        var pairs = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var kvp = pair.Split('=', 2);
            if (kvp.Length == 2)
            {
                var key = Uri.UnescapeDataString(kvp[0]);
                var value = Uri.UnescapeDataString(kvp[1]);
                result[key] = value;
            }
            else if (kvp.Length == 1)
            {
                // 値なしのキー（例: "?flag"）
                var key = Uri.UnescapeDataString(kvp[0]);
                result[key] = "";
            }
        }

        return result;
    }
}
