using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;

// ReSharper disable InconsistentNaming
namespace UnitTests.MockedData
{
    public static class TranscriptRegions
    {
        public static readonly ITranscriptRegion[] ENST00000290663 =
        {
            new TranscriptRegion(TranscriptRegionType.Exon,   8, 43383917, 43384552, 848, 1483),
            new TranscriptRegion(TranscriptRegionType.Intron, 7, 43384553, 43385045, 847, 848),
            new TranscriptRegion(TranscriptRegionType.Exon,   7, 43385046, 43385106, 787, 847),
            new TranscriptRegion(TranscriptRegionType.Intron, 6, 43385107, 43385977, 786, 787),
            new TranscriptRegion(TranscriptRegionType.Exon,   6, 43385978, 43386226, 538, 786),
            new TranscriptRegion(TranscriptRegionType.Intron, 5, 43386227, 43386588, 537, 538),
            new TranscriptRegion(TranscriptRegionType.Exon,   5, 43386589, 43386670, 456, 537),
            new TranscriptRegion(TranscriptRegionType.Intron, 4, 43386671, 43386857, 455, 456),
            new TranscriptRegion(TranscriptRegionType.Exon,   4, 43386858, 43386998, 315, 455),
            new TranscriptRegion(TranscriptRegionType.Intron, 3, 43386999, 43387502, 314, 315),
            new TranscriptRegion(TranscriptRegionType.Exon,   3, 43387503, 43387647, 170, 314),
            new TranscriptRegion(TranscriptRegionType.Intron, 2, 43387648, 43388309, 169, 170),
            new TranscriptRegion(TranscriptRegionType.Exon,   2, 43388310, 43388428, 51,  169),
            new TranscriptRegion(TranscriptRegionType.Intron, 1, 43388429, 43389758, 50,  51),
            new TranscriptRegion(TranscriptRegionType.Exon,   1, 43389759, 43389808, 1,   50)
        };

        public static readonly ITranscriptRegion[] ENST00000370673 =
        {
            new TranscriptRegion(TranscriptRegionType.Exon,   1, 84298366, 84298567, 1,   202),
            new TranscriptRegion(TranscriptRegionType.Intron, 1, 84298568, 84303202, 202, 203),
            new TranscriptRegion(TranscriptRegionType.Exon,   2, 84303203, 84303287, 203, 287),
            new TranscriptRegion(TranscriptRegionType.Intron, 2, 84303288, 84325636, 287, 288),
            new TranscriptRegion(TranscriptRegionType.Exon,   3, 84325637, 84325748, 288, 399),
            new TranscriptRegion(TranscriptRegionType.Intron, 3, 84325749, 84349630, 399, 400),
            new TranscriptRegion(TranscriptRegionType.Exon,   4, 84349631, 84350798, 400, 1567)
        };

        public static readonly ITranscriptRegion[] ENST00000615053 =
        {
            new TranscriptRegion(TranscriptRegionType.Exon,   13, 130463799, 130464144, 1581, 1926),
            new TranscriptRegion(TranscriptRegionType.Intron, 12, 130464145, 130465651, 1580, 1581),
            new TranscriptRegion(TranscriptRegionType.Exon,   12, 130465652, 130465664, 1568, 1580),
            new TranscriptRegion(TranscriptRegionType.Intron, 11, 130465665, 130465666, 1567, 1568),
            new TranscriptRegion(TranscriptRegionType.Exon,   11, 130465667, 130465772, 1462, 1567),
            new TranscriptRegion(TranscriptRegionType.Intron, 10, 130465773, 130474377, 1461, 1462),
            new TranscriptRegion(TranscriptRegionType.Exon,   10, 130474378, 130474534, 1305, 1461),
            new TranscriptRegion(TranscriptRegionType.Intron, 9,  130474535, 130488188, 1304, 1305),
            new TranscriptRegion(TranscriptRegionType.Exon,   9,  130488189, 130488201, 1292, 1304),
            new TranscriptRegion(TranscriptRegionType.Intron, 8,  130488202, 130489237, 1291, 1292),
            new TranscriptRegion(TranscriptRegionType.Exon,   8,  130489238, 130489279, 1250, 1291),
            new TranscriptRegion(TranscriptRegionType.Intron, 7,  130489280, 130490669, 1249, 1250),
            new TranscriptRegion(TranscriptRegionType.Exon,   7,  130490670, 130490740, 1179, 1249),
            new TranscriptRegion(TranscriptRegionType.Intron, 6,  130490741, 130496551, 1178, 1179),
            new TranscriptRegion(TranscriptRegionType.Exon,   6,  130496552, 130496622, 1108, 1178),
            new TranscriptRegion(TranscriptRegionType.Intron, 5,  130496623, 130499083, 1107, 1108),
            new TranscriptRegion(TranscriptRegionType.Exon,   5,  130499084, 130499221, 970,  1107),
            new TranscriptRegion(TranscriptRegionType.Intron, 4,  130499222, 130500535, 969,  970),
            new TranscriptRegion(TranscriptRegionType.Exon,   4,  130500536, 130500642, 863,  969),
            new TranscriptRegion(TranscriptRegionType.Intron, 3,  130500643, 130503445, 862,  863),
            new TranscriptRegion(TranscriptRegionType.Exon,   3,  130503446, 130503619, 689,  862),
            new TranscriptRegion(TranscriptRegionType.Intron, 2,  130503620, 130503779, 688,  689),
            new TranscriptRegion(TranscriptRegionType.Exon,   2,  130503780, 130503894, 574,  688),
            new TranscriptRegion(TranscriptRegionType.Intron, 1,  130503895, 130508714, 573,  574),
            new TranscriptRegion(TranscriptRegionType.Exon,   1,  130508715, 130509287, 1,    573)
        };

