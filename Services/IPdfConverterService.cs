using MyApps.Models;

namespace MyApps.Services
{
    /// <summary>
    /// Interface for PDF Converter Service
    /// </summary>
    public interface IPdfConverterService
    {
        /// <summary>
        /// Convert text to PDF
        /// </summary>
        byte[] ConvertTextToPdf(string content, PdfOptions options);

        /// <summary>
        /// Convert HTML to PDF
        /// </summary>
        byte[] ConvertHtmlToPdf(string htmlContent, PdfOptions options);

        /// <summary>
        /// Convert Markdown to PDF
        /// </summary>
        byte[] ConvertMarkdownToPdf(string markdownContent, PdfOptions options);

        /// <summary>
        /// Convert table data to PDF
        /// </summary>
        byte[] ConvertTableToPdf(TableDataModel tableData, PdfOptions options);

        /// <summary>
        /// Generate sample PDF for testing
        /// </summary>
        byte[] GenerateSamplePdf();

        /// <summary>
        /// Convert images to PDF
        /// </summary>
        byte[] ConvertImagesToPdf(List<IFormFile> images, PdfOptions options, string imageFit = "FitPage", bool oneImagePerPage = true);

        /// <summary>
        /// Convert mixed content (text + images) to PDF
        /// </summary>
        byte[] ConvertMixedToPdf(string content, List<IFormFile> images, PdfOptions options, string imageFit = "FitPage", bool oneImagePerPage = true);
    }
}
