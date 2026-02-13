using Microsoft.AspNetCore.Mvc;
using MyApps.Models;
using MyApps.Services;

namespace MyApps.Controllers
{
    /// <summary>
    /// Controller for PDF Converter
    /// </summary>
    public class PdfConverterController : Controller
    {
        private readonly IPdfConverterService _pdfService;
        private readonly ILogger<PdfConverterController> _logger;

        public PdfConverterController(IPdfConverterService pdfService, ILogger<PdfConverterController> logger)
        {
            _pdfService = pdfService;
            _logger = logger;
        }

        #region Index

        [HttpGet]
        public IActionResult Index()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(new PdfConversionModel());
        }

        #endregion

        #region Convert to PDF

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Convert(PdfConversionModel model)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            if (!ModelState.IsValid)
                return View("Index", model);

            try
            {
                var options = new PdfOptions
                {
                    Title = model.Title ?? "Document",
                    Author = model.Author ?? HttpContext.Session.GetString("UserName") ?? "User",
                    PageSize = model.PageSize,
                    IncludeHeader = model.IncludeHeader,
                    IncludeFooter = model.IncludeFooter,
                    IncludePageNumbers = model.IncludePageNumbers,
                    FontSize = model.FontSize
                };

                // Always convert as plain text
                var pdfBytes = _pdfService.ConvertTextToPdf(model.Content, options);

                var fileName = $"{(model.Title ?? "document").Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting to PDF");
                TempData["Error"] = "Failed to convert to PDF. Please check your content and try again.";
                return View("Index", model);
            }
        }

        #endregion

        #region Convert Images to PDF

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConvertImages(string Title, List<IFormFile> Images, string PageSize = "A4", 
            string ImageFit = "FitPage", bool OneImagePerPage = true)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            if (Images == null || Images.Count == 0)
            {
                TempData["Error"] = "Please select at least one image.";
                return RedirectToAction("Index");
            }

            // Validate image files
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            foreach (var image in Images)
            {
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    TempData["Error"] = $"Invalid file type: {image.FileName}. Only JPG, PNG, GIF, and BMP are supported.";
                    return RedirectToAction("Index");
                }
            }

            try
            {
                var options = new PdfOptions
                {
                    Title = Title ?? "Images",
                    Author = HttpContext.Session.GetString("UserName") ?? "User",
                    PageSize = PageSize,
                    IncludeHeader = true,
                    IncludeFooter = true,
                    IncludePageNumbers = true
                };

                var pdfBytes = _pdfService.ConvertImagesToPdf(Images, options, ImageFit, OneImagePerPage);

                var fileName = $"{(Title ?? "images").Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting images to PDF");
                TempData["Error"] = "Failed to convert images to PDF. Please try again.";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Convert Mixed (Text + Images) to PDF

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConvertMixed(string Title, string? Content, List<IFormFile>? Images, 
            string PageSize = "A4", int FontSize = 12, string? Author = null,
            bool IncludeHeader = true, bool IncludeFooter = true, bool IncludePageNumbers = true,
            string ImageFit = "FitPage", bool OneImagePerPage = true)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // Validate that at least one content type is provided
            bool hasText = !string.IsNullOrWhiteSpace(Content);
            bool hasImages = Images != null && Images.Count > 0;

            if (!hasText && !hasImages)
            {
                TempData["Error"] = "Please provide either text content or images to convert.";
                return RedirectToAction("Index");
            }

            // Validate image files if provided
            if (hasImages)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                foreach (var image in Images!)
                {
                    var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["Error"] = $"Invalid file type: {image.FileName}. Only JPG, PNG, GIF, and BMP are supported.";
                        return RedirectToAction("Index");
                    }
                }
            }

            try
            {
                var options = new PdfOptions
                {
                    Title = Title ?? "Document",
                    Author = Author ?? HttpContext.Session.GetString("UserName") ?? "User",
                    PageSize = PageSize,
                    IncludeHeader = IncludeHeader,
                    IncludeFooter = IncludeFooter,
                    IncludePageNumbers = IncludePageNumbers,
                    FontSize = FontSize
                };

                byte[] pdfBytes;

                // Determine conversion type
                if (hasText && hasImages)
                {
                    // Both text and images
                    pdfBytes = _pdfService.ConvertMixedToPdf(Content!, Images!, options, ImageFit, OneImagePerPage);
                }
                else if (hasImages)
                {
                    // Only images
                    pdfBytes = _pdfService.ConvertImagesToPdf(Images!, options, ImageFit, OneImagePerPage);
                }
                else
                {
                    // Only text
                    pdfBytes = _pdfService.ConvertTextToPdf(Content!, options);
                }

                var fileName = $"{(Title ?? "document").Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting to PDF");
                TempData["Error"] = "Failed to convert to PDF. Please try again.";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Helper Methods

        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
        }

        #endregion
    }
}
