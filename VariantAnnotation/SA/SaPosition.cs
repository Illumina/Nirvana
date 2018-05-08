using IO;
using VariantAnnotation.Interface.SA;

namespace VariantAnnotation.SA
{
    public sealed class SaPosition : ISaPosition
    {
        public ISaDataSource[] DataSources { get; }
        public string GlobalMajorAllele { get; }

        public SaPosition(ISaDataSource[] dataSources, string globalMajorAllele)
        {
            DataSources       = dataSources;
            GlobalMajorAllele = globalMajorAllele;
        }

        public static ISaPosition Read(ExtendedBinaryReader reader)
        {
            string globalMajorAllele = reader.ReadAsciiString();
            int numDataSources       = reader.ReadOptInt32();

            ISaDataSource[] dataSources = null;
            if (numDataSources > 0)
            {
                dataSources = new ISaDataSource[numDataSources];
                for (var i = 0; i < numDataSources; i++) dataSources[i] = SaDataSource.Read(reader);
            }

            return new SaPosition(dataSources, globalMajorAllele);
        }

        public void Write(IExtendedBinaryWriter writer)
        {
            writer.WriteOptAscii(GlobalMajorAllele);
            writer.WriteOpt(DataSources.Length);
            foreach (var dataSource in DataSources) dataSource.Write(writer);
        }
    }
}