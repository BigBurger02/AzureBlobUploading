using TestTask.Model;

namespace TestTask.Data;

public class DbInitializer
{
    public static void Initialize(BlobContext context)
    {
        context.Database.EnsureCreated();

        if (context.Uploads.Any())
            return;

        var upload = new Uploads()
        {
            Id = 1,
            FileName = "Example.docx",
            Email = "example@example.com",
            Uri = "https://google.com"
        };
        context.Uploads.Add(upload);
        context.SaveChanges();
    }
}