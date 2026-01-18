namespace Jdx.Servers.Proxy;

/// <summary>
/// HTTPメソッド
/// </summary>
public enum ProxyHttpMethod
{
    Unknown = 0,
    Get = 1,
    Post = 2,
    Head = 3,
    Put = 4,
    Delete = 5,
    Connect = 6,
    Options = 7,
    Trace = 8
}
