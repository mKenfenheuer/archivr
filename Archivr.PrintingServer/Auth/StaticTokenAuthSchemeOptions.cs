using Microsoft.AspNetCore.Authentication;

namespace Archivr.PrintingServer.Auth
{
    public class StaticTokenAuthSchemeOptions : AuthenticationSchemeOptions
    {
        public IConfiguration Configuration { get; set; }
    }
}