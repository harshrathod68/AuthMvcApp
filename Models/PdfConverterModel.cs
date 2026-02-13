using System.ComponentModel.DataAnnotations;

namespace MyApps.Models
{
    /// <summary>
    /// Model for PDF conversion request
    /// </summary>
    public class PdfConversionModel
    {
        [Required(ErrorMessage = "Content is required")]
        [Display(Name = "Content")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Conversion type is required")]
        [Display(Name = "Conversion Type")]
        public string ConversionType { get; set; } = "Text"; // Text, HTML, Markdown, Table

        [Display(Name = "Document Title")]
        [StringLength(200)]
        public string? Title { get; set; }

        [Display(Name = "Page Size")]
        public string PageSize { get; set; } = "A4"; // A4, Letter, Legal

        [Display(Name = "Include Header")]
        public bool IncludeHeader { get; set; } = true;

        [Display(Name = "Include Footer")]
        public bool IncludeFooter { get; set; } = true;

        [Display(Name = "Include Page Numbers")]
        public bool IncludePageNumbers { get; set; } = true;

        [Display(Name = "Font Size")]
        [Range(8, 24)]
        public int FontSize { get; set; } = 12;

        [Display(Name = "Author Name")]
        [StringLength(100)]
        public string? Author { get; set; }
    }

    /// <summary>
    /// Model for table data conversion
    /// </summary>
    public class TableDataModel
    {
        public List<string> Headers { get; set; } = new List<string>();
        public List<List<string>> Rows { get; set; } = new List<List<string>>();
    }

    /// <summary>
    /// PDF generation options
    /// </summary>
    public class PdfOptions
    {
        public string Title { get; set; } = "Document";
        public string Author { get; set; } = "MyApps User";
        public string PageSize { get; set; } = "A4";
        public bool IncludeHeader { get; set; } = true;
        public bool IncludeFooter { get; set; } = true;
        public bool IncludePageNumbers { get; set; } = true;
        public int FontSize { get; set; } = 12;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
