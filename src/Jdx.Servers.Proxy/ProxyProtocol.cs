namespace Jdx.Servers.Proxy;

/// <summary>
/// Proxyプロトコル種別
/// </summary>
public enum ProxyProtocol
{
    Unknown = 0,
    Http = 1,
    Ssl = 2,
    Ftp = 3
}

/// <summary>
/// クライアント/サーバー識別
/// </summary>
public enum ProxyConnectionSide
{
    Client = 0,
    Server = 1
}
