using System.Collections.Generic;
using SAUtils.DataStructures;

namespace SAUtils.Interface
{
    public interface ITsvReader
    {
        SaHeader SaHeader { get; }
        IEnumerable<string> RefNames{ get; }
    }
}