namespace JordiAragonZaragoza.SharedKernel.Application.Pipelines
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;
    using JordiAragonZaragoza.SharedKernel.Application.Attributes;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;

    public class RequestAuthorizationService<TRequest> : IRequestAuthorizationService<TRequest>
        where TRequest : notnull
    {
        private readonly IAuthorizationService authorizationService;

        public RequestAuthorizationService(
            IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        }

        public async Task<Result> TryAuthorizeAsync(TRequest request, CancellationToken cancellationToken = default)
        {
            var authorizationAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>().ToList();
            if (authorizationAttributes.Count == 0)
            {
                return Result.Success();
            }

            // One requirement per attribute: the grouping must be preserved so that the roles
            // of one attribute are alternatives (OR) while the attributes are cumulative (AND).
            var requirements = authorizationAttributes
                .Select(attribute => new AuthorizationRequirement(
                    Split(attribute.Roles),
                    Split(attribute.Permissions),
                    Split(attribute.Policies)))
                .ToList()
                .AsReadOnly();

            Guid? resourceId = request is IPolicyResourceRequest resourceRequest
                ? resourceRequest.ResourceId
                : null;

            return await this.authorizationService.AuthorizeAsync(
                requirements,
                resourceId,
                cancellationToken);
        }

        /// <summary>
        /// Splits a comma delimited value. Values are trimmed and empty entries are discarded,
        /// so "a, b" means the same as "a,b" (a leading space would never match).
        /// </summary>
        private static ReadOnlyCollection<string> Split(string? commaSeparated)
            => (commaSeparated?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [])
                .Distinct(StringComparer.Ordinal)
                .ToList()
                .AsReadOnly();
    }
}
