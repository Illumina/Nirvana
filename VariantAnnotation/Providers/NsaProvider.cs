using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorHandling.Exceptions;
using Genome;
using VariantAnnotation.GeneFusions.IO;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using Variants;

namespace VariantAnnotation.Providers
{
    public sealed class NsaProvider : IAnnotationProvider
    {
        public string                          Name               => "Supplementary annotation provider";
        public GenomeAssembly                  Assembly           { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }

        private readonly INsaReader[]          _nsaReaders;
        private readonly INsiReader[]          _nsiReaders;
        private readonly IGeneFusionSaReader[] _fusionReaders;

        private readonly bool _hasFusionReaders;
        private          bool _hasLoadedGeneFusions;

        private readonly List<(string refAllele, string altAllele, string jsonString)> _annotations = new();

        public NsaProvider(INsaReader[] nsaReaders, INsiReader[] nsiReaders, IGeneFusionSaReader[] fusionReaders)
        {
            _nsaReaders    = nsaReaders;
            _nsiReaders    = nsiReaders;
            _fusionReaders = fusionReaders;

            if (fusionReaders != null && fusionReaders.Length > 0) _hasFusionReaders = true;

            (List<ISaMetadata> variant, List<ISaMetadata> position, List<ISaMetadata> all) = OrganizeReaders(nsaReaders, nsiReaders, fusionReaders);

            (Assembly, DataSourceVersions) = GetReaderMetadata(all);
            CheckDuplicatePositionKeys(position);
            CheckDuplicateVariantKeys(variant);
        }

        private static (List<ISaMetadata> Variant, List<ISaMetadata> Position, List<ISaMetadata> All) OrganizeReaders(
            INsaReader[] nsaReaders, INsiReader[] nsiReaders, IGeneFusionSaReader[] fusionReaders)
        {
            List<ISaMetadata> variant  = new();
            List<ISaMetadata> position = new();
            List<ISaMetadata> all      = new();

            if (nsaReaders != null)
            {
                foreach (INsaReader reader in nsaReaders)
                {
                    variant.Add(reader);
                    all.Add(reader);
                }
            }

            if (nsiReaders != null)
            {
                foreach (INsiReader reader in nsiReaders)
                {
                    position.Add(reader);
                    all.Add(reader);
                }
            }

            if (fusionReaders != null)
            {
                foreach (IGeneFusionSaReader reader in fusionReaders)
                {
                    variant.Add(reader);
                    all.Add(reader);
                }
            }

            return (variant, position, all);
        }

        private static void CheckDuplicateVariantKeys(List<ISaMetadata> readers)
        {
            var jsonKeys = new HashSet<string>();
            foreach (ISaMetadata reader in readers) CheckJsonKey(reader.JsonKey, "variant-level (.nsa or fusion)", jsonKeys);
        }

        private static void CheckDuplicatePositionKeys(List<ISaMetadata> readers)
        {
            var jsonKeys = new HashSet<string>();
            foreach (ISaMetadata reader in readers) CheckJsonKey(reader.JsonKey, "position-level (.nsi)", jsonKeys);
        }

        private static void CheckJsonKey(string jsonKey, string description, HashSet<string> jsonKeys)
        {
            if (jsonKeys.Contains(jsonKey)) throw new UserErrorException($"Duplicate {description} JSON keys found for: {jsonKey}");
            jsonKeys.Add(jsonKey);
        }

        private static (GenomeAssembly Assembly, IEnumerable<IDataSourceVersion> Versions) GetReaderMetadata(List<ISaMetadata> readers)
        {
            HashSet<GenomeAssembly>  assemblies = new();
            List<IDataSourceVersion> versions   = new();
            var                      sb         = new StringBuilder();

            foreach (ISaMetadata reader in readers)
            {
                if (reader.Assembly != GenomeAssembly.rCRS && reader.Assembly != GenomeAssembly.Unknown) assemblies.Add(reader.Assembly);
                versions.Add(reader.Version);
                sb.AppendLine($"{reader.Version}, Assembly: {reader.Assembly}");
            }

            if (assemblies.Count == 1) return (assemblies.First(), versions);

            throw new UserErrorException($"Multiple genome assemblies detected in Supplementary annotation directory.\n{sb}");
        }

