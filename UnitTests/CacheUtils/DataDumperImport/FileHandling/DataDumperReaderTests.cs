using System.IO;
using System.Text;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.IO;
using Xunit;

namespace UnitTests.CacheUtils.DataDumperImport.FileHandling
{
    public sealed class DataDumperReaderTests
    {
        [Fact]
        public void GetRootNode_EndToEnd()
        {
            ObjectKeyValueNode rootNode;

            using (var ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
                {
                    writer.WriteLine("$VAR1 = {");
                    writer.WriteLine("          '22' => {");
                    writer.WriteLine("                    'RegulatoryFeature' => [");
                    writer.WriteLine("                                             bless( {");
                    writer.WriteLine("                                                      'seq' => 'AGGGG'");
                    writer.WriteLine("                                                      'tmp_frequencies' => '87 167 281 56 8 744 40 107 851 5 333 54 12 56 104 372 82 117 402");
                    writer.WriteLine("291 145 49 800 903 13 528 433 11 0 3 12 0 8 733 13 482 322 181");
                    writer.WriteLine("76 414 449 21 0 65 334 48 32 903 566 504 890 775 5 507 307 73 266");
                    writer.WriteLine("459 187 134 36 2 91 11 324 18 3 9 341 8 71 67 17 37 396 59");
                    writer.WriteLine("'");
                    writer.WriteLine("                                                      'cell_types' => {},");
                    writer.WriteLine("                                                      '_bound_lengths' => [");
                    writer.WriteLine("                                                                            0,");
                    writer.WriteLine("                                                                            0");
                    writer.WriteLine("                                                                          ],");
                    writer.WriteLine("                                                      'transcript' => $VAR1->{'1'}[0],");
                    writer.WriteLine("                                                    }, 'Bio::EnsEMBL::Funcgen::RegulatoryFeature' )");
                    writer.WriteLine("                                           ]");
                    writer.WriteLine("                  }");
                    writer.WriteLine("        };");
                }

                ms.Position = 0;

                using (var reader = new DataDumperReader(ms)) rootNode = reader.GetRootNode();
            }

            Assert.NotNull(rootNode);
            var node = rootNode;
            Assert.Equal("$VAR1", node.Key);

            var chr22Node = node.Value.Values[0] as ObjectKeyValueNode;
            Assert.NotNull(chr22Node);
            Assert.Equal("22", chr22Node.Key);

            var rfNode = chr22Node.Value.Values[0] as ListObjectKeyValueNode;
            Assert.NotNull(rfNode);
            Assert.Equal("RegulatoryFeature", rfNode.Key);

            var blessNode = rfNode.Values[0] as ObjectValueNode;
            Assert.NotNull(blessNode);
            Assert.Null(blessNode.Key);
            Assert.Equal("Bio::EnsEMBL::Funcgen::RegulatoryFeature", blessNode.Type);

            var nodes = blessNode.Values;
            var seqNode = nodes[0] as StringKeyValueNode;
            Assert.NotNull(seqNode);
            Assert.Equal("seq", seqNode.Key);
            Assert.Equal("AGGGG", seqNode.Value);

            var tmpFreqNode = nodes[1] as StringKeyValueNode;
            Assert.NotNull(tmpFreqNode);
            Assert.Equal("tmp_frequencies", tmpFreqNode.Key);
            Assert.Equal("87 167 281 56 8 744 40 107 851 5 333 54 12 56 104 372 82 117 402 291 145 49 800 903 13 528 433 11 0 3 12 0 8 733 13 482 322 181 76 414 449 21 0 65 334 48 32 903 566 504 890 775 5 507 307 73 266 459 187 134 36 2 91 11 324 18 3 9 341 8 71 67 17 37 396 59", tmpFreqNode.Value);

            var cellTypesNode = nodes[2] as StringKeyValueNode;
            Assert.NotNull(cellTypesNode);
            Assert.Equal("cell_types", cellTypesNode.Key);
            Assert.Null(cellTypesNode.Value);

            var boundLengthsNode = nodes[3] as ListObjectKeyValueNode;
            Assert.NotNull(boundLengthsNode);
            Assert.Equal("_bound_lengths", boundLengthsNode.Key);

            var bl1Node = boundLengthsNode.Values[0] as StringValueNode;
            Assert.NotNull(bl1Node);
            Assert.Equal("0", bl1Node.Key);

            var bl2Node = boundLengthsNode.Values[1] as StringValueNode;
            Assert.NotNull(bl2Node);
            Assert.Equal("0", bl2Node.Key);

            var transcriptNode = nodes[4] as StringKeyValueNode;
            Assert.NotNull(transcriptNode);
            Assert.Equal("transcript", transcriptNode.Key);
            Assert.Equal("$VAR1->{'1'}[0]", transcriptNode.Value);
        }

        [Fact]
        public void GetRootNode_ObjectValue_UnhandledEntryType_ThrowsException()
        {
            Assert.Throws<InvalidDataException>(delegate
            {
                using (var ms = new MemoryStream())
                {
                    using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
                    {
                        writer.WriteLine("$VAR1 = {");
                        writer.WriteLine("                bless( {");
                        writer.WriteLine("                        0");
                        writer.WriteLine("                }, 'Bio::EnsEMBL::Funcgen::RegulatoryFeature' )");
                        writer.WriteLine("        };");
                    }

                    ms.Position = 0;
                    using (var reader = new DataDumperReader(ms)) reader.GetRootNode();
                }
            });
        }

        [Fact]
        public void GetRootNode_ListObjectKeyValue_UnhandledEntryType_ThrowsException()
        {
            Assert.Throws<InvalidDataException>(delegate
            {
                using (var ms = new MemoryStream())
                {
                    using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
                    {
                        writer.WriteLine("$VAR1 = {");
                        writer.WriteLine("                '_bound_lengths' => [");
                        writer.WriteLine("                        'seq' => 'AGGGG'");
                        writer.WriteLine("                ]");
                        writer.WriteLine("        };");
                    }

                    ms.Position = 0;
                    using (var reader = new DataDumperReader(ms)) reader.GetRootNode();
                }
            });
        }

        [Fact]
        public void GetRootNode_EmptyStream_ThrowsException()
        {
            Assert.Throws<InvalidDataException>(delegate
            {
                using (var ms = new MemoryStream())
                {
                    using (var reader = new DataDumperReader(ms)) reader.GetRootNode();
                }
            });
        }

        [Fact]
        public void GetRootNode_NoRootObject_ThrowsException()
        {
            Assert.Throws<InvalidDataException>(delegate
            {
                using (var ms = new MemoryStream())
                {
                    using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
                    {
                        writer.WriteLine("'seq' => 'AGGGG'");
                    }

                    ms.Position = 0;
                    using (var reader = new DataDumperReader(ms)) reader.GetRootNode();
                }
            });
        }
    }
}
