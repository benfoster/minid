using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Minid;

/// <summary>
/// URL friendly compact identifiers derived from Guids
/// </summary>
/// <remarks>
/// Based on @khellang's CompactId implementation https://gist.github.com/khellang/4993fcfbf8fb2ecdeccc2c822567037c
/// </remarks>
[TypeConverter(typeof(IdConverter))]
[Serializable]
public struct Id : IEquatable<Id>
{
    // Allocation free byte array ref https://vcsjones.dev/csharp-readonly-span-bytes-static/
    private const int Length = 26;
    private static ReadOnlySpan<byte> CharMap => new[]
    {
        (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9',
        (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'j', (byte)'k',
        (byte)'m', (byte)'n', (byte)'p', (byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'v', (byte)'w', (byte)'x',
        (byte)'y', (byte)'z'
    };

    private static readonly string EmptyString = new('0', Length);

    private readonly Guid _value;

    public Id(Guid value)
    {
        _value = value;
    }

    public static Id NewId() => new(Guid.NewGuid());
    public static Id Empty => new(Guid.Empty);

    public static bool TryParse(ReadOnlySpan<char> value, out Id result)
    {
        if (value.Length != Length)
        {
            result = default;
            return false;
        }

        Span<byte> source = stackalloc byte[Length];

        Encoding.ASCII.GetBytes(value, source);

        if (Decoder.TryDecode(source, out Guid decoded))
        {
            result = new Id(decoded);
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Converts the ID to a base-32 encoded value
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Encode(_value);

    /// <summary>
    /// Encodes the provided Guid value
    /// </summary>
    /// <param name="value">The Guid value to derive the ID from</param>
    /// <returns></returns>
    private static string Encode(Guid value)
    {
        if (value == Guid.Empty)
        {
            return EmptyString;
        }

        // Do not use the value parameter as it would introduce a closure
        // that cannot be cached
        // Ref https://www.meziantou.net/some-performance-tricks-with-dotnet-strings.htm#using-string-create
        return string.Create(
            Length, value, (buffer, state) => Encoder.Encode(state, buffer)
        );
    }

    private static Span<ulong> AsSpan<T>(ref T value) where T : unmanaged
    {
        // Note that this will produce different outputs depending on the endianess of the host
        return MemoryMarshal.Cast<T, ulong>(MemoryMarshal.CreateSpan(ref value, 1));
    }

    public override bool Equals(object? obj) => obj is Id id && Equals(id);

    public bool Equals(Id other) => other._value.Equals(_value);

    public override int GetHashCode() => _value.GetHashCode();

    public static bool operator ==(Id left, Id right) => left.Equals(right);
    public static bool operator !=(Id left, Id right) => !left.Equals(right);

    private static class Encoder
    {
        public static void Encode(Guid value, Span<char> target)
        {
            Span<ulong> longs = AsSpan(ref value);

            EncodeUInt64(target.Slice(0, Length / 2), longs[0]);
            EncodeUInt64(target.Slice(Length / 2), longs[1]);
        }

        private static void EncodeUInt64(Span<char> chars, ulong result)
        {
            var index = 0;

            // Because a GUID is 128 bits and 26 characters with 5 bits each is 130
            // we limit the 1st and 13th character to 4 bits (hex).
            chars[index++] = (char)CharMap[(int)(result >> 60)];
            result <<= 4;

            while (index < chars.Length)
            {
                // Each following character carries 5 bits each.
                chars[index++] = (char)CharMap[(int)(result >> 59)];
                result <<= 5;
            }
        }
    }

    private static class Decoder
    {
        private static readonly int[] AsciiMapping = GenerateAsciiMapping();

        public static bool TryDecode(ReadOnlySpan<byte> bytes, out Guid result)
        {
            result = Guid.Empty;

            Span<ulong> longs = AsSpan(ref result);

            return TryDecodeUInt64(bytes.Slice(0, Length / 2), ref longs[0])
                && TryDecodeUInt64(bytes.Slice(Length / 2), ref longs[1]);
        }

        private static bool TryDecodeUInt64(ReadOnlySpan<byte> bytes, ref ulong result)
        {
            for (var i = 0; i < bytes.Length; i++)
            {
                var index = bytes[i];

                if (index >= AsciiMapping.Length)
                {
                    return false; // Not ASCII.
                }

                var value = AsciiMapping[index];

                if (value == -1)
                {
                    return false; // Invalid ASCII character.
                }

                result = (result << 5) | (uint)value;
            }

            return true;
        }

        private static int[] GenerateAsciiMapping()
        {
            const char start = '\x00', end = '\x7F';

            var mapping = new int[end - start + 1];

            for (var i = start; i <= end; i++)
            {
                mapping[i] = CharMap.IndexOf((byte)char.ToLower(i));
            }

            // Treat "o" as 0
            mapping['o'] = mapping['O'] = 0;

            // Treat "i", "I", "l", "L" as 1
            mapping['i'] = mapping['I'] = mapping['l'] = mapping['L'] = 1;

            return mapping;
        }
    }
}