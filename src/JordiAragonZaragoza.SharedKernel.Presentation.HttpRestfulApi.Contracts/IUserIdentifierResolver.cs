namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts
{
    using System;
    using System.Security.Claims;

    public interface IUserIdentifierResolver
    {
        bool TryResolve(ClaimsPrincipal principal, out Guid userId);
    }
}