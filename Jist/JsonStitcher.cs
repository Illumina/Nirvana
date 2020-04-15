using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
        private readonly HashSet<string> _geneLines;

        public JsonStitcher(Stream[] jsonStreams, Stream[] jasixStreams, Stream outStream, bool leaveOutStreamOpen=false)
        {
            _jasixStreams = jasixStreams;
            _jsonStreams = jsonStreams;
            _outStream = outStream;
            _leaveOutStreamOpen = leaveOutStreamOpen;
            _geneLines = new HashSet<string>();
        }
        
        public const string GeneHeaderLine = "\n],\"genes\":[";
        public const string FooterLine = "]}";

        private static bool _isFirstHeaderBlock = true;
        private static readonly byte[] BgzBlock = new byte[BlockGZipStream.BlockGZipFormatCommon.MaxBlockSize];
        private static readonly byte[] CommaBlock = JistUtilities.GetCompressedBlock(",\n");//will be added to the end of a block when needed
        
        
        public int Stitch()
        {
            var positionBlockCount = 0;
            var geneLineCount = 0;

            using (var writer = new BinaryWriter(_outStream, Encoding.Default, _leaveOutStreamOpen))
            {
                var needsCommaBlock = false;
                
                for (var i=0; i < _jsonStreams.Length; i++)
                {
                    if (needsCommaBlock) writer.Write(CommaBlock, 0, CommaBlock.Length);
                    var jsonStream = _jsonStreams[i];
                    var jasixStream = _jasixStreams[i];
                    
                    positionBlockCount+= WritePositionBlocks(jsonStream, jasixStream, writer);
                    geneLineCount+= ReadGeneLines(jsonStream);
                    //after the first file, every file will need a comma block to maintain valid json after positions block
                    // and after each gene block
                    needsCommaBlock = true;
                }
                writer.Flush();
                //write out the gene blocks
                WriteGeneBlocks(_outStream);
            }

            Console.WriteLine($"Total position blocks written: {positionBlockCount}");
            Console.WriteLine($"Gene lines read: {geneLineCount}");
            Console.WriteLine($"Unique gene lines: {_geneLines.Count}");
            return (int) ExitCodes.Success;
        }

        private int ReadGeneLines(Stream jsonStream)
        {
            var lineCount = 0;
            using (var bGzipStream = new BlockGZipStream(jsonStream, CompressionMode.Decompress))
            using(var reader = new StreamReader(bGzipStream))
            {
                string line;
                while ((line= reader.ReadLine())!= null)
                {
                    if (line==string.Empty) continue;
                    if (line == FooterLine) break;
                    if (!line.EndsWith(',')) line += ',';
                    lineCount++;
                    _geneLines.Add(line);
                }
            }

            return lineCount;
        }

        private void WriteGeneBlocks(Stream stream)
        {
            
            using (var bGzipStream = new BlockGZipStream(stream, CompressionMode.Compress, _leaveOutStreamOpen)) 
            using(var writer = new StreamWriter(bGzipStream))
            {
                var count = _geneLines.Count;
                if (count == 0)
                {
                    writer.WriteLine(FooterLine);
                    return;
                }
                writer.WriteLine(GeneHeaderLine);
                var i = 0;
                foreach (var geneLine in _geneLines.OrderBy(x=>x))
                {
                    i++;
                    //the last gene line shouldn't have a comma at the end
                    writer.WriteLine(i == count ? geneLine.TrimEnd(',') : geneLine);
                }
                writer.WriteLine(FooterLine);
            }
        }

        private static int WritePositionBlocks(Stream jsonStream, Stream jasixStream,
            BinaryWriter writer)
        {
            var blockCount = 0;
            using (var reader = new BgzBlockReader(jsonStream, true))
            using (var jasixIndex = new JasixIndex(jasixStream))
            {
                int count;
                var isFirstBlock     = true;
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
                            //setting back the stream to the gene section begin
                            jsonStream.Position = geneSectionBegin >> 16;
                            return blockCount;
                        }


                        if (reader.Position << 16 <= positionSectionBegin ||
                            reader.Position << 16 >= geneSectionBegin) continue;
                        blockCount++;
                        writer.Write(BgzBlock, 0, count);
                    }
                } while (count > 0);
            }

            return blockCount;
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