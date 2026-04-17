namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using Ardalis.SmartEnum;

    public sealed class ActorType : SmartEnum<ActorType>
    {
        public static readonly ActorType User = new("user", 1);
        public static readonly ActorType System = new("system", 2);

        public static readonly ActorType External = new("external", 3);

        private ActorType(string name, int value)
            : base(name, value)
        {
        }
    }
}