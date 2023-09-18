using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Reenbit.FunctionApp4;

public class Trigger1
{
    [FunctionName("Trigger1")]
    public void Run(
        [BlobTrigger("container1/{name}")] Stream myBlob,
        string name,
        ILogger log)
    {
        log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
    }
}