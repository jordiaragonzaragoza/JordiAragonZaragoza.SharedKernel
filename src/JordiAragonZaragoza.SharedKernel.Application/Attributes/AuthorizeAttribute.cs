namespace JordiAragonZaragoza.SharedKernel.Application.Attributes
{
    using System;

    /// <summary>
    /// Specifies the class this attribute is applied to requires authorization.
    /// <para>
    /// Same model as ASP.NET Core: every attribute is a requirement and ALL the attributes
    /// applied to a class must be satisfied (AND). Inside one attribute:
    /// the user must hold ANY of the listed roles (OR), ALL the listed permissions and
    /// ALL the listed policies must pass; roles, permissions and policies of the same attribute
    /// are also cumulative.
    /// </para>
    /// <para>
    /// To require several roles at once, apply several attributes
    /// (<c>[Authorize(Roles = "A")] [Authorize(Roles = "B")]</c>).
    /// For any other "this OR that" rule (permissions, ownership...) use a policy.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class AuthorizeAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a comma delimited list of roles. The user must hold ANY of them.
        /// </summary>
        public string? Roles { get; set; }

        /// <summary>
        /// Gets or sets a comma delimited list of policies. ALL of them must succeed.
        /// </summary>
        public string? Policies { get; set; }

        /// <summary>
        /// Gets or sets a comma delimited list of permissions. The user must hold ALL of them.
        /// </summary>
        public string? Permissions { get; set; }
    }
}
