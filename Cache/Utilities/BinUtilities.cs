using System.Runtime.CompilerServices;

namespace Cache.Utilities;

public static class BinUtilities
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetBin(int position) => (byte) ((position - 1) >> 20);
}