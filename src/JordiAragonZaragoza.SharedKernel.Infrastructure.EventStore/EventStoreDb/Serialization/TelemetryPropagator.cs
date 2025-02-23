namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.EventStoreDb.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using OpenTelemetry;
    using OpenTelemetry.Context.Propagation;

    // TODO: Temporal. This class will be relocated on OpenTelemetry implementation.
    public static class TelemetryPropagator
    {
        private static TextMapPropagator propagator = Propagators.DefaultTextMapPropagator;

        public static void UseDefaultCompositeTextMapPropagator()
        {
            propagator =
#pragma warning disable IDE0300
                new CompositeTextMapPropagator(new TextMapPropagator[]
#pragma warning restore IDE0300
                {
                    new TraceContextPropagator(),
                    new BaggagePropagator(),
                });
        }

        public static void Inject<T>(
            this PropagationContext context,
            T carrier,
            Action<T, string, string> setter)
        {
            propagator.Inject(context, carrier, setter);
        }

        public static PropagationContext Extract<T>(
            T carrier,
            Func<T, string, IEnumerable<string>> getter)
        {
            return propagator.Extract(default, carrier, getter);
        }

        public static PropagationContext Extract<T>(
            PropagationContext context,
            T carrier,
            Func<T, string, IEnumerable<string>> getter)
        {
            return propagator.Extract(context, carrier, getter);
        }

        public static PropagationContext? Propagate<T>(this Activity activity, T carrier, Action<T, string, string> setter)
        {
            if (activity?.Context == null)
            {
                return null;
            }

            var propagationContext = new PropagationContext(activity.Context, Baggage.Current);
            propagationContext.Inject(carrier, setter);

            return propagationContext;
        }

        public static PropagationContext? GetPropagationContext(Activity? activity = null)
        {
            var activityContext = (activity ?? Activity.Current)?.Context;
            if (!activityContext.HasValue)
            {
                return null;
            }

            return new PropagationContext(activityContext.Value, Baggage.Current);
        }
    }
}