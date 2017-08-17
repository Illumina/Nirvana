using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling;

namespace CommandLine.Builders
{
    public static class ValidationExtensions
    {
        public static IConsoleAppValidator Enable(this IConsoleAppValidator validator, bool condition, Action method)
        {
            if (condition) method();
            return validator;
        }

        /// <summary>
        /// checks if an input file exists
        /// </summary>
        public static IConsoleAppValidator CheckInputFilenameExists(this IConsoleAppValidator validator,
            string filePath, string description, string commandLineOption, string ignoreValue = null)
        {
            if (validator.SkipValidation) return validator;

            if (string.IsNullOrEmpty(filePath))
            {
                validator.Data.AddError(
                    $"The {description} file was not specified. Please use the {commandLineOption} parameter.",
                    ExitCodes.MissingCommandLineOption);
            }
            else if (ignoreValue != null && filePath == ignoreValue) { }
            else if (!File.Exists(filePath))
            {
                validator.Data.AddError($"The {description} file ({filePath}) does not exist.", ExitCodes.FileNotFound);
            }

            return validator;
        }

        /// <summary>
        /// checks if an input directory exists
        /// </summary>
        public static IConsoleAppValidator CheckEachDirectoryContainsFiles(this IConsoleAppValidator validator,
            IEnumerable<string> directories, string description, string commandLineOption, string searchPattern)
        {
            if (validator.SkipValidation) return validator;

            foreach (var directoryPath in directories)
            {
                var files = Directory.Exists(directoryPath) ? Directory.GetFiles(directoryPath, searchPattern) : null;
                if (files != null && files.Length != 0) continue;

                validator.Data.AddError(
                    $"The {description} directory ({directoryPath}) does not contain the required files ({searchPattern}). Please use the {commandLineOption} parameter.",
                    ExitCodes.FileNotFound);
            }

            return validator;
        }

        /// <summary>
        /// checks if the required parameter has been set
        /// </summary>
        public static IConsoleAppValidator HasRequiredParameter<T>(this IConsoleAppValidator validator,
            T parameterValue, string description, string commandLineOption)
        {
            if (validator.SkipValidation) return validator;

            if (EqualityComparer<T>.Default.Equals(parameterValue, default(T)))
            {
                validator.Data.AddError($"The {description} was not specified. Please use the {commandLineOption} parameter.",
                    ExitCodes.MissingCommandLineOption);
            }

            return validator;
        }
    }
}
