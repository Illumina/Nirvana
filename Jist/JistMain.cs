using System;
using System.IO;
using ErrorHandling;
using IO;
using Jasix.DataStructures;

namespace Jist
{
    public class JistMain
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("Running Nirvana Json Stitching tool");
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: dotnet jist.dll input-Json1-pattern outputJson.bgz ");
                Environment.Exit((int)ExitCodes.UserError);
            }

            var inputFilePattern = args[0];
            var outputStream = FileUtilities.GetCreateStream(args[1]);
            var inputFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), inputFilePattern);
            Array.Sort(inputFiles);
            Console.WriteLine("Files to stitch");
            foreach (var file in inputFiles)
            {
                Console.WriteLine(file);
            }
            if (inputFiles.Length < 2)
            {
                
                Console.WriteLine($"Found {inputFiles.Length} files to stitch. Need at least 2.");
                Environment.Exit((int)ExitCodes.UserError);
            }

            var inputStreams = new Stream[inputFiles.Length];
            var indexStreams = new Stream[inputFiles.Length];
            for (var i = 0; i < inputFiles.Length; i++)
            {
                inputStreams[i] = FileUtilities.GetReadStream(inputFiles[i]);
                indexStreams[i] = FileUtilities.GetReadStream(inputFiles[i] + JasixCommons.FileExt);
            }

            using (var stitcher = new JsonStitcher(inputStreams, indexStreams, outputStream))
            {
                return stitcher.Stitch();
            }
        }
        
    }
}