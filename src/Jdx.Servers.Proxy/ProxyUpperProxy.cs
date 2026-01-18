namespace Jdx.Servers.Proxy;

/// <summary>
/// 上位プロキシ情報
/// bjd5-master/ProxyHttpServer/UpperProxy.cs に対応
/// </summary>
public class ProxyUpperProxy
{
    public bool Use { get; set; }
    public string Server { get; private set; }
    public int Port { get; private set; }
    public List<string> DisableAddressList { get; private set; }
    public string AuthUser { get; private set; }
    public string AuthPass { get; private set; }
    public bool UseAuth { get; set; }

    public ProxyUpperProxy(
        bool use,
        string server,
        int port,
        List<string> disableAddressList,
        bool useAuth,
        string authUser,
        string authPass)
    {
        Use = use;
        Server = server;
        Port = port;
        DisableAddressList = disableAddressList;
        UseAuth = useAuth;
        AuthUser = authUser;
        AuthPass = authPass;
    }
}