        public void Annotate(IAnnotatedPosition annotatedPosition)
        {
            if (_nsaReaders != null) AddPositionAndAlleleAnnotations(annotatedPosition);
            if (_nsiReaders != null) GetStructuralVariantAnnotations(annotatedPosition);
            if (_hasFusionReaders && annotatedPosition.Position.HasStructuralVariant) GetGeneFusionAnnotations(annotatedPosition);
        }

        private void GetGeneFusionAnnotations(IAnnotatedPosition annotatedPosition)
        {
            foreach (IAnnotatedVariant variant in annotatedPosition.AnnotatedVariants)
            {
                IGeneFusionPair[] fusionPairs = GetGeneFusionPairs(variant);
                if (fusionPairs == null) continue;

                // this only needs to happen if we have a gene fusion
                if (!_hasLoadedGeneFusions) LoadGeneFusions();

                foreach (IGeneFusionSaReader reader in _fusionReaders) reader.AddAnnotations(fusionPairs, variant.SaList);
            }
        }

        private void LoadGeneFusions()
        {
            foreach (IGeneFusionSaReader reader in _fusionReaders) reader.LoadAnnotations();
            _hasLoadedGeneFusions = true;
        }

        private static IGeneFusionPair[] GetGeneFusionPairs(IAnnotatedVariant variant)
        {
            var fusionPairs = new HashSet<IGeneFusionPair>();
            foreach (IAnnotatedTranscript transcript in variant.Transcripts) transcript.AddGeneFusionPairs(fusionPairs);
            return fusionPairs.Count == 0 ? null : fusionPairs.ToArray();
        }

        private void GetStructuralVariantAnnotations(IAnnotatedPosition annotatedPosition)
        {
            bool needSaIntervals     = annotatedPosition.AnnotatedVariants.Any(x => x.Variant.Behavior.NeedSaInterval);
            bool needSmallAnnotation = annotatedPosition.AnnotatedVariants.Any(x => x.Variant.Behavior == AnnotationBehavior.SmallVariants);

            foreach (INsiReader nsiReader in _nsiReaders)
            {
                IPosition position = annotatedPosition.Position;
                if (nsiReader.ReportFor == ReportFor.SmallVariants      && !needSmallAnnotation) continue;
                if (nsiReader.ReportFor == ReportFor.StructuralVariants && !needSaIntervals) continue;

                IEnumerable<string> annotations = nsiReader.GetAnnotation(position.Variants[0]);
                if (annotations == null) continue;

                annotatedPosition.SupplementaryIntervals.Add(new SupplementaryAnnotation(nsiReader.JsonKey, true, false, null, annotations));
            }
        }

        private void AddPositionAndAlleleAnnotations(IAnnotatedPosition annotatedPosition)
        {
            foreach (IAnnotatedVariant annotatedVariant in annotatedPosition.AnnotatedVariants)
            {
                bool needSaPosition = annotatedVariant.Variant.Behavior.NeedSaPosition;
                bool needSaAllele   = annotatedVariant.Variant.Behavior.NeedSaAllele;
                if (!needSaPosition && !needSaAllele) continue;
                AddSmallAnnotations(annotatedVariant, needSaPosition, needSaAllele);
            }
        }

