using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace VariantAnnotation.SA
{
    public class SaDataSource : ISaDataSource
    {
        public string KeyName { get; }
        public string VcfkeyName { get; }
        public bool MatchByAllele { get; }
        public bool IsArray { get; }
        public string AltAllele { get; }
        public string[] JsonStrings { get; }
        public string VcfString { get; }

        /// <summary>
        /// constructor
        /// </summary>
        public SaDataSource(string keyName, string vcfkeyName, string altAllele, bool matchByAllele, bool isArray,
            string vcfString, string[] jsonStrings)
        {
            KeyName = keyName;
            VcfkeyName = vcfkeyName;
            MatchByAllele = matchByAllele;
            IsArray = isArray;
            AltAllele = altAllele;
            JsonStrings = jsonStrings;
            VcfString = vcfString;
        }

        public static ISaDataSource Read(ExtendedBinaryReader reader)
        {
            var keyName = reader.ReadString();
            var vcfkeyName = reader.ReadString();
            var altAllele = reader.ReadString();
            var flags = reader.ReadByte();
            var matchByAllele = (flags & 1) != 0;
            var isArray = (flags & 2) != 0;
            var vcfString = reader.ReadString();

            var numJsonStrings = reader.ReadOptInt32();

            string[] jsonStrings = null;
            if (numJsonStrings > 0)
            {
                jsonStrings = new string[numJsonStrings];
                for (int i = 0; i < numJsonStrings; i++) jsonStrings[i] = reader.ReadString();
            }

            return new SaDataSource(keyName, vcfkeyName, altAllele, matchByAllele, isArray, vcfString, jsonStrings);
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