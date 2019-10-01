using System;
using System.IO;
using System.Net;

namespace IO
{
    public interface IConnect
    {
        (HttpWebResponse Response, Stream Stream) Connect(long position);
    }
}
