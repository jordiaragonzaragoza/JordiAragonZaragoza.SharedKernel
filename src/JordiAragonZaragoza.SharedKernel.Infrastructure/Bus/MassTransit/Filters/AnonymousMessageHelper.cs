namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MassTransit.Filters
{
    using System;
    using global::MassTransit;

    public static class AnonymousMessageHelper
    {
        public static bool IsAnonymousAllowed<T>(ConsumeContext<T> context)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(context);

            return false;
        }
    }
}