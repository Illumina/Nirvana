using System.Collections.Generic;
using SAUtils.MitoMap;
using Xunit;

namespace UnitTests.SAUtils.MitoMap
{
    public sealed class ParsingUtilitiesTests
    {
        private static readonly MitoMapInputDb MitoMapInputDb = new MitoMapInputDb(new Dictionary<string, string>
        {
            {"1", "101"},
            {"2", "102"},
            {"13", "103"},
            {"4100", "104"},
            {"5678", "105"},
            {"23202", "105"}
        });

        [Theory]
        [InlineData("<a href='/cgi-bin/print_ref_list?refs=4100&title=Control+Polymorphism+T-A+at+14' target='_blank'>1</a>", "104")]
        [InlineData("<a href='/cgi-bin/print_ref_list?refs=1,13,5678,23202&title=Mutation+T-C+at+15784' target='_blank'>3</a>", "101,103,105")]
        [InlineData("<a href='/cgi-bin/print_ref_list?refs=1,2,13,4100&title=Simple+Insertion+%2B266+308-573+D-Loop+region+573+D-Loop+region+D%2C+7%2F7+25' target='_blank'>4</a>", "101,102,103,104")]
        public void GetPubMedIds_AsExpected(string field, string pubmedIds)
        {
            Assert.Equal(string.Join(',', ParsingUtilities.GetPubMedIds(field, MitoMapInputDb)), pubmedIds);
        }

        [Theory]
        [InlineData("<a href='/cgi-bin/print_ref_list?refs=1,2,13,4100&title=Simple+Insertion+%2B266+308-573+D-Loop+region+573+D-Loop+region+D%2C+7%2F7+25' target='_blank'>4</a>", "1,2,13,4100")]
        [InlineData("<a href='/cgi-bin/print_ref_list?refs=45,247,280,303,312,330,332,394,396,541,3311,3370,3427,3569,3584,3732,3943,4287,4946,5113,5329,5348,5451,5452,5628,6169,6221,6228,6421,6490,6531,6603,6623,7647,7695,8405,8492,20372,20373,90237,90296,90302,90311,90331,90333,90365,90377,90453,90468,90477,90483,90534,90572,90627,90764,90809,90812,90835,90903,90904,90941,90963,91015,91020,91030,91041,91056,91068,91077,91081,91094,91099,91206,91214,91215,91221,91227,91228,91256,91269,91311,91318,91394,91420,91431,91514,91687,91737,91775,91789,91830,91958,91999,92063,92082,92100,99016&title=Coding+Polymorphism+G-A+at+3010' target='_blank'>97</a>", "45,247,280,303,312,330,332,394,396,541,3311,3370,3427,3569,3584,3732,3943,4287,4946,5113,5329,5348,5451,5452,5628,6169,6221,6228,6421,6490,6531,6603,6623,7647,7695,8405,8492,20372,20373,90237,90296,90302,90311,90331,90333,90365,90377,90453,90468,90477,90483,90534,90572,90627,90764,90809,90812,90835,90903,90904,90941,90963,91015,91020,91030,91041,91056,91068,91077,91081,91094,91099,91206,91214,91215,91221,91227,91228,91256,91269,91311,91318,91394,91420,91431,91514,91687,91737,91775,91789,91830,91958,91999,92063,92082,92100,99016")]
        public void ExtractInternalIds_AsExpected(string field, string internalIds)
        {
            Assert.Equal(string.Join(',', ParsingUtilities.ExtractInternalIds(field)), internalIds);
        }
    }
}