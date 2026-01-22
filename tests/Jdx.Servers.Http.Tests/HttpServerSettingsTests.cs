using Jdx.Core.Settings;
using Xunit;

namespace Jdx.Servers.Http.Tests;

/// <summary>
/// HttpServerSettingsのユニットテスト
/// </summary>
public class HttpServerSettingsTests
{
    #region Default Values Tests

    [Fact]
    public void DefaultSettings_HaveCorrectBasicValues()
    {
        // Act
        var settings = new HttpServerSettings();

        // Assert
        Assert.False(settings.Enabled);
        Assert.Equal("HTTP", settings.Protocol);
        Assert.Equal("0.0.0.0", settings.BindAddress);
        Assert.Equal(3, settings.TimeOut);
        Assert.False(settings.UseResolve);
    }

    [Fact]
    public void DefaultSettings_HaveCorrectDocumentValues()
    {
        // Act
        var settings = new HttpServerSettings();

        // Assert
        Assert.Equal("", settings.DocumentRoot);
        Assert.Equal("index.html", settings.WelcomeFileName);
        Assert.False(settings.UseHidden);
        Assert.False(settings.UseDot);
        Assert.False(settings.UseDirectoryEnum);
        Assert.False(settings.UseExpansion);
        Assert.Equal("JumboDogX Version $v", settings.ServerHeader);
        Assert.False(settings.UseEtag);
        Assert.Equal("", settings.ServerAdmin);
    }

    [Fact]
    public void DefaultSettings_HaveCorrectCgiValues()
    {
        // Act
        var settings = new HttpServerSettings();

        // Assert
        Assert.False(settings.UseCgi);
        Assert.Empty(settings.CgiCommands);
        Assert.Equal(10, settings.CgiTimeout);
        Assert.Empty(settings.CgiPaths);
    }

    [Fact]
    public void DefaultSettings_HaveCorrectSsiValues()
    {
        // Act
        var settings = new HttpServerSettings();

        // Assert
        Assert.False(settings.UseSsi);
        Assert.Equal("html,htm", settings.SsiExt);
        Assert.False(settings.UseExec);
    }

    [Fact]
    public void DefaultSettings_HaveCorrectWebDavValues()
    {
        // Act
        var settings = new HttpServerSettings();

        // Assert
        Assert.False(settings.UseWebDav);
        Assert.Empty(settings.WebDavPaths);
    }

    [Fact]
    public void DefaultSettings_HaveEmptyLists()
    {
        // Act
        var settings = new HttpServerSettings();

        // Assert
        Assert.Empty(settings.Aliases);
        Assert.Empty(settings.MimeTypes);
        Assert.Empty(settings.AuthList);
        Assert.Empty(settings.UserList);
        Assert.Empty(settings.GroupList);
    }

    [Fact]
    public void DefaultSettings_HaveCorrectTemplateValues()
    {
        // Act
        var settings = new HttpServerSettings();

        // Assert
        Assert.Equal("UTF-8", settings.Encode);
        Assert.Equal("", settings.IndexDocument);
        Assert.Equal("", settings.ErrorDocument);
    }

    [Fact]
    public void DefaultSettings_HaveCorrectAclValues()
    {
        // Act
        var settings = new HttpServerSettings();

        // Assert
        Assert.Equal(0, settings.EnableAcl);
        Assert.Empty(settings.AclList);
    }

    [Fact]
    public void DefaultSettings_HaveCorrectAdvancedValues()
    {
        // Act
        var settings = new HttpServerSettings();

        // Assert
        Assert.False(settings.UseAutoAcl);
        Assert.False(settings.AutoAclApacheKiller);
        Assert.True(settings.UseKeepAlive);
        Assert.Equal(5, settings.KeepAliveTimeout);
        Assert.Equal(100, settings.MaxKeepAliveRequests);
        Assert.True(settings.UseRangeRequests);
        Assert.Equal(20, settings.MaxRangeCount);
    }

    [Fact]
    public void DefaultSettings_HaveCorrectSslValues()
    {
        // Act
        var settings = new HttpServerSettings();

        // Assert
        Assert.Equal("", settings.CertificateFile);
        Assert.Equal("", settings.CertificatePassword);
    }

