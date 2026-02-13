using MyApps.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MyApps.Services
{
    /// <summary>
    /// Service for converting various data types to PDF using QuestPDF
    /// </summary>
    public class PdfConverterService : IPdfConverterService
    {
        private readonly ILogger<PdfConverterService> _logger;

        public PdfConverterService(ILogger<PdfConverterService> logger)
        {
            _logger = logger;
            
            // Set QuestPDF license (Community license for non-commercial use)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        #region Text to PDF

        public byte[] ConvertTextToPdf(string content, PdfOptions options)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        ConfigurePage(page, options);

                        page.Content().Padding(20).Column(column =>
                        {
                            column.Spacing(10);

                            // Title
                            if (!string.IsNullOrEmpty(options.Title))
                            {
                                column.Item().Text(options.Title)
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);
                            }

                            // Content
                            column.Item().Text(content)
                                .FontSize(options.FontSize)
                                .LineHeight(1.5f);
                        });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting text to PDF");
                throw;
            }
        }

        #endregion

        #region HTML to PDF

        public byte[] ConvertHtmlToPdf(string htmlContent, PdfOptions options)
        {
            try
            {
                // Simple HTML parsing - convert basic tags
                var plainText = StripHtmlTags(htmlContent);
                
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        ConfigurePage(page, options);

                        page.Content().Padding(20).Column(column =>
                        {
                            column.Spacing(10);

                            if (!string.IsNullOrEmpty(options.Title))
                            {
                                column.Item().Text(options.Title)
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);
                            }

                            column.Item().Text(plainText)
                                .FontSize(options.FontSize)
                                .LineHeight(1.5f);
                        });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting HTML to PDF");
                throw;
            }
        }

        #endregion

        #region Markdown to PDF

        public byte[] ConvertMarkdownToPdf(string markdownContent, PdfOptions options)
        {
            try
            {
                // Simple markdown parsing
                var lines = markdownContent.Split('\n');
                
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        ConfigurePage(page, options);

                        page.Content().Padding(20).Column(column =>
                        {
                            column.Spacing(10);

                            if (!string.IsNullOrEmpty(options.Title))
                            {
                                column.Item().Text(options.Title)
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);
                            }

                            foreach (var line in lines)
                            {
                                if (line.StartsWith("# "))
                                {
                                    column.Item().Text(line.Substring(2))
                                        .FontSize(18)
                                        .Bold();
                                }
                                else if (line.StartsWith("## "))
                                {
                                    column.Item().Text(line.Substring(3))
                                        .FontSize(16)
                                        .Bold();
                                }
                                else if (line.StartsWith("### "))
                                {
                                    column.Item().Text(line.Substring(4))
                                        .FontSize(14)
                                        .Bold();
                                }
                                else if (!string.IsNullOrWhiteSpace(line))
                                {
                                    column.Item().Text(line)
                                        .FontSize(options.FontSize)
                                        .LineHeight(1.5f);
                                }
                            }
                        });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting Markdown to PDF");
                throw;
            }
        }

        #endregion

        #region Table to PDF

        public byte[] ConvertTableToPdf(TableDataModel tableData, PdfOptions options)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        ConfigurePage(page, options);

                        page.Content().Padding(20).Column(column =>
                        {
                            column.Spacing(10);

                            if (!string.IsNullOrEmpty(options.Title))
                            {
                                column.Item().Text(options.Title)
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);
                            }

                            // Table
                            column.Item().Table(table =>
                            {
                                // Define columns
                                foreach (var _ in tableData.Headers)
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                    });
                                }

                                // Header row
                                foreach (var header in tableData.Headers)
                                {
                                    table.Cell().Element(CellStyle).Text(header).Bold();
                                }

                                // Data rows
                                foreach (var row in tableData.Rows)
                                {
                                    foreach (var cell in row)
                                    {
                                        table.Cell().Element(CellStyle).Text(cell);
                                    }
                                }

                                IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(5);
                                }
                            });
                        });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting table to PDF");
                throw;
            }
        }

        #endregion

        #region Sample PDF

        public byte[] GenerateSamplePdf()
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header()
                            .Text("Sample PDF Document")
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                column.Spacing(20);

                                column.Item().Text("Welcome to PDF Converter!")
                                    .FontSize(16)
                                    .Bold();

                                column.Item().Text("This is a sample PDF generated using QuestPDF library. " +
                                    "You can convert various types of content to PDF format including text, HTML, markdown, and tables.")
                                    .LineHeight(1.5f);

                                column.Item().Text("Features:")
                                    .FontSize(14)
                                    .Bold();

                                column.Item().Text("• Text to PDF conversion\n" +
                                    "• HTML to PDF conversion\n" +
                                    "• Markdown to PDF conversion\n" +
                                    "• Table data to PDF conversion\n" +
                                    "• Custom page sizes and formatting")
                                    .LineHeight(1.5f);
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                                x.Span(" of ");
                                x.TotalPages();
                            });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sample PDF");
                throw;
            }
        }

        #endregion

        #region Images to PDF

        public byte[] ConvertImagesToPdf(List<IFormFile> images, PdfOptions options, string imageFit = "FitPage", bool oneImagePerPage = true)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // Set page size
                        page.Size(options.PageSize switch
                        {
                            "Letter" => PageSizes.Letter,
                            "Legal" => PageSizes.Legal,
                            _ => PageSizes.A4
                        });

                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);

                        // Header
                        if (options.IncludeHeader)
                        {
                            page.Header()
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .PaddingBottom(10)
                                .Row(row =>
                                {
                                    row.RelativeItem().Column(column =>
                                    {
                                        if (!string.IsNullOrEmpty(options.Title))
                                        {
                                            column.Item().Text(options.Title)
                                                .FontSize(14)
                                                .Bold();
                                        }
                                        if (!string.IsNullOrEmpty(options.Author))
                                        {
                                            column.Item().Text($"By: {options.Author}")
                                                .FontSize(10)
                                                .FontColor(Colors.Grey.Darken1);
                                        }
                                    });

                                    row.ConstantItem(100).AlignRight().Text(options.CreatedDate.ToString("MMM dd, yyyy"))
                                        .FontSize(10)
                                        .FontColor(Colors.Grey.Darken1);
                                });
                        }

                        // Footer
                        if (options.IncludeFooter)
                        {
                            page.Footer()
                                .BorderTop(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .PaddingTop(10)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text("Generated by MyApps PDF Converter")
                                        .FontSize(9)
                                        .FontColor(Colors.Grey.Darken1);

                                    if (options.IncludePageNumbers)
                                    {
                                        row.ConstantItem(100)
                                            .AlignRight()
                                            .DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken1))
                                            .Text(text =>
                                            {
                                                text.Span("Page ");
                                                text.CurrentPageNumber();
                                                text.Span(" of ");
                                                text.TotalPages();
                                            });
                                    }
                                });
                        }

                        // Content with images
                        page.Content().Column(column =>
                        {
                            if (oneImagePerPage)
                            {
                                // One image per page
                                for (int i = 0; i < images.Count; i++)
                                {
                                    var image = images[i];
                                    using var memoryStream = new MemoryStream();
                                    image.CopyTo(memoryStream);
                                    var imageBytes = memoryStream.ToArray();

                                    if (i > 0)
                                    {
                                        column.Item().PageBreak();
                                    }

                                    column.Item().AlignCenter().AlignMiddle().Element(container =>
                                    {
                                        ApplyImageFit(container, imageBytes, imageFit);
                                    });
                                }
                            }
                            else
                            {
                                // Multiple images per page
                                column.Spacing(20);
                                foreach (var image in images)
                                {
                                    using var memoryStream = new MemoryStream();
                                    image.CopyTo(memoryStream);
                                    var imageBytes = memoryStream.ToArray();

                                    column.Item().AlignCenter().Element(container =>
                                    {
                                        ApplyImageFit(container, imageBytes, imageFit);
                                    });
                                }
                            }
                        });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting images to PDF");
                throw;
            }
        }

        private void ApplyImageFit(IContainer container, byte[] imageBytes, string imageFit)
        {
            switch (imageFit)
            {
                case "FitPage":
                    container.Image(imageBytes).FitArea();
                    break;
                case "FitWidth":
                    container.Image(imageBytes).FitWidth();
                    break;
                case "Original":
                    container.Image(imageBytes);
                    break;
                default:
                    container.Image(imageBytes).FitArea();
                    break;
            }
        }

        #endregion

        #region Mixed Content (Text + Images) to PDF

        public byte[] ConvertMixedToPdf(string content, List<IFormFile> images, PdfOptions options, string imageFit = "FitPage", bool oneImagePerPage = true)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // Set page size
                        page.Size(options.PageSize switch
                        {
                            "Letter" => PageSizes.Letter,
                            "Legal" => PageSizes.Legal,
                            _ => PageSizes.A4
                        });

                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(options.FontSize));

                        // Header
                        if (options.IncludeHeader)
                        {
                            page.Header()
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .PaddingBottom(10)
                                .Row(row =>
                                {
                                    row.RelativeItem().Column(column =>
                                    {
                                        if (!string.IsNullOrEmpty(options.Title))
                                        {
                                            column.Item().Text(options.Title)
                                                .FontSize(14)
                                                .Bold();
                                        }
                                        if (!string.IsNullOrEmpty(options.Author))
                                        {
                                            column.Item().Text($"By: {options.Author}")
                                                .FontSize(10)
                                                .FontColor(Colors.Grey.Darken1);
                                        }
                                    });

                                    row.ConstantItem(100).AlignRight().Text(options.CreatedDate.ToString("MMM dd, yyyy"))
                                        .FontSize(10)
                                        .FontColor(Colors.Grey.Darken1);
                                });
                        }

                        // Footer
                        if (options.IncludeFooter)
                        {
                            page.Footer()
                                .BorderTop(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .PaddingTop(10)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text("Generated by MyApps PDF Converter")
                                        .FontSize(9)
                                        .FontColor(Colors.Grey.Darken1);

                                    if (options.IncludePageNumbers)
                                    {
                                        row.ConstantItem(100)
                                            .AlignRight()
                                            .DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken1))
                                            .Text(text =>
                                            {
                                                text.Span("Page ");
                                                text.CurrentPageNumber();
                                                text.Span(" of ");
                                                text.TotalPages();
                                            });
                                    }
                                });
                        }

                        // Content with text and images
                        page.Content().Column(column =>
                        {
                            column.Spacing(20);

                            // Add text content first
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                column.Item().Text(content)
                                    .FontSize(options.FontSize)
                                    .LineHeight(1.5f);
                            }

                            // Add images
                            if (oneImagePerPage)
                            {
                                // One image per page
                                for (int i = 0; i < images.Count; i++)
                                {
                                    var image = images[i];
                                    using var memoryStream = new MemoryStream();
                                    image.CopyTo(memoryStream);
                                    var imageBytes = memoryStream.ToArray();

                                    column.Item().PageBreak();

                                    column.Item().AlignCenter().AlignMiddle().Element(container =>
                                    {
                                        ApplyImageFit(container, imageBytes, imageFit);
                                    });
                                }
                            }
                            else
                            {
                                // Multiple images on same page
                                foreach (var image in images)
                                {
                                    using var memoryStream = new MemoryStream();
                                    image.CopyTo(memoryStream);
                                    var imageBytes = memoryStream.ToArray();

                                    column.Item().AlignCenter().Element(container =>
                                    {
                                        ApplyImageFit(container, imageBytes, imageFit);
                                    });
                                }
                            }
                        });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting mixed content to PDF");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private void ConfigurePage(PageDescriptor page, PdfOptions options)
        {
            // Set page size
            page.Size(options.PageSize switch
            {
                "Letter" => PageSizes.Letter,
                "Legal" => PageSizes.Legal,
                _ => PageSizes.A4
            });

            page.Margin(2, Unit.Centimetre);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(options.FontSize));

            // Header
            if (options.IncludeHeader)
            {
                page.Header()
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .PaddingBottom(10)
                    .Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            if (!string.IsNullOrEmpty(options.Title))
                            {
                                column.Item().Text(options.Title)
                                    .FontSize(14)
                                    .Bold();
                            }
                            if (!string.IsNullOrEmpty(options.Author))
                            {
                                column.Item().Text($"By: {options.Author}")
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Darken1);
                            }
                        });

                        row.ConstantItem(100).AlignRight().Text(options.CreatedDate.ToString("MMM dd, yyyy"))
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    });
            }

            // Footer
            if (options.IncludeFooter)
            {
                page.Footer()
                    .BorderTop(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .PaddingTop(10)
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text("Generated by MyApps PDF Converter")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);

                        if (options.IncludePageNumbers)
                        {
                            row.ConstantItem(100)
                                .AlignRight()
                                .DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken1))
                                .Text(text =>
                                {
                                    text.Span("Page ");
                                    text.CurrentPageNumber();
                                    text.Span(" of ");
                                    text.TotalPages();
                                });
                        }
                    });
            }
        }

        private string StripHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // Simple HTML tag removal
            var result = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
            return System.Net.WebUtility.HtmlDecode(result);
        }

        #endregion
    }
}
