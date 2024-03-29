﻿using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Vcf.VariantCreator;

namespace Vcf
{
    public sealed class VcfReader : IVcfReader
    {
        private readonly StreamReader _headerReader;
        private readonly StreamReader _reader;
        private readonly VariantFactory _variantFactory;
        private readonly IRefMinorProvider _refMinorProvider;
        private readonly ISequenceProvider _sequenceProvider;
        private readonly Dictionary<string, Chromosome> _refNameToChromosome;
        private readonly IVcfFilter _vcfFilter;
        private readonly IMitoHeteroplasmyProvider _mitoHeteroplasmyProvider;
        public bool IsRcrsMitochondrion { get; private set; }
        public string VcfLine { get; private set; }
        public GenomeAssembly InferredGenomeAssembly { get; private set; } = GenomeAssembly.Unknown;

        private string[] _sampleNames;
        private List<string> _headerLines;
        private readonly Queue<ISimplePosition> _queuedPositions = new Queue<ISimplePosition>();

        private readonly HashSet<string> _observedReferenceNames = new HashSet<string>();
        private string _currentReferenceName;

        public          string[]        GetSampleNames() => _sampleNames;
        public readonly bool            EnableDq;
        public readonly HashSet<string> CustomInfoKeys;

        private VcfReader(
            StreamReader headerReader,
            StreamReader vcfLineReader,
            ISequenceProvider sequenceProvider,
            IRefMinorProvider refMinorProvider,
            IVcfFilter vcfFilter,
            IVariantIdCreator vidCreator,
            IMitoHeteroplasmyProvider mitoHeteroplasmyProvider,
            bool enableDq = false,
            HashSet<string> customInfoKeys = null,
            HashSet<string> customSampleInfoKeys=null
        )
        {
            _headerReader             = headerReader;
            _reader                   = vcfLineReader;
            _variantFactory           = new VariantFactory(sequenceProvider.Sequence, vidCreator, customSampleInfoKeys);
            _sequenceProvider         = sequenceProvider;
            _refMinorProvider         = refMinorProvider;
            _vcfFilter                = vcfFilter;
            _refNameToChromosome      = sequenceProvider.RefNameToChromosome;
            _mitoHeteroplasmyProvider = mitoHeteroplasmyProvider;
            EnableDq                  = enableDq;
            CustomInfoKeys            = customInfoKeys;
        }

        public static VcfReader Create(StreamReader headerReader, StreamReader vcfLineReader, ISequenceProvider sequenceProvider,
            IRefMinorProvider refMinorProvider, IVcfFilter vcfFilter, IVariantIdCreator vidCreator,
            IMitoHeteroplasmyProvider mitoHeteroplasmyProvider, bool enableDq = false, 
            HashSet<string> customInfoKeys=null, HashSet<string> customSampleInfoKeys=null)
        {
            var vcfReader = new VcfReader(headerReader, vcfLineReader, sequenceProvider, refMinorProvider, vcfFilter, 
                vidCreator, mitoHeteroplasmyProvider, enableDq, customInfoKeys, customSampleInfoKeys);
            vcfReader.ParseHeader();
            return vcfReader;
        }
        
        private void ParseHeader()
        {
            _headerLines = new List<string>();

            string line;
            while ((line = _headerReader.ReadLine()) != null)
            {
                CheckContigId(line);
                _headerLines.Add(line);
                if (line.StartsWith(VcfCommon.ChromosomeHeader)) break;
            }

            ValidateVcfHeader();
            _sampleNames = ExtractSampleNames(line);
            _vcfFilter.FastForward(_reader);
        }

        private void CheckContigId(string line)
        {
            string[] chromAndLengthInfo = GetChromAndLengthInfo(line);
            if (chromAndLengthInfo.Length == 0) return;

            if (!_refNameToChromosome.TryGetValue(chromAndLengthInfo[0], out Chromosome chromosome)) return;
            if (!int.TryParse(chromAndLengthInfo[1], out int length)) return;

            var assemblyThisChrom = ContigInfo.GetGenomeAssembly(chromosome, length);

            if (assemblyThisChrom == GenomeAssembly.rCRS)
            {
                IsRcrsMitochondrion = true;
                return;
            }

            if (!GenomeAssemblyHelper.AutosomeAndAllosomeAssemblies.Contains(assemblyThisChrom)) return;

            if (InferredGenomeAssembly == GenomeAssembly.Unknown) InferredGenomeAssembly = assemblyThisChrom;

            if (InferredGenomeAssembly != assemblyThisChrom)
                throw new UserErrorException($"Inconsistent genome assemblies inferred:\ncurrent line \"{line}\" indicates {assemblyThisChrom}, whereas the lines above it indicate {InferredGenomeAssembly}.");
        }

