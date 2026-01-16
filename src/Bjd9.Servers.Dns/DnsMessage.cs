using System.Text;

namespace Bjd9.Servers.Dns;

/// <summary>
/// 簡易DNSメッセージ（A レコードクエリ/レスポンスのみサポート）
/// </summary>
public class DnsMessage
{
    public ushort TransactionId { get; set; }
    public bool IsResponse { get; set; }
    public ushort QuestionCount { get; set; }
    public ushort AnswerCount { get; set; }
    public string QueryName { get; set; } = "";
    public ushort QueryType { get; set; }
    public ushort QueryClass { get; set; }

    /// <summary>
    /// DNSクエリをパースする（簡易実装）
    /// </summary>
    public static DnsMessage ParseQuery(byte[] data)
    {
        if (data.Length < 12)
        {
            throw new FormatException("DNS message too short");
        }

        var message = new DnsMessage
        {
            TransactionId = (ushort)((data[0] << 8) | data[1]),
            IsResponse = (data[2] & 0x80) != 0,
            QuestionCount = (ushort)((data[4] << 8) | data[5])
        };

        // クエスチョンセクションを解析
        if (message.QuestionCount > 0)
        {
            int pos = 12;
            var labels = new List<string>();

            // ドメイン名を読み取る
            while (pos < data.Length && data[pos] != 0)
            {
                int length = data[pos];
                if (length > 63 || pos + length >= data.Length)
                {
                    break;
                }

                pos++;
                var label = Encoding.ASCII.GetString(data, pos, length);
                labels.Add(label);
                pos += length;
            }

            message.QueryName = string.Join(".", labels);
            pos++; // Skip null terminator

            if (pos + 4 <= data.Length)
            {
                message.QueryType = (ushort)((data[pos] << 8) | data[pos + 1]);
                message.QueryClass = (ushort)((data[pos + 2] << 8) | data[pos + 3]);
            }
        }

        return message;
    }

    /// <summary>
    /// DNS応答を生成する（A レコード固定）
    /// </summary>
    public byte[] CreateResponse(string ipAddress)
    {
        var response = new List<byte>();

        // ヘッダー（12バイト）
        response.Add((byte)(TransactionId >> 8));
        response.Add((byte)(TransactionId & 0xFF));
        response.Add(0x81); // QR=1 (response), Opcode=0, AA=0, TC=0, RD=1
        response.Add(0x80); // RA=1, Z=0, RCODE=0
        response.Add(0x00); // QDCOUNT high
        response.Add(0x01); // QDCOUNT low (1 question)
        response.Add(0x00); // ANCOUNT high
        response.Add(0x01); // ANCOUNT low (1 answer)
        response.Add(0x00); // NSCOUNT high
        response.Add(0x00); // NSCOUNT low
        response.Add(0x00); // ARCOUNT high
        response.Add(0x00); // ARCOUNT low

        // クエスチョンセクション（元のクエリをエコー）
        AddDomainName(response, QueryName);
        response.Add((byte)(QueryType >> 8));
        response.Add((byte)(QueryType & 0xFF));
        response.Add((byte)(QueryClass >> 8));
        response.Add((byte)(QueryClass & 0xFF));

        // アンサーセクション
        AddDomainName(response, QueryName);
        response.Add(0x00); // Type high (A record = 1)
        response.Add(0x01); // Type low
        response.Add(0x00); // Class high (IN = 1)
        response.Add(0x01); // Class low
        response.Add(0x00); // TTL (300 seconds)
        response.Add(0x00);
        response.Add(0x01);
        response.Add(0x2C);
        response.Add(0x00); // RDLENGTH high
        response.Add(0x04); // RDLENGTH low (4 bytes for IPv4)

        // IP アドレス
        var ipParts = ipAddress.Split('.');
        if (ipParts.Length == 4)
        {
            foreach (var part in ipParts)
            {
                response.Add(byte.Parse(part));
            }
        }
        else
        {
            // デフォルトIP
            response.AddRange(new byte[] { 127, 0, 0, 1 });
        }

        return response.ToArray();
    }

    /// <summary>
    /// ドメイン名をDNS形式でバイト配列に追加
    /// </summary>
    private void AddDomainName(List<byte> buffer, string domainName)
    {
        var labels = domainName.Split('.');
        foreach (var label in labels)
        {
            buffer.Add((byte)label.Length);
            buffer.AddRange(Encoding.ASCII.GetBytes(label));
        }
        buffer.Add(0); // Null terminator
    }
}
