using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            var result = new ConvertApiResponse();
            var streamContent = new StreamContent(GenerateStreamFromString(response.Content));
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(response.ContentType);
            var provider = streamContent.ReadAsMultipartAsync().Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                result.Files = new List<ConvertedFiles>(); 
                foreach (var content in provider.Contents)
                {
                    var file = new ConvertedFiles
                    {
                        Name = Helper.GetFileName(content.Headers.ContentDisposition),
                        Size = (int) content.Headers.ContentDisposition.Size.GetValueOrDefault(0),
                        Data = content.Headers.ContentLocation == null ? content.ReadAsByteArrayAsync().Result : null,
                        Url = content.Headers.ContentLocation
                    };
                    result.Files.Add(file);
                }   
            }
            else
            {
                result.ApiError = new ConvertApiResponse.ApiErrorData();
                foreach (var content in provider.Contents)
                {
                    IEnumerable<string> conversionCode;

                    var conversionCodeExist = content.Headers.TryGetValues("Code", out conversionCode);
                    if (conversionCodeExist)
                    {
                        result.ApiError.StatusCode = int.Parse(conversionCode.First());
                        result.ApiError.ReasonPhrase = content.ReadAsStringAsync().Result;
                    }

                    IEnumerable<string> parameterValidation;
                    var parameterValidationExist = content.Headers.TryGetValues("Parameter", out parameterValidation);

                    if (parameterValidationExist)
                        result.ApiError.ParametersError.Add(new KeyValuePair<string, string>(parameterValidation.First(),
                            content.ReadAsStringAsync().Result));
                }
            }
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