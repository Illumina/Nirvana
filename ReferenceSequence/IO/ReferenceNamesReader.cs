﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using OptimizedCore;

namespace ReferenceSequence.IO
{
    public static class ReferenceNamesReader
    {
        private const int RefIndex     = 0;
        private const int EnsemblIndex = 1;
        private const int UcscIndex    = 2;

        public static List<Chromosome> GetReferenceNames(Stream stream)
        {
            var names = new List<Chromosome>();

            using (var reader = new StreamReader(stream))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null) break;

                    string[] cols        = line.OptimizedSplit('\t');
                    ushort   refIndex    = ushort.Parse(cols[RefIndex]);
                    string   ensemblName = cols[EnsemblIndex];
                    string   ucscName    = cols[UcscIndex];

                    names.Add(new Chromosome(ucscName, ensemblName, null, null, 0, refIndex));
                }
            }

            return names.OrderBy(x => x.Index).ToList();
        }
    }
}
