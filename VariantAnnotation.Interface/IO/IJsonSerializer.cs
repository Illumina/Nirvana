using System.Text;

namespace VariantAnnotation.Interface.IO
{
	public interface IJsonSerializer
	{
		void SerializeJson(StringBuilder sb);
	}
}