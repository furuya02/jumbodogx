using System.Text;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Proxy;

/// <summary>
/// Proxyリクエスト処理
/// bjd5-master/ProxyHttpServer/Request.cs に対応
/// </summary>
public class ProxyRequest
{
    public string HostName { get; private set; } = "";
    public string Uri { get; private set; } = "";
    public string Extension { get; private set; } = "";
    public string RequestLine { get; private set; } = "";
    public int Port { get; private set; } = 80;
    public ProxyHttpMethod HttpMethod { get; private set; } = ProxyHttpMethod.Unknown;
    public ProxyProtocol Protocol { get; private set; } = ProxyProtocol.Unknown;
    public string HttpVersion { get; private set; } = "";
    public string User { get; private set; } = "";
    public string Password { get; private set; } = "";
    public Dictionary<string, string> Headers { get; private set; } = new();
    public byte[]? Body { get; private set; }

    private Encoding _encoding = Encoding.ASCII;

    /// <summary>
    /// リクエストライン用のバイトデータを生成
    /// </summary>
    public byte[] GetRequestLineBytes(bool useUpperProxy)
    {
        string line;
        if (useUpperProxy)
        {
            // 上位プロキシ経由の場合は元のリクエストラインをそのまま使用
            line = $"{RequestLine}\r\n";
        }
        else
        {
            // 直接接続の場合はURIのみ
            line = $"{HttpMethod.ToString().ToUpper()} {Uri} {HttpVersion}\r\n";
        }

        return _encoding.GetBytes(line);
    }

