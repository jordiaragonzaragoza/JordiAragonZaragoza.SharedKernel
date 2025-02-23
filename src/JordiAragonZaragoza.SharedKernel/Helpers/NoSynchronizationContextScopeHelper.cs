namespace JordiAragonZaragoza.SharedKernel.Helpers
{
    using System;
    using System.Threading;

    public static class NoSynchronizationContextScopeHelper
    {
        public static Disposable Enter()
        {
            var context = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            return new Disposable(context);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Ok for this helper")]
        public readonly struct Disposable : IDisposable, IEquatable<Disposable>
        {
            private readonly SynchronizationContext? synchronizationContext;

            public Disposable(SynchronizationContext? synchronizationContext)
            {
                this.synchronizationContext = synchronizationContext;
            }

            public static bool operator ==(Disposable left, Disposable right) =>
                left.Equals(right);

            public static bool operator !=(Disposable left, Disposable right) =>
                !(left == right);

            public void Dispose()
            {
                SynchronizationContext.SetSynchronizationContext(this.synchronizationContext);
            }

            public override bool Equals(object? obj)
            {
                return obj is Disposable other && this.Equals(other);
            }

            public bool Equals(Disposable other)
            {
                return Equals(this.synchronizationContext, other.synchronizationContext);
            }

            public override int GetHashCode()
            {
                return this.synchronizationContext?.GetHashCode() ?? 0;
            }
        }
    }
}