using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Compression.FileHandling;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Utilities;

namespace SAUtils.TsvWriters
{
    public sealed class LiteGnomadTsvWriter : ISaItemTsvWriter
    {
        private readonly StreamWriter _writer;
        private readonly ISequenceProvider _sequenceProvider;
        public void Dispose()
        {
            _writer?.Dispose();
        }

        public LiteGnomadTsvWriter(string fileName, ISequenceProvider sequenceProvider)
        {
            _sequenceProvider = sequenceProvider;
            _writer = new StreamWriter(new BlockGZipStream(FileUtilities.GetCreateStream(fileName), CompressionMode.Compress));
        }

        public void WritePosition(IEnumerable<SupplementaryDataItem> saItems)
        {
            if (saItems == null) return;

            var gnomadItems = new List<GnomadItem>();
            foreach (var item in saItems)
            {
                if (!(item is GnomadItem gnomadItem))
                    throw new InvalidDataException("Expected GnomadItems list!!");
                gnomadItems.Add(gnomadItem);
            }

            SupplementaryDataItem.RemoveConflictingAlleles(gnomadItems);


            foreach (var gnomadItem in gnomadItems)
            {
                AddEntry(gnomadItem.Chromosome.EnsemblName, gnomadItem.Start, gnomadItem.ReferenceAllele, gnomadItem.AlternateAllele, gnomadItem.GetJsonString());
            }
        }

        private void AddEntry(string chromosome, int position, string refAllele, string altAllele, string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString)) return;
            if (!SaUtilsCommon.ValidateReference(chromosome, position, refAllele, _sequenceProvider)) return;

            refAllele = string.IsNullOrEmpty(refAllele) ? "-" : refAllele;
            altAllele = string.IsNullOrEmpty(altAllele) ? "-" : altAllele;

            _writer.WriteLine($"{chromosome}\t{position}\t{refAllele}\t{altAllele}\t\t{jsonString}");
        }
    }
}