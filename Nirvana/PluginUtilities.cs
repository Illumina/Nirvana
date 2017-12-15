using System.Collections.Generic;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using ErrorHandling.Exceptions;
using VariantAnnotation.Interface.Plugins;

namespace Nirvana
{
    public static class PluginUtilities
    {
        private const string ConfigExtension = ".config";
        public static IPlugin[] LoadPlugins(string pluginDirectory)
        {
            IEnumerable<IPlugin> plugins;
            var executableLocation = Assembly.GetEntryAssembly().Location;
            var path = pluginDirectory ?? Path.Combine(Path.GetDirectoryName(executableLocation), "Plugins");

            if (!Directory.Exists(path)) return null;

            var pluginFileNames = Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);
            var assemblies = pluginFileNames.Select(AssemblyLoadContext.Default.LoadFromAssemblyPath).ToArray();

            var configuration = new ContainerConfiguration().WithAssemblies(assemblies);

            using (var container = configuration.CreateContainer())
            {
                plugins = container.GetExports<IPlugin>();
            }
            var pluginArray = plugins.ToArray();

            if (pluginFileNames.Length != pluginArray.Length)
                throw new UserErrorException("Some dlls are not plugins !!");

            foreach (var pluginFileName in pluginFileNames)
            {
                var configFilePath = Path.Combine(path, pluginFileName + ConfigExtension);
                if (! File.Exists(configFilePath))
                    throw new FileNotFoundException($"Missing expected config file: {configFilePath}!!");
            }
            return pluginArray;
        }
        
    }
}