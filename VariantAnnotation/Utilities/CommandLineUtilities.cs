using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.Utilities
{
    public static class CommandLineUtilities {

        #region members

        private static readonly string Copyright;
        private static readonly string Title;
        public static readonly string InformationalVersion;
        public static readonly string Version;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        static CommandLineUtilities()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null) return;

            // get the assembly fields
            Copyright            = GetCopyright(entryAssembly);
            Title                = GetTitle(entryAssembly);
            Version              = GetVersion(entryAssembly);
            InformationalVersion = GetInformationalVersion(entryAssembly);
        }

        private static string GetCopyright(Assembly entryAssembly)
        {
            var attr = GetAssemblyAttributes<AssemblyCopyrightAttribute>(entryAssembly);
            return attr == null ? $"(c) {DateTime.Now.Year} Illumina, Inc." : attr.Copyright.Replace("©", "(c)");
        }

        private static string GetVersion(Assembly entryAssembly)
        {
            var attr = GetAssemblyAttributes<AssemblyFileVersionAttribute>(entryAssembly);
            return attr == null ? "1.0.0" : attr.Version;
        }

        private static string GetInformationalVersion(Assembly entryAssembly)
        {
            var attr = GetAssemblyAttributes<AssemblyInformationalVersionAttribute>(entryAssembly);
            return attr == null ? "1.0.0" : attr.InformationalVersion;
        }

        private static string GetTitle(Assembly entryAssembly)
        {
            var attr = GetAssemblyAttributes<AssemblyTitleAttribute>(entryAssembly);
            return attr == null ? entryAssembly.GetName().Name : attr.Title;
        }

        private static T GetAssemblyAttributes<T>(Assembly entryAssembly)
        {
            var attrs = entryAssembly.GetCustomAttributes(typeof(T)) as T[];
            // ReSharper disable once PossibleNullReferenceException
            return attrs.Length == 0 ? default(T) : attrs[0];
        }

        /// <summary>
        /// Displays the command-line banner for this program
        /// </summary>
        public static void DisplayBanner(string author)
        {
            // create the top and bottom lines
            const int lineLength = 75;
            var line = new string('-', lineLength);

            // create the filler string
            int fillerLength  = lineLength - Title.Length - Copyright.Length;
            int fillerLength2 = lineLength - author.Length - InformationalVersion.Length;

            if (fillerLength < 1) {
                throw new GeneralException("Unable to display the program banner, the program name is too long.");
            }

            if (fillerLength2 < 1)
            {
                throw new GeneralException("Unable to display the program banner, the author name and version string is too long.");
            }

            var filler  = new string(' ', fillerLength);
            var filler2 = new string(' ', fillerLength2);

            // display the actual banner
            Console.WriteLine(line);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(Title);
            Console.ResetColor();
            Console.WriteLine("{0}{1}", filler, Copyright);
            Console.WriteLine("{0}{1}{2}", author, filler2, InformationalVersion);
            Console.WriteLine("{0}\n", line);
        }

        /// <summary>
        /// Displays a list of the unsupported options encountered on the command line
        /// </summary>
        public static void ShowUnsupportedOptions(List<string> unsupportedOps)
        {
            if ((unsupportedOps == null) || (unsupportedOps.Count == 0)) return;

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Unsupported options:");
            Console.ResetColor();

            var spacer = new string(' ', 3);
            foreach (var option in unsupportedOps)
            {
                Console.WriteLine(spacer + option);
            }
        }
    }
}
