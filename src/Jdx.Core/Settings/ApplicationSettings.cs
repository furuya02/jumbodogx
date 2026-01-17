namespace Jdx.Core.Settings;

/// <summary>
/// アプリケーション設定
/// </summary>
public class ApplicationSettings
{
    public HttpServerSettings HttpServer { get; set; } = new();
    public FtpServerSettings FtpServer { get; set; } = new();
    public DnsServerSettings DnsServer { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
}

/// <summary>
/// HTTPサーバー設定
/// </summary>
public class HttpServerSettings
{
    // 基本設定
    public bool Enabled { get; set; }
    public string Protocol { get; set; } = "HTTP";  // HTTP or HTTPS
    public int Port { get; set; }
    public string BindAddress { get; set; } = "0.0.0.0";
    public bool UseResolve { get; set; } = false;
    public int TimeOut { get; set; } = 3;
    public int MaxConnections { get; set; }

    // ドキュメント設定
    public string DocumentRoot { get; set; } = "";
    public string WelcomeFileName { get; set; } = "index.html";
    public bool UseHidden { get; set; } = false;
    public bool UseDot { get; set; } = false;
    public bool UseDirectoryEnum { get; set; } = false;
    public bool UseExpansion { get; set; } = false;  // Proxy拡張機能（RemoteHostヘッダー追加）
    public string ServerHeader { get; set; } = "BlackJumboDog Version $v";
    public bool UseEtag { get; set; } = false;
    public string ServerAdmin { get; set; } = "";

    // CGI設定
    public bool UseCgi { get; set; } = false;
    public List<CgiCommandEntry> CgiCommands { get; set; } = new();
    public int CgiTimeout { get; set; } = 10;
    public List<CgiPathEntry> CgiPaths { get; set; } = new();

    // SSI設定
    public bool UseSsi { get; set; } = false;
    public string SsiExt { get; set; } = "html,htm";
    public bool UseExec { get; set; } = false;

    // WebDAV設定
    public bool UseWebDav { get; set; } = false;
    public List<WebDavPathEntry> WebDavPaths { get; set; } = new();

    // Alias設定
    public List<AliasEntry> Aliases { get; set; } = new();

    // MIME設定
    public List<MimeEntry> MimeTypes { get; set; } = new();

    // 認証設定
    public List<AuthEntry> AuthList { get; set; } = new();

    // ユーザ・グループ設定
    public List<UserEntry> UserList { get; set; } = new();
    public List<GroupEntry> GroupList { get; set; } = new();

    // モデル文設定
    public string Encode { get; set; } = "UTF-8";
    public string IndexDocument { get; set; } = "";
    public string ErrorDocument { get; set; } = "";

    // 自動ACL設定
    public bool UseAutoAcl { get; set; } = false;
    public bool AutoAclApacheKiller { get; set; } = false;

    // ACL設定
    public int EnableAcl { get; set; } = 0;  // 0=Allow, 1=Deny
    public List<AclEntry> AclList { get; set; } = new();

    // Keep-Alive設定
    public bool UseKeepAlive { get; set; } = true;
    public int KeepAliveTimeout { get; set; } = 5;  // 秒
    public int MaxKeepAliveRequests { get; set; } = 100;

    // Range Requests設定
    public bool UseRangeRequests { get; set; } = true;
    public int MaxRangeCount { get; set; } = 20;  // Apache Killer対策

    // SSL/TLS設定
    public string CertificateFile { get; set; } = "";
    // WARNING: Storing passwords in plaintext is insecure.
    // Consider using environment variables or ASP.NET Core User Secrets in production.
    // Example: Environment.GetEnvironmentVariable("CERT_PASSWORD")
    public string CertificatePassword { get; set; } = "";

    // Virtual Host設定
    public List<VirtualHostEntry> VirtualHosts { get; set; } = new();
}

// テーブル形式データ用のモデルクラス
public class CgiCommandEntry
{
    public string Extension { get; set; } = "";
    public string Program { get; set; } = "";
}

public class CgiPathEntry
{
    public string Path { get; set; } = "";
    public string Directory { get; set; } = "";
}

public class WebDavPathEntry
{
    public string Path { get; set; } = "";
    public bool AllowWrite { get; set; } = false;
    public string Directory { get; set; } = "";
}

public class AliasEntry
{
    public string Name { get; set; } = "";
    public string Directory { get; set; } = "";
}

public class MimeEntry
{
    public string Extension { get; set; } = "";
    public string MimeType { get; set; } = "";
}

public class AuthEntry
{
    public string Directory { get; set; } = "";
    public string AuthName { get; set; } = "";
    public string Require { get; set; } = "";
}

public class UserEntry
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";  // SHA256ハッシュで保存
}

public class GroupEntry
{
    public string GroupName { get; set; } = "";
    public string UserName { get; set; } = "";
}

public class AclEntry
{
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
}

public class VirtualHostEntry
{
    public string Host { get; set; } = "";  // ホスト名（例: example.com:8080）
    public string DocumentRoot { get; set; } = "";
    public string CertificateFile { get; set; } = "";  // HTTPS用（オプション）
    public string CertificatePassword { get; set; } = "";  // HTTPS用（オプション）
}

