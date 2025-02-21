namespace JordiAragonZaragoza.SharedKernel
{
    using System.Reflection;
    using Autofac;
    using JordiAragonZaragoza.SharedKernel.Contracts.DependencyInjection;
    using Module = Autofac.Module;

    public abstract class AssemblyModule : Module
    {
        protected abstract Assembly CurrentAssembly { get; }

        protected override void Load(ContainerBuilder builder)
        {
            // Register Transient dependencies.
            _ = builder.RegisterAssemblyTypes(this.CurrentAssembly)
                    .Where(static x => typeof(ITransientDependency).IsAssignableFrom(x))
                    .AsImplementedInterfaces()
                    .InstancePerDependency();

            // Register Scoped dependencies.
            _ = builder.RegisterAssemblyTypes(this.CurrentAssembly)
                    .Where(static x => typeof(IScopedDependency).IsAssignableFrom(x))
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();

            // Register singleton dependencies.
            _ = builder.RegisterAssemblyTypes(this.CurrentAssembly)
                    .Where(static x => typeof(ISingletonDependency).IsAssignableFrom(x))
                    .AsImplementedInterfaces()
                    .SingleInstance();

            // Default without marker interface, register as transient.
            _ = builder.RegisterAssemblyTypes(this.CurrentAssembly)
                .Where(static x =>
                    !typeof(ITransientDependency).IsAssignableFrom(x)
                    && !typeof(IScopedDependency).IsAssignableFrom(x)
                    && !typeof(ISingletonDependency).IsAssignableFrom(x)
                    && !typeof(IIgnoreDependency).IsAssignableFrom(x))
               .AsImplementedInterfaces()
               .InstancePerDependency();
        }
    }
}