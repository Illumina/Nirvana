using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Compression.FileHandling;
using ErrorHandling;
using Jasix.DataStructures;

namespace Jist
{
    public sealed class JsonStitcher:IDisposable
    {
        private readonly Stream[] _jsonStreams;
        private readonly Stream[] _jasixStreams;
        private readonly Stream _outStream;
        private readonly bool _leaveOutStreamOpen;

        public JsonStitcher(Stream[] jsonStreams, Stream[] jasixStreams, Stream outStream, bool leaveOutStreamOpen=false)
        {
            _jasixStreams = jasixStreams;
            _jsonStreams = jsonStreams;
            _outStream = outStream;
            _leaveOutStreamOpen = leaveOutStreamOpen;
        }
        
        public const string GeneHeaderLine = "],\"genes\":[\n";
        public const string FooterLine = "\n]}";

        private static bool _isFirstHeaderBlock = true;
        private static readonly byte[] BgzBlock = new byte[BlockGZipStream.BlockGZipFormatCommon.MaxBlockSize];
        private static readonly byte[] FooterBlock = JistUtilities.GetCompressedBlock(FooterLine);
        private static readonly byte[] CommaBlock = JistUtilities.GetCompressedBlock(",\n");//will be added to the end of a block when needed
        
        
        public int Stitch()
        {
            //gene blocks need to be saved to be written down at the end of all positions
            var geneBlocks = new List<byte[]>();

            var totalBlockCount = 0;

            using (var writer = new BinaryWriter(_outStream, Encoding.Default, _leaveOutStreamOpen))
            {
                var needsCommaBlock = false;
                for (var i=0; i < _jsonStreams.Length; i++)
                {
                    if (needsCommaBlock) writer.Write(CommaBlock, 0, CommaBlock.Length);
                    var jsonStream = _jsonStreams[i];
                    var jasixStream = _jasixStreams[i];

                    WritePositionBlocks(jsonStream, jasixStream, writer, geneBlocks, ref totalBlockCount);
                    //after the first file, every file will need a comma block to maintain valid json
                    needsCommaBlock = true;
                }
                //write out the gene blocks
                WriteGeneBlocks(writer, geneBlocks);
                writer.Write(FooterBlock, 0, FooterBlock.Length);
            }

            Console.WriteLine($"Total blocks written: {totalBlockCount}");
            return (int) ExitCodes.Success;
        }

        private static void WriteGeneBlocks(BinaryWriter writer, List<byte[]> geneBlocks)
        {
            var  geneHeaderBlock = JistUtilities.GetCompressedBlock(GeneHeaderLine);
            writer.Write(geneHeaderBlock, 0, geneHeaderBlock.Length);
            var needsCommaBlock = false;
            
            foreach (var geneBlock in geneBlocks)
            {
                if (needsCommaBlock) writer.Write(CommaBlock, 0, CommaBlock.Length);
                writer.Write(geneBlock, 0, geneBlock.Length);
                needsCommaBlock = true;
            }
        }

        private static void WritePositionBlocks(Stream jsonStream, Stream jasixStream,
            BinaryWriter writer, List<byte[]> geneBlocks, ref int totalBlockCount)
        {
            using (var reader = new BgzBlockReader(jsonStream))
            using (var jasixIndex = new JasixIndex(jasixStream))
            {
                int count;
                var isFirstBlock         = true;
                var positionSectionBegin = jasixIndex.GetSectionBegin(JasixCommons.PositionsSectionTag);
                var geneSectionBegin     = jasixIndex.GetSectionBegin(JasixCommons.GenesSectionTag);
                var geneSectionEnd       = jasixIndex.GetSectionEnd(JasixCommons.GenesSectionTag);
                do
                {
                    count = reader.ReadCompressedBlock(BgzBlock);
                    if (isFirstBlock)
                    {
                        if (_isFirstHeaderBlock)
                        {
                            writer.Write(BgzBlock, 0, count);
                            _isFirstHeaderBlock = false;
                        }

                        isFirstBlock = false;
                    }
                    else
                    {
                        if (count <= 0) continue;
                        // the 16 bit left shift is due to the format of bgzip file
                        if (reader.Position << 16 > geneSectionBegin && reader.Position << 16 <= geneSectionEnd)
                        {
                            //saving the gene blocks to be appended later
                            var geneBlock = new byte[count];
                            Array.Copy(BgzBlock, 0, geneBlock, 0, count);
                            geneBlocks.Add(geneBlock);
                            continue;
                        }

                        if (reader.Position << 16 > positionSectionBegin &&
                            reader.Position << 16 < geneSectionBegin)
                        {
                            totalBlockCount++;
                            writer.Write(BgzBlock, 0, count);
                        }
                    }
                } while (count > 0);
            }
            
        }

        public void Dispose()
        {
            if (_jsonStreams != null)
            {
                foreach (Stream jsonStream in _jsonStreams)
                {
                    jsonStream?.Dispose();
                }
            }
            
            if (_jasixStreams != null)
            {
                foreach (Stream jasixStream in _jasixStreams)
                {
                    jasixStream?.Dispose();
                }
            }

            if (_leaveOutStreamOpen)
            {
                _outStream.Flush();
                return;
            }

            _outStream?.Dispose();
        }
    }
}