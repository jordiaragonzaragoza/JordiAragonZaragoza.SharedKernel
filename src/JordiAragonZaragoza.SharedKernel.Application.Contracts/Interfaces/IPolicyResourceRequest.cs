namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;

    /// <summary>
    /// Implemented by commands/queries that reference a single resource whose
    /// id is needed by an ownership-based policy (e.g. ReservationId for
    /// Policies.SelfOrAdmin on CancelReservationCommand). The policy enforcer
    /// uses this id to look up the resource and resolve its owner — it does
    /// not need to know anything else about the request shape.
    /// </summary>
    public interface IPolicyResourceRequest
    {
        Guid ResourceId { get; }
    }
}