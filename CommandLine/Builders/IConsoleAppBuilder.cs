using System;
using System.Collections.Generic;
using CommandLine.NDesk.Options;
using ErrorHandling;
using VariantAnnotation.Interface.Providers;

namespace CommandLine.Builders
{
    /// We are using separate interfaces to enforce ordering in the console application
    /// builder.

    public interface IConsoleAppBuilder
    {
        // ReSharper disable once UnusedMemberInSuper.Global
        IConsoleAppBuilder UseVersionProvider(IVersionProvider versionProvider);
        IConsoleAppValidator Parse();
    }

    public interface IConsoleAppValidator
    {
        IConsoleAppValidator DisableOutput(bool condition = true);
        IConsoleAppBanner ShowBanner(string authors);
        IConsoleAppBanner SkipBanner();
        IConsoleAppBuilderData Data { get; }
        bool SkipValidation { get; }
    }

    public interface IConsoleAppBanner
    {
        IConsoleAppHelpMenu ShowHelpMenu(string description, string commandLineExample);
    }

    public interface IConsoleAppHelpMenu
    {
        IConsoleAppErrors ShowErrors();
    }

    public interface IConsoleAppErrors
    {
        ExitCodes Execute(Func<ExitCodes> executeMethod);
    }

    public interface IConsoleAppBuilderData
    {
        OptionSet Ops { get; }
        List<string> UnsupportedOps { get; set; }
        List<string> Errors { get; }
        ExitCodes ExitCode { get; set; }
        bool DisableOutput { get; set; }
        bool HasArguments { get; }
        IVersionProvider VersionProvider { get; set; }
        bool ShowHelpMenu { get; set; }
        bool ShowVersion { get; set; }
        void AddError(string errorMessage, ExitCodes exitCode);
    }
}
