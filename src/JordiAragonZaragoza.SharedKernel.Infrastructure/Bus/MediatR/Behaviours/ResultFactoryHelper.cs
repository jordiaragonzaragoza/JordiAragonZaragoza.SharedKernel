namespace JordiAragonZaragoza.SharedKernel.Infrastructure.Bus.MediatR.Behaviours
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Ardalis.Result;

    /// <summary>
    /// Helper to create TResponse instances from ResultStatus and error messages,
    /// using reflection, caching MethodInfo to minimize runtime cost.
    /// </summary>
    public static class ResultFactoryHelper
    {
        private static readonly ConcurrentDictionary<(Type, string), MethodInfo> MethodCache = new();

        public static TResponse CreateFromStatus<TResponse>(
            ResultStatus status,
            IEnumerable<string>? errors = null)
            where TResponse : IResult
        {
            var methodName = status.ToString();
            var responseType = typeof(TResponse);

            var methodWithString = GetCachedMethod(responseType, methodName, [typeof(string)]);
            if (methodWithString != null)
            {
                var message = GetFirstError(errors) ?? $"Operation failed with status: {status}";
                return Invoke<TResponse>(methodWithString, [message]);
            }

            var methodNoParams = GetCachedMethod(responseType, methodName, Type.EmptyTypes)
                ?? throw new InvalidOperationException(
                    $"Method not found '{methodName}' in {responseType.FullName}.");

            return Invoke<TResponse>(methodNoParams, null);
        }

        public static TResponse CreateInvalid<TResponse>(IEnumerable<ValidationError> validationErrors)
            where TResponse : IResult
        {
            var responseType = typeof(TResponse);

            var method = GetCachedMethod(responseType, "Invalid", [typeof(IEnumerable<ValidationError>)])
                ?? throw new InvalidOperationException(
                    $"Method not found 'Invalid(IEnumerable<ValidationError>)' in {responseType.FullName}.");

            return Invoke<TResponse>(method, [validationErrors]);
        }

        private static MethodInfo? GetCachedMethod(Type type, string methodName, Type[] paramTypes)
        {
            var cacheKey = BuildKey(type, methodName, paramTypes);

            if (MethodCache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            var found = type.GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.Public,
                binder: null,
                paramTypes,
                modifiers: null);

            if (found != null)
            {
                MethodCache[cacheKey] = found;
            }

            return found;
        }

        private static TResponse Invoke<TResponse>(MethodInfo method, object?[]? args)
        {
            var result = method.Invoke(null, args)
                ?? throw new InvalidOperationException(
                    $"Method '{method.Name}' returned null in {method.DeclaringType?.FullName}.");

            return (TResponse)result;
        }

        private static string? GetFirstError(IEnumerable<string>? errors)
            => errors?.FirstOrDefault();

        private static (Type Type, string MethodName) BuildKey(Type type, string methodName, Type[] paramTypes)
            => (type, paramTypes.Length == 0 ? methodName : $"{methodName}({string.Join(",", Array.ConvertAll(paramTypes, t => t.Name))})");
    }
}