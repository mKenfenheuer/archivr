namespace Archivr.PrintingServer.Auth
{
    public class StaticTokenAuthSchemeConstants
    {
        public const string TokenAuthScheme = "Bearer";
        public const string NToken = $"{TokenAuthScheme} (?<token>.*)";
    }
}
