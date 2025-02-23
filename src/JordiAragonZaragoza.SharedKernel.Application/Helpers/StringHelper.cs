namespace JordiAragonZaragoza.SharedKernel.Application.Helpers
{
    using System;
    using System.IO;
    using System.Text;

    // TODO: Remove. Not used.
    public static class StringHelper
    {
        public static Stream ConvertStreamFromText(this string text)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(text));
        }

        public static Stream ConvertStreamFromBase64(this string base64String)
        {
            var bytes = Convert.FromBase64String(base64String);
            return new MemoryStream(bytes);
        }

        public static string ToLowerFirstCharacter(this string text)
        {
            if (string.IsNullOrEmpty(text) || char.IsLower(text, 0))
            {
                return text;
            }

            return char.ToLowerInvariant(text[0]) + text[1..];
        }
    }
}