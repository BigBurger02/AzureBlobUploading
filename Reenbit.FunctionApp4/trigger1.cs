using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Reenbit.FunctionApp4;

public class Trigger1
{
    [FunctionName("Trigger1")]
    public async Task Run([BlobTrigger("container1/{name}")] Stream myBlob, string name, ILogger log)
    {
        log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        
        var client = new HttpClient();
        try
        {
            var content = await client.GetStringAsync("https://localhost:7243/api/Triggered/" + name);
            log.LogInformation($"Localhost server response: {content}");
        }
        catch (Exception e)
        {
            log.LogInformation($"Exception thrown while connecting to localhost: {e}. Trying to call another uri");
            try
            {
                var content = client.GetStringAsync("https://reenbitapplication.azurewebsites.net/api/Triggered/" + name);
                log.LogInformation($"Azure server response: {content}");
            }
            catch (Exception exception)
            {
                log.LogInformation($"Exception thrown while connecting to Azure: {exception}");
            }
        }
        
        log.LogInformation($"Exiting the trigger");
    }
}