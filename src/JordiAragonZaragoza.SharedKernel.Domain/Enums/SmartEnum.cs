namespace JordiAragonZaragoza.SharedKernel.Domain.Enums
{
    using ArdalisSmartEnum = Ardalis.SmartEnum;

    public abstract class SmartEnum<TEnum> : ArdalisSmartEnum.SmartEnum<TEnum>
        where TEnum : ArdalisSmartEnum.SmartEnum<TEnum, int>
    {
        protected SmartEnum(string name, int value)
            : base(name, value)
        {
        }
    }
}