        public static readonly ITranscriptRegion[] ENST00000347849 =
        {
            new TranscriptRegion(TranscriptRegionType.Exon,   1,  130356045, 130356200, 1,    156),
            new TranscriptRegion(TranscriptRegionType.Intron, 1,  130356201, 130369132, 156,  157),
            new TranscriptRegion(TranscriptRegionType.Exon,   2,  130369133, 130369201, 157,  225),
            new TranscriptRegion(TranscriptRegionType.Intron, 2,  130369202, 130369764, 225,  226),
            new TranscriptRegion(TranscriptRegionType.Exon,   3,  130369765, 130369827, 226,  288),
            new TranscriptRegion(TranscriptRegionType.Intron, 3,  130369828, 130370047, 288,  289),
            new TranscriptRegion(TranscriptRegionType.Exon,   4,  130370048, 130370190, 289,  431),
            new TranscriptRegion(TranscriptRegionType.Intron, 4,  130370191, 130370556, 431,  432),
            new TranscriptRegion(TranscriptRegionType.Exon,   5,  130370557, 130370623, 432,  498),
            new TranscriptRegion(TranscriptRegionType.Intron, 5,  130370624, 130370704, 498,  499),
            new TranscriptRegion(TranscriptRegionType.Exon,   6,  130370705, 130370782, 499,  576),
            new TranscriptRegion(TranscriptRegionType.Intron, 6,  130370783, 130370874, 576,  577),
            new TranscriptRegion(TranscriptRegionType.Exon,   7,  130370875, 130370964, 577,  666),
            new TranscriptRegion(TranscriptRegionType.Intron, 7,  130370965, 130371198, 666,  667),
            new TranscriptRegion(TranscriptRegionType.Exon,   8,  130371199, 130371287, 667,  755),
            new TranscriptRegion(TranscriptRegionType.Intron, 8,  130371288, 130372256, 755,  756),
            new TranscriptRegion(TranscriptRegionType.Exon,   9,  130372257, 130372483, 756,  982),
            new TranscriptRegion(TranscriptRegionType.Intron, 9,  130372484, 130372872, 982,  983),
            new TranscriptRegion(TranscriptRegionType.Exon,   10, 130372873, 130372947, 983,  1057),
            new TranscriptRegion(TranscriptRegionType.Intron, 10, 130372948, 130373156, 1057, 1058),
            new TranscriptRegion(TranscriptRegionType.Exon,   11, 130373157, 130374571, 1058, 2472)
        };

        public static readonly ITranscriptRegion[] ENST00000427819 =
        {
            new TranscriptRegion(TranscriptRegionType.Exon,   1, 85276715, 85276797, 1,   83),
            new TranscriptRegion(TranscriptRegionType.Intron, 1, 85276798, 85277640, 83,  84),
            new TranscriptRegion(TranscriptRegionType.Exon,   2, 85277641, 85277738, 84,  181),
            new TranscriptRegion(TranscriptRegionType.Intron, 2, 85277739, 85376765, 181, 182),
            new TranscriptRegion(TranscriptRegionType.Exon,   3, 85376766, 85376835, 182, 251),
            new TranscriptRegion(TranscriptRegionType.Intron, 3, 85376836, 85380373, 251, 252),
            new TranscriptRegion(TranscriptRegionType.Exon,   4, 85380374, 85380565, 252, 443),
            new TranscriptRegion(TranscriptRegionType.Intron, 4, 85380566, 85398456, 443, 444),
            new TranscriptRegion(TranscriptRegionType.Exon,   5, 85398457, 85399963, 444, 1950)
        };
    }
}