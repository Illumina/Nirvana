using System.Collections.Generic;
using SAUtils.CreateMitoMapDb;
using Xunit;

namespace UnitTests.SAUtils.CreateMitoMapDb
{
    public sealed class ParsingUtilitiesTests
    {
        private static MitoMapInputDb MitoMapInputDb = new MitoMapInputDb(new Dictionary<string, string>
        {
            {"1", "101"},
            {"2", "102"},
            {"13", "103"},
            {"4100", "104"},
            {"5678", "105"},
        });

        [Theory]
        [InlineData("<a href='/cgi-bin/print_ref_list?refs=4100&title=Control+Polymorphism+T-A+at+14' target='_blank'>1</a>", "104")]
        [InlineData("<a href='/cgi-bin/print_ref_list?refs=1,13,5678&title=Mutation+T-C+at+15784' target='_blank'>3</a>", "101,103,105")]
        [InlineData("<a href='/cgi-bin/print_ref_list?refs=1,2,13,4100&title=Simple+Insertion+%2B266+308-573+D-Loop+region+573+D-Loop+region+D%2C+7%2F7+25' target='_blank'>4</a>", "101,102,103,104")]
        public void GetPubMedIds_AsExpected(string field, string pubmedIds)
        {
            Assert.Equal(string.Join(',', ParsingUtilities.GetPubMedIds(field, MitoMapInputDb)), pubmedIds);
        }
    }
}