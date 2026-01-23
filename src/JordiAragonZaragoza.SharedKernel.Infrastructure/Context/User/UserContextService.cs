namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Context.User
{
    using System;
    using System.Threading;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;

    public class UserContextService : IUserContextService
    {
        private static readonly AsyncLocal<UserContext?> AsyncUserContext = new();

        public UserContext CurrentContext =>
            AsyncUserContext.Value ?? throw new InvalidOperationException("UserContext has not been set.");

        public void SetUserContext(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("UserId cannot be null or empty.", nameof(userId));
            }

            AsyncUserContext.Value = new UserContext(userId);
        }
    }
}