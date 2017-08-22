namespace Compression.Algorithms
{
    public interface ICompressionAlgorithm
    {        
        int Compress(byte[] source, int srcLength, byte[] destination, int destLength);
        int Decompress(byte[] source, int srcLength, byte[] destination, int destLength);
        int GetDecompressedLength(byte[] source, int srcLength);
        int GetCompressedBufferBounds(int srcLength);
    }
}
