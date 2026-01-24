# JumboDogX マニュアル

JumboDogXは、.NET 9で構築されたマルチプロトコルサーバーソリューションです。

## 共通ドキュメント

- [インストール手順](common/installation.md)
- [ACL設定](common/acl-configuration.md)
- [ロギング設定](common/logging.md)
- [セキュリティベストプラクティス](common/security-best-practices.md)

## サーバー別マニュアル

### HTTP/HTTPSサーバー
- [概要](http/README.md)
- [クイックスタート](http/getting-started.md) ⭐
- [詳細設定](http/configuration.md)
- [Virtual Host設定](http/virtual-hosts.md)
- [SSL/TLS設定](http/ssl-tls.md)
- [CGI/SSI設定](http/cgi-ssi.md)
- [WebDAV設定](http/webdav.md)
- [トラブルシューティング](http/troubleshooting.md)

### FTPサーバー
- [概要](ftp/README.md)
- [クイックスタート](ftp/getting-started.md) ⭐
- [ユーザー管理](ftp/user-management.md)
- [FTPS設定](ftp/ftps.md)
- [トラブルシューティング](ftp/troubleshooting.md)

### DNSサーバー
- [概要](dns/README.md)
- [クイックスタート](dns/getting-started.md) ⭐
- [ゾーン設定](dns/zone-configuration.md)
- [リソースレコード](dns/resource-records.md)
- [トラブルシューティング](dns/troubleshooting.md)

### SMTPサーバー
- [概要](smtp/README.md)
- [クイックスタート](smtp/getting-started.md) ⭐
- [リレー設定](smtp/relay-configuration.md)
- [認証設定](smtp/authentication.md)
- [トラブルシューティング](smtp/troubleshooting.md)

### POP3サーバー
- [概要](pop3/README.md)
- [クイックスタート](pop3/getting-started.md) ⭐
- [認証設定](pop3/authentication.md)
- [トラブルシューティング](pop3/troubleshooting.md)

### DHCPサーバー
- [概要](dhcp/README.md)
- [クイックスタート](dhcp/getting-started.md) ⭐
- [IPプール設定](dhcp/ip-pool-configuration.md)
- [MAC ACL設定](dhcp/mac-acl.md)
- [トラブルシューティング](dhcp/troubleshooting.md)

### TFTPサーバー
- [概要](tftp/README.md)
- [クイックスタート](tftp/getting-started.md) ⭐
- [アクセス制御](tftp/access-control.md)
- [トラブルシューティング](tftp/troubleshooting.md)

### Proxyサーバー
- [概要](proxy/README.md)
- [クイックスタート](proxy/getting-started.md) ⭐
- [キャッシュ設定](proxy/cache-configuration.md)
- [URLフィルタリング](proxy/url-filtering.md)
- [トラブルシューティング](proxy/troubleshooting.md)

## リファレンス

- [技術ドキュメント](../docs/)
- [開発ガイド](../docs/01_development_docs/02_development_setup.md)
