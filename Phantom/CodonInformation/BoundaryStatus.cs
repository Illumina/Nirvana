using System.Collections.Generic;

namespace Phantom.CodonInformation
{
    public sealed class BoundaryStatus
    {
        public List<int> Starts { get; }
        public List<int> Ends { get; }
        public List<int> SingleBaseBlocks { get; }

        public BoundaryStatus()
        {
            Starts = new List<int>();
            Ends = new List<int>();
            SingleBaseBlocks = new List<int>();
        }
    }
}