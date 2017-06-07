using System;
using System.Collections.Generic;

namespace CARA_example
{
    public class ConvertApiResponse
    {
        public class ApiErrorData
        {
            public ApiErrorData()
            {
                ParametersError = new List<KeyValuePair<string, string>>();
            }

            public int? StatusCode;
            public List<KeyValuePair<string, string>> ParametersError;
            public string ReasonPhrase;
        }
        
        public List<ConvertedFiles> Files { get; set; }
        public ApiErrorData ApiError { get; set; }
    }

    public class ConvertedFiles
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public byte[] Data { get; set; }
        public Uri Url { get; set; }
    }
}