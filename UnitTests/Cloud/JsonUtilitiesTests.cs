using System.IO;
using System.Linq;
using System.Text;
using Cloud.Utilities;
using Xunit;

namespace UnitTests.Cloud
{
    public sealed class JsonUtilitiesTests
    {

        [Fact]
        public void Serialize_AsExpected()
        {
            var inputObject = new[]
            {
                new ObjectExample {Name = "Ada", Age = 8, Skills = new []{"dancing", "skating"}},
                new ObjectExample {Name = "Bob", Age = 10, Skills = new []{"programming"}}
            };
            var memStream = JsonUtilities.Serialize(inputObject);

            const string expectedString = "[{\"Name\":\"Ada\",\"Age\":8,\"Skills\":[\"dancing\",\"skating\"]},{\"Name\":\"Bob\",\"Age\":10,\"Skills\":[\"programming\"]}]";
            var expectedStream = new MemoryStream(Encoding.ASCII.GetBytes(expectedString));

            Assert.Equal(expectedStream.Length, memStream.Length);
            Assert.True(expectedStream.ToArray().SequenceEqual(memStream.ToArray()));
        }

        [Fact]
        public void Stringify_AsExpected()
        {
            var inputObject = new[]
            {
                new ObjectExample {Name = "Ken", Age = 16, Skills = new[] {"boxing"}},
                new ObjectExample {Name = "Armanda", Age = 18, Skills = new[] {"cooking"}}
            };

            const string expectedString = "[{\"Name\":\"Ken\",\"Age\":16,\"Skills\":[\"boxing\"]},{\"Name\":\"Armanda\",\"Age\":18,\"Skills\":[\"cooking\"]}]";

            Assert.Equal(expectedString, JsonUtilities.Stringify(inputObject));
        }
    }

    public sealed class ObjectExample
    {
        public string Name;
        public int Age;
        public string[] Skills;
    }
}