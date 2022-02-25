using System.Collections.Generic;
using IO;

namespace VariantAnnotation.GenericScore
{
    public sealed class ChromosomeBlock
    {
        private         List<ScoreIndexBlock> ScoreIndexBlocks { get; }
        public          int                   BlockCount;
        public readonly long                  StartingPosition;

        public ChromosomeBlock(List<ScoreIndexBlock> scoreIndexBlocks, int blockCount, long startingPosition)
        {
            ScoreIndexBlocks = scoreIndexBlocks;
            BlockCount       = blockCount;
            StartingPosition = startingPosition;
        }

        /// <summary>
        /// Add the index block to the list of all blocks for each chromosome
        /// </summary>
        /// <param name="indexBlock"></param>
        public void Add(ScoreIndexBlock indexBlock)
        {
            ScoreIndexBlocks.Add(indexBlock);
            BlockCount++;
        }
        
        /// <summary>
        /// Returns the index block corresponding to the blocknumber
        /// </summary>
        /// <param name="blockNumber"></param>
        /// <returns></returns>
        public ScoreIndexBlock Get(int blockNumber)
        {
            return blockNumber < BlockCount ? ScoreIndexBlocks[blockNumber] : null;
        }
        

        /// <summary>
        /// Serialize the instance to writer stream
        /// </summary>
        /// <param name="writer"></param>
        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOpt(BlockCount);
            writer.WriteOpt(StartingPosition);
            foreach (ScoreIndexBlock scoreIndexBlock in ScoreIndexBlocks)
            {
                scoreIndexBlock.Write(writer);
            }
        }

        /// <summary>
        /// Deserialize the instance from reader stream
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ChromosomeBlock Read(ExtendedBinaryReader reader)
        {
            int  blockCount       = reader.ReadOptInt32();
            long startingPosition = reader.ReadOptInt64();

            var scoreIndexBlocks = new List<ScoreIndexBlock>(blockCount);
            for (var i = 0; i < blockCount; i++)
            {
                scoreIndexBlocks.Add(ScoreIndexBlock.Read(reader));
            }

            return new ChromosomeBlock(scoreIndexBlocks, blockCount, startingPosition);
        }
    }
}