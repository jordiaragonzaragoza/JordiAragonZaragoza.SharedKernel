namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.AssemblyConfiguration
{
    using System.Reflection;
    using Autofac;
    using JordiAragonZaragoza.SharedKernel;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.Interceptors;

    public class EntityFrameworkModule : AssemblyModule
    {
        protected override Assembly CurrentAssembly => InfrastructureEntityFrameworkAssemblyReference.Assembly;

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            _ = builder.RegisterType<SoftDeleteEntitySaveChangesInterceptor>()
                .InstancePerLifetimeScope();
        }
    }
}