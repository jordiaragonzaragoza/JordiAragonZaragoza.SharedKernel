namespace JordiAragonZaragoza.SharedKernel.Helpers
{
    using System;

    public static class OptionsHelper
    {
        /// <summary>
        /// Determines whether the specified string contains a configured value,
        /// excluding null, empty, whitespace, or unresolved configuration placeholders.
        /// </summary>
        /// <param name="value">The string value to check.</param>
        /// <returns>
        /// <see langword="true"/> if the value is not null, empty, whitespace, or an unresolved
        /// configuration placeholder; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsConfigured(string? value) =>
            !string.IsNullOrWhiteSpace(value) && !value.Contains('<', StringComparison.Ordinal);
    }
}