/// <summary>
/// FTPサーバー設定
/// </summary>
public class FtpServerSettings
{
    // 基本設定 (Page1 - General)
    public bool Enabled { get; set; } = false;
    public string BindAddress { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 2121;  // 開発環境用デフォルトポート
    public int TimeOut { get; set; } = 300;  // 秒
    public int MaxConnections { get; set; } = 10;
    public string BannerMessage { get; set; } = "BlackJumboDog FTP Server Ready";
    public bool UseSyst { get; set; } = false;  // SYST コマンドへの応答
    public int ReservationTime { get; set; } = 30;  // PASV予約時間（秒）

    // FTPS設定 (Page1 - General)
    public bool UseFtps { get; set; } = false;
    public string CertificateFile { get; set; } = "";
    // WARNING: Storing passwords in plaintext is insecure.
    // Consider using environment variables or ASP.NET Core User Secrets in production.
    // Example: Environment.GetEnvironmentVariable("FTP_CERT_PASSWORD")
    public string CertificatePassword { get; set; } = "";

    // ユーザー設定 (Page3 - User)
    public List<FtpUserEntry> UserList { get; set; } = new();

    // 仮想フォルダ設定 (Page2 - VirtualFolder)
    public List<FtpMountEntry> MountList { get; set; } = new();

    // ACL設定 (PageAcl - Acl)
    public int EnableAcl { get; set; } = 0;  // 0=Allow, 1=Deny
    public List<AclEntry> AclList { get; set; } = new();
}

/// <summary>
/// FTPユーザーエントリ
/// </summary>
public class FtpUserEntry
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";  // 暗号化して保存（bjd5-masterと同様）
    public string HomeDirectory { get; set; } = "";
    public FtpAccessControl AccessControl { get; set; } = FtpAccessControl.Full;
}

/// <summary>
/// FTPアクセス制御
/// bjd5-master FtpAcl.cs に対応
/// </summary>
public enum FtpAccessControl
{
    Full = 0,  // フル権限（アップロード・ダウンロード両方可）
    Down = 1,  // ダウンロードのみ
    Up = 2     // アップロードのみ
}

/// <summary>
/// FTP仮想フォルダマウント
/// bjd5-master OneMount.cs に対応
/// </summary>
public class FtpMountEntry
{
    public string FromFolder { get; set; } = "";  // 仮想パス（FTP上のパス）
    public string ToFolder { get; set; } = "";    // 実際のディレクトリパス
}

/// <summary>
/// DNSサーバー設定
/// bjd5-master/DnsServer/Option.cs に対応
/// </summary>
public class DnsServerSettings
{
    // 基本設定 (TAB1: Basic)
    public bool Enabled { get; set; } = false;
    public int Port { get; set; } = 53;  // 標準DNSポート（特権必要）
    public int MaxConnections { get; set; } = 10;
    public int TimeOut { get; set; } = 30;  // 秒

    // キャッシュ設定
    public string RootCache { get; set; } = "named.ca";

    // 再帰設定
    public bool UseRecursion { get; set; } = true;

    // SOA (Start of Authority) レコード設定
    public string SoaMail { get; set; } = "postmaster";
    public int SoaSerial { get; set; } = 1;
    public int SoaRefresh { get; set; } = 3600;  // 秒
    public int SoaRetry { get; set; } = 300;  // 秒
    public int SoaExpire { get; set; } = 360000;  // 秒
    public int SoaMinimum { get; set; } = 3600;  // 秒（最小TTL）

    // ドメイン管理
    public List<DnsDomainEntry> DomainList { get; set; } = new();

    // リソースレコード管理
    public List<DnsResourceEntry> ResourceList { get; set; } = new();

    // ACL設定 (TAB2: ACL)
    public int EnableAcl { get; set; } = 0;  // 0=Allow, 1=Deny
    public List<AclEntry> AclList { get; set; } = new();
}

/// <summary>
/// DNSドメインエントリ
/// bjd5-master/DnsServer/OptionDnsDomain.cs に対応
/// </summary>
public class DnsDomainEntry
{
    public string Name { get; set; } = "";  // ドメイン名（例: example.com）
    public bool IsAuthority { get; set; } = false;  // 権威サーバフラグ
}

/// <summary>
/// DNSリソースレコードエントリ
/// bjd5-master/DnsServer/OptionDnsResource.cs に対応
/// </summary>
public class DnsResourceEntry
{
    public DnsType Type { get; set; } = DnsType.A;  // レコードタイプ
    public string Name { get; set; } = "";  // ホスト名/ドメイン名
    public string Alias { get; set; } = "";  // エイリアス（CNAME用）
    public string Address { get; set; } = "";  // IPアドレス（A/AAAA用）
    public int Priority { get; set; } = 0;  // 優先度（MX用）
}

/// <summary>
/// DNSレコードタイプ
/// bjd5-master/DnsServer/DnsType.cs に対応
/// </summary>
public enum DnsType
{
    A = 1,       // IPv4アドレス
    Ns = 2,      // ネームサーバ
    Cname = 5,   // 正規名（エイリアス）
    Soa = 6,     // ゾーン転送情報
    Ptr = 12,    // ポインタ（逆引き）
    Mx = 15,     // メールサーバ
    Aaaa = 28    // IPv6アドレス
}

/// <summary>
/// ログ設定
/// </summary>
public class LoggingSettings
{
    public string LogLevel { get; set; } = "Information";
    public int MaxEntries { get; set; }
}
