using System.Threading.Tasks;

namespace Downloader.Utilities
{
    public static class SyncUtilities
    {
        public static T AsSync<T>(this Task<T> task) => task.ConfigureAwait(false).GetAwaiter().GetResult();
    }
}