using System.IO;
using System.Text;
using IO;
using Moq;
using Xunit;

namespace UnitTests.IO
{
    public sealed class PersistentStreamTests
    {
        private Stream GetMockStream()
        {
            var memStream = new MemoryStream();
            using (var writer = new StreamWriter(memStream, Encoding.Default, 4096, true))
            {
                writer.WriteLine("2551e067cb59c540a4da905a99ee5ff4-ClinGen/2/GRCh37/ClinGen_20160414.nsi");
                writer.WriteLine("43321b1a4f1c73724c00223e07d5e812-1kgSv/3/GRCh37/1000_Genomes_Project_Phase_3_v5a.nsi");
                writer.WriteLine("929439472713ec609b92b97dc22a2d42-dbSNP/4/GRCh37/dbSNP_151.nsa");
            }

            memStream.Position = 0;
            return memStream;
        }
        private IConnect GetWebRequest_connect_on_third()
        {
            var moqRequest = new Mock<IConnect>();

            //Connect succeeds on 3rd attempt
            moqRequest.SetupSequence(x => x.Connect(0))
                .Throws(new IOException())
                .Throws(new IOException())
                .Returns((null,GetMockStream()));

            return moqRequest.Object;
        }

        private IConnect GetWebRequest_flaky_stream()
        {
            var moqRequest = new Mock<IConnect>();

            moqRequest.SetupSequence(x => x.Connect(0))
                .Returns((null, null))
                .Returns((null, GetMockStream()));
            
            return moqRequest.Object;
        }

        private IConnect GetWebRequest_connect_on_seventh()
        {
            var moqRequest = new Mock<IConnect>();

            //Connect succeeds on 3rd attempt
            moqRequest.SetupSequence(x => x.Connect(0))
                .Throws(new IOException())
                .Throws(new IOException())
                .Throws(new IOException())
                .Throws(new IOException())
                .Throws(new IOException())
                .Throws(new IOException())
                .Returns((null, GetMockStream()));

            return moqRequest.Object;
        }

        [Fact]
        public void TestFlakyConnection()
        {
            // pStream attempts to connect at construction time. It should succeed at the third attempt
            var pStream = new PersistentStream(GetWebRequest_connect_on_third(),0);
            // no exception thrown means this test succeeded
        }

        [Fact]
        public void FailToConnect()
        {
            Assert.Throws<IOException>(()=>new PersistentStream(GetWebRequest_connect_on_seventh(),0));
        }

        [Fact]
        public void ReadFlakyStream()
        {
            var pStream = new PersistentStream(GetWebRequest_flaky_stream(),0);
            var buffer = new byte[4096];
            Assert.Equal(100, pStream.Read(buffer, 0, 100));
        }
    }
}
