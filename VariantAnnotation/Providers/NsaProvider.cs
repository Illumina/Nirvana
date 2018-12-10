using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ErrorHandling.Exceptions;
using Genome;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using Variants;

namespace VariantAnnotation.Providers
{
    public sealed class NsaProvider : IAnnotationProvider
    {
        public string Name => "Supplementary annotation provider";
        public GenomeAssembly Assembly { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }
        private readonly INsaReader[] _nsaReaders;
        private readonly INsiReader[] _nsiReaders;
        private static readonly ImmutableHashSet<GenomeAssembly> AssembliesIgnoredInConsistancyCheck =
            new HashSet<GenomeAssembly> { GenomeAssembly.Unknown, GenomeAssembly.rCRS }.ToImmutableHashSet();

        public NsaProvider(INsaReader[] nsaReaders, INsiReader[] nsiReaders)
        {
            _nsaReaders = nsaReaders;
            _nsiReaders = nsiReaders;

            IEnumerable<GenomeAssembly> assemblies = null;
            if (_nsaReaders != null)
            {
                DataSourceVersions = _nsaReaders.Select(x => x.Version);
                assemblies = _nsaReaders.Select(x => x.Assembly);
            }

            if (_nsiReaders != null)
            {
                assemblies = assemblies?.Concat(_nsiReaders.Select(x => x.Assembly)) ?? _nsiReaders.Select(x => x.Assembly);
                DataSourceVersions = DataSourceVersions?.Concat(_nsiReaders.Select(x => x.Version)) ?? _nsiReaders.Select(x => x.Version);
            }

            var distinctAssemblies = assemblies?.Where(x => !AssembliesIgnoredInConsistancyCheck.Contains(x)).Distinct().ToArray();
            if (distinctAssemblies == null || distinctAssemblies.Length > 1)
            {
                if (_nsaReaders != null)
                    foreach (INsaReader nsaReader in _nsaReaders)
                    {
                        Console.WriteLine(nsaReader.Version + "\tAssembly:" + nsaReader.Assembly);
                    }
                if (_nsiReaders != null)
                    foreach (INsiReader nsiReader in _nsiReaders)
                    {
                        Console.WriteLine(nsiReader.Version + "\tAssembly:" + nsiReader.Assembly);
                    }
                throw new UserErrorException("Multilpe genome assembly detected in Supplementary annotation directory");
            }

            Assembly = distinctAssemblies[0];
        }

        public void Annotate(IAnnotatedPosition annotatedPosition)
        {
            if (_nsaReaders != null) AddSmallVariantAnnotations(annotatedPosition);

            if (_nsiReaders != null) GetStructuralVariantAnnotations(annotatedPosition);
        }

        private void GetStructuralVariantAnnotations(IAnnotatedPosition annotatedPosition)
        {
            //if (annotatedPosition.AnnotatedVariants.Any(x => x.Variant.Behavior.NeedSaInterval != true)) return;
            var needSaIntervals = annotatedPosition.AnnotatedVariants.Any(x => x.Variant.Behavior.NeedSaInterval);
            var needSmallAnnotation = annotatedPosition.AnnotatedVariants.Any(x => x.Variant.Behavior.NeedSaPosition);

            foreach (INsiReader nsiReader in _nsiReaders)
            {
                var position = annotatedPosition.Position;
                if (nsiReader.ReportFor == ReportFor.SmallVariants && !needSmallAnnotation) continue;
                if (nsiReader.ReportFor == ReportFor.StructuralVariants && !needSaIntervals) continue;

                var annotations = nsiReader.GetAnnotation(position.Variants[0]);
                if (annotations == null) continue;

                annotatedPosition.SupplementaryIntervals.Add(new SupplementaryAnnotation(nsiReader.JsonKey, true, false, null, annotations));
            }

        }

