namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Context
{
    using System;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Interceptors;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public abstract class BaseBusinessModelContext : BaseContext
    {
        private readonly SoftDeleteEntitySaveChangesInterceptor softDeleteEntitySaveChangesInterceptor;

        protected BaseBusinessModelContext(
            DbContextOptions options,
            ILoggerFactory loggerFactory,
            IHostEnvironment hostEnvironment,
            SoftDeleteEntitySaveChangesInterceptor softDeleteEntitySaveChangesInterceptor)
            : base(options, loggerFactory, hostEnvironment)
        {
            this.softDeleteEntitySaveChangesInterceptor = softDeleteEntitySaveChangesInterceptor ?? throw new ArgumentNullException(nameof(softDeleteEntitySaveChangesInterceptor));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));

            _ = optionsBuilder.AddInterceptors(this.softDeleteEntitySaveChangesInterceptor);

            base.OnConfiguring(optionsBuilder);
        }
    }
}