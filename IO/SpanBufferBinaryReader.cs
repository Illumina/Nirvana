using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace IO;

public static class SpanBufferBinaryReader
{
    private const int MostSignificantBit = 128;
    private const int VlqBitShift        = 7;

    public static ushort ReadOptUInt16(ref ReadOnlySpan<byte> byteSpan)
    {
        ushort value = 0;
        var    shift = 0;
        var    index = 0;

        while (shift != 21)
        {
            byte b = byteSpan[index++];
            value |= (ushort) ((b & sbyte.MaxValue) << shift);
            shift += VlqBitShift;

            // ReSharper disable once InvertIf
            if ((b & MostSignificantBit) == 0)
            {
                byteSpan = byteSpan.Slice(index);
                return value;
            }
        }

        throw new FormatException("Unable to read the 7-bit encoded unsigned short");
    }

    public static int ReadOptInt32(ref ReadOnlySpan<byte> byteSpan)
    {
        var value = 0;
        var shift = 0;
        var index = 0;

        while (shift != 35)
        {
            byte b = byteSpan[index++];
            value |= (b & sbyte.MaxValue) << shift;
            shift += VlqBitShift;

            // ReSharper disable once InvertIf
            if ((b & MostSignificantBit) == 0)
            {
                byteSpan = byteSpan.Slice(index);
                return value;
            }
        }

        throw new FormatException("Unable to read the 7-bit encoded integer");
    }

    public static long ReadOptInt64(ref ReadOnlySpan<byte> byteSpan)
    {
        long value = 0;
        var  shift = 0;
        var  index = 0;

        while (shift != 70)
        {
            byte b = byteSpan[index++];
            value |= (long) (b & sbyte.MaxValue) << shift;
            shift += VlqBitShift;

            // ReSharper disable once InvertIf
            if ((b & MostSignificantBit) == 0)
            {
                byteSpan = byteSpan.Slice(index);
                return value;
            }
        }

        throw new FormatException("Unable to read the 7-bit encoded long");
    }

    public static ulong ReadOptUInt64(ref ReadOnlySpan<byte> byteSpan)
    {
        ulong value = 0;
        var   shift = 0;
        var   index = 0;

        while (shift != 70)
        {
            byte b = byteSpan[index++];
            value |= (ulong) (b & sbyte.MaxValue) << shift;
            shift += VlqBitShift;

            // ReSharper disable once InvertIf
            if ((b & MostSignificantBit) == 0)
            {
                byteSpan = byteSpan.Slice(index);
                return value;
            }
        }

        throw new FormatException("Unable to read the 7-bit encoded ulong");
    }

    public static string ReadUtf8String(ref ReadOnlySpan<byte> byteSpan)
    {
        int numBytes = ReadOptInt32(ref byteSpan);
        if (numBytes == 0) return string.Empty;

        string value = Encoding.UTF8.GetString(byteSpan[..numBytes]);
        byteSpan = byteSpan.Slice(numBytes);

        return value;
    }

    public static string ReadAsciiString(ref ReadOnlySpan<byte> byteSpan)
    {
        int numBytes = ReadOptInt32(ref byteSpan);
        if (numBytes == 0) return string.Empty;

        string value = Encoding.ASCII.GetString(byteSpan[..numBytes]);
        byteSpan = byteSpan.Slice(numBytes);

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SkipString(ref ReadOnlySpan<byte> byteSpan)
    {
        int numBytes = ReadOptInt32(ref byteSpan);
        if (numBytes == 0) return;
        byteSpan = byteSpan.Slice(numBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ReadByte(ref ReadOnlySpan<byte> byteSpan)
    {
        byte value = byteSpan[0];
        byteSpan = byteSpan.Slice(1);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> ReadBytes(ref ReadOnlySpan<byte> byteSpan, int numBytes)
    {
        ReadOnlySpan<byte> value = byteSpan[..numBytes];
        byteSpan = byteSpan.Slice(numBytes);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadInt32(ref ReadOnlySpan<byte> byteSpan)
    {
        var value = MemoryMarshal.Read<int>(byteSpan);
        byteSpan = byteSpan.Slice(4);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadUInt64(ref ReadOnlySpan<byte> byteSpan)
    {
        var value = MemoryMarshal.Read<ulong>(byteSpan);
        byteSpan = byteSpan.Slice(8);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadUInt16(ref ReadOnlySpan<byte> byteSpan)
    {
        var value = MemoryMarshal.Read<ushort>(byteSpan);
        byteSpan = byteSpan.Slice(2);
        return value;
    }
}