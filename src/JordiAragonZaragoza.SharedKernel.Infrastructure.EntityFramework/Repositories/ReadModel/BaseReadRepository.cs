namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Repositories.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Specification;
    using Ardalis.Specification.EntityFrameworkCore;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts;
    using JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Contracts.DependencyInjection;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Context;
    using Microsoft.EntityFrameworkCore;

    public abstract class BaseReadRepository<TReadModel> : RepositoryBase<TReadModel>, IPaginatedSpecificationReadRepository<TReadModel>, IScopedDependency
        where TReadModel : class, IReadModel
    {
        protected BaseReadRepository(BaseReadModelContext readContext)
            : base(readContext)
        {
        }

        public virtual Task<TReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return this.GetByIdAsync<Guid>(id, cancellationToken);
        }

        public async Task<PaginatedCollectionOutputDto<TReadModel>> PaginatedListAsync(IPaginatedSpecification<TReadModel> paginatedSpecification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(paginatedSpecification);
            var request = paginatedSpecification.Request;

            var totalCount = await this.ApplySpecification(paginatedSpecification).CountAsync(cancellationToken);
            if (totalCount == 0)
            {
#pragma warning disable IDE0028
                return new PaginatedCollectionOutputDto<TReadModel>(default, default, totalCount, new List<TReadModel>());
#pragma warning restore IDE0028
            }

            var totalPages = request.PageSize > 0 ? (int)Math.Ceiling(totalCount / (double)request.PageSize) : 1;

            var actualPage = request.PageNumber == 0 ? 1 : request.PageNumber;
            actualPage = actualPage > totalPages ? totalPages : actualPage;
            actualPage = request.PageSize == 0 ? 1 : actualPage;

            var query = this.ApplySpecification(paginatedSpecification);

            if (actualPage > 1)
            {
                var skip = (actualPage - 1) * request.PageSize;
                query = query.Skip(skip);
            }

            if (request.PageSize > 0)
            {
                query = query.Take(request.PageSize);
            }

            var items = await query.ToListAsync(cancellationToken);

            return new PaginatedCollectionOutputDto<TReadModel>(actualPage, totalPages, totalCount, items);
        }
    }
}