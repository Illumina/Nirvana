using System;
using System.IO;
using System.Text;
using Cache.Data;
using IO;
using Xunit;

namespace UnitTests.Cache.Data;

public class CigarOpTests
{
    [Fact]
    public void Write_EndToEnd_ExpectedResults()
    {
        CigarOp expected = new(CigarType.Deletion, 23195);

        using var ms = new MemoryStream();
        using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
        {
            expected.Write(writer);
        }

        byte[]             bytes    = ms.ToArray();
        ReadOnlySpan<byte> byteSpan = bytes.AsSpan();
        CigarOp            actual   = CigarOp.Read(ref byteSpan);

        Assert.Equal(expected, actual);
    }
}