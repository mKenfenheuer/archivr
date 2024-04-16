using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Text;

namespace Archivr.PrintingServer.Auth
{
    public class StaticTokenAuthHandler
        : AuthenticationHandler<StaticTokenAuthSchemeOptions>
    {
        public StaticTokenAuthHandler(
            IOptionsMonitor<StaticTokenAuthSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // validation comes in here
            if (!Request.Headers.ContainsKey(HeaderNames.Authorization))
            {
                return Task.FromResult(AuthenticateResult.Fail("Header Not Found."));
            }

            var header = Request.Headers[HeaderNames.Authorization].ToString();
            var tokenMatch = Regex.Match(header, StaticTokenAuthSchemeConstants.NToken);

            if (tokenMatch.Success)
            {
                // the token is captured in this group
                // as declared in the Regex
                var token = tokenMatch.Groups["token"].Value;

                if (token == Options.Configuration["StaticBearerToken"])
                    return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { }, nameof(StaticTokenAuthHandler))), this.Scheme.Name)));
            }

            // failure branch
            // return failure
            // with an optional message
            return Task.FromResult(AuthenticateResult.Fail("Token verification failed."));
        }
    }
}