        private void AddSmallVariantAnnotations(IAnnotatedPosition annotatedPosition)
        {
            foreach (var annotatedVariant in annotatedPosition.AnnotatedVariants)
            {
                if (annotatedVariant.Variant.Start > 20129 && annotatedVariant.Variant.Start < 22154)
                    Console.WriteLine("bug");

                if (!annotatedVariant.Variant.Behavior.NeedSaPosition) continue;
                foreach (INsaReader nsaReader in _nsaReaders)
                {
                    var variant = annotatedVariant.Variant;
                    var annotations = nsaReader.GetAnnotation(variant.Chromosome, variant.Start);
                    if (annotations == null) continue;

                    if (nsaReader.IsPositional)
                    {
                        AddPositionalAnnotation(annotations, annotatedVariant, nsaReader);
                        continue;
                    }

                    if (nsaReader.MatchByAllele) AddAlleleSpecificAnnotation(nsaReader, annotations, annotatedVariant, variant);

                    else AddNonAlleleSpecificAnnotations(annotations, variant, annotatedVariant, nsaReader);

                }
                
                //check for interval annotations that applies to all variants
                if(_nsiReaders ==null) continue;
                foreach (INsiReader nsiReader in _nsiReaders)
                {
                   if (nsiReader.ReportFor == ReportFor.StructuralVariants ||
                        nsiReader.ReportFor == ReportFor.None) continue;

                    var variant = annotatedVariant.Variant;
                    var annotations = nsiReader.GetAnnotation(variant);
                    if(annotations !=null ) AddPositionalAnnotation(annotations, annotatedVariant, nsiReader);
                }
            }
        }

        private void AddPositionalAnnotation(IEnumerable<string> annotations, IAnnotatedVariant annotatedVariant, INsiReader nsiReader)
        {
            annotatedVariant.SaList.Add(new SupplementaryAnnotation(nsiReader.JsonKey, true, true, null, annotations));
        }

        private static void AddPositionalAnnotation(IEnumerable<(string refAllele, string altAllele, string annotation)> annotations, IAnnotatedVariant annotatedVariant,
            INsaReader nsaReader)
        {
            //e.g. ancestral allele, global minor allele
            var jsonString = annotations.First().annotation;
            annotatedVariant.SaList.Add(new SupplementaryAnnotation(nsaReader.JsonKey, nsaReader.IsArray,
                nsaReader.IsPositional, jsonString, null));
        }

        private static void AddNonAlleleSpecificAnnotations(IEnumerable<(string refAllele, string altAllele, string annotation)> annotations, IVariant variant,
            IAnnotatedVariant annotatedVariant, INsaReader nsaReader)
        {
            var jsonStrings = new List<string>();
            foreach ((string refAllele, string altAllele, string jsonString) in annotations)
            {
                if (refAllele == variant.RefAllele && altAllele == variant.AltAllele)
                    jsonStrings.Add(jsonString + ",\"isAlleleSpecific\":true");
                else jsonStrings.Add(jsonString);
            }

            if (jsonStrings.Count > 0)
                annotatedVariant.SaList.Add(new SupplementaryAnnotation(nsaReader.JsonKey, nsaReader.IsArray,
                    nsaReader.IsPositional, null, jsonStrings));
        }

        private static void AddAlleleSpecificAnnotation(INsaReader nsaReader, IEnumerable<(string refAllele, string altAllele, string annotation)> annotations,
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
                    annotatedVariant.SaList.Add(new SupplementaryAnnotation(nsaReader.JsonKey, nsaReader.IsArray,
                        nsaReader.IsPositional, null, jsonStrings));
            }
            else
                foreach ((string refAllele, string altAllele, string jsonString) in annotations)
                {
                    if (refAllele != variant.RefAllele || altAllele != variant.AltAllele) continue;
                    annotatedVariant.SaList.Add(new SupplementaryAnnotation(nsaReader.JsonKey, nsaReader.IsArray,
                        nsaReader.IsPositional, jsonString, null));
                    break;
                }
        }

        public void PreLoad(IChromosome chromosome, List<int> positions)
        {
            //var benchmark = new Benchmark();
            foreach (INsaReader nsaReader in _nsaReaders)
            {
                nsaReader.PreLoad(chromosome, positions);
            }

            //var totalTime = benchmark.GetElapsedTime();
            //var rate = totalBytes * 1.0 / (totalTime.TotalSeconds * 1_000_000);// MB/sec
            //Console.WriteLine($"\nPreloaded SA in {Benchmark.ToHumanReadable(totalTime)}. Data rate {rate:#.##} MB/sec");
            //Console.WriteLine($"No of http stream sources created {HttpStreamSource.Count}");
        }
    }
}