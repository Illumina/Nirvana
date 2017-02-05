using System.Collections.Generic;
using CacheUtils.UpdateMiniCacheFiles.DataStructures;

namespace CacheUtils.UpdateMiniCacheFiles.Updaters
{
    public interface IUpdater
    {
        UpdateStatus Update(DataBundle bundle, string oldMiniCachePath, ushort desiredVepVersion, List<string> outputFiles);
        ushort RefIndex { get; }
        string TranscriptDataSource { get; }
    }
}
