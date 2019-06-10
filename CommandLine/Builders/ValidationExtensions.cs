using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ErrorHandling;

namespace CommandLine.Builders
{
    public static class ValidationExtensions
    {
        public static IConsoleAppValidator CheckEachFilenameExists(this IConsoleAppValidator validator,
            IEnumerable<string> filePaths, string description, string commandLineOption, bool isRequired = true)
        {
            foreach (string filePath in filePaths)
            {
                validator.CheckInputFilenameExists(filePath, description, commandLineOption, isRequired);
            }
            return validator;
        }

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
            else if (isRequired && (ignoreValue == null || filePath != ignoreValue) && !File.Exists(filePath) && !CheckUrlExist(filePath))
            {
                validator.Data.AddError($"The {description} file ({filePath}) does not exist.", ExitCodes.FileNotFound);
            }

            return validator;
        }

        private static bool CheckUrlExist(string url)
        {
            try
            {
                var webRequest = WebRequest.Create(url);
                webRequest.GetResponse();
            }
            catch //If exception thrown then couldn't get response from address
            {
                return false;
            }
            return true;
        }      

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

        public static IConsoleAppValidator HasRequiredParameter<T>(this IConsoleAppValidator validator,
            T parameterValue, string description, string commandLineOption)
        {
            if (validator.SkipValidation) return validator;

            if (EqualityComparer<T>.Default.Equals(parameterValue, default))
            {
                validator.Data.AddError($"The {description} was not specified. Please use the {commandLineOption} parameter.",
                    ExitCodes.MissingCommandLineOption);
            }

            return validator;
        }

        public static IConsoleAppValidator HasRequiredDate(this IConsoleAppValidator validator, string date,
            string description, string commandLineOption)
        {
            if (validator.SkipValidation) return validator;

            validator.HasRequiredParameter(date, description, commandLineOption);
            if (string.IsNullOrEmpty(date)) return validator;

            if (!DateTime.TryParse(date, out _))
            {
                validator.Data.AddError($"The {description} was not specified as a date (YYYY-MM-dd). Please use the {commandLineOption} parameter.", ExitCodes.BadArguments);
            }

            return validator;
        }
    }
}
