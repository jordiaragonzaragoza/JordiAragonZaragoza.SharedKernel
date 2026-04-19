namespace JordiAragonZaragoza.SharedKernel.Infrastructure.ServiceIdentity
{
    using System.ComponentModel.DataAnnotations;

    public class ServiceIdentityOptions
    {
        public const string Section = "ServiceIdentity";

        [Required]
        public string Name { get; init; } = default!;
    }
}