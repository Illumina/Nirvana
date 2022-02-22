using System.Text;

namespace JSON;

public interface IJsonSerializer
{
	void SerializeJson(StringBuilder sb);
}