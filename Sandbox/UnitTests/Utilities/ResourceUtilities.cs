using System.IO;
using System.Reflection;

namespace UnitTests.Utilities
{
    public static class ResourceUtilities
    {
        /// <summary>
        /// given a resource filename, this method returns a stream corresponding to the file if
        /// it exists. Otherwise a file not found exception is thrown.
        /// </summary>
        // ReSharper disable once UnusedParameter.Global
        public static Stream GetResourceStream(string resourcePath, bool checkMissingFile = true)
        {
            var stream = Assembly.GetEntryAssembly().GetManifestResourceStream(resourcePath);

            if (checkMissingFile && stream == null)
            {
                throw new FileNotFoundException($"ERROR: The embedded resource file ({resourcePath}) was not found.");
            }

            return stream;
        }
    }
}
