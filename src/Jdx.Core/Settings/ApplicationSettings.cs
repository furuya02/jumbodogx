namespace Jdx.Core.Settings;

/// <summary>
/// アプリケーション設定
/// </summary>
public class ApplicationSettings
{
    public HttpServerSettings HttpServer { get; set; } = new();
    public FtpServerSettings FtpServer { get; set; } = new();
    public DnsServerSettings DnsServer { get; set; } = new();
    public TftpServerSettings TftpServer { get; set; } = new();
    public DhcpServerSettings DhcpServer { get; set; } = new();
    public Pop3ServerSettings Pop3Server { get; set; } = new();
    public SmtpServerSettings SmtpServer { get; set; } = new();
    public ProxyServerSettings ProxyServer { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
}

/// <summary>
/// HTTPサーバー設定
///
/// ハイブリッド設計:
/// - Virtual Hostが設定されていない場合、この設定がデフォルトとして使用される
/// - Virtual Hostが設定されている場合、各VirtualHostSettingsが優先される
/// </summary>
public class HttpServerSettings
{
    // サーバー全体の基本設定
    public bool Enabled { get; set; }
    public string Protocol { get; set; } = "HTTP";  // HTTP or HTTPS
    public int Port { get; set; }
    public string BindAddress { get; set; } = "0.0.0.0";
    public bool UseResolve { get; set; } = false;
    public int TimeOut { get; set; } = 3;
    public int MaxConnections { get; set; }

    // デフォルト設定（Virtual Hostがない場合に使用）
    // ドキュメント設定
    public string DocumentRoot { get; set; } = "";
    public string WelcomeFileName { get; set; } = "index.html";
    public bool UseHidden { get; set; } = false;
    public bool UseDot { get; set; } = false;
    public bool UseDirectoryEnum { get; set; } = false;
    public bool UseExpansion { get; set; } = false;
    public string ServerHeader { get; set; } = "JumboDogX Version $v";
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
    public List<UserEntry> UserList { get; set; } = new();
    public List<GroupEntry> GroupList { get; set; } = new();

    // テンプレート設定
    public string Encode { get; set; } = "UTF-8";
    public string IndexDocument { get; set; } = "";
    public string ErrorDocument { get; set; } = "";

    // ACL設定
    public int EnableAcl { get; set; } = 0;  // 0=Allow, 1=Deny
    public List<AclEntry> AclList { get; set; } = new();

    // Advanced設定
    public bool UseAutoAcl { get; set; } = false;
    public bool AutoAclApacheKiller { get; set; } = false;
    public bool UseKeepAlive { get; set; } = true;
    public int KeepAliveTimeout { get; set; } = 5;
    public int MaxKeepAliveRequests { get; set; } = 100;
    public bool UseRangeRequests { get; set; } = true;
    public int MaxRangeCount { get; set; } = 20;

    // SSL/TLS設定
    public string CertificateFile { get; set; } = "";
    public string CertificatePassword { get; set; } = "";

    // Virtual Host設定
    // NOTE: 各Virtual Hostが個別の設定を持つ（上記のデフォルト設定を上書き）
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
    public bool Enabled { get; set; } = true;  // 個別起動/停止フラグ
    public string? BindAddress { get; set; } = null;  // バインドアドレス（nullの場合は親設定を使用）
    public VirtualHostSettings Settings { get; set; } = new();

    /// <summary>
    /// Hostからホスト名部分を取得（例: "example.com:8080" → "example.com"）
    /// </summary>
    public string GetHostName()
    {
        if (string.IsNullOrEmpty(Host))
            return "";

        var colonIndex = Host.LastIndexOf(':');
        return colonIndex >= 0 ? Host.Substring(0, colonIndex) : Host;
    }

    /// <summary>
    /// Hostからポート番号を取得（例: "example.com:8080" → 8080）
    /// ポート番号が指定されていない場合は8080を返す
    /// </summary>
    public int GetPort()
    {
        if (string.IsNullOrEmpty(Host))
            return 8080;

        var colonIndex = Host.LastIndexOf(':');
        if (colonIndex < 0)
            return 8080;

        var portString = Host.Substring(colonIndex + 1);
        return int.TryParse(portString, out var port) ? port : 8080;
    }
}

/// <summary>
/// Virtual Host個別設定
/// 各Virtual Hostが持つ独立した設定
/// </summary>
public class VirtualHostSettings
{
    // ドキュメント設定 (Document)
    public string DocumentRoot { get; set; } = "";
    public string WelcomeFileName { get; set; } = "index.html";
    public bool UseHidden { get; set; } = false;
    public bool UseDot { get; set; } = false;
    public bool UseDirectoryEnum { get; set; } = false;
    public bool UseExpansion { get; set; } = false;
    public string ServerHeader { get; set; } = "JumboDogX Version $v";
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

