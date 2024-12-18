﻿namespace JordiAragonZaragoza.SharedKernel.Infrastructure.AssemblyConfiguration
{
    using System.Reflection;
    using Autofac;
    using JordiAragonZaragoza.SharedKernel;
    using JordiAragonZaragoza.SharedKernel.Infrastructure.InternalBus.MediatR;
    using MediatR;
    using Volo.Abp.Guids;

    public class InfrastructureModule : AssemblyModule
    {
        protected override Assembly CurrentAssembly => InfrastructureAssemblyReference.Assembly;

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType(typeof(CustomMediator))
                .As(typeof(IMediator))
                .InstancePerLifetimeScope();

            builder.RegisterType(typeof(SequentialGuidGenerator))
                .As(typeof(IGuidGenerator))
                .InstancePerLifetimeScope();
        }
    }
}