namespace PROG6212POE.ViewModel
{
    public class InvoiceViewModel
    {
        public string LecturerId { get; set; }
        public string LecturerName { get; set; }
        public string LecturerEmail { get; set; }
        public int ClaimId { get; set; }
        public string ClaimPeriod { get; set; }
        public double TotalAmount { get; set; }
        public double TaxRate { get; set; }
        public double TaxAmount { get; set; }
        public double SetAmount { get; set; }
        public double TotalWithDeduction { get; set; }
    }
}
