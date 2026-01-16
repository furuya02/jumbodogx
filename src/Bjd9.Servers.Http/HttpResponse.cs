using System.Text;

namespace Bjd9.Servers.Http;

/// <summary>
/// HTTPレスポンス
/// </summary>
public class HttpResponse
{
    /// <summary>ステータスコード</summary>
    public int StatusCode { get; set; } = 200;

    /// <summary>ステータステキスト</summary>
    public string StatusText { get; set; } = "OK";

    /// <summary>HTTPヘッダー</summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>レスポンスボディ</summary>
    public string Body { get; set; } = "";

    /// <summary>
    /// HTTP形式のバイト配列に変換
    /// </summary>
    public byte[] ToBytes()
    {
        var sb = new StringBuilder();

        // ステータスライン
        sb.AppendLine($"HTTP/1.1 {StatusCode} {StatusText}");

        // ヘッダー
        if (!Headers.ContainsKey("Content-Length"))
        {
            Headers["Content-Length"] = Encoding.UTF8.GetByteCount(Body).ToString();
        }
        if (!Headers.ContainsKey("Content-Type"))
        {
            Headers["Content-Type"] = "text/html; charset=utf-8";
        }

        foreach (var header in Headers)
        {
            sb.AppendLine($"{header.Key}: {header.Value}");
        }

        // 空行
        sb.AppendLine();

        // ボディ
        sb.Append(Body);

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    /// <summary>
    /// 200 OKレスポンスを作成
    /// </summary>
    public static HttpResponse Ok(string body, string contentType = "text/html; charset=utf-8")
    {
        return new HttpResponse
        {
            StatusCode = 200,
            StatusText = "OK",
            Body = body,
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = contentType
            }
        };
    }

    /// <summary>
    /// 404 Not Foundレスポンスを作成
    /// </summary>
    public static HttpResponse NotFound()
    {
        return new HttpResponse
        {
            StatusCode = 404,
            StatusText = "Not Found",
            Body = "<html><body><h1>404 Not Found</h1></body></html>"
        };
    }
}
