using System;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using Jasix.DataStructures;
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

        public void Add(IPosition position, long fileLocation)
        {
            string chromName = position.Chromosome.EnsemblName;
            int start        = position.Start;
            int? end          = position.InfoData?.End;

            if (chromName == _lastChromName && start < _lastPosition)
            {
                throw new UserErrorException($"The Json file is not sorted at {position.Chromosome.UcscName}: {start}");
            }

            _lastPosition  = start;
            _lastChromName = chromName;

            if (end == null)
            {
                string[] altAlleles = position.AltAlleles;
                int altAlleleOffset = altAlleles != null && altAlleles.All(Utilities.IsNucleotideAllele) && altAlleles.Any(x => x.Length > 1) ? 1 : 0;

                end = Math.Max(position.RefAllele.Length - 1, altAlleleOffset) + start;
            }

            _jasixIndex.Add(position.Chromosome.EnsemblName, start, end.Value, fileLocation, position.Chromosome.UcscName);
        }

        public void BeginSection(string sectionName, long fileLocation)
        {
            _jasixIndex.BeginSection(sectionName, fileLocation);
        }

        public void EndSection(string sectionName, long fileLocation)
        {
            _jasixIndex.EndSection(sectionName, fileLocation);
        }

        public void Dispose()
        {
            _jasixIndex.Write(_indexStream);
            _indexStream.Flush();
            _indexStream.Dispose();
        }
    }
}