namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    /// <summary>
    /// Marks a read model as scope-aware. All read models implement this
    /// so that the tenant query filter and interceptor can operate generically.
    /// ScopeInfo is an owned EF Core type flattened into the same table.
    /// </summary>
    public interface IScopedReadModel
    {
        ScopeInfo Scope { get; }
    }
}