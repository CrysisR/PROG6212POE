using System.ComponentModel.DataAnnotations;

namespace PROG6212POE.Models
{
    public class File
    {
        [Key]
        public int FileId { get; set; }
        public string? FileName { get; set; }
        public long? FileLength { get; set; }
        public byte[]? FileData { get; set; }
        public DateTime DateUploaded { get; set; }

        //relation to product model
        public int? ClaimId { get; set; }
        public Claims? Claim { get; set; }
    }
}