        internal static string[] GetChromAndLengthInfo(string line)
        {
            if (!line.StartsWith("##contig=<ID=")) return Array.Empty<string>();
            if (!line.Contains(",length=")) return Array.Empty<string>();
            string[] chromAndLength = line.TrimEnd('>').Substring(13).Split(",length=");
            return chromAndLength.Length == 2 ? chromAndLength : Array.Empty<string>();
        }

        private void ValidateVcfHeader()
        {
            if (_headerLines.Count == 0 || !_headerLines[0].StartsWith("##fileformat=VCFv"))
                throw new UserErrorException("Please provide a valid VCF file with proper fileformat field.");

            if (!_headerLines[_headerLines.Count - 1].StartsWith(VcfCommon.ChromosomeHeader))
                throw new UserErrorException($"Could not find the vcf header line starting with {VcfCommon.ChromosomeHeader}. Is this a valid vcf file?");
        }

        private static string[] ExtractSampleNames(string line)
        {
            string[] cols = line.OptimizedSplit('\t');
            bool hasSampleGenotypes = cols.Length >= VcfCommon.MinNumColumnsSampleGenotypes;
            if (!hasSampleGenotypes) return null;

            int numSamples = cols.Length - VcfCommon.GenotypeIndex;
            var samples = new string[numSamples];
            for (var i = 0; i < numSamples; i++) samples[i] = cols[VcfCommon.GenotypeIndex + i];
            return samples;
        }
        
        private ISimplePosition GetNextSimplePosition()
        {
            while (_queuedPositions.Count == 0)
            {
                VcfLine = _vcfFilter.GetNextLine(_reader);
                
                if (VcfLine != null) 
                {
                    string[] vcfFields = VcfLine.OptimizedSplit('\t');
                    var chromosome = ReferenceNameUtilities.GetChromosome(_refNameToChromosome, vcfFields[VcfCommon.ChromIndex]);
                    CheckVcfOrder(vcfFields[VcfCommon.ChromIndex]);

                    (int start, bool foundError) = vcfFields[VcfCommon.PosIndex].OptimizedParseInt32();
                    if (foundError) throw new InvalidDataException($"Unable to convert the VCF position to an integer: {vcfFields[VcfCommon.PosIndex]}");

                    if (InconsistentSampleFields(vcfFields))
                    {
                        int sampleCount = _sampleNames?.Length ?? 0;
                        throw new UserErrorException($"Inconsistent number of sample fields in line:\n{VcfLine}\nExpected number of sample fields: {sampleCount}");
                    }
                    _queuedPositions.Enqueue(SimplePosition.GetSimplePosition(chromosome, start, vcfFields, _vcfFilter));
                }

                if (VcfLine == null) break;
            }

            return _queuedPositions.Count == 0 ? null : _queuedPositions.Dequeue();
        }

        private bool InconsistentSampleFields(string[] vcfFields)
        {
            int sampleCount = _sampleNames?.Length ?? 0;
            if (sampleCount != 0)
            {
                return vcfFields.Length != VcfCommon.FormatIndex + 1 + sampleCount;
            }

            return vcfFields.Length != VcfCommon.InfoIndex + 1;
        }

        private void CheckVcfOrder(string referenceName)
        {
            if (referenceName == _currentReferenceName) return;

            if (_observedReferenceNames.Contains(referenceName))
            {
                throw new FileNotSortedException("The current input vcf file is not sorted. Please sort the vcf file before running variant annotation using a tool like vcf-sort in vcftools.");
            }

            _observedReferenceNames.Add(referenceName);
            _currentReferenceName = referenceName;
        }

        public IPosition GetNextPosition() => Position.ToPosition(GetNextSimplePosition(), 
            _refMinorProvider, _sequenceProvider, _mitoHeteroplasmyProvider, _variantFactory, 
            EnableDq, CustomInfoKeys);

        public void Dispose() => _reader?.Dispose();
    }
}
