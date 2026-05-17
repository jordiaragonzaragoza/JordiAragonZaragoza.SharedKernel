namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using Ardalis.SmartEnum;

    public sealed class ActorType : SmartEnum<ActorType>
    {
        /// <summary>
        /// When a human user initiates the action or when it is the result of an event propagated from a user action.
        /// </summary>
        public static readonly ActorType User = new("user", 1);

        /// <summary>
        /// When the system initiates something on its own. Examples: Nightly batch job, initial seed, timeout/scheduler, automatic compensation.
        /// </summary>
        public static readonly ActorType System = new("system", 2);

        /// <summary>
        /// When the intention arises outside your platform, outside your organizational control.
        /// ⚠️ Warning. Another microservice of yours is NOT external, other bounded contexts of the same system are NOT external.
        /// </summary>
        public static readonly ActorType External = new("external", 3);

        private ActorType(string name, int value)
            : base(name, value)
        {
        }
    }
}