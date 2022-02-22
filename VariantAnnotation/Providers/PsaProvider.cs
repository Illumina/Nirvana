using System;
using System.Collections.Generic;
using System.Linq;
using ErrorHandling.Exceptions;
using Genome;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.PSA;
using VariantAnnotation.SA;
using Versioning;

namespace VariantAnnotation.Providers
{
    public sealed class PsaProvider : IProvider, IDisposable
    {
        public           string                          Name               => "Protein SA provider";
        public           GenomeAssembly                  Assembly           { get; }
        public           IEnumerable<IDataSourceVersion> DataSourceVersions { get; }
        private readonly PsaReader[]                     _psaReaders;

        public PsaProvider(PsaReader[] psaReaders)
        {
            _psaReaders        = psaReaders;
            Assembly           = GetAssembly(psaReaders);
            DataSourceVersions = psaReaders.Select(x => x.Header.Version);
        }

        private GenomeAssembly GetAssembly(PsaReader[] psaReaders)
        {
            HashSet<GenomeAssembly> assemblies = psaReaders.Select(x => x.Header.Assembly).ToHashSet();
            if (assemblies.Count == 1) return assemblies.First();

            throw new UserErrorException($"Multiple genome assemblies detected in PSA files.");
        }

        public void Annotate(AnnotatedTranscript annotatedTranscript, int position, char altAllele)
        {
            var    transcript = annotatedTranscript.Transcript;
            ushort chrIndex   = transcript.Chromosome.Index;

            string transcriptId = transcript.Id;

            foreach (PsaReader psaReader in _psaReaders)
            {
                var scorePrediction = psaReader.GetScore(chrIndex, transcriptId, position, altAllele);
                if (scorePrediction.score == null) continue;
                
                var predictionScore = new PredictionScore(scorePrediction.prediction, scorePrediction.score.Value);

                switch (psaReader.Header.JsonKey)
                {
                    case SaCommon.SiftTag:
                        annotatedTranscript.AddSift(predictionScore);
                        break;
                    case SaCommon.PolyPhenTag:
                        annotatedTranscript.AddPolyPhen(predictionScore);
                        break;
                }
            }
        }

        public void Dispose()
        {
            if (_psaReaders == null) return;
            foreach (PsaReader psaReader in _psaReaders)
            {
                psaReader.Dispose();
            }
        }
    }
}