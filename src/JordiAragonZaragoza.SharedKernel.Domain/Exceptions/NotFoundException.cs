namespace JordiAragonZaragoza.SharedKernel.Domain.Exceptions
{
    using System;

    public class NotFoundException : Exception
    {
        public NotFoundException()
            : base()
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }

        public NotFoundException(string name, object key)
        : base($"{name}: {key} not found.")
        {
        }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}