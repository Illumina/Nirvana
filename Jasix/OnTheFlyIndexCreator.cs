using System;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using Jasix.DataStructures;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;

namespace Jasix
{
    public sealed class OnTheFlyIndexCreator : IDisposable
    {
        private readonly Stream _indexStream;
        private readonly JasixIndex _jasixIndex;
        private int _lastPosition;
        private string _lastChromName;

        
        public OnTheFlyIndexCreator(Stream indexStream)
        {
            _indexStream = indexStream;
            _jasixIndex  = new JasixIndex();
        }

        public void SetHeader(string header)
        {
            _jasixIndex.HeaderLine = header;
        }

        public void Add(IPosition position, long fileLocation)
        {
            var chromName = position.VcfFields[VcfCommon.ChromIndex];
            var start     = position.Start;
            var end       = position.InfoData.End;

            if (chromName == _lastChromName && start < _lastPosition)
            {
                throw new UserErrorException($"The Json file is not sorted at {position.Chromosome.UcscName}: {start}");
            }

            _lastPosition  = start;
            _lastChromName = chromName;

            if (end == null)
            {
                var altAlleles = position.AltAlleles;
                int altAlleleOffset = altAlleles != null && altAlleles.All(Utilities.IsNucleotideAllele) && altAlleles.Any(x => x.Length > 1) ? 1 : 0;

                end = Math.Max(position.RefAllele.Length - 1, altAlleleOffset) + start;
            }

            _jasixIndex.Add(position.Chromosome.EnsemblName, start, end.Value, fileLocation);
        }

        public void Dispose()
        {
            _jasixIndex.Write(_indexStream);
            _indexStream.Flush();
            _indexStream.Dispose();
        }
    }
}