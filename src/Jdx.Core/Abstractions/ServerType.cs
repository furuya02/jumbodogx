namespace Jdx.Core.Abstractions;

/// <summary>
/// サーバーの種類を表す列挙型
/// </summary>
public enum ServerType
{
    /// <summary>HTTPサーバー</summary>
    Http,

    /// <summary>DNSサーバー</summary>
    Dns,

    /// <summary>SMTPサーバー</summary>
    Smtp,

    /// <summary>POP3サーバー</summary>
    Pop3,

    /// <summary>FTPサーバー</summary>
    Ftp,

    /// <summary>DHCPサーバー</summary>
    Dhcp,

    /// <summary>TFTPサーバー</summary>
    Tftp,

    /// <summary>SIPサーバー</summary>
    Sip,

    /// <summary>Proxyサーバー</summary>
    Proxy,

    /// <summary>HTTPプロキシ</summary>
    ProxyHttp,

    /// <summary>FTPプロキシ</summary>
    ProxyFtp,

    /// <summary>SMTPプロキシ</summary>
    ProxySmtp,

    /// <summary>POP3プロキシ</summary>
    ProxyPop3,

    /// <summary>Telnetプロキシ</summary>
    ProxyTelnet,

    /// <summary>トンネルサーバー</summary>
    Tunnel
}
