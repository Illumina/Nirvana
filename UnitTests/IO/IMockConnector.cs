using System.IO;

namespace UnitTests.IO
{
    public interface IMockConnector
    {
        Stream ConnectorFunc(long position);
    }
}
   