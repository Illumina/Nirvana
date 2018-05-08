using IO;
using VariantAnnotation.Interface.SA;

namespace VariantAnnotation.SA
{
    public sealed class SaDataSource : ISaDataSource
    {
        public string KeyName { get; }
        public string VcfkeyName { get; }
        public bool MatchByAllele { get; }
        public bool IsArray { get; }
        public string AltAllele { get; }
        public string[] JsonStrings { get; }
        public string VcfString { get; }

        public SaDataSource(string keyName, string vcfkeyName, string altAllele, bool matchByAllele, bool isArray,
            string vcfString, string[] jsonStrings)
        {
            KeyName       = keyName;
            VcfkeyName    = vcfkeyName;
            MatchByAllele = matchByAllele;
            IsArray       = isArray;
            AltAllele     = altAllele;
            JsonStrings   = jsonStrings;
            VcfString     = vcfString;
        }

        public static ISaDataSource Read(ExtendedBinaryReader reader)
        {
            string keyName     = reader.ReadString();
            string vcfkeyName  = reader.ReadString();
            string altAllele   = reader.ReadString();
            byte flags         = reader.ReadByte();
            bool matchByAllele = (flags & 1) != 0;
            bool isArray       = (flags & 2) != 0;
            string vcfString   = reader.ReadString();

            int numJsonStrings = reader.ReadOptInt32();

            string[] jsonStrings = null;
            if (numJsonStrings > 0)
            {
                jsonStrings = new string[numJsonStrings];
                for (var i = 0; i < numJsonStrings; i++) jsonStrings[i] = reader.ReadString();
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
            foreach (string jsonString in JsonStrings) writer.Write(jsonString);
        }
    }
}