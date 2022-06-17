using Cloud;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.Cloud;

public sealed class ConsistencyTests
{
    [Fact]
    public void Consistency_with_SAUtils()
    {
        Assert.Equal(LambdaUrlHelper.SaSchemaVersion, SaCommon.SchemaVersion);
    }
}