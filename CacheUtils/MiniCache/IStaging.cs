using System.IO;

namespace CacheUtils.MiniCache
{
    public interface IStaging
    {
        void Write(Stream stream);
    }
}
