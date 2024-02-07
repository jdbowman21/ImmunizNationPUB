using MimeKit;

namespace ImmunizNation.Client.Models
{
    public class EmailAttachment
    {
        public string Name { get; set; }
        public byte[] ByteArray { get; set; }
        public ContentType ContentType { get; set; }
    }
}