    [Fact]
    public void DefaultSettings_HaveEmptyVirtualHosts()
    {
        // Act
        var settings = new HttpServerSettings();

        // Assert
        Assert.Empty(settings.VirtualHosts);
    }

    #endregion

    #region Property Setter Tests

    [Theory]
    [InlineData(80)]
    [InlineData(443)]
    [InlineData(8080)]
    [InlineData(8443)]
    public void Port_CanBeSet(int port)
    {
        // Arrange
        var settings = new HttpServerSettings();

        // Act
        settings.Port = port;

        // Assert
        Assert.Equal(port, settings.Port);
    }

    [Theory]
    [InlineData("HTTP")]
    [InlineData("HTTPS")]
    public void Protocol_CanBeSet(string protocol)
    {
        // Arrange
        var settings = new HttpServerSettings();

        // Act
        settings.Protocol = protocol;

        // Assert
        Assert.Equal(protocol, settings.Protocol);
    }

    [Theory]
    [InlineData("/var/www/html")]
    [InlineData("C:\\inetpub\\wwwroot")]
    [InlineData("./www")]
    public void DocumentRoot_CanBeSet(string path)
    {
        // Arrange
        var settings = new HttpServerSettings();

        // Act
        settings.DocumentRoot = path;

        // Assert
        Assert.Equal(path, settings.DocumentRoot);
    }

    [Theory]
    [InlineData("index.html")]
    [InlineData("index.htm")]
    [InlineData("default.html")]
    public void WelcomeFileName_CanBeSet(string fileName)
    {
        // Arrange
        var settings = new HttpServerSettings();

        // Act
        settings.WelcomeFileName = fileName;

        // Assert
        Assert.Equal(fileName, settings.WelcomeFileName);
    }

    [Fact]
    public void CgiFlags_CanBeSet()
    {
        // Arrange
        var settings = new HttpServerSettings();

        // Act
        settings.UseCgi = true;
        settings.CgiTimeout = 30;

        // Assert
        Assert.True(settings.UseCgi);
        Assert.Equal(30, settings.CgiTimeout);
    }

    [Fact]
    public void SsiFlags_CanBeSet()
    {
        // Arrange
        var settings = new HttpServerSettings();

        // Act
        settings.UseSsi = true;
        settings.SsiExt = "shtml,shtm";
        settings.UseExec = true;

        // Assert
        Assert.True(settings.UseSsi);
        Assert.Equal("shtml,shtm", settings.SsiExt);
        Assert.True(settings.UseExec);
    }

    [Fact]
    public void WebDavFlags_CanBeSet()
    {
        // Arrange
        var settings = new HttpServerSettings();

        // Act
        settings.UseWebDav = true;

        // Assert
        Assert.True(settings.UseWebDav);
    }

    [Fact]
    public void KeepAliveSettings_CanBeSet()
    {
        // Arrange
        var settings = new HttpServerSettings();

        // Act
        settings.UseKeepAlive = false;
        settings.KeepAliveTimeout = 10;
        settings.MaxKeepAliveRequests = 200;

        // Assert
        Assert.False(settings.UseKeepAlive);
        Assert.Equal(10, settings.KeepAliveTimeout);
        Assert.Equal(200, settings.MaxKeepAliveRequests);
    }

    [Fact]
    public void RangeRequestSettings_CanBeSet()
    {
        // Arrange
        var settings = new HttpServerSettings();

        // Act
        settings.UseRangeRequests = false;
        settings.MaxRangeCount = 50;

        // Assert
        Assert.False(settings.UseRangeRequests);
        Assert.Equal(50, settings.MaxRangeCount);
    }

