namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts
{
    using System.ComponentModel;

    // TODO: Review. Use uint instead of int for PageNumber and PageSize with default values.
    public record class PaginatedRequest
    {
        [DefaultValue(1)]
        public int PageNumber { get; init; }

        [DefaultValue(10)]
        public int PageSize { get; init; }
    }
}