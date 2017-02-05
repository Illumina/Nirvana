namespace CacheUtils.CreateCache.FileHandling
{
    internal interface IVepReader<out T>
    {
        T Next();
    }
}
