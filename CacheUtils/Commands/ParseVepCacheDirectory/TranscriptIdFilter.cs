using System.IO;
using System.Linq;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.Commands.ParseVepCacheDirectory
{
    public sealed class TranscriptIdFilter
    {
        private readonly string[] _whitelist;

        public TranscriptIdFilter(Source source)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (source)
            {
                case Source.Ensembl:
                    _whitelist = new[] { "ENSE0", "ENSG0", "ENSP0", "ENST0" };
                    break;
                case Source.RefSeq:
                    _whitelist = new[] { "NG_", "NM_", "NP_", "NR_", "XM_", "XP_", "XR_", "YP_" };
                    break;
                default:
                    throw new InvalidDataException($"Unhandled import mode found: {source}");
            }
        }

        public bool Pass(MutableTranscript transcript) => _whitelist.Any(prefix => transcript.Id.StartsWith(prefix)) && !transcript.Id.Contains("dupl");
    }
}
