using System;
using System.Collections.Generic;
using SAUtils.DataStructures;

namespace SAUtils.Interface
{
    public interface ITsvReader : IDisposable
    {
        SaHeader SaHeader { get; }
        IEnumerable<string> RefNames{ get; }
    }
}