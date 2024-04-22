using Archivr.PrintingServer.Model;
using Archivr.UI.Models;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Diagnostics;
using System.Text.Json;
using Wkhtmltopdf.NetCore;
using Wkhtmltopdf.NetCore.Interfaces;
using Wkhtmltopdf.NetCore.Options;

namespace Archivr.PrintingServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LabelsController : Controller
    {
        private readonly GeneratePdf _generatePdf;
        private readonly ILogger<LabelsController> _logger;
        private readonly IConfiguration _configuration;

        public LabelsController(ILogger<LabelsController> logger, GeneratePdf generatePdf, IConfiguration configuration)
        {
            _logger = logger;
            _generatePdf = generatePdf;
            _configuration = configuration;
        }

        [HttpPost("GetPdf")]
        public async Task<IActionResult> GetPdf([FromBody] Label label, [FromQuery] string layout = "Horizontal")
        {
            var pdf = await GenerateLabel(_generatePdf, label, layout);
            return new FileContentResult(pdf, "application/pdf")
            {
                FileDownloadName = $"{label.Code}.pdf",
                LastModified = DateTime.Now,
            };
        }

        [HttpPost("PrintLabel")]
        public async Task<IActionResult> PrintLabel([FromBody] Label label, [FromQuery] string layout = "Horizontal")
        {
            var pdf = await GenerateLabel(_generatePdf, label, layout);
            return await PrintPdf(_configuration, pdf) ? Ok() : new ContentResult() { StatusCode = 500, Content = "Connection to printing server failed." };
        }

        internal static async Task<byte[]> GenerateLabel(GeneratePdf pdfgen, Label label, string layout)
        {
            ConvertOptions options = new ConvertOptions();

            if (layout == "Horizontal")
                options.PageHeight = 30;
            else if (layout == "Vertical")
                options.PageHeight = 85;
            options.PageWidth = 62;
            options.PageMargins.Bottom = 0;
            options.PageMargins.Top = 0;
            options.PageMargins.Left = 0;
            options.PageMargins.Right = 0;
            options.HeaderSpacing = 0;
            options.FooterSpacing = 0;
            options.IsGrayScale = true;
            options.PageOrientation = Orientation.Portrait;

            pdfgen.SetConvertOptions(options);

            if (label.QrCodeImage == null)
            {
                QRCodeGenerator QrGenerator = new QRCodeGenerator();
                QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(label.QrCodeLink ?? label.Code, QRCodeGenerator.ECCLevel.Q);
                BitmapByteQRCode QrCode = new BitmapByteQRCode(QrCodeInfo);
                byte[] BitmapArray = QrCode.GetGraphic(10);
                label.QrCodeImage = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
            }

            byte[] pdf = await pdfgen.GetByteArray($"Views/Labels/{layout}.cshtml", label);

            return pdf;
        }

        internal static async Task<bool> PrintPdf(IConfiguration config, byte[] pdf)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(config["Printing:Server"]);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config["Printing:AuthToken"]}");

            var response = await httpClient.PostAsync("/Printing/PrintLabel", new ByteArrayContent(pdf));
            return response.IsSuccessStatusCode;
        }
    }
}