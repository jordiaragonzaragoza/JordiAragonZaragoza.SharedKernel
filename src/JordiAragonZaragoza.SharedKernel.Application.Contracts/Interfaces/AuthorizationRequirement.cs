namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The requirements declared by ONE [Authorize] attribute. A request is authorized only when
    /// EVERY requirement (one per attribute) is satisfied; inside a requirement:
    /// the user must hold ANY of the roles (when there are any), ALL the permissions
    /// and ALL the policies must pass. Same model as ASP.NET Core.
    /// </summary>
    /// <param name="AnyOfRoles">Roles of which the user must hold at least one. Empty means no role requirement.</param>
    /// <param name="AllOfPermissions">Permissions the user must hold all of.</param>
    /// <param name="AllOfPolicies">Policies that must all succeed.</param>
    public sealed record AuthorizationRequirement(
        IReadOnlyCollection<string> AnyOfRoles,
        IReadOnlyCollection<string> AllOfPermissions,
        IReadOnlyCollection<string> AllOfPolicies)
    {
        /// <summary>
        /// Returns the required permissions that are not among the held ones.
        /// </summary>
        /// <param name="heldPermissions">The permissions the user holds.</param>
        /// <returns>The missing permissions; empty when all of them are held.</returns>
        public IReadOnlyCollection<string> GetMissingPermissions(IReadOnlySet<string> heldPermissions)
        {
            ArgumentNullException.ThrowIfNull(heldPermissions);

            return this.AllOfPermissions
                .Where(permission => !heldPermissions.Contains(permission))
                .ToList();
        }

        /// <summary>
        /// Checks the role requirement: satisfied when no role is required or the user holds at least one of them.
        /// </summary>
        /// <param name="heldRoles">The roles the user holds.</param>
        /// <returns>True when the role requirement is satisfied.</returns>
        public bool IsRoleRequirementSatisfied(IReadOnlySet<string> heldRoles)
        {
            ArgumentNullException.ThrowIfNull(heldRoles);

            return this.AnyOfRoles.Count == 0
                || this.AnyOfRoles.Any(heldRoles.Contains);
        }
    }
}
