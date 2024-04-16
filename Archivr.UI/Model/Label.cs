using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Archivr.PrintingServer.Model
{
    public class Label
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? QrCodeImage { get; set; }
        public string? QrCodeLink { get; set; }

    }
}