        private void AddSmallAnnotations(IAnnotatedVariant annotatedVariant, bool needSaPosition, bool needSaAllele)
        {
            foreach (INsaReader nsaReader in _nsaReaders)
            {
                IVariant variant = annotatedVariant.Variant;
                nsaReader.GetAnnotation(variant.Start, _annotations);
                if (_annotations.Count == 0) continue;

                if (nsaReader.IsPositional && needSaPosition)
                {
                    AddPositionalAnnotation(_annotations, annotatedVariant, nsaReader);
                    continue;
                }

                if (nsaReader.MatchByAllele && needSaAllele) AddAlleleSpecificAnnotation(nsaReader, _annotations, annotatedVariant, variant);

                else AddNonAlleleSpecificAnnotations(_annotations, variant, annotatedVariant, nsaReader);
            }
        }

        private static void AddPositionalAnnotation(IEnumerable<(string refAllele, string altAllele, string annotation)> annotations,
                                                    IAnnotatedVariant annotatedVariant, INsaReader nsaReader)
        {
            // e.g. ancestral allele, global minor allele
            string jsonString = annotations.First().annotation;
            annotatedVariant.SaList.Add(new SupplementaryAnnotation(nsaReader.JsonKey, nsaReader.IsArray, nsaReader.IsPositional, jsonString, null));
        }

        private static void AddNonAlleleSpecificAnnotations(IEnumerable<(string refAllele, string altAllele, string annotation)> annotations,
                                                            IVariant variant, IAnnotatedVariant annotatedVariant, INsaReader nsaReader)
        {
            var jsonStrings = new List<string>();
            foreach ((string refAllele, string altAllele, string jsonString) in annotations)
            {
                if (refAllele == variant.RefAllele && altAllele == variant.AltAllele) jsonStrings.Add(jsonString + ",\"isAlleleSpecific\":true");
                else jsonStrings.Add(jsonString);
            }

            if (jsonStrings.Count > 0)
                annotatedVariant.SaList.Add(new SupplementaryAnnotation(nsaReader.JsonKey, nsaReader.IsArray, nsaReader.IsPositional, null,
                    jsonStrings));
        }

        private static void AddAlleleSpecificAnnotation(INsaReader nsaReader,
                                                        IEnumerable<(string refAllele, string altAllele, string annotation)> annotations,
                                                        IAnnotatedVariant annotatedVariant, IVariant variant)
        {
            if (nsaReader.IsArray)
            {
                var jsonStrings = new List<string>();
                foreach ((string refAllele, string altAllele, string jsonString) in annotations)
                {
                    if (refAllele == variant.RefAllele && altAllele == variant.AltAllele)
                        jsonStrings.Add(jsonString);
                }

                if (jsonStrings.Count > 0)
                    annotatedVariant.SaList.Add(new SupplementaryAnnotation(nsaReader.JsonKey, nsaReader.IsArray, nsaReader.IsPositional, null,
                        jsonStrings));
            }
            else
                foreach ((string refAllele, string altAllele, string jsonString) in annotations)
                {
                    if (refAllele != variant.RefAllele || altAllele != variant.AltAllele) continue;

                    annotatedVariant.SaList.Add(new SupplementaryAnnotation(nsaReader.JsonKey, nsaReader.IsArray, nsaReader.IsPositional, jsonString,
                        null));
                    break;
                }
        }

        public void PreLoad(Chromosome chromosome, List<int> positions)
        {
            Task[] preloadTasks = _nsaReaders.Select(x => DoPreload(x, chromosome, positions)).ToArray();
            Task.WaitAll(preloadTasks);
            foreach (Task preloadTask in preloadTasks) preloadTask.Dispose();
        }

        private static Task DoPreload(INsaReader nsaReader, Chromosome chromosome, List<int> positions) =>
            Task.Run(() => { nsaReader.PreLoad(chromosome, positions); });

        public void Dispose()
        {
            if (_nsaReaders != null)
                foreach (INsaReader reader in _nsaReaders)
                    reader.Dispose();

            if (_fusionReaders != null)
                foreach (IGeneFusionSaReader reader in _fusionReaders)
                    reader.Dispose();
        }
    }
}