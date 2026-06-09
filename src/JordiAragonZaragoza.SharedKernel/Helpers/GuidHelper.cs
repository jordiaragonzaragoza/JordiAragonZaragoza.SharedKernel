namespace JordiAragonZaragoza.SharedKernel.Helpers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Cryptography;
    using System.Text;

    public static class GuidHelper
    {
        [SuppressMessage(
            "Security",
            "CA5350:Do Not Use Weak Cryptographic Algorithms",
            Justification = "SHA-1 is required by RFC 4122 §4.3 for UUID v5 generation. This is not a cryptographic use.")]
        public static Guid CreateDeterministicGuid(Guid namespaceId, string name)
        {
            ArgumentNullException.ThrowIfNull(name);

            // Get the bytes of the namespace GUID (in .NET's native format / Little-Endian)
            Span<byte> namespaceBytes = stackalloc byte[16];
            namespaceId.TryWriteBytes(namespaceBytes);

            // Convert to Big-Endian (Network Byte Order) as required by RFC 4122
            SwapByteOrder(namespaceBytes);

            // Encode the name string to UTF-8 bytes
            int nameByteCount = Encoding.UTF8.GetByteCount(name);
            Span<byte> nameBytes = nameByteCount <= 512
                ? stackalloc byte[nameByteCount]
                : new byte[nameByteCount];
            Encoding.UTF8.GetBytes(name, nameBytes);

            // Combine both buffers for the hash input
            int totalLength = namespaceBytes.Length + nameBytes.Length;
            Span<byte> combinedBuffer = totalLength <= 1024
                ? stackalloc byte[totalLength]
                : new byte[totalLength];
            namespaceBytes.CopyTo(combinedBuffer);
            nameBytes.CopyTo(combinedBuffer[namespaceBytes.Length..]);

            // Compute SHA-1 hash (20 bytes), take the first 16
            Span<byte> hash = stackalloc byte[20];
            SHA1.HashData(combinedBuffer, hash);
            Span<byte> newGuidBytes = hash[..16];

            // Set version bits to 5 (0x50) in byte 6
            newGuidBytes[6] = (byte)((newGuidBytes[6] & 0x0F) | 0x50);

            // Set variant bits to RFC 4122 (0x80) in byte 8
            newGuidBytes[8] = (byte)((newGuidBytes[8] & 0x3F) | 0x80);

            // Convert back from Big-Endian to .NET's internal Little-Endian layout
            SwapByteOrder(newGuidBytes);

            return new Guid(newGuidBytes);
        }

        private static void SwapByteOrder(Span<byte> guid)
        {
            guid[0..4].Reverse(); // Data1 (int)
            guid[4..6].Reverse(); // Data2 (short)
            guid[6..8].Reverse(); // Data3 (short)
        }
    }
}