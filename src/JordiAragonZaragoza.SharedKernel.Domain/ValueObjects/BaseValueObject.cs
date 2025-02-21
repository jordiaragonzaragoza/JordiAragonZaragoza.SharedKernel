namespace JordiAragonZaragoza.SharedKernel.Domain.ValueObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using JordiAragonZaragoza.SharedKernel.Contracts.DependencyInjection;
    using JordiAragonZaragoza.SharedKernel.Domain.Contracts.Interfaces;
    using JordiAragonZaragoza.SharedKernel.Domain.Exceptions;

    // See: https://enterprisecraftsmanship.com/posts/value-object-better-implementation/
    [Serializable]
    [SuppressMessage("Minor Code Smell", "S1210:\"Equals\" and the comparison operators should be overridden when implementing \"IComparable\"", Justification = "Ok for BaseValueObject.")]
    [SuppressMessage("Design", "CA1036:Override methods on comparable types", Justification = "Ok for BaseValueObject.")]
    public abstract class BaseValueObject : IComparable, IComparable<BaseValueObject>, IIgnoreDependency
    {
        private int? cachedHashCode;

        public static bool operator ==(BaseValueObject a, BaseValueObject b)
        {
            if (a is null && b is null)
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(BaseValueObject a, BaseValueObject b) => !(a == b);

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (GetUnproxiedType(this) != GetUnproxiedType(obj))
            {
                return false;
            }

            var valueObject = (BaseValueObject)obj;

            return this.GetEqualityComponents().SequenceEqual(valueObject.GetEqualityComponents());
        }

        [SuppressMessage("Minor Bug", "S2328:\"GetHashCode\" should not reference mutable fields", Justification = "Ok for BaseValueObject.")]
        public override int GetHashCode()
        {
            if (!this.cachedHashCode.HasValue)
            {
                this.cachedHashCode = this.GetEqualityComponents()
                    .Aggregate(1, static (current, obj) =>
                    {
                        unchecked
                        {
                            return (current * 23) + (obj?.GetHashCode() ?? 0);
                        }
                    });
            }

            return this.cachedHashCode.Value;
        }

        public int CompareTo(object? obj)
        {
            if (obj == null)
            {
                return 1;
            }

            var thisType = GetUnproxiedType(this);
            var otherType = GetUnproxiedType(obj);

            if (thisType != otherType)
            {
                return string.Compare(thisType.ToString(), otherType.ToString(), StringComparison.Ordinal);
            }

            var other = (BaseValueObject)obj;

            var components = this.GetEqualityComponents().ToArray();
            var otherComponents = other.GetEqualityComponents().ToArray();

            for (var i = 0; i < components.Length; i++)
            {
                var comparison = CompareComponents(components[i], otherComponents[i]);
                if (comparison != 0)
                {
                    return comparison;
                }
            }

            return 0;
        }

        public int CompareTo(BaseValueObject? other)
        {
            return this.CompareTo(other as object);
        }

        internal static Type GetUnproxiedType(object obj)
        {
            ArgumentNullException.ThrowIfNull(obj);

            const string EFCoreProxyPrefix = "Castle.Proxies.";
            const string NHibernateProxyPostfix = "Proxy";

            var type = obj.GetType();
            var typeString = type.ToString();

            if (typeString.Contains(EFCoreProxyPrefix, StringComparison.InvariantCulture)
                || typeString.EndsWith(NHibernateProxyPostfix, StringComparison.InvariantCulture))
            {
                return type.BaseType!;
            }

            return type;
        }

        protected static void CheckRule(IBusinessRule rule)
        {
            ArgumentNullException.ThrowIfNull(rule, nameof(rule));

            if (rule.IsBroken())
            {
                throw new BusinessRuleValidationException(rule);
            }
        }

        protected abstract IEnumerable<object> GetEqualityComponents();

        private static int CompareComponents(object object1, object object2)
        {
            if (object1 is null && object2 is null)
            {
                return 0;
            }

            if (object1 is null)
            {
                return -1;
            }

            if (object2 is null)
            {
                return 1;
            }

            if (object1 is IComparable comparable1 && object2 is IComparable comparable2)
            {
                return comparable1.CompareTo(comparable2);
            }

            return object1.Equals(object2) ? 0 : -1;
        }
    }
}