namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Identity
{
    using System;
    using System.Security.Claims;
    using JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts;
    using Microsoft.Extensions.Options;

    public sealed class ClaimUserIdentifierResolver : IUserIdentifierResolver
    {
        private readonly string claimType;

        public ClaimUserIdentifierResolver(
            IOptions<IdentityClaimsOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options);

            this.claimType = options.Value.UserIdClaimType ?? throw new ArgumentException("User ID claim type must be provided.", nameof(options));
        }

        public bool TryResolve(ClaimsPrincipal principal, out Guid userId)
        {
            ArgumentNullException.ThrowIfNull(principal);

            userId = Guid.Empty;
            var value = principal.FindFirst(this.claimType)?.Value;
            return Guid.TryParse(value, out userId) && userId != Guid.Empty;
        }
    }
}