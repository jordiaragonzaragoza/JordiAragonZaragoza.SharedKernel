namespace JordiAragonZaragoza.SharedKernel.Domain.Enums
{
    using JordiAragonZaragoza.SharedKernel.Contracts.DependencyInjection;
    using ArdalisSmartEnum = Ardalis.SmartEnum;

    public abstract class SmartEnum<TEnum> : ArdalisSmartEnum.SmartEnum<TEnum>, IIgnoreDependency
        where TEnum : ArdalisSmartEnum.SmartEnum<TEnum, int>
    {
        protected SmartEnum(string name, int value)
            : base(name, value)
        {
        }
    }
}