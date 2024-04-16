
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Archivr.PrintingServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PrintingController : Controller
    {
        private readonly ILogger<PrintingController> _logger;
        private readonly IConfiguration _configuration;

        public PrintingController(ILogger<PrintingController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("HealthCheck")]
        [Authorize]
        public ActionResult HealthCheck()
        {            
            return new ContentResult()
            {
                StatusCode = 201,
                Content = "{\"status\":\"healthy\"}"
            };
        }

        [HttpPost("PrintLabel")]
        [Authorize]
        [Consumes("application/pdf")]
        public async Task<IActionResult> PrintLabel()
        {           
            var tempFile = Path.GetTempFileName();
            FileStream stream = new FileStream(tempFile, FileMode.OpenOrCreate);
            MemoryStream body = new MemoryStream();
            await Request.Body.CopyToAsync(body);
            byte[] pdf = body.ToArray();
            stream.Write(pdf, 0, pdf.Length);
            await stream.FlushAsync();
            stream.Close();

            Process p = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "/usr/bin/lp",
                    Arguments = $"-h {_configuration["CUPS:Server"]} -o pdfautorotate=off {tempFile}",
                }
            };

            p.Start();
            await p.WaitForExitAsync();

            System.IO.File.Delete(tempFile);

            return p.ExitCode == 0 ? Ok() : new ContentResult() { StatusCode = 500 };
        }
    }
}