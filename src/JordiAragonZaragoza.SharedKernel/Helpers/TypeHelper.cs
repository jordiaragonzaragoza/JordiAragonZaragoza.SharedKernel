namespace JordiAragonZaragoza.SharedKernel.Helpers
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class TypeHelper
    {
        public static Type GetFirstMatchingTypeFromCurrentDomainAssembly(string typeName)
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(static a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        return ex.Types
                                .Where(static t => t is not null)
                                .Cast<Type>();
                    }
                });

            // Prefer exact FullName match
            var byFullName = allTypes.FirstOrDefault(x => x.FullName == typeName);
            if (byFullName is not null)
            {
                return byFullName;
            }

            throw new InvalidOperationException(
                $"No type found with FullName '{typeName}' in the current application domain. " +
                $"Ensure the assembly containing this event type is loaded.");
        }
    }
}