using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using RestSharp;
using RestSharp.Deserializers;

namespace CARA_example
{
    public class MultipartDeserializer : IDeserializer
    {
        public static Stream GenerateStreamFromString(string s)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(s));
        }

        public T Deserialize<T>(IRestResponse response)
        {
            var streamContent = new StreamContent(GenerateStreamFromString(response.Content));
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(response.ContentType);

            var provider = streamContent.ReadAsMultipartAsync().Result;

            var result = provider.Contents.Select(content => new ConvertedFileModel
                {
                    Name = Helper.GetFileName(content.Headers.ContentDisposition),
                    Size = (int)content.Headers.ContentDisposition.Size.GetValueOrDefault(0),
                    Data = content.Headers.ContentLocation == null ? content.ReadAsByteArrayAsync().Result : null,
                    Url = content.Headers.ContentLocation
                })
                .ToList();

            return (T)(object)result;
        }

        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
    }

    public static class StringExtensions
    {
        public static string RemoveQuotes(this string value) => value.Replace("\"", "").Replace("'", "");

        public static bool HasInvalidFileNameChars(this string value) => value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0;
    }

    public static class Helper
    {
        public static string GetFileName(ContentDispositionHeaderValue cd, string defaultName = "file")
        {
            var fileName = cd?.FileName?.RemoveQuotes() ?? cd?.FileNameStar?.RemoveQuotes();
            if (string.IsNullOrEmpty(fileName)) return null;
            if (fileName.HasInvalidFileNameChars())
            {
                var extension = Path.GetExtension(fileName);
                fileName = defaultName;
                if (!string.IsNullOrEmpty(extension) && !extension.HasInvalidFileNameChars()) fileName += extension;
            }
            else
            {
                fileName = Path.GetFileName(fileName);
            }

            return fileName;
        }

    }
}