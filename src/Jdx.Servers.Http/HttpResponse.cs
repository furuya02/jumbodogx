using System.Net.Sockets;
using System.Text;

namespace Jdx.Servers.Http;

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

    /// <summary>レスポンスボディ（テキスト）</summary>
    public string Body { get; set; } = "";

    /// <summary>レスポンスボディ（バイナリ）</summary>
    public byte[]? BodyBytes { get; set; }

    /// <summary>レスポンスボディ（ストリーム）</summary>
    public Stream? BodyStream { get; set; }

    /// <summary>コンテンツ長</summary>
    public long? ContentLength { get; set; }

    /// <summary>
    /// HTTP形式のバイト配列に変換
    /// </summary>
    public byte[] ToBytes()
    {
        // ヘッダー生成
        var headerText = BuildHeaders();
        var headerBytes = Encoding.UTF8.GetBytes(headerText);

        // バイナリボディ対応
        if (BodyBytes != null)
        {
            var result = new byte[headerBytes.Length + BodyBytes.Length];
            Array.Copy(headerBytes, 0, result, 0, headerBytes.Length);
            Array.Copy(BodyBytes, 0, result, headerBytes.Length, BodyBytes.Length);
            return result;
        }

        // テキストボディ（既存）
        var bodyBytes = Encoding.UTF8.GetBytes(Body);
        var fullResponse = new byte[headerBytes.Length + bodyBytes.Length];
        Array.Copy(headerBytes, 0, fullResponse, 0, headerBytes.Length);
        Array.Copy(bodyBytes, 0, fullResponse, headerBytes.Length, bodyBytes.Length);

        return fullResponse;
    }

    /// <summary>
    /// ヘッダー文字列を構築
    /// </summary>
    private string BuildHeaders()
    {
        var sb = new StringBuilder();

        // ステータスライン
        sb.AppendLine($"HTTP/1.1 {StatusCode} {StatusText}");

        // Content-Length設定
        if (!Headers.ContainsKey("Content-Length"))
        {
            if (ContentLength.HasValue)
            {
                Headers["Content-Length"] = ContentLength.Value.ToString();
            }
            else if (BodyBytes != null)
            {
                Headers["Content-Length"] = BodyBytes.Length.ToString();
            }
            else
            {
                Headers["Content-Length"] = Encoding.UTF8.GetByteCount(Body).ToString();
            }
        }

        // Content-Type設定
        if (!Headers.ContainsKey("Content-Type"))
        {
            Headers["Content-Type"] = "text/html; charset=utf-8";
        }

        // ヘッダー出力
        foreach (var header in Headers)
        {
            sb.AppendLine($"{header.Key}: {header.Value}");
        }

        // 空行
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// ソケットにレスポンスを送信（ストリーム対応）
    /// </summary>
    public async Task SendAsync(Socket socket, CancellationToken cancellationToken)
    {
        // ヘッダー送信
        var headerBytes = Encoding.UTF8.GetBytes(BuildHeaders());
        await socket.SendAsync(headerBytes, SocketFlags.None, cancellationToken);

        // ストリームボディ送信（1MBバッファ）
        if (BodyStream != null)
        {
            var buffer = new byte[1024 * 1024]; // 1MB buffer
            int bytesRead;
            while ((bytesRead = await BodyStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await socket.SendAsync(buffer.AsMemory(0, bytesRead), SocketFlags.None, cancellationToken);
            }

            // ストリームを閉じる
            await BodyStream.DisposeAsync();
        }
        // バイナリボディ送信
        else if (BodyBytes != null)
        {
            await socket.SendAsync(BodyBytes, SocketFlags.None, cancellationToken);
        }
        // テキストボディ送信
        else if (!string.IsNullOrEmpty(Body))
        {
            var bodyBytes = Encoding.UTF8.GetBytes(Body);
            await socket.SendAsync(bodyBytes, SocketFlags.None, cancellationToken);
        }
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
