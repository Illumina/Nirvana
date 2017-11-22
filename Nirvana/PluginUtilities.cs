using System.Collections;
using System.Collections.Generic;
using System.Composition;
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
        public static IEnumerable<IPlugin> LoadPlugins(string pluginDirectory)
        {
            IEnumerable<IPlugin> plugins;
            var executableLocation = Assembly.GetEntryAssembly().Location;
            var path = pluginDirectory ?? Path.Combine(Path.GetDirectoryName(executableLocation), "Plugins");

            if (!Directory.Exists(path)) return null;

            var assemblies = Directory
                .GetFiles(path, "*.dll", SearchOption.AllDirectories)
                .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
                .ToList();
            var configuration = new ContainerConfiguration()
                .WithAssemblies(assemblies);
            using (var container = configuration.CreateContainer())
            {
                plugins = container.GetExports<IPlugin>();
            }

            return plugins;
        }
    }
}