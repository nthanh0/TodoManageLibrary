using System.Collections.Generic;

namespace ManageLibrary.Models
{
    // ViewModel chính chứa tất cả dữ liệu cho trang Report
    public class ReportViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalReaders { get; set; }
        public int CurrentLoans { get; set; }
        public int OverdueLoans { get; set; }
        public List<BookStatsViewModel> TopLoanedBooks { get; set; }
        public List<CategoryStatsViewModel> BooksByCategory { get; set; }
    }

    // ViewModel cho thống kê Top sách
    public class BookStatsViewModel
    {
        public string BookName { get; set; }
        public int LoanCount { get; set; }
    }

    // ViewModel cho thống kê Thể loại
    public class CategoryStatsViewModel
    {
        public string CategoryName { get; set; }
        public int BookCount { get; set; }
    }
}