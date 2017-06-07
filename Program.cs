using System;
using System.Collections.Generic;
using System.IO;
using RestSharp;


namespace CARA_example
{
    class Program
    {
        static void Main(string[] args)
        {
            const string folderToSave = @"C:\Projects\_temp";
            const string secret = "s9245r7a90123a53";

            var client = new RestClient("https://v2.convertapi.com")
            {
                Timeout = (int)TimeSpan.FromMinutes(10).TotalMilliseconds
            };
            client.AddHandler("multipart/mixed", new MultipartDeserializer());
            var request = new RestRequest("docx/to/png", Method.POST);            
            request.AddQueryParameter("secret", secret);
            request.AddHeader("Accept", "multipart/mixed");
            request.AddFile("file", @"C:\Projects\CARA\TestFiles\BatchTest\test-docx.docx");
            var response = client.Execute<List<ConvertedFileModel>>(request);

            foreach (var convertedFile in response.Data)
            {                
                var fileToSave = Path.Combine(folderToSave, convertedFile.Name);
                File.WriteAllBytes(fileToSave, convertedFile.Data);
                Console.WriteLine($"File saved to {fileToSave}");
            }

            Console.ReadLine();
        }
    }
}
