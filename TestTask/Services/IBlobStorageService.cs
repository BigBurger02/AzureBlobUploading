namespace TestTask.Services;

public interface IBlobStorageService
{
    Task<string> UploadFileToBlobAsync(string strFileName, string contentType, Stream fileStream);
}