namespace TestTask.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadFileToBlobAsync(string strFileName, string contentType, Stream fileStream);
}