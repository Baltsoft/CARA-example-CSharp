using System;

namespace CARA_example
{
    public class ConvertedFileModel
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public byte[] Data { get; set; }
        public Uri Url { get; set; }
    }
}