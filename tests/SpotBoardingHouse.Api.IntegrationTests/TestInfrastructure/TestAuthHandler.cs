using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SpotBoardingHouse.Api.IntegrationTests.TestInfrastructure
{
    public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "Test";
        public const string UserIdHeader = "X-Test-UserId";
        public const string RoleHeader = "X-Test-Role";

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(UserIdHeader, out var userIdValues) ||
                !Guid.TryParse(userIdValues.FirstOrDefault(), out var userId))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var role = Request.Headers.TryGetValue(RoleHeader, out var roleValues)
                ? roleValues.FirstOrDefault() ?? "Owner"
                : "Owner";

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
