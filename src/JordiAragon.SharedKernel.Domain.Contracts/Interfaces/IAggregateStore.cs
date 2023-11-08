namespace JordiAragon.SharedKernel.Domain.Contracts.Interfaces
{
    using System.Threading.Tasks;

    public interface IAggregateStore // TODO: Temporal remove.
    {
        Task<bool> ExistsAsync<T, TId>(TId aggregateId);

        Task SaveAsync<T, TId>(T aggregate)
            where T : IEventSourcedAggregateRoot<TId>;

        Task<T> LoadAsync<T, TId>(TId aggregateId)
            where T : IEventSourcedAggregateRoot<TId>;
    }
}