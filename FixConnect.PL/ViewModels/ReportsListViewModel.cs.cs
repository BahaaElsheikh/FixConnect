using FixConnect.DAL.Models;


namespace FixConnect.PL.ViewModels
{
    public class ReportsListViewModel
    {
        public PaginatedList<Report> Reports { get; set; } = new();
        public string? SearchQuery { get; set; }
        public string? CategoryFilter { get; set; }
        public string SortOrder { get; set; } = "date_desc";
    }
}