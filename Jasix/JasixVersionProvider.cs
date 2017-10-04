using VariantAnnotation.Interface.Providers;

namespace Jasix
{
    public sealed class JasixVersionProvider : IVersionProvider
    {
        public string GetProgramVersion()
        {
            return "Nirvana 2.0.0";
        }

        public string GetDataVersion() => null;
    }
}