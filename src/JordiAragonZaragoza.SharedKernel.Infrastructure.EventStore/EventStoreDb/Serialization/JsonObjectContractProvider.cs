namespace JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.EventStoreDb.Serialization
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public static class JsonObjectContractProvider
    {
        private static readonly Type ConstructorAttributeType = typeof(JsonConstructorAttribute);
        private static readonly ConcurrentDictionary<string, JsonObjectContract> Constructors = new();

        public static JsonObjectContract UsingNonDefaultConstructor(
            JsonObjectContract contract,
            Type objectType,
            Func<ConstructorInfo, JsonPropertyCollection, IList<JsonProperty>> createConstructorParameters)
        {
            return Constructors.GetOrAdd(objectType?.AssemblyQualifiedName!, _ =>
            {
                ArgumentNullException.ThrowIfNull(objectType, nameof(objectType));

                var nonDefaultConstructor = GetNonDefaultConstructor(objectType);

                if (nonDefaultConstructor == null)
                {
                    return contract;
                }

                contract.OverrideCreator = GetObjectConstructor(nonDefaultConstructor);
                contract.CreatorParameters.Clear();
                foreach (var constructorParameter in
                         createConstructorParameters(nonDefaultConstructor, contract.Properties))
                {
                    contract.CreatorParameters.Add(constructorParameter);
                }

                return contract;
            });
        }

        private static ObjectConstructor<object> GetObjectConstructor(MethodBase method)
        {
            var c = method as ConstructorInfo;

            if (c == null)
            {
                return a => method.Invoke(null, a)!;
            }

            if (c.GetParameters().Length == 0)
            {
                return _ => c.Invoke([]);
            }

            return c.Invoke;
        }

        private static ConstructorInfo? GetNonDefaultConstructor(Type objectType)
        {
            // Use default contract for non-object types.
            if (objectType.IsPrimitive || objectType.IsEnum)
            {
                return null;
            }

            return GetAttributeConstructor(objectType)
                   ?? GetTheMostSpecificConstructor(objectType);
        }

        private static ConstructorInfo? GetAttributeConstructor(Type objectType)
        {
            // Use default contract for non-object types.
            if (objectType.IsPrimitive || objectType.IsEnum)
            {
                return null;
            }

            var constructors = objectType
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(static c => c.GetCustomAttributes().Any(static a => a.GetType() == ConstructorAttributeType)).ToList();

            return constructors.Count switch
            {
                1 => constructors[0],
                > 1 => throw new JsonException($"Multiple constructors with a {ConstructorAttributeType.Name}."),
                _ => null,
            };
        }

        private static ConstructorInfo? GetTheMostSpecificConstructor(Type objectType)
        {
            return objectType
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .OrderByDescending(static e => e.GetParameters().Length)
                .FirstOrDefault();
        }
    }
}