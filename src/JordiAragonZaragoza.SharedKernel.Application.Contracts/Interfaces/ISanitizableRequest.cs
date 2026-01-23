namespace JordiAragonZaragoza.SharedKernel.Application.Contracts.Interfaces
{
    /// <summary>
    /// Defines a contract for requests that contain sensitive information
    /// and require secure representation for logging or traceability purposes.
    /// Implement this interface for commands or queries that need to hide,
    /// anonymize, or transform part of their data before logging it.
    /// </summary>
    public interface ISanitizableRequest
    {
        /// <summary>
        /// Returns a secure representation of the current object,
        /// removing or anonymizing sensitive information for logging purposes.
        /// </summary>
        /// <returns>
        /// An object that securely represents the current request, with its sensitive fields processed.
        /// </returns>
        object GetSanitized();
    }
}