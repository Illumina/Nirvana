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

        public static IConsoleAppValidator CheckNonZero(this IConsoleAppValidator validator, int num, string description)
        {
            if (num == 0)
            {
                validator.Data.AddError(
                    $"At least one {description} should be provided.",
                    ExitCodes.MissingCommandLineOption);
            }
            return validator;
        }

        /// <summary>
        /// check if each file exists
        /// </summary>
        public static IConsoleAppValidator CheckEachFilenameExists(this IConsoleAppValidator validator,
            IEnumerable<string> filePaths, string description, string commandLineOption, bool isRequired = true)
        {
            foreach (string filePath in filePaths)
            {
                validator.CheckInputFilenameExists(filePath, description, commandLineOption, isRequired);
            }
            return validator;
        }

        /// <summary>
        /// checks if an input file exists
        /// </summary>
        public static IConsoleAppValidator CheckInputFilenameExists(this IConsoleAppValidator validator,
            string filePath, string description, string commandLineOption, bool isRequired = true, string ignoreValue = null)
        {
            if (validator.SkipValidation) return validator;

            if (string.IsNullOrEmpty(filePath) && isRequired)
            {
                validator.Data.AddError(
                    $"The {description} file was not specified. Please use the {commandLineOption} parameter.",
                    ExitCodes.MissingCommandLineOption);
            }
            else if (isRequired && (ignoreValue == null || filePath != ignoreValue) && !File.Exists(filePath))
            {
                validator.Data.AddError($"The {description} file ({filePath}) does not exist.", ExitCodes.FileNotFound);
            }

            return validator;
        }

        /// <summary>
        /// checks if an input file exists and has the appropriate filename suffix
        /// </summary>
        public static IConsoleAppValidator CheckOutputFilenameSuffix(this IConsoleAppValidator validator,
            string filePath, string fileSuffix, string description)
        {
            if (validator.SkipValidation) return validator;

            if (!filePath.EndsWith(fileSuffix))
            {
                validator.Data.AddError($"The {description} file ({filePath}) does not end with a {fileSuffix}.", ExitCodes.BadArguments);
            }

            return validator;
        }

        /// <summary>
        /// checks if an input directory exists
        /// </summary>
        public static IConsoleAppValidator CheckDirectoryExists(this IConsoleAppValidator validator, string dirPath,
            string description, string commandLineOption)
        {
            if (validator.SkipValidation) return validator;

            if (string.IsNullOrEmpty(dirPath))
            {
                validator.Data.AddError(
                    $"The {description} directory was not specified. Please use the {commandLineOption} parameter.",
                    ExitCodes.MissingCommandLineOption);
            }
            else if (!Directory.Exists(dirPath))
            {
                validator.Data.AddError($"The {description} directory ({dirPath}) does not exist.", ExitCodes.PathNotFound);
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

            foreach (string directoryPath in directories)
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

        /// <summary>
        /// checks if the required date has been set and is parseable
        /// </summary>
        public static IConsoleAppValidator HasRequiredDate(this IConsoleAppValidator validator, string date,
            string description, string commandLineOption)
        {
            if (validator.SkipValidation) return validator;

            validator.HasRequiredParameter(date, description, commandLineOption);
            if (string.IsNullOrEmpty(date)) return validator;

            if (!DateTime.TryParse(date, out var _))
            {
                validator.Data.AddError($"The {description} was not specified as a date (YYYY-MM-dd). Please use the {commandLineOption} parameter.", ExitCodes.BadArguments);
            }

            return validator;
        }
    }
}
