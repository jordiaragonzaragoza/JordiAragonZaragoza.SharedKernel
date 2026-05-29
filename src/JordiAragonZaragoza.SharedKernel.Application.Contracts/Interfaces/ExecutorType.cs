namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    using Ardalis.SmartEnum;

    /// <summary>
    /// Represents the system that is actually running the action.
    /// </summary>
    public sealed class ExecutorType : SmartEnum<ExecutorType>
    {
        /// <summary>
        /// A service, such as an API that is executing the action as part of its normal operation.
        /// </summary>
        public static readonly ExecutorType Service = new("service", 1);

        /// <summary>
        /// A background worker or a scheduled job that is executing the action outside of the context of an API request,
        /// such as processing a queue or running a periodic task.
        /// </summary>
        public static readonly ExecutorType Worker = new("worker", 2);

        /// <summary>
        /// A tool, such as a command-line interface (CLI) or an administrative dashboard,
        /// that is executing the action as part of a manual operation by an administrator or developer.
        /// </summary>
        public static readonly ExecutorType Tool = new("tool", 3);

        private ExecutorType(string name, int value)
            : base(name, value)
        {
        }
    }
}