using System;
using System.IO;
using System.Net;
using RestSharp;


namespace CARA_example
{
    class Program
    {
        static void Main(string[] args)
        {
            const string resource = "docx/to/png";
            const string fileToConvert = @"C:\Projects\CARA\TestFiles\BatchTest\test-docx.docx";
            const string folderToSave = @"C:\Projects\_temp";
            const string secret = "s9245r7a90123a53";

            var client = new RestClient("https://v2.convertapi.com")
            {
                Timeout = (int)TimeSpan.FromMinutes(10).TotalMilliseconds
            };
            client.AddHandler("multipart/mixed", new MultipartDeserializer());            
            var request = new RestRequest(resource, Method.POST);
            request.AddQueryParameter("secret", secret);
            request.AddHeader("Accept", "multipart/mixed");
            request.AddParameter("TimeOut", 30);            
            request.AddFile("file", fileToConvert);
            var response = client.Execute<ConvertApiResponse>(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                foreach (var convertedFile in response.Data.Files)
                {
                    var fileToSave = Path.Combine(folderToSave, convertedFile.Name);
                    File.WriteAllBytes(fileToSave, convertedFile.Data);
                    Console.WriteLine($"File saved to {fileToSave}");
                }
            }
            else
            {
                Console.WriteLine($"Web exception with HTTP status code {(int)response.StatusCode} and message: {response.StatusCode}");
                Console.WriteLine($"API status code {response.Data.ApiError.StatusCode} and message: {response.Data.ApiError.ReasonPhrase}. ");
                foreach (var keyValuePair in response.Data.ApiError.ParametersError)
                {
                    Console.WriteLine($"Parameter error: {keyValuePair.Key}: {keyValuePair.Value}. ");
                }
            }

            Console.ReadLine();
        }
    }
}
