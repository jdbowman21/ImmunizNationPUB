using System;

namespace ImmunizNation.Client.Models
{
    public class Resource
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public string MimeType { get; set; }
        public string FileType { get; set; }
        public string Path { get; set; }
        public string FileSize { get; set; }
        public string ThumbnailPath { get; set; }
        public int Order { get; set; }
    }
}
