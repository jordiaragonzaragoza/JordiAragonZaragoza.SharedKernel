namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi
{
    using System;

    /// <summary>
    /// Marks an endpoint as infrastructure-only (health, metrics, swagger).
    /// The ExecutionContextMiddleware performs a full bypass for these —
    /// no context is set, no correlation id is propagated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class InfrastructureEndpointAttribute : Attribute
    {
    }
}