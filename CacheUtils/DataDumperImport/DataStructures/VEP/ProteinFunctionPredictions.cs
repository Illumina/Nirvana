using System;
using System.IO;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.DataStructures.VEP
{
    public sealed class ProteinFunctionPredictions
    {
        public PolyPhen PolyPhen = null;
        public Sift Sift         = null;

        public static void Serialize(BinaryWriter writer, string matrix, ushort refIndex)
        {
            // convert the base 64 string representation to our compressed prediction data
            var uncompressedDataWithHeader = Convert.FromBase64String(matrix);
            const int headerLength = 3;

            // skip the 'VEP' header
            int newLength = uncompressedDataWithHeader.Length - headerLength;

            // sanity check: we should have an even number of bytes
            if ((newLength & 1) != 0)
            {
                throw new GeneralException($"Expected an even number of bytes when serializing the protein function prediction matrix: {newLength}");
            }

            var shorts = new short[newLength / 2];

            Buffer.BlockCopy(uncompressedDataWithHeader, headerLength, shorts, 0, newLength);

            writer.Write(refIndex);
            writer.Write(shorts.Length);
            foreach(var s in shorts) writer.Write(s);
        }
    }
}
