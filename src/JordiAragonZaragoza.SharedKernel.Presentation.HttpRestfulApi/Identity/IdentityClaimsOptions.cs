namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Identity
{
    using System.ComponentModel.DataAnnotations;

    public sealed class IdentityClaimsOptions
    {
        public const string SectionName = "Authentication:Claims";

        [Required]
        public string UserIdClaimType { get; init; } = string.Empty;
    }
}