using System.Text.RegularExpressions;

namespace UpdateMiniCacheFiles
{
    interface IUpdater
    {
        UpdateStatus Update(string oldMiniCachePath, Match match);
    }
}
