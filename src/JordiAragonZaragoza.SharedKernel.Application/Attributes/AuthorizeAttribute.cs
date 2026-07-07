namespace JordiAragonZaragoza.SharedKernel.Application.Attributes
{
    using System;

    /// <summary>
    /// Specifies the class this attribute is applied to requires authorization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class AuthorizeAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a comma delimited list of roles that are allowed to access the resource.
        /// </summary>
        public string? Roles { get; set; }

        /// <summary>
        /// Gets or sets a comma delimited list of policies that determine access to the resource.
        /// </summary>
        public string? Policies { get; set; }

        /// <summary>
        /// Gets or sets a comma delimited list of permissions that determine access to the resource.
        /// </summary>
        public string? Permissions { get; set; }
    }
}