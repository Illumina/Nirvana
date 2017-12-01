using System.Collections.Generic;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using VariantAnnotation.Interface.Plugins;

namespace Nirvana
{
    public static class PluginUtilities
    {
        public const string ConfigExtension = ".dll.config";
        public static IPlugin[] LoadPlugins(string pluginDirectory)
        {
            IEnumerable<IPlugin> plugins;
            var executableLocation = Assembly.GetEntryAssembly().Location;
            var path = pluginDirectory ?? Path.Combine(Path.GetDirectoryName(executableLocation), "Plugins");

            if (!Directory.Exists(path)) return null;

            var assemblies = Directory
                .GetFiles(path, "*.dll", SearchOption.AllDirectories)
                .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
                .ToList();

            var configuration = new ContainerConfiguration().WithAssemblies(assemblies);

            using (var container = configuration.CreateContainer())
            {
                plugins = container.GetExports<IPlugin>();
            }
            var pluginArray = plugins.ToArray();
            //check for config files

            foreach (var plugin in pluginArray)
            {
                var configFilePath = Path.Combine(path, plugin.Name + ConfigExtension);
                if (! File.Exists(configFilePath))
                    throw new FileNotFoundException($"Missing expected config file: {configFilePath}!!");
            }
            return pluginArray;
        }
        
    }
}