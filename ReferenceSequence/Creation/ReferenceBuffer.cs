namespace ReferenceSequence.Creation
{
    public sealed class ReferenceBuffer
    {
        public readonly ushort RefIndex;
        public readonly byte[] Buffer;
        public readonly int    BufferSize;

        public ReferenceBuffer(ushort refIndex, byte[] buffer, int bufferSize)
        {
            RefIndex   = refIndex;
            Buffer     = buffer;
            BufferSize = bufferSize;
        }
    }
}