namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using Ardalis.SmartEnum;

    public sealed class ExecutorType : SmartEnum<ExecutorType>
    {
        public static readonly ExecutorType Service = new("service", 1);
        public static readonly ExecutorType Worker = new("worker", 2);
        public static readonly ExecutorType Tool = new("tool", 3);

        private ExecutorType(string name, int value)
            : base(name, value)
        {
        }
    }
}