    [Fact]
    public void SslSettings_CanBeSet()
    {
        // Arrange
        var settings = new HttpServerSettings();

        // Act
        settings.CertificateFile = "/path/to/cert.pfx";
        settings.CertificatePassword = "secret";

        // Assert
        Assert.Equal("/path/to/cert.pfx", settings.CertificateFile);
        Assert.Equal("secret", settings.CertificatePassword);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void EnableAcl_CanBeSet(int aclMode)
    {
        // Arrange
        var settings = new HttpServerSettings();

        // Act
        settings.EnableAcl = aclMode;

        // Assert
        Assert.Equal(aclMode, settings.EnableAcl);
    }

    [Fact]
    public void AutoAclSettings_CanBeSet()
    {
        // Arrange
        var settings = new HttpServerSettings();

        // Act
        settings.UseAutoAcl = true;
        settings.AutoAclApacheKiller = true;

        // Assert
        Assert.True(settings.UseAutoAcl);
        Assert.True(settings.AutoAclApacheKiller);
    }

    #endregion

    #region List Modification Tests

    [Fact]
    public void CgiCommands_CanBeModified()
    {
        // Arrange
        var settings = new HttpServerSettings();
        var entry = new CgiCommandEntry { Extension = ".cgi", Program = "/usr/bin/perl" };

        // Act
        settings.CgiCommands.Add(entry);

        // Assert
        Assert.Single(settings.CgiCommands);
        Assert.Equal(".cgi", settings.CgiCommands[0].Extension);
        Assert.Equal("/usr/bin/perl", settings.CgiCommands[0].Program);
    }

    [Fact]
    public void CgiPaths_CanBeModified()
    {
        // Arrange
        var settings = new HttpServerSettings();
        var entry = new CgiPathEntry { Path = "/cgi-bin/", Directory = "/var/www/cgi-bin" };

        // Act
        settings.CgiPaths.Add(entry);

        // Assert
        Assert.Single(settings.CgiPaths);
        Assert.Equal("/cgi-bin/", settings.CgiPaths[0].Path);
    }

    [Fact]
    public void WebDavPaths_CanBeModified()
    {
        // Arrange
        var settings = new HttpServerSettings();
        var entry = new WebDavPathEntry { Path = "/dav/", AllowWrite = true, Directory = "/var/webdav" };

        // Act
        settings.WebDavPaths.Add(entry);

        // Assert
        Assert.Single(settings.WebDavPaths);
        Assert.Equal("/dav/", settings.WebDavPaths[0].Path);
        Assert.True(settings.WebDavPaths[0].AllowWrite);
    }

    [Fact]
    public void Aliases_CanBeModified()
    {
        // Arrange
        var settings = new HttpServerSettings();
        var entry = new AliasEntry { Name = "/images", Directory = "/var/www/images" };

        // Act
        settings.Aliases.Add(entry);

        // Assert
        Assert.Single(settings.Aliases);
        Assert.Equal("/images", settings.Aliases[0].Name);
    }

    [Fact]
    public void MimeTypes_CanBeModified()
    {
        // Arrange
        var settings = new HttpServerSettings();
        var entry = new MimeEntry { Extension = ".json", MimeType = "application/json" };

        // Act
        settings.MimeTypes.Add(entry);

        // Assert
        Assert.Single(settings.MimeTypes);
        Assert.Equal(".json", settings.MimeTypes[0].Extension);
        Assert.Equal("application/json", settings.MimeTypes[0].MimeType);
    }

    [Fact]
    public void AuthList_CanBeModified()
    {
        // Arrange
        var settings = new HttpServerSettings();
        var entry = new AuthEntry { Directory = "/admin", AuthName = "Admin Area", Require = "valid-user" };

        // Act
        settings.AuthList.Add(entry);

        // Assert
        Assert.Single(settings.AuthList);
        Assert.Equal("/admin", settings.AuthList[0].Directory);
        Assert.Equal("Admin Area", settings.AuthList[0].AuthName);
    }

    [Fact]
    public void UserList_CanBeModified()
    {
        // Arrange
        var settings = new HttpServerSettings();
        var entry = new UserEntry { UserName = "admin", Password = "hashed_password" };

        // Act
        settings.UserList.Add(entry);

        // Assert
        Assert.Single(settings.UserList);
        Assert.Equal("admin", settings.UserList[0].UserName);
    }

    [Fact]
    public void GroupList_CanBeModified()
    {
        // Arrange
        var settings = new HttpServerSettings();
        var entry = new GroupEntry { GroupName = "admins", UserName = "admin" };

        // Act
        settings.GroupList.Add(entry);

        // Assert
        Assert.Single(settings.GroupList);
        Assert.Equal("admins", settings.GroupList[0].GroupName);
    }

    [Fact]
    public void AclList_CanBeModified()
    {
        // Arrange
        var settings = new HttpServerSettings();
        var entry = new AclEntry { Name = "Local Network", Address = "192.168.1.0/24" };

        // Act
        settings.AclList.Add(entry);

        // Assert
        Assert.Single(settings.AclList);
        Assert.Equal("Local Network", settings.AclList[0].Name);
        Assert.Equal("192.168.1.0/24", settings.AclList[0].Address);
    }

    [Fact]
    public void VirtualHosts_CanBeModified()
    {
        // Arrange
        var settings = new HttpServerSettings();
        var entry = new VirtualHostEntry
        {
            Host = "example.com:8080",
            Enabled = true,
            BindAddress = "192.168.1.100"
        };

        // Act
        settings.VirtualHosts.Add(entry);

        // Assert
        Assert.Single(settings.VirtualHosts);
        Assert.Equal("example.com:8080", settings.VirtualHosts[0].Host);
        Assert.True(settings.VirtualHosts[0].Enabled);
    }

    #endregion
}

/// <summary>
/// CgiCommandEntryのユニットテスト
/// </summary>
public class CgiCommandEntryTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Act
        var entry = new CgiCommandEntry();

