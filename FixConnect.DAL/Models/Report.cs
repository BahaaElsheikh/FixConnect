using System;

namespace FixConnect.DAL.Models
{
    public class Report
    {
        public int ReportId { get; set; }

        // الشخص المشتكي (المستخدم الحالي اللي جيبناه من الـ Claims)
        public int ReporterId { get; set; }

        // الأطراف المعنية بالشكوى (كلها Nullable حسب السياق)
        public int? CustomerId { get; set; }
        public int? WorkerId { get; set; }

        // الكاتيجوريز (الـ IDs بتاعت السياق)
        public int? RequestId { get; set; }
        public int? ProposalId { get; set; }
        public int? JobId { get; set; }
        public int? ReviewId { get; set; }

        // تفاصيل الشكوى والتوقيت
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsResolved { get; set; } = false;

        // Navigation Properties (الاختيارية للربط مع جدول المستخدمين الأساسي)
        public User Reporter { get; set; } = null!;
        public Customer? Customer { get; set; }
        public Worker? Worker { get; set; }
        public Request? Request { get; set; }
        public Proposal? Proposal { get; set; }
        public Job? Job { get; set; }
        public Review? Review { get; set; }
    }
}