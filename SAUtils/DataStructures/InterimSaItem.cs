﻿using IO;
using SAUtils.Interface;
using VariantAnnotation.Interface.SA;

namespace SAUtils.DataStructures
{

    public sealed class InterimSaItem : IInterimSaItem, ISaDataSource
    {
        public string KeyName { get; }
        public string VcfkeyName { get; }
        public bool MatchByAllele { get; }
        public bool IsArray { get; }
        public string Chromosome { get; }
        public int Position { get; }
        public string AltAllele { get; }
        public string[] JsonStrings { get; }
        public string VcfString { get; }


        /// <summary>
        /// constructor
        /// </summary>
        public InterimSaItem(string keyName, string vcfkeyName, string chr, int pos, string altAllele,
            bool matchByAllele, bool isArray, string vcfString, string[] jsonStrings)
        {
            KeyName = keyName;
            VcfkeyName = vcfkeyName;
            MatchByAllele = matchByAllele;
            IsArray = isArray;
            Chromosome = chr;
            Position = pos;
            AltAllele = altAllele;
            JsonStrings = jsonStrings;
            VcfString = vcfString;
        }

        public int CompareTo(IInterimSaItem other)
        {
            if (other == null) return -1;
            return Chromosome.Equals(other.Chromosome) ? Position.CompareTo(other.Position) : string.CompareOrdinal(Chromosome, other.Chromosome);
        }

        public void Write(IExtendedBinaryWriter writer)
        {
            writer.Write(KeyName);
            writer.Write(VcfkeyName);
            writer.Write(AltAllele);

            byte flags = 0;
            if (MatchByAllele) flags |= 1;
            if (IsArray) flags |= 2;
            writer.Write(flags);

            writer.Write(VcfString);

            writer.WriteOpt(JsonStrings.Length);
            foreach (var jsonString in JsonStrings) writer.Write(jsonString);
        }

    }
}
