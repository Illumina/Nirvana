﻿using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class RefMinorItem:ISupplementaryDataItem
    {
        public Chromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }
        public string GlobalMajor { get; }

        public RefMinorItem(Chromosome chromosome, int position, string globalMajor)
        {
            Chromosome = chromosome;
            Position = position;
            GlobalMajor = globalMajor;
        }

        public string GetJsonString()
        {
            var sb = StringBuilderPool.Get();
            var jsonObject = new JsonObject(sb);

            jsonObject.AddBoolValue("isReferenceMinor", true);

            return StringBuilderPool.GetStringAndReturn(sb);
        }

        public string InputLine { get; set; }
    }
}