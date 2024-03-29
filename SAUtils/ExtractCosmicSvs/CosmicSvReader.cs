﻿using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using VariantAnnotation.Providers;

namespace SAUtils.ExtractCosmicSvs
{
    public sealed class CosmicSvReader:IDisposable
    {
        private readonly Stream _cnvStream;
        private readonly Stream _breakendStream;
        private readonly DataSourceVersion _version;
        private readonly string _outputDirectory;
        private readonly GenomeAssembly _genomeAssembly;
        private readonly Dictionary<string, Chromosome> _refNameToChorm;

        public CosmicSvReader(Stream cnvStream, Stream breakendStream, DataSourceVersion version, string outputDir, GenomeAssembly assembly, Dictionary<string, Chromosome> refNameToChromosome)
        {
            _cnvStream       = cnvStream;
            _breakendStream  = breakendStream;
            _version         = version;
            _outputDirectory = outputDir;
            _genomeAssembly  = assembly;
            _refNameToChorm  = refNameToChromosome;
        }

        //public void CreateTsv()
        //{
        //    var benchMark = new Benchmark();
        //    const string dataSource = "COSMIC";

        //    if (_cnvStream != null)
        //    {
        //        using (var writer = new IntervalTsvWriter(_outputDirectory, _version,
        //            _genomeAssembly.ToString(), SaTsvCommon.CosmicSvSchemaVersion, DataSourceTags.CosmicCnvTag, ReportFor.StructuralVariants))
        //        using (var cnvReader = new CosmicCnvReader(_cnvStream, _refNameToChorm, _genomeAssembly))
        //        {
        //            foreach (var cnvEntry in cnvReader.GetEntries())
        //            {
        //                writer.AddEntry(cnvEntry.Chromosome.EnsemblName, cnvEntry.Start, cnvEntry.End, cnvEntry.GetJsonString());
        //            }
        //        }

        //    }


        //    var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
        //    TsvWriterUtilities.WriteCompleteInfo(dataSource, _version.Version, timeSpan);
        //}

        public void Dispose()
        {
            _cnvStream?.Dispose();
            _breakendStream?.Dispose();
        }
    }
}