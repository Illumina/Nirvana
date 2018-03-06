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
        private const string DllExtension = ".dll";
        private const string ConfigExtension = ".config";
        public static IPlugin[] LoadPlugins(string pluginDirectory)
        {
            var executableLocation = Assembly.GetEntryAssembly().Location;
            var path = pluginDirectory ?? Path.Combine(Path.GetDirectoryName(executableLocation), "Plugins");

            if (!Directory.Exists(path)) return null;

            var pluginFileNames = Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);
            var assemblies = pluginFileNames.Select(AssemblyLoadContext.Default.LoadFromAssemblyPath).ToArray();

            var configuration = new ContainerConfiguration().WithAssemblies(assemblies);

            IPlugin[] plugins;
            using (var container = configuration.CreateContainer())
            {
                plugins = container.GetExports<IPlugin>().ToArray();
            }

            foreach (var plugin in plugins)
            {
                var configFilePath = Path.Combine(path, plugin.Name + DllExtension + ConfigExtension);
                if (!File.Exists(configFilePath))
                    throw new FileNotFoundException($"Missing expected config file: {configFilePath}!!");
            }
            return plugins;
        }

    }
}