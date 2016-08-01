using System.Text;

namespace VariantAnnotation.DataStructures.JsonAnnotations
{
    public interface IJsonSerializer
    {
        void SerializeJson(StringBuilder sb);
    }
}
