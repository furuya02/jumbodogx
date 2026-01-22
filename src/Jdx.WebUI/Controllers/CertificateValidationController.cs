using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc;

namespace Jdx.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CertificateValidationController : ControllerBase
{
    private readonly ILogger<CertificateValidationController> _logger;

    public CertificateValidationController(ILogger<CertificateValidationController> logger)
    {
        _logger = logger;
    }

    [HttpPost("validate")]
    public IActionResult ValidateCertificate([FromBody] CertificateValidationRequest request)
    {
        try
        {
            // 1. 証明書ファイルが設定されているかチェック
            if (string.IsNullOrWhiteSpace(request.CertificateFile))
            {
                return Ok(new CertificateValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "証明書ファイルが設定されていません。"
                });
            }

            // 2. ファイルが存在するかチェック
            if (!System.IO.File.Exists(request.CertificateFile))
            {
                return Ok(new CertificateValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = $"証明書ファイルが見つかりません: {request.CertificateFile}"
                });
            }

            // 3. 証明書の読み込みとパスワード検証
            X509Certificate2? certificate = null;
            try
            {
                certificate = X509CertificateLoader.LoadPkcs12FromFile(
                    request.CertificateFile,
                    request.CertificatePassword);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load certificate from {File}", request.CertificateFile);

                // パスワードエラーか、証明書形式エラーかを判定
                if (ex.Message.Contains("password") || ex.Message.Contains("invalid password"))
                {
                    return Ok(new CertificateValidationResponse
                    {
                        IsValid = false,
                        ErrorMessage = "証明書のパスワードが正しくありません。"
                    });
                }

                return Ok(new CertificateValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = $"証明書の読み込みに失敗しました: {ex.Message}"
                });
            }

            // 4. 証明書の有効期限チェック
            if (certificate.NotAfter < DateTime.Now)
            {
                return Ok(new CertificateValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = $"証明書の有効期限が切れています (有効期限: {certificate.NotAfter:yyyy/MM/dd})"
                });
            }

            if (certificate.NotBefore > DateTime.Now)
            {
                return Ok(new CertificateValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = $"証明書はまだ有効ではありません (有効開始日: {certificate.NotBefore:yyyy/MM/dd})"
                });
            }

            // すべての検証に成功
            return Ok(new CertificateValidationResponse
            {
                IsValid = true,
                ErrorMessage = null,
                CertificateInfo = new CertificateInfo
                {
                    Subject = certificate.Subject,
                    Issuer = certificate.Issuer,
                    NotBefore = certificate.NotBefore,
                    NotAfter = certificate.NotAfter
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during certificate validation");
            return Ok(new CertificateValidationResponse
            {
                IsValid = false,
                ErrorMessage = $"証明書の検証中に予期しないエラーが発生しました: {ex.Message}"
            });
        }
    }
}

public class CertificateValidationRequest
{
    public string CertificateFile { get; set; } = "";
    public string? CertificatePassword { get; set; }
}

public class CertificateValidationResponse
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public CertificateInfo? CertificateInfo { get; set; }
}

public class CertificateInfo
{
    public string Subject { get; set; } = "";
    public string Issuer { get; set; } = "";
    public DateTime NotBefore { get; set; }
    public DateTime NotAfter { get; set; }
}
