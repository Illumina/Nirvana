using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using ErrorHandling.DataStructures;
using NDesk.Options;
using Newtonsoft.Json.Linq;
using VariantAnnotation.CommandLine;
using Constants = VariantAnnotation.DataStructures.Constants;

namespace UpdateNirvanaVersion
{
    public class VersionUpdater : AbstractCommandLineHandler
    {
        #region members

        private bool _useGit;
        private bool _hasVersion;
        private readonly HashSet<string> _blackListedProjects;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public VersionUpdater(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        {
            _blackListedProjects = new HashSet<string> { "NDesk.Options", "UnitTests", "SandboxUnitTests" };
        }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            _useGit     = !string.IsNullOrEmpty(ConfigurationSettings.GitPath);
            _hasVersion = !string.IsNullOrEmpty(ConfigurationSettings.Version);

            CheckDirectoryExists(ConfigurationSettings.SourceDirectory, "source", "--dir");
            HasOneOptionSelected(_useGit, "--git", _hasVersion, "--toversion");

            if (_useGit)     CheckInputFilenameExists(ConfigurationSettings.GitPath, "git", "--git");
            if (_hasVersion) CheckVersionFormat(ConfigurationSettings.Version);
        }

        private void CheckVersionFormat(string version)
        {
            var versionRegex = new Regex("^\\d+.\\d+.\\d+$");
            var match = versionRegex.Match(version);
            if (match.Success) return;

            ErrorBuilder.AppendFormat("{0}ERROR: The specified version ({1}) does not conform to #.#.# notation.\n", ErrorSpacer, version);
            SetExitCode(ExitCodes.BadArguments);
        }

        protected override void ProgramExecution()
        {
            var vi = new VersionInfo();

            if (_useGit)
            {
                vi = GetGitVersion(ConfigurationSettings.GitPath, ConfigurationSettings.SourceDirectory);
            }
            else
            {
                vi.InformationalVersion = $"{ConfigurationSettings.Version}";
                vi.DependencyVersion    = $"{ConfigurationSettings.Version}-*";
            }

            UpdateVersionInProjectFiles(vi, ConfigurationSettings.SourceDirectory);
        }

        private void UpdateVersionInProjectFiles(VersionInfo vi, string sourceDirectory)
        {
            HashSet<string> projectDirs;
            var allProjectJsonPaths = Directory.GetFiles(sourceDirectory, "project.json", SearchOption.AllDirectories);
            var projectJsonPaths = Sanitize(allProjectJsonPaths, out projectDirs);

            foreach (var path in projectJsonPaths)
            {
                var backupPath = $"{path}.bak";
                UpdateProjectJsonFile(path, projectDirs, backupPath, vi);
            }

            Console.WriteLine("Updated {0} project.json file(s).", projectJsonPaths.Count);
        }

        private List<string> Sanitize(string[] projectJsonPaths, out HashSet<string> projectDirs)
        {
            var goodPaths = new List<string>();
            projectDirs   = new HashSet<string>();

            foreach (var path in projectJsonPaths)
            {
                var parentDir = Path.GetFileName(Path.GetDirectoryName(path));
                goodPaths.Add(path);
                projectDirs.Add(parentDir);
            }

            return goodPaths;
        }

        private void UpdateProjectJsonFile(string path, HashSet<string> projectDirs, string backupPath, VersionInfo vi)
        {
            var json        = File.ReadAllText(path);
            var projectJson = JObject.Parse(json);

            bool updateVersion = true;
            var parentDir = Path.GetFileName(Path.GetDirectoryName(path));
            if (_blackListedProjects.Contains(parentDir)) updateVersion = false;

            if(updateVersion) projectJson["version"] = vi.InformationalVersion;

            var dependencies = (JObject)projectJson["dependencies"];

            foreach (var line in dependencies)
            {
                if (!projectDirs.Contains(line.Key) || _blackListedProjects.Contains(line.Key)) continue;
                dependencies[line.Key] = vi.DependencyVersion;
            }

            using (var writer = new StreamWriter(new FileStream(backupPath, FileMode.Create)))
            {
                writer.WriteLine(projectJson);
            }

            File.Delete(path);
            File.Move(backupPath, path);
        }

        private static VersionInfo GetGitVersion(string gitPath, string sourceDirectory)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute        = false,
                    RedirectStandardOutput = true,
                    FileName               = gitPath,
                    Arguments              = "describe --long",
                    WorkingDirectory       = sourceDirectory
                }
            };

            process.Start();
            string results = process.StandardOutput.ReadToEnd();

            var match = new Regex("v((\\d+).(\\d+).(\\d+)-(\\d+)-(\\S+))", RegexOptions.Compiled).Match(results);
            if (!match.Success)
            {
                Console.WriteLine("Unable to successfully extract the version information from: [{0}]", results);
                Environment.Exit(1);
            }

            return new VersionInfo
            {
                InformationalVersion = match.Groups[1].Value,
                DependencyVersion    = $"{match.Groups[2].Value}.{match.Groups[3].Value}.{match.Groups[4].Value}-*"
            };
        }

        public static void Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "git|g=",
                    "update using {git path}",
                    v => ConfigurationSettings.GitPath = v
                },
                {
                    "dir|d=",
                    "source {directory}",
                    v => ConfigurationSettings.SourceDirectory = v
                },
                {
                    "toversion=",
                    "update to a specific {version}",
                    v => ConfigurationSettings.Version = v
                }
            };

            var commandLineExample = "-g /usr/bin/git -d /home/test/nirvana";

            var parser = new VersionUpdater("updates the versions in the project.json files", ops, commandLineExample, Constants.Authors);
            parser.Execute(args);
        }

        private class VersionInfo
        {
            public string InformationalVersion;
            public string DependencyVersion;
        }
    }
}