    // 認証設定 (Authentication)
    public List<AuthEntry> AuthList { get; set; } = new();
    public List<UserEntry> UserList { get; set; } = new();
    public List<GroupEntry> GroupList { get; set; } = new();

    // テンプレート設定 (Template)
    public string Encode { get; set; } = "UTF-8";
    public string IndexDocument { get; set; } = "";
    public string ErrorDocument { get; set; } = "";

    // ACL設定
    public int EnableAcl { get; set; } = 0;  // 0=Allow, 1=Deny
    public List<AclEntry> AclList { get; set; } = new();

    // SSL/TLS設定
    public string CertificateFile { get; set; } = "";
    public string CertificatePassword { get; set; } = "";

    // Advanced設定
    public bool UseAutoAcl { get; set; } = false;
    public bool AutoAclApacheKiller { get; set; } = false;
    public bool UseKeepAlive { get; set; } = true;
    public int KeepAliveTimeout { get; set; } = 5;
    public int MaxKeepAliveRequests { get; set; } = 100;
    public bool UseRangeRequests { get; set; } = true;
    public int MaxRangeCount { get; set; } = 20;
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
    public string BannerMessage { get; set; } = "JumboDogX FTP Server Ready";
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
/// TFTPサーバー設定
/// bjd5-master/TftpServer/Option.cs に対応
/// </summary>
public class TftpServerSettings
{
    // 基本設定 (Page1 - Basic)
    public bool Enabled { get; set; } = false;
    public string BindAddress { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 69;  // 標準TFTPポート
    public int TimeOut { get; set; } = 60;  // 秒
    public int MaxConnections { get; set; } = 10;

    public string WorkDir { get; set; } = "";  // 作業フォルダ
    public bool Read { get; set; } = true;  // 読込み許可
    public bool Write { get; set; } = false;  // 書込み許可
    public bool Override { get; set; } = false;  // 上書き許可

    // ACL設定 (Page2 - Acl)
    public int EnableAcl { get; set; } = 0;  // 0=Allow, 1=Deny
    public List<AclEntry> AclList { get; set; } = new();
}

/// <summary>
/// DHCPサーバー設定
/// bjd5-master/DhcpServer/Option.cs に対応
/// </summary>
public class DhcpServerSettings
{
    // 基本設定 (Page1 - Basic)
    public bool Enabled { get; set; } = false;
    public string BindAddress { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 67;  // 標準DHCPポート
    public int TimeOut { get; set; } = 30;  // 秒
    public int MaxConnections { get; set; } = 10;

    public int LeaseTime { get; set; } = 18000;  // リース期間（秒）
    public string StartIp { get; set; } = "";  // IP範囲開始
    public string EndIp { get; set; } = "";  // IP範囲終了
    public string MaskIp { get; set; } = "";  // サブネットマスク
    public string GwIp { get; set; } = "";  // ゲートウェイIP
    public string DnsIp0 { get; set; } = "";  // DNS IP #1
    public string DnsIp1 { get; set; } = "";  // DNS IP #2
    public bool UseWpad { get; set; } = false;  // WPAD対応
    public string WpadUrl { get; set; } = "";  // WPAD URL

    // MAC ACL設定 (Page2 - MacAcl)
    public bool UseMacAcl { get; set; } = false;  // MAC確認有効化
    public List<DhcpMacEntry> MacAclList { get; set; } = new();
}

/// <summary>
/// DHCPMACエントリ
/// bjd5-master/DhcpServer/OptionMacAcl.cs に対応
/// </summary>
public class DhcpMacEntry
{
    public string MacAddress { get; set; } = "";  // MACアドレス
    public string V4Address { get; set; } = "";  // IPv4アドレス
    public string MacName { get; set; } = "";  // MAC名称
}

/// <summary>
/// POP3サーバー設定
/// bjd5-master/Pop3Server/Option.cs に対応
/// </summary>
public class Pop3ServerSettings
{
    // 基本設定 (Page1 - Basic)
    public bool Enabled { get; set; } = false;
    public string BindAddress { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 110;  // 標準POP3ポート
    public int TimeOut { get; set; } = 30;  // 秒
    public int MaxConnections { get; set; } = 10;

    public string BannerMessage { get; set; } = "$p $v";  // バナーメッセージ（$p=プログラム名, $v=バージョン）
    public Pop3AuthType AuthType { get; set; } = Pop3AuthType.UserPass;  // 認証方式
    public int AuthTimeout { get; set; } = 30;  // 認証タイムアウト（秒）

    // Change Password設定 (Page2 - ChangePassword)
    public bool UseChps { get; set; } = false;  // CPHS（パスワード変更）有効化
    public int MinimumLength { get; set; } = 8;  // 最小長
    public bool DisableJoe { get; set; } = false;  // Joe's rules無効化
    public bool UseNum { get; set; } = false;  // 数字必須
    public bool UseSmall { get; set; } = false;  // 小文字必須
    public bool UseLarge { get; set; } = false;  // 大文字必須
    public bool UseSign { get; set; } = false;  // 記号必須

    // AutoDeny設定 (Page3 - AutoDeny)
    public bool UseAutoAcl { get; set; } = false;  // 自動ACL有効化
    public int AutoAclMax { get; set; } = 5;  // 最大失敗回数
    public int AutoAclSec { get; set; } = 60;  // 対象期間（秒）

    // ACL設定 (Page4 - Acl)
    public int EnableAcl { get; set; } = 0;  // 0=Allow, 1=Deny
    public List<AclEntry> AclList { get; set; } = new();
}

/// <summary>
/// POP3認証方式
/// bjd5-master/Pop3Server/AuthType.cs に対応
/// </summary>
public enum Pop3AuthType
{
    UserPass = 0,  // USER/PASS
    Apop = 1,      // APOP
    Other = 2      // その他
}

/// <summary>
/// SMTPサーバー設定
/// bjd5-master/SmtpServer/Option.cs に対応
/// </summary>
public class SmtpServerSettings
{
    // 基本設定 (Page1 - Basic)
    public bool Enabled { get; set; } = false;
    public string BindAddress { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 25;  // 標準SMTPポート
    public int TimeOut { get; set; } = 30;  // 秒
    public int MaxConnections { get; set; } = 10;

    public string DomainName { get; set; } = "";  // ドメイン名（カンマ区切り複数対応）
    public string BannerMessage { get; set; } = "$d SMTP Server Ready";  // バナーメッセージ
    public string ReceivedHeader { get; set; } = "from $h by $d with SMTP id $i; $t";  // Receivedヘッダテンプレート
    public int SizeLimit { get; set; } = 5000;  // メールサイズ制限（KB）
    public string ErrorFrom { get; set; } = "";  // エラー返信元
    public bool UseNullFrom { get; set; } = false;  // Null送信元許可
    public bool UseNullDomain { get; set; } = false;  // Nullドメイン許可
    public bool UsePopBeforeSmtp { get; set; } = false;  // POP-before-SMTP有効化
    public int TimePopBeforeSmtp { get; set; } = 600;  // POP-before-SMTP有効期間（秒）
    public bool UseCheckFrom { get; set; } = false;  // From確認

    // ESMTP設定 (Page2 - Esmtp)
    public bool UseEsmtp { get; set; } = false;  // ESMTP有効化
    public bool UseAuthCramMd5 { get; set; } = false;  // CRAM-MD5対応
    public bool UseAuthPlain { get; set; } = false;  // PLAIN対応
    public bool UseAuthLogin { get; set; } = false;  // LOGIN対応
    public bool UsePopAccount { get; set; } = false;  // POP3アカウント認証
    public List<SmtpUserEntry> EsmtpUserList { get; set; } = new();  // ESMTP専用ユーザリスト
    public int EnableEsmtp { get; set; } = 0;  // ESMTP許可範囲（0=全て、1=特定ユーザ）
    public List<SmtpRangeEntry> RangeList { get; set; } = new();  // ESMTP許可範囲リスト

    // Relay設定 (Page3 - Relay)
    public int Order { get; set; } = 0;  // フィルタリングルール（0=許可優先、1=拒否優先）
    public List<string> AllowList { get; set; } = new();  // 許可リスト（アドレス指定）
    public List<string> DenyList { get; set; } = new();  // 拒否リスト（アドレス指定）

    // Queue設定 (Page4 - Queue)
    public bool Always { get; set; } = false;  // キュー常時処理
    public int ThreadSpan { get; set; } = 300;  // スレッド待機時間（秒）
    public int RetryMax { get; set; } = 5;  // リトライ最大回数
    public int ThreadMax { get; set; } = 5;  // 処理スレッド数
    public bool MxOnly { get; set; } = false;  // MXレコード優先

    // Host設定 (Page5 - Host)
    public List<SmtpHostEntry> HostList { get; set; } = new();  // ホスト転送設定

    // Header設定 (Page6 - Header)
    public List<SmtpHeaderPatternEntry> PatternList { get; set; } = new();  // パターン置換リスト
    public List<SmtpHeaderAppendEntry> AppendList { get; set; } = new();  // ヘッダ追加リスト

    // Aliases設定 (Page7 - Aliases)
    public List<SmtpAliasEntry> AliasList { get; set; } = new();  // エリアスリスト

    // AutoReception設定 (Page8 - AutoReception)
    public List<SmtpFetchEntry> FetchList { get; set; } = new();  // 自動受信リスト

    // ACL設定 (Page9 - Acl)
    public int EnableAcl { get; set; } = 0;  // 0=Allow, 1=Deny
    public List<AclEntry> AclList { get; set; } = new();
}

/// <summary>
/// SMTPユーザーエントリ（ESMTP認証用）
/// bjd5-master/SmtpServer/SmtpAuthUserList.cs に対応
/// </summary>
public class SmtpUserEntry
{
    public string UserName { get; set; } = "";  // ユーザー名
    public string Password { get; set; } = "";  // パスワード
}

/// <summary>
/// SMTP範囲エントリ
/// bjd5-master/SmtpServer/SmtpAuthRange.cs に対応
/// </summary>
public class SmtpRangeEntry
{
    public string Address { get; set; } = "";  // IPアドレス範囲（IP/CIDR）
}

/// <summary>
/// SMTPホストエントリ（スマートホスト設定）
/// bjd5-master/SmtpServer/OptionHost.cs に対応
/// </summary>
public class SmtpHostEntry
{
    public string HostName { get; set; } = "";  // ホスト名またはIPアドレス
    public int Port { get; set; } = 25;  // ポート番号
}

/// <summary>
/// SMTPヘッダパターンエントリ（削除/拒否ルール）
/// bjd5-master/SmtpServer/OptionHeader.cs に対応
/// </summary>
public class SmtpHeaderPatternEntry
{
    public string Pattern { get; set; } = "";  // パターン（マッチング文字列）
    public int Action { get; set; } = 0;  // アクション（0=削除、1=拒否）
}

/// <summary>
/// SMTPヘッダ追加エントリ
/// bjd5-master/SmtpServer/OptionHeader.cs に対応
/// </summary>
public class SmtpHeaderAppendEntry
{
    public string Header { get; set; } = "";  // ヘッダ名
    public string Value { get; set; } = "";  // ヘッダ値
}

/// <summary>
/// SMTPエリアスエントリ
/// bjd5-master/SmtpServer/OptionAlias.cs に対応
/// </summary>
public class SmtpAliasEntry
{
    public string AliasName { get; set; } = "";  // エイリアスアドレス
    public string UserName { get; set; } = "";  // 実ユーザアカウント
}

/// <summary>
/// SMTP自動受信エントリ（フェッチ設定）
/// bjd5-master/SmtpServer/OptionAutoReception.cs に対応
/// </summary>
public class SmtpFetchEntry
{
    public string Server { get; set; } = "";  // 受信サーバ
    public int Port { get; set; } = 110;  // 受信ポート
    public string UserName { get; set; } = "";  // 認証ユーザ
    public string Password { get; set; } = "";  // 認証パスワード
    public int Interval { get; set; } = 300;  // 受信間隔（秒）
    public int MaxCount { get; set; } = 100;  // 最大取得件数
    public bool UseSsl { get; set; } = false;  // SSL/TLS使用
    public bool UseApop { get; set; } = false;  // APOP認証使用
}

/// <summary>
/// ログ設定
/// </summary>
public class LoggingSettings
{
    public string LogLevel { get; set; } = "Information";
    public int MaxEntries { get; set; }
}

/// <summary>
/// Proxyサーバー設定
/// bjd5-master/ProxyHttpServer/Option.cs に対応
/// </summary>
public class ProxyServerSettings
{
    // 基本設定 (TAB1: Basic)
    public bool Enabled { get; set; } = false;
    public string BindAddress { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 8080;  // 標準Proxyポート
    public int TimeOut { get; set; } = 60;  // 秒
    public int MaxConnections { get; set; } = 300;

    // リクエストログ
    public bool UseRequestLog { get; set; } = false;

    // 匿名設定
    public string AnonymousAddress { get; set; } = "JumboDogX@";
    public string ServerHeader { get; set; } = "JumboDogX Version $v";

    // ブラウザヘッダー
    public bool UseBrowserHeader { get; set; } = false;

    // 追加ヘッダー（UseBrowserHeader が false の場合有効）
    public bool AddHeaderRemoteHost { get; set; } = false;
    public bool AddHeaderXForwardedFor { get; set; } = false;
    public bool AddHeaderForwarded { get; set; } = false;

    // 上位プロキシ設定 (TAB2: HigherProxy)
    public bool UseUpperProxy { get; set; } = false;
    public string UpperProxyServer { get; set; } = "";
    public int UpperProxyPort { get; set; } = 8080;
    public bool UpperProxyUseAuth { get; set; } = false;
    public string UpperProxyAuthName { get; set; } = "";
    // WARNING: Storing passwords in plaintext is insecure.
    // Consider using environment variables or ASP.NET Core User Secrets in production.
    public string UpperProxyAuthPass { get; set; } = "";
    public List<ProxyDisableAddressEntry> DisableAddressList { get; set; } = new();

    // キャッシュ設定 (TAB3: Cache1)
    public bool UseCache { get; set; } = false;
    public string CacheDir { get; set; } = "";
    public int TestTime { get; set; } = 3;  // 分
    public int MemorySize { get; set; } = 1000;  // KB
    public int DiskSize { get; set; } = 5000;  // KB
    public int Expires { get; set; } = 24;  // 時間
    public int MaxSize { get; set; } = 1200;  // KB

    // キャッシュホスト・拡張子設定 (TAB4: Cache2)
    public int EnableHost { get; set; } = 1;  // 0=すべて, 1=指定したホストのみ
    public List<ProxyCacheHostEntry> CacheHostList { get; set; } = new();
    public int EnableExt { get; set; } = 1;  // 0=すべて, 1=指定した拡張子のみ
    public List<ProxyCacheExtEntry> CacheExtList { get; set; } = new();

    // URL制限設定 (TAB5: LimitUrl)
    public List<ProxyLimitUrlEntry> LimitUrlAllowList { get; set; } = new();
    public List<ProxyLimitUrlEntry> LimitUrlDenyList { get; set; } = new();

    // コンテンツ制限設定 (TAB6: LimitContents)
    public List<ProxyLimitStringEntry> LimitStringList { get; set; } = new();

    // ACL設定 (TAB7: Acl)
    public int EnableAcl { get; set; } = 0;  // 0=Allow, 1=Deny
    public List<AclEntry> AclList { get; set; } = new();
}

/// <summary>
/// Proxy除外アドレスエントリ（上位プロキシを使用しないアドレス）
/// bjd5-master/ProxyHttpServer/Option.cs Page2 に対応
/// </summary>
public class ProxyDisableAddressEntry
{
    public string Address { get; set; } = "";  // アドレス（IP、ホスト名）
}

/// <summary>
/// Proxyキャッシュホストエントリ
/// bjd5-master/ProxyHttpServer/Option.cs Page4 に対応
/// </summary>
public class ProxyCacheHostEntry
{
    public string Host { get; set; } = "";  // ホスト名
}

/// <summary>
/// Proxyキャッシュ拡張子エントリ
/// bjd5-master/ProxyHttpServer/Option.cs Page4 に対応
/// </summary>
public class ProxyCacheExtEntry
{
    public string Ext { get; set; } = "";  // 拡張子（例: jpg, png, css）
}

/// <summary>
/// ProxyURL制限エントリ
/// bjd5-master/ProxyHttpServer/Option.cs Page5 に対応
/// </summary>
public class ProxyLimitUrlEntry
{
    public string Url { get; set; } = "";  // URL
    public int Matching { get; set; } = 0;  // マッチング方式（0=前方一致, 1=後方一致, 2=部分一致, 3=正規表現）
}

/// <summary>
/// Proxyコンテンツ制限文字列エントリ
/// bjd5-master/ProxyHttpServer/Option.cs Page6 に対応
/// </summary>
public class ProxyLimitStringEntry
{
    public string String { get; set; } = "";  // 制限する文字列
}
