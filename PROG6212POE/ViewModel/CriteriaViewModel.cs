using System.ComponentModel.DataAnnotations;

namespace PROG6212POE.ViewModel
{
    public class CriteriaViewModel
    {
        [Required]
        [Display(Name = "Allowed Start Date")]
        public DateOnly AllowedStartDate { get; set; }

        [Required]
        [Display(Name = "Allowed End Date")]
        public DateOnly AllowedEndDate { get; set; }

        [Required]
        [Display(Name = "Maximum Total Amount")]
        public double TotalAmount { get; set; }

        [Display(Name = "Require Description?")]
        public bool RequireDescription { get; set; }

        [Display(Name = "Minimum Number of Documents")]
        public int MinimumDocuments { get; set; }
    }
}
