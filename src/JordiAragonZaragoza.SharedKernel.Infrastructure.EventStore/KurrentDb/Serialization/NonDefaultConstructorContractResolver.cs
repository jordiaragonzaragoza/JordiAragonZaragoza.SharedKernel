namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.KurrentDb.Serialization
{
    using System;
    using Newtonsoft.Json.Serialization;

    public class NonDefaultConstructorContractResolver : DefaultContractResolver
    {
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            return JsonObjectContractProvider.UsingNonDefaultConstructor(
                base.CreateObjectContract(objectType),
                objectType,
                base.CreateConstructorParameters);
        }
    }
}