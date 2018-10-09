using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using VariantAnnotation.Interface.Providers;
using static IO.FileUtilities;

namespace VariantAnnotation.PhyloP
{
    public static class PhylopCommon
    {
        public const string Header = "NirvanaPhylopDB";
        public const ushort SchemaVersion = 4;
        public const ushort DataVersion = 1;

        public const int MaxIntervalLength = 4048;
        public const string FileExtension = ".npd";
    }
}
