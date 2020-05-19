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
                Console.WriteLine("Usage: dotnet jist.dll input-json.gz-prefix output-json.gz ");
                Environment.Exit((int)ExitCodes.UserError);
            }

            var inputFilePrefix = args[0];
            var outputFileName = args[1];
            var directory = Path.GetDirectoryName(inputFilePrefix);
            if (string.IsNullOrEmpty(directory)) directory = ".";
            var prefix = Path.GetFileName(inputFilePrefix);
            var inputFiles = Directory.GetFiles(directory, prefix+"*.json.gz");
            Array.Sort(inputFiles);
            Console.WriteLine("Files to stitch");
            foreach (var file in inputFiles)
            {
                Console.WriteLine(file);
                if (!File.Exists(file + JasixCommons.FileExt))
                {
                    Console.WriteLine($"Cannot find {file +JasixCommons.FileExt}. Please provide corresponding {JasixCommons.FileExt} files for each input JSON");
                    return (int)ExitCodes.UserError;
                }
            }

            if (inputFiles.Length == 0)
            {
                Console.WriteLine($"Found {inputFiles.Length} files to stitch. Need at least 1.");
                Environment.Exit((int)ExitCodes.UserError);
            }
            
            if (inputFiles.Length == 1)
            {
                Console.WriteLine("Found only one input JSON. Copying it to output file...");
                File.Copy(inputFiles[0], outputFileName, true);
                return (int)ExitCodes.Success;
            }

            
            var inputStreams = new Stream[inputFiles.Length];
            var indexStreams = new Stream[inputFiles.Length];
            for (var i = 0; i < inputFiles.Length; i++)
            {
                inputStreams[i] = FileUtilities.GetReadStream(inputFiles[i]);
                indexStreams[i] = FileUtilities.GetReadStream(inputFiles[i] + JasixCommons.FileExt);
            }

            using(var outputStream = FileUtilities.GetCreateStream(outputFileName))
            using (var stitcher = new JsonStitcher(inputStreams, indexStreams, outputStream))
            {
                return stitcher.Stitch();
            }
        }
        
    }
}