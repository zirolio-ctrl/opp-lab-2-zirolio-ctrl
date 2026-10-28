using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;

namespace opp_lab_2_zirolio_ctrl.Services;

public sealed class GoogleDriveService
{
    private static readonly string SecretPath = Path.Combine(AppContext.BaseDirectory, "client_secret.json");
    private static readonly string TokenDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DormitoryApp", "gdrive-token");

    private DriveService? _service;

    public static bool IsConfigured => File.Exists(SecretPath);

    private async Task<DriveService> GetServiceAsync(CancellationToken ct)
    {
        if (_service != null) return _service;

        await using var stream = File.OpenRead(SecretPath);
        var secrets = GoogleClientSecrets.FromStream(stream).Secrets;

        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            secrets,
            new[] { DriveService.Scope.DriveFile },
            "user", ct, new FileDataStore(TokenDir, true));

        return _service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "DormitoryApp"
        });
    }

    public async Task<string?> UploadAsync(string fileName, string content, string mimeType,
        CancellationToken ct = default)
    {
        var service = await GetServiceAsync(ct);
        var meta = new Google.Apis.Drive.v3.Data.File { Name = fileName };

        using var ms = new MemoryStream(new UTF8Encoding(false).GetBytes(content));
        var request = service.Files.Create(meta, ms, mimeType);
        request.Fields = "id, webViewLink";

        var result = await request.UploadAsync(ct);
        if (result.Status != UploadStatus.Completed)
            throw result.Exception ?? new Exception("Не вдалося завантажити файл на Google Drive");

        return request.ResponseBody?.WebViewLink;
    }
}