        // Assert
        Assert.Equal("", entry.Extension);
        Assert.Equal("", entry.Program);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var entry = new CgiCommandEntry();

        // Act
        entry.Extension = ".pl";
        entry.Program = "/usr/bin/perl";

        // Assert
        Assert.Equal(".pl", entry.Extension);
        Assert.Equal("/usr/bin/perl", entry.Program);
    }
}

/// <summary>
/// CgiPathEntryのユニットテスト
/// </summary>
public class CgiPathEntryTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Act
        var entry = new CgiPathEntry();

        // Assert
        Assert.Equal("", entry.Path);
        Assert.Equal("", entry.Directory);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var entry = new CgiPathEntry();

        // Act
        entry.Path = "/cgi-bin/";
        entry.Directory = "/var/www/cgi-bin";

        // Assert
        Assert.Equal("/cgi-bin/", entry.Path);
        Assert.Equal("/var/www/cgi-bin", entry.Directory);
    }
}

/// <summary>
/// WebDavPathEntryのユニットテスト
/// </summary>
public class WebDavPathEntryTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Act
        var entry = new WebDavPathEntry();

        // Assert
        Assert.Equal("", entry.Path);
        Assert.False(entry.AllowWrite);
        Assert.Equal("", entry.Directory);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var entry = new WebDavPathEntry();

        // Act
        entry.Path = "/webdav/";
        entry.AllowWrite = true;
        entry.Directory = "/var/webdav";

        // Assert
        Assert.Equal("/webdav/", entry.Path);
        Assert.True(entry.AllowWrite);
        Assert.Equal("/var/webdav", entry.Directory);
    }
}

/// <summary>
/// AliasEntryのユニットテスト
/// </summary>
public class AliasEntryTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Act
        var entry = new AliasEntry();

        // Assert
        Assert.Equal("", entry.Name);
        Assert.Equal("", entry.Directory);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var entry = new AliasEntry();

        // Act
        entry.Name = "/icons";
        entry.Directory = "/usr/share/icons";

        // Assert
        Assert.Equal("/icons", entry.Name);
        Assert.Equal("/usr/share/icons", entry.Directory);
    }
}

/// <summary>
/// MimeEntryのユニットテスト
/// </summary>
public class MimeEntryTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Act
        var entry = new MimeEntry();

        // Assert
        Assert.Equal("", entry.Extension);
        Assert.Equal("", entry.MimeType);
    }

    [Theory]
    [InlineData(".html", "text/html")]
    [InlineData(".css", "text/css")]
    [InlineData(".js", "application/javascript")]
    [InlineData(".json", "application/json")]
    [InlineData(".png", "image/png")]
    public void Properties_CanBeSetWithCommonMimeTypes(string extension, string mimeType)
    {
        // Arrange
        var entry = new MimeEntry();

        // Act
        entry.Extension = extension;
        entry.MimeType = mimeType;

        // Assert
        Assert.Equal(extension, entry.Extension);
        Assert.Equal(mimeType, entry.MimeType);
    }
}

/// <summary>
/// AuthEntryのユニットテスト
/// </summary>
public class AuthEntryTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Act
        var entry = new AuthEntry();

        // Assert
        Assert.Equal("", entry.Directory);
        Assert.Equal("", entry.AuthName);
        Assert.Equal("", entry.Require);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var entry = new AuthEntry();

        // Act
        entry.Directory = "/secure";
        entry.AuthName = "Secure Area";
        entry.Require = "valid-user";

        // Assert
        Assert.Equal("/secure", entry.Directory);
        Assert.Equal("Secure Area", entry.AuthName);
        Assert.Equal("valid-user", entry.Require);
    }
}

