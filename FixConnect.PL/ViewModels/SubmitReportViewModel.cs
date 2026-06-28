using System.ComponentModel.DataAnnotations;

namespace FixConnect.PL.ViewModels
{
    public class SubmitReportViewModel
    {
        public int? JobId { get; set; }
        public int? RequestId { get; set; }
        public int? ProposalId { get; set; }
        public int? ReviewId { get; set; }
        public int? CustomerId { get; set; }
        public int? WorkerId { get; set; }

        public string ContextTitle { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide detailed description for the issue.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters.")]
        public string Description { get; set; } = null!;
    }
}