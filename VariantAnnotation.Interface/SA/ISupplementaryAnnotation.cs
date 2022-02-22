using JSON;

namespace VariantAnnotation.Interface.SA
{
    public interface ISupplementaryAnnotation:IJsonSerializer
    {
        string JsonKey { get; }
    }
}