/// <summary>
/// UserEntryのユニットテスト
/// </summary>
public class UserEntryTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Act
        var entry = new UserEntry();

        // Assert
        Assert.Equal("", entry.UserName);
        Assert.Equal("", entry.Password);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var entry = new UserEntry();

        // Act
        entry.UserName = "testuser";
        entry.Password = "hashedpassword";

        // Assert
        Assert.Equal("testuser", entry.UserName);
        Assert.Equal("hashedpassword", entry.Password);
    }
}

/// <summary>
/// GroupEntryのユニットテスト
/// </summary>
public class GroupEntryTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Act
        var entry = new GroupEntry();

        // Assert
        Assert.Equal("", entry.GroupName);
        Assert.Equal("", entry.UserName);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var entry = new GroupEntry();

        // Act
        entry.GroupName = "editors";
        entry.UserName = "john";

        // Assert
        Assert.Equal("editors", entry.GroupName);
        Assert.Equal("john", entry.UserName);
    }
}

/// <summary>
/// AclEntryのユニットテスト
/// </summary>
public class AclEntryTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Act
        var entry = new AclEntry();

        // Assert
        Assert.Equal("", entry.Name);
        Assert.Equal("", entry.Address);
    }

    [Theory]
    [InlineData("Single IP", "192.168.1.1")]
    [InlineData("CIDR /24", "192.168.1.0/24")]
    [InlineData("CIDR /8", "10.0.0.0/8")]
    [InlineData("Localhost", "127.0.0.1")]
    public void Properties_CanBeSetWithVariousAddressFormats(string name, string address)
    {
        // Arrange
        var entry = new AclEntry();

        // Act
        entry.Name = name;
        entry.Address = address;

        // Assert
        Assert.Equal(name, entry.Name);
        Assert.Equal(address, entry.Address);
    }
}

/// <summary>
/// VirtualHostEntryのユニットテスト
/// </summary>
public class VirtualHostEntryTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Act
        var entry = new VirtualHostEntry();

        // Assert
        Assert.Equal("", entry.Host);
        Assert.True(entry.Enabled);
        Assert.Null(entry.BindAddress);
        Assert.NotNull(entry.Settings);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var entry = new VirtualHostEntry();

        // Act
        entry.Host = "www.example.com:8080";
        entry.Enabled = false;
        entry.BindAddress = "192.168.1.100";

        // Assert
        Assert.Equal("www.example.com:8080", entry.Host);
        Assert.False(entry.Enabled);
        Assert.Equal("192.168.1.100", entry.BindAddress);
    }

    [Fact]
    public void GetHostName_WithPort_ReturnsHostNameOnly()
    {
        // Arrange
        var entry = new VirtualHostEntry { Host = "example.com:8080" };

        // Act
        var hostName = entry.GetHostName();

        // Assert
        Assert.Equal("example.com", hostName);
    }

    [Fact]
    public void GetHostName_WithoutPort_ReturnsFullHost()
    {
        // Arrange
        var entry = new VirtualHostEntry { Host = "example.com" };

        // Act
        var hostName = entry.GetHostName();

        // Assert
        Assert.Equal("example.com", hostName);
    }

    [Fact]
    public void GetHostName_WithEmptyHost_ReturnsEmptyString()
    {
        // Arrange
        var entry = new VirtualHostEntry { Host = "" };

        // Act
        var hostName = entry.GetHostName();

        // Assert
        Assert.Equal("", hostName);
    }

    [Fact]
    public void GetPort_WithPort_ReturnsPort()
    {
        // Arrange
        var entry = new VirtualHostEntry { Host = "example.com:8080" };

        // Act
        var port = entry.GetPort();

        // Assert
        Assert.Equal(8080, port);
    }

    [Fact]
    public void GetPort_WithoutPort_ReturnsDefault8080()
    {
        // Arrange
        var entry = new VirtualHostEntry { Host = "example.com" };

        // Act
        var port = entry.GetPort();

        // Assert
        Assert.Equal(8080, port);
    }
}
