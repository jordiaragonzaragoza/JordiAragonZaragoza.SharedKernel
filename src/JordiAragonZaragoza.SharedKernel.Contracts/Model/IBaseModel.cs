namespace JordiAragonZaragoza.SharedKernel.Contracts.Model
{
    /// <summary>
    /// Generic abstraction for a base model.
    /// </summary>
    /// <typeparam name="TId">The id for the base model.</typeparam>
    public interface IBaseModel<out TId> : IBaseModel
        where TId : notnull
    {
        TId Id { get; }
    }
}