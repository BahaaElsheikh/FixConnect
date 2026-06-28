namespace FixConnect.PL.ViewModels
{
    public class AdminJobsListViewModel
    {
        public PaginatedList<AdminJobRowViewModel> Jobs { get; set; } = null!;
    }

    public class AdminJobRowViewModel
    {
        public int JobId { get; set; }
        public string RequestTitle { get; set; } = null!;
        public string WorkerName { get; set; } = null!;

        public int WorkerId { get; set; } 
        public string CustomerName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal TotalInvoice { get; set; }
        public DateTime? CreatedAt { get; set; }



        // تفاصيل الـ Request للـ Modal
        public int RequestId { get; set; }
        public string RequestDescription { get; set; } = null!;
        public string RequestType { get; set; } = null!;
        public string RegionName { get; set; } = null!;

        // تفاصيل الـ Proposal للـ Modal
        public int ProposalId { get; set; }
        public decimal? LaborCost { get; set; }
        public decimal? MaterialCost { get; set; }
        public int? DurationEstimate { get; set; }

        // تفاصيل الفاتورة والعناوين للـ Modal الثالث
        public string CustomerAddress { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;
        public List<AdminInvoiceItemViewModel> InvoiceItems { get; set; } = new();
    }

    public class AdminInvoiceItemViewModel
    {
        public string Description { get; set; } = null!;
        public decimal Cost { get; set; }
    }
}