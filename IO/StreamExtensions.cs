using System.IO;

namespace IO
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Reads a specified number of bytes from the stream
        /// </summary>
        /// <returns>number of bytes read</returns>
        public static int ForcedRead(this Stream stream, byte[] buffer, int offset, int numBytesToRead)
        {
            var numBytesRead = 0;

            while (numBytesToRead > 0)
            {
                int count = stream.Read(buffer, offset, numBytesToRead);
                if (count == 0) return numBytesRead;

                offset         += count;
                numBytesRead   += count;
                numBytesToRead -= count;
            }

            return numBytesRead;
        }
    }
}
