using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROG6212POE.Models
{
    public class Claims
    {
        //columns for claims
        [Key]
        public int ClaimId { get; set; }
        [Required]
        public string? LecturerId { get; set; }
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }
        [Required]
        public DateOnly ClaimsPeriodStart { get; set; }
        [Required]
        public DateOnly ClaimsPeriodEnd { get; set; }
        [Required]
        public double HoursWorked { get; set; }
        [Required]
        public double RatePerHour { get; set; }
        [Required]
        public double TotalAmount { get; set; }
        [Required]
        public string? DescriptionOfWork { get; set; }

        //setting status to pending from the start since that is how all claims begin
        public string? Status { get; set; } = "Pending";

        //not mapped means it does not write itself to the database as a column
        [NotMapped]
        public List<IFormFile>? SupportingDocuments { get; set; }

        //how the application stores multiple files, seperates them with a ";" delimiter
        public string? SupportingDocumentsPaths { get; set; }
    }
}
