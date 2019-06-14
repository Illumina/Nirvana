using System.Collections.Generic;
using Downloader;

namespace UnitTests.Downloader
{
    internal sealed class RemoteFileComparer : EqualityComparer<RemoteFile>
    {
        public override bool Equals(RemoteFile x, RemoteFile y)
        {
            return x.LocalPath   == y.LocalPath  &&
                   x.RemotePath  == y.RemotePath &&
                   x.Description == y.Description;
        }

        public override int GetHashCode(RemoteFile obj)
        {
            unchecked
            {
                int hashCode = obj.RemotePath.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.LocalPath.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Description.GetHashCode();
                return hashCode;
            }
        }
    }
}

