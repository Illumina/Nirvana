This guide provides:
* protocols for contributing new features or bug fixes
* high-level information about our development process

Information is added as pertinent questions/discussions come up in the contributor community,
so this guide is not intended to provide complete coverage of the above topics.

# Table of Contents
* [Scrum (agile development) ](#scrum-agile-development)
* [Developer environment](#developer-environment)
* [Coding conventions](#coding-conventions)
* [Branching model](#branching-model)
* [Unit testing](#unit-testing)
* [Continuous integration](#continuous-integration)
* [Portability](#portability)

# Scrum (agile development)

The development team uses Scrum agile development methodology. Our sprints are two weeks long and consistent of the four key ceremonies:

* sprint planning
* daily stand-ups
* sprint retrospective
* sprint review

For external developers interested in contributing to the project, we would be happy to invite you to these ceremonies. Please contact any of the team members and we'll make the necessary arrangements.

# Developer environment

## IDE

The development team is using Microsoft Visual Studio 2015 to develop Nirvana. Developers could in theory choose to use other C# IDEs such as [MonoDevelop](http://www.monodevelop.com/), [SharpDevelop](https://sourceforge.net/projects/sharpdevelop/), or [Project Rider](https://www.jetbrains.com/rider/). However, we have not evaluated those IDEs at the moment.

## Extensions

<p align="center">
  <img src="https://www.jetbrains.com/resharper/img/screenshots/code-analysis.png" width="600" />
</p>

JetBrains makes an incredible Visual Studio extension called [ReSharper](https://www.jetbrains.com/resharper/). No other tool comes as close to helping developers produce clean C# code while offering powerful functionality to make refactoring a breeze. For our internal development team, we require the use of ReSharper.

# Coding conventions

We use the same coding conventions (naming, layout, and commenting conventions) as is used in Microsoft's [C# Coding Conventions Guide](https://msdn.microsoft.com/en-us/library/ff926074.aspx). The only exception to this is the variable naming scheme that ReSharper suggests (i.e. private class variables should begin with an underscore).

Here's a small example class that demonstrates most of these conventions:

```C#
using System;
using System.Collections.Generic;

namespace Demo
{
    public class Fibonacci
    {
        #region members

        private readonly List<int> _fibonacciSeries;
        public readonly string Description;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public Fibonacci(string description, int numValues)
        {
            Description = description;
            _fibonacciSeries = new List<int>(numValues);
            Calculate(numValues);
        }

        /// <summary>
        /// iteratively calculates the first n values of the Fibonacci series
        /// </summary>
        private void Calculate(int numValues)
        {
            int a = 1, b = 1;

            _fibonacciSeries.Add(a);
            _fibonacciSeries.Add(b);

            for (int i = 2; i < numValues; i++)
            {
                int sum = a + b;
                _fibonacciSeries.Add(sum);
                a = b;
                b = sum;
            }
        }

        /// <summary>
        /// displays all the calculated values of our fibonacci series
        /// </summary>
        public void Display()
        {
            Console.WriteLine($"{Description}:");
            foreach(var value in _fibonacciSeries) Console.Write($"{value} ");
            Console.WriteLine();
        }

        /// <summary>
        /// displays the nth calculated value of our fibonacci series
        /// </summary>
        public void Display(int index)
        {
            if ((index < 1) || (index > _fibonacciSeries.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            Console.WriteLine($"{Description}: {_fibonacciSeries[index - 1]}");
        }
    }
}
```

# Branching model

<img src="https://github.com/Illumina/Nirvana/wiki/images/GitFlow.png" width="400" align="right" />

The development team uses [GitFlow](http://nvie.com/posts/a-successful-git-branching-model/) to organize all of our branches.

## Feature branches

In essence, all of our day-to-day work is on the **develop branch**. When work begins on a new **story** or **bug fix**, we will create a feature branch from the develop branch. When work on the feature branch has been completed, a **pull request is required** before it can be merged back to the develop branch. 

When the feature has finished development, we typically go through the following steps:

1. pull the latest develop branch
1. merge the develop branch to the feature branch
1. ensure that all unit tests pass
1. ensure that all regression and integration tests pass (internal developers only)
1. create a pull request 
1. once approved, merge the feature branch to the develop branch

Internal developers will also check the status of the Jenkins integration and regression tests before merging a feature branch back.

### Naming

Our feature branch names obey the following convention:

```
features/short_description_1234
bugfixes/short_description_1234
```

All feature branches are prefixed by either **features/** or **bugfixes/**. This naming scheme is exploited by our continuous integration framework. The number 1234 is used as a convenience to hold our JIRA ID (external developers are not required to add a numerical identifier).

## Builds and releases

When we're ready to issue a new build, the develop branch is merged to the **master branch** and an **annotated tag** is added to the master branch.

```
git tag -a v1.4.3 -m "Nirvana 1.4.3"
git push origin v1.4.3
```

## Release and hotfix branches

Our team typically creates releases and hotfix branches for internal projects. As such, they will only be visible on our internal GitHub Enterprise server.

# Unit testing

<img src="https://github.com/Illumina/Nirvana/wiki/images/UnitTesting.png" width="400" align="right" />

Our team strives to have high unit test code coverage of all Nirvana code. Currently, the code coverage of the Illumina.VariantAnnotation library is around **82%** and we aspire to increase that to 90% or greater within the next few months.

We prefer using a [TDD methodology](https://en.wikipedia.org/wiki/Test-driven_development), but we are not forcing developers to use it at this time. TDD has had a measured effect on improving our code quality.

Any time our continuous integration pipeline shows an annotation that deviates from the baseline, we create a unit test to demonstrate the correct behavior and to ensure that future regressions do not occur.

<br clear=all>

# Continuous integration

At Illumina, we have developed an extensive testing framework on top of the [Jenkins continuous integration framework](https://jenkins.io/). During our daily stand-ups, we check the status of every field in every variant for a few dozen data sets against the baseline. This translates to 100's of millions of variants (or billions of annotation fields) being checked on a daily basis.

Unfortunately, our Jenkins servers sits behind our corporate firewall at the moment; but here's a snapshot of the information provided by our CI framework. We run a full set of smoke tests on every git commit on the develop branch. Developers can trigger both smoke and regression tests on any of the branches:

<table>
<tr>
<td valign="top"><a href="https://github.com/Illumina/Nirvana/wiki/images/CI_AllCurrentBranches.png"><img src="https://github.com/Illumina/Nirvana/wiki/images/CI_AllCurrentBranches.png" /></a></td>
<td valign="top"><a href="https://github.com/Illumina/Nirvana/wiki/images/SmokeTests.png"><img src="https://github.com/Illumina/Nirvana/wiki/images/SmokeTests.png" /></a></td>
</tr>
</table>

For each smoke or regression test, our testing framework provides a wealth of information for each input VCF file:

<table>
<tr>
<td valign="top"><a href="https://github.com/Illumina/Nirvana/wiki/images/NA12877_GlobalAccuracy.png"><img src="https://github.com/Illumina/Nirvana/wiki/images/NA12877_GlobalAccuracy.png" height="500" /></a></td>
<td valign="top"><a href="https://github.com/Illumina/Nirvana/wiki/images/NA12877_GlobalStatistics.png"><img src="https://github.com/Illumina/Nirvana/wiki/images/NA12877_GlobalStatistics.png" height="500" /></a></td>
</tr>
</table>

In some cases, deviations from our baseline are found. When this happens, we add it as a bug in our JIRA project and prioritize it accordingly in our backlog until it's ready to committed for a sprint:

<p align="center"><a href="https://github.com/Illumina/Nirvana/wiki/images/NA12877_TranscriptDeviations.png"><img src="https://github.com/Illumina/Nirvana/wiki/images/NA12877_TranscriptDeviations.png" /></a></p>

# Portability

While development is mainly performed in a Windows environment, Nirvana is expected to run on multiple platforms (Windows and Linux) reliably. We test Nirvana on a daily basis on both platforms.