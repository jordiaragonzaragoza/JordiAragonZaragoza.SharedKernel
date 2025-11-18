namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    public interface IUserContextService
    {
        UserContext CurrentContext { get; }

        void SetUserContext(string userId);
    }
}