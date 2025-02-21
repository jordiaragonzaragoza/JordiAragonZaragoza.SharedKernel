namespace JordiAragonZaragoza.SharedKernel.Application.AssemblyConfiguration
{
    using System.Reflection;
    using Autofac;
    using JordiAragonZaragoza.SharedKernel;
    using JordiAragonZaragoza.SharedKernel.Application.Behaviours;
    using JordiAragonZaragoza.SharedKernel.Application.Commands.Decorators;
    using JordiAragonZaragoza.SharedKernel.Application.Events.Decorators;
    using MediatR;
    using MediatR.Pipeline;

    public class ApplicationModule : AssemblyModule
    {
        protected override Assembly CurrentAssembly => ApplicationAssemblyReference.Assembly;

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            _ = builder.RegisterGeneric(typeof(RequestPreProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            _ = builder.RegisterGeneric(typeof(LoggerBehaviour<>)).As(typeof(IRequestPreProcessor<>));
            _ = builder.RegisterGeneric(typeof(ExceptionHandlerBehaviour<,>)).As(typeof(IPipelineBehavior<,>));
            _ = builder.RegisterGeneric(typeof(UnitOfWorkBehaviour<,>)).As(typeof(IPipelineBehavior<,>));
            _ = builder.RegisterGeneric(typeof(ValidationBehaviour<,>)).As(typeof(IPipelineBehavior<,>));
            _ = builder.RegisterGeneric(typeof(CachingBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            _ = builder.RegisterGeneric(typeof(InvalidateCachingBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            _ = builder.RegisterGeneric(typeof(DomainEventsDispatcherBehaviour<,>)).As(typeof(IPipelineBehavior<,>));
            _ = builder.RegisterGeneric(typeof(PerformanceBehaviour<,>)).As(typeof(IPipelineBehavior<,>));

            builder.RegisterGenericDecorator(
                typeof(DomainEventsHandlerDecorator<>),
                typeof(INotificationHandler<>));

            builder.RegisterGenericDecorator(
                typeof(EventNotificationHandlerDecorator<>),
                typeof(INotificationHandler<>));

            builder.RegisterGenericDecorator(
                typeof(CommandHandlerDecorator<>),
                typeof(IRequestHandler<,>));

            builder.RegisterGenericDecorator(
                typeof(CommandHandlerDecorator<,>),
                typeof(IRequestHandler<,>));
        }
    }
}