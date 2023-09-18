using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace TestTask.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BlobStorageService> _logger;
    private readonly string _blobConnectionString = String.Empty;
    private readonly string _blobContainerName = String.Empty;
    
    public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _blobConnectionString = _configuration.GetConnectionString("BLOB") ?? throw new InvalidOperationException("Connection string 'BLOB' not found.");
        _blobContainerName = _configuration.GetConnectionString("Container") ?? throw new InvalidOperationException("Connection string 'Container' not found.");
    }
    
    public async Task<string> UploadFileToBlobAsync(string fileName, string contentType, Stream fileStream)
    {
        try
        {
            var container = new BlobContainerClient(_blobConnectionString, _blobContainerName);
            var createResponse = await container.CreateIfNotExistsAsync();
            if (createResponse != null && createResponse.GetRawResponse().Status == 201)
                await container.SetAccessPolicyAsync(PublicAccessType.Blob);
            var blob = container.GetBlobClient(fileName);
            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            await blob.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });
            var urlString = blob.Uri.ToString();
            return urlString;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex.ToString());
            throw;
        }
    }
}