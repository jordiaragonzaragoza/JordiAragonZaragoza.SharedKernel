namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi
{
    using Microsoft.Extensions.DependencyInjection;

    public static class HttpRestfulApiDependencyInjection
    {
        public static IServiceCollection AddSharedKernelPresentationHttpRestfulApi(this IServiceCollection services)
        {
            return services;
        }
    }
}