    /// <summary>
    /// クライアントからリクエストを受信してパース
    /// </summary>
    public async Task<bool> ReceiveAsync(
        Stream stream,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // リクエストラインを読み込み
            var line = await ReadLineAsync(stream, cancellationToken);
            if (string.IsNullOrWhiteSpace(line))
                return false;

            RequestLine = line.Trim();

            // リクエストラインをパース: "GET http://hostname:port/path HTTP/1.1"
            var parts = RequestLine.Split(' ');
            if (parts.Length < 3)
                return false;

            // HTTPメソッドをパース
            if (!Enum.TryParse<ProxyHttpMethod>(parts[0], true, out var method))
            {
                logger.LogWarning("Unsupported HTTP method: {Method}", parts[0]);
                return false;
            }
            HttpMethod = method;

            // CONNECTメソッドの場合はSSL
            if (HttpMethod == ProxyHttpMethod.Connect)
            {
                Protocol = ProxyProtocol.Ssl;
                Port = 443;
            }

            // URIとHTTPバージョン
            var uriPart = parts[1];
            HttpVersion = parts[2];

            // HTTPバージョンチェック
            if (HttpVersion != "HTTP/0.9" && HttpVersion != "HTTP/1.0" && HttpVersion != "HTTP/1.1")
            {
                logger.LogWarning("Unsupported HTTP version: {Version}", HttpVersion);
                return false;
            }

            // URIをパース
            if (!ParseUri(uriPart, logger))
                return false;

            // ヘッダーを読み込み
            await ReadHeadersAsync(stream, cancellationToken);

            // POSTやPUTの場合はボディも読み込み
            if (HttpMethod == ProxyHttpMethod.Post || HttpMethod == ProxyHttpMethod.Put)
            {
                await ReadBodyAsync(stream, cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error receiving request");
            return false;
        }
    }

    private bool ParseUri(string uriPart, ILogger logger)
    {
        try
        {
            // プロトコル判定（まだ不明の場合）
            if (Protocol == ProxyProtocol.Unknown)
            {
                if (uriPart.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    Protocol = ProxyProtocol.Http;
                    uriPart = uriPart.Substring(7); // "http://" を削除
                }
                else if (uriPart.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
                {
                    Protocol = ProxyProtocol.Ftp;
                    Port = 21;
                    uriPart = uriPart.Substring(6); // "ftp://" を削除
                }
                else if (uriPart.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    Protocol = ProxyProtocol.Ssl;
                    Port = 443;
                    uriPart = uriPart.Substring(8); // "https://" を削除
                }
                else
                {
                    // プロトコル指定なし（相対パス）
                    Protocol = ProxyProtocol.Http;
                    Uri = uriPart;
                    return true;
                }
            }

            // ホスト名とパスを分離
            var slashIndex = uriPart.IndexOf('/');
            if (slashIndex >= 0)
            {
                HostName = uriPart.Substring(0, slashIndex);
                Uri = uriPart.Substring(slashIndex);
            }
            else
            {
                HostName = uriPart;
                Uri = "/";
            }

            // ポート番号の抽出
            var colonIndex = HostName.IndexOf(':');
            if (colonIndex >= 0)
            {
                var portStr = HostName.Substring(colonIndex + 1);
                if (int.TryParse(portStr, out var port))
                {
                    Port = port;
                }
                HostName = HostName.Substring(0, colonIndex);
            }

            // 認証情報の抽出（user:pass@host形式）
            var atIndex = HostName.IndexOf('@');
            if (atIndex >= 0)
            {
                var authPart = HostName.Substring(0, atIndex);
                HostName = HostName.Substring(atIndex + 1);

                var authColonIndex = authPart.IndexOf(':');
                if (authColonIndex >= 0)
                {
                    User = authPart.Substring(0, authColonIndex);
                    Password = authPart.Substring(authColonIndex + 1);
                }
                else
                {
                    User = authPart;
                }
            }

            // 拡張子の取得
            var queryIndex = Uri.IndexOf('?');
            var pathPart = queryIndex >= 0 ? Uri.Substring(0, queryIndex) : Uri;
            var lastDotIndex = pathPart.LastIndexOf('.');
            if (lastDotIndex >= 0)
            {
                Extension = pathPart.Substring(lastDotIndex + 1);
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error parsing URI: {Uri}", uriPart);
            return false;
        }
    }

    private async Task ReadHeadersAsync(Stream stream, CancellationToken cancellationToken)
    {
        while (true)
        {
            var line = await ReadLineAsync(stream, cancellationToken);
            if (string.IsNullOrWhiteSpace(line))
                break; // 空行でヘッダー終了

            var colonIndex = line.IndexOf(':');
            if (colonIndex > 0)
            {
                var headerName = line.Substring(0, colonIndex).Trim();
                var headerValue = line.Substring(colonIndex + 1).Trim();
                Headers[headerName] = headerValue;
            }
        }
    }

    private async Task ReadBodyAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (Headers.TryGetValue("Content-Length", out var lengthStr) &&
            int.TryParse(lengthStr, out var length) &&
            length > 0)
        {
            var buffer = new byte[length];
            var totalRead = 0;
            while (totalRead < length)
            {
                var read = await stream.ReadAsync(buffer, totalRead, length - totalRead, cancellationToken);
                if (read == 0)
                    break;
                totalRead += read;
            }
            Body = buffer;
        }
    }

    private async Task<string> ReadLineAsync(Stream stream, CancellationToken cancellationToken)
    {
        var buffer = new List<byte>();
        var prevByte = (byte)0;

        while (true)
        {
            var byteBuffer = new byte[1];
            var read = await stream.ReadAsync(byteBuffer, 0, 1, cancellationToken);
            if (read == 0)
                break;

            var currentByte = byteBuffer[0];
            buffer.Add(currentByte);

            // CRLF検出
            if (prevByte == '\r' && currentByte == '\n')
            {
                // CRLF を削除
                if (buffer.Count >= 2)
                {
                    buffer.RemoveAt(buffer.Count - 1);
                    buffer.RemoveAt(buffer.Count - 1);
                }
                break;
            }

            prevByte = currentByte;
        }

        return Encoding.ASCII.GetString(buffer.ToArray());
    }
}
