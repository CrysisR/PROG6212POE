using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;

namespace PROG6212POE.Models
{
    public class Email
    {
        [NotMapped]
        public string FromEmail { get; set; }
        [NotMapped]
        public string ToEmail {  get; set; }
        [NotMapped]
        public string Subject { get; set; }
        [NotMapped]
        public string Body { get; set; }
        [NotMapped]
        public Attachment Attachment {  get; set; }
    }
}
