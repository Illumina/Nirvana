using System;
using System.Collections.Generic;
using ErrorHandling;

namespace CommandLine.Builders
{
    public interface ITopLevelAppBuilder
    {
        // ReSharper disable once UnusedMemberInSuper.Global
        ITopLevelAppValidator Parse();
    }

    public interface ITopLevelAppValidator
    {
        ITopLevelAppBanner ShowBanner(string authors);
    }

    public interface ITopLevelAppBanner
    {
        ITopLevelAppHelpMenu ShowHelpMenu(string description);
    }

    public interface ITopLevelAppHelpMenu
    {
        ITopLevelAppErrors ShowErrors();
    }

    public interface ITopLevelAppErrors
    {
        ExitCodes Execute();
    }

    public interface ITopLevelAppBuilderData
    {
        string[] Arguments { get; }
        Dictionary<string, TopLevelOption> Ops { get; }
        bool HasArguments { get; }
        string Command { get; }

        List<string> Errors { get; }
        ExitCodes ExitCode { get; set; }
        
        bool ShowHelpMenu { get; set; }

        Func<string, string[], ExitCodes> ExecuteMethod { get; set; }
        void AddError(string errorMessage, ExitCodes exitCode);
    }
}
