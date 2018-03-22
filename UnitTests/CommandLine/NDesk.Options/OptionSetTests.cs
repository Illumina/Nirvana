using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommandLine.NDesk.Options;
using Xunit;

namespace UnitTests.CommandLine.NDesk.Options
{
    public sealed class OptionSetTests
    {
        private readonly OptionSet _optionSet;
        private string _a;

        public OptionSetTests()
        {
            _optionSet = new OptionSet
            {
                {"a=", "", v => _a = v},
                {"b", "", v => { }},
                {"c", "", v => { }},
                {"n=", "", (int v) => { }}
            };
        }

        [Fact]
        public void BundledValues()
        {
            var defines = new List<string>();
            var libs = new List<string>();
            bool debug = false;

            var optionSet = new OptionSet
            {
                { "D|define=",  "", v => defines.Add (v) },
                { "L|library:", "", v => libs.Add (v) },
                { "Debug",      "", v => debug = v != null },
                { "E",          "", v => { /* ignore */ } }
            };

            optionSet.Parse(new[] { "-DNAME", "-D", "NAME2", "-Debug", "-L/foo", "-L", "/bar", "-EDNAME3" });

            Assert.Equal(3, defines.Count);
            Assert.Equal("NAME", defines[0]);
            Assert.Equal("NAME2", defines[1]);
            Assert.Equal("NAME3", defines[2]);
            Assert.True(debug);

            Assert.Equal(2, libs.Count);
            Assert.Equal("/foo", libs[0]);
            Assert.Null(libs[1]);

            Assert.Throws<OptionException>(delegate
            {
                optionSet.Parse(new[] { "-EVALUENOTSUP" });
            });
        }

        [Fact]
        public void RequiredValues()
        {
            string a = null;
            int n = 0;

            var optionSet = new OptionSet
            {
                { "a=", "", v => a = v },
                { "n=", "",(int v) => n = v }
            };

            var extra = optionSet.Parse(new[] { "a", "-a", "s", "-n=42", "n" });
            Assert.Equal(2, extra.Count);
            Assert.Equal("a", extra[0]);
            Assert.Equal("n", extra[1]);
            Assert.Equal("s", a);
            Assert.Equal(42, n);

            extra = optionSet.Parse(new[] { "-a=" });
            Assert.Empty(extra);
            Assert.Equal("", a);
        }

        [Fact]
        public void OptionalValues()
        {
            string a = null;
            int n = -1;
            Foo foo = null;

            var optionSet = new OptionSet
            {
                {"a:", "", v => a = v},
                {"n:", "", (int v) => n = v},
                {"f:", "", (Foo v) => foo = v}
            };

            optionSet.Parse(new[] { "-a=s" });
            Assert.Equal("s", a);
            optionSet.Parse(new[] { "-a" });
            Assert.Null(a);
            optionSet.Parse(new[] { "-a=" });
            Assert.Equal("", a);

            optionSet.Parse(new[] { "-f", "A" });
            Assert.Null(foo);
            optionSet.Parse(new[] { "-f" });
            Assert.Null(foo);

            optionSet.Parse(new[] { "-n42" });
            Assert.Equal(42, n);
            optionSet.Parse(new[] { "-n=42" });
            Assert.Equal(42, n);

            Assert.Throws<OptionException>(delegate
            {
                optionSet.Parse(new[] { "-n=" });
            });
        }

        [Fact]
        public void BooleanValues()
        {
            bool a = false;
            var optionSet = new OptionSet
            {
                { "a", "", v => a = v != null }
            };

            optionSet.Parse(new[] { "-a" });
            Assert.True(a);

            optionSet.Parse(new[] { "-a+" });
            Assert.True(a);

            optionSet.Parse(new[] { "-a-" });
            Assert.False(a);
        }

        [Fact]
        public void CombinationPlatter()
        {
            int a = -1, b = -1;
            string av = null, bv = null;
            int help = 0;
            int verbose = 0;

            var optionSet = new OptionSet
            {
                { "a=", "", v => { a = 1; av = v; } },
                { "b", "desc", v => {b = 2; bv = v;} },
                { "v", "", v => { ++verbose; } },
                { "h|?|help", "", v =>
                {
                    switch (v)
                    {
                        case "h":
                            help |= 0x1;
                            break;
                        case "?":
                            help |= 0x2;
                            break;
                        case "help":
                            help |= 0x4;
                            break;
                    }
                } }
            };

            var e = optionSet.Parse(new[] { "foo", "-v", "-a=42", "/b-", "-a", "64", "bar", "/h", "-?", "--help", "-v" });

            Assert.Equal(2, e.Count);
            Assert.Equal("foo", e[0]);
            Assert.Equal("bar", e[1]);
            Assert.Equal(1, a);
            Assert.Equal("64", av);
            Assert.Equal(2, b);
            Assert.Null(bv);
            Assert.Equal(2, verbose);
            Assert.Equal(0x7, help);
        }

        [Fact]
        public void Should_ThrowException_When_MissingRequiredValue()
        {
            Assert.Throws<OptionException>(delegate
            {
                _optionSet.Parse(new[] { "-a" });
            });
        }

        [Fact]
        public void ShouldNot_ThrowException_When_ProvidingMoreOptionsThanExpected()
        {
            var ex = Record.Exception(() =>
            {
                _optionSet.Parse(new[] { "-a", "-a" });
            });

            Assert.Null(ex);
            Assert.Equal("-a", _a);
        }

        [Fact]
        public void ShouldNot_ThrowException_When_ProvidingUnregisteredNamedOption()
        {
            var ex = Record.Exception(() =>
            {
                _optionSet.Parse(new[] { "-a", "-b" });
            });

            Assert.Null(ex);
            Assert.Equal("-b", _a);
        }

        [Fact]
        public void Should_ThrowException_When_ArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                _optionSet.Add(null);
            });
        }

        [Fact]
        public void Should_ThrowException_With_InvalidType()
        {
            Assert.Throws<OptionException>(delegate
            {
                _optionSet.Parse(new[] { "-n", "value" });
            });
        }

        [Fact]
        public void Should_ThrowException_When_BundlingWithOptionRequiringValue()
        {
            Assert.Throws<OptionException>(delegate
            {
                _optionSet.Parse(new[] { "-cz", "extra" });
            });
        }

        [Fact]
        public void WriteOptionDescriptions()
        {
            var optionSet = new OptionSet
            {
                { "p|indicator-style=", "append / indicator to directories",    v => {} },
                { "color:",             "controls color info",                  v => {} },
                { "color2:",            "set {color}",                          v => {} },
                { "long-desc",
                    "This has a really\nlong, multi-line description that also\ntests\n" +
                    "the-builtin-supercalifragilisticexpialidicious-break-on-hyphen.  " +
                    "Also, a list:\n" +
                    "  item 1\n" +
                    "  item 2",
                    v => {} },
                { "long-desc2",
                    "IWantThisDescriptionToBreakInsideAWordGeneratingAutoWordHyphenation.",
                    v => {} },
                { "long-desc3",
                    "OnlyOnePeriod.AndNoWhitespaceShouldBeSupportedEvenWithLongDescriptions",
                    v => {} },
                { "long-desc4",
                    "Lots of spaces in the middle 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 and more until the end.",
                    v => {} },
                { "long-desc5",
                    "Lots of spaces in the middle - . - . - . - . - . - . - . - and more until the end.",
                    v => {} },
                { "h|?|help",           "show help text",                       v => {} },
                { "version",            "output version information and exit",  v => {} },
                { "<>", "", v => {} }
            };

            var expected = new StringBuilder();
            expected.AppendLine("  -p, --indicator-style <VALUE>");
            expected.AppendLine("                             append / indicator to directories");
            expected.AppendLine("      --color [<VALUE>]      controls color info");
            expected.AppendLine("      --color2 [<color>]     set color");
            expected.AppendLine("      --long-desc            This has a really");
            expected.AppendLine("                               long, multi-line description that also");
            expected.AppendLine("                               tests");
            expected.AppendLine("                               the-builtin-supercalifragilisticexpialidicious-");
            expected.AppendLine("                               break-on-hyphen.  Also, a list:");
            expected.AppendLine("                                 item 1");
            expected.AppendLine("                                 item 2");
            expected.AppendLine("      --long-desc2           IWantThisDescriptionToBreakInsideAWordGeneratingAu-");
            expected.AppendLine("                               toWordHyphenation.");
            expected.AppendLine("      --long-desc3           OnlyOnePeriod.");
            expected.AppendLine("                               AndNoWhitespaceShouldBeSupportedEvenWithLongDesc-");
            expected.AppendLine("                               riptions");
            expected.AppendLine("      --long-desc4           Lots of spaces in the middle 1 2 3 4 5 6 7 8 9 0");
            expected.AppendLine("                               1 2 3 4 5 and more until the end.");
            expected.AppendLine("      --long-desc5           Lots of spaces in the middle - . - . - . - . - . -");
            expected.AppendLine("                                . - . - and more until the end.");
            expected.AppendLine("  -h, -?, --help             show help text");
            expected.AppendLine("      --version              output version information and exit");

            var actual = new StringWriter();
            optionSet.WriteOptionDescriptions(actual);

            Assert.Equal(expected.ToString(), actual.ToString());
        }

        [Fact]
        public void OptionBundling()
        {
            string a, b, c, f;

            a = b = c = f = null;
            var optionSet = new OptionSet
            {
                { "a", "", v => a = "a" },
                { "b", "", v => b = "b" },
                { "c", "", v => c = "c" },
                { "f=", "", v => f = v }
            };

            var extra = optionSet.Parse(new[] { "-abcf", "foo", "bar" });
            Assert.Single(extra);
            Assert.Equal("bar", extra[0]);
            Assert.Equal("a", a);
            Assert.Equal("b", b);
            Assert.Equal("c", c);
            Assert.Equal("foo", f);
        }

        [Fact]
        public void HaltProcessing()
        {
            var optionSet = new OptionSet
            {
                { "a", "", v => {} },
                { "b", "", v => {} }
            };

            var e = optionSet.Parse(new[] { "-a", "-b", "--", "-a", "-b" });
            Assert.Equal(2, e.Count);
            Assert.Equal("-a", e[0]);
            Assert.Equal("-b", e[1]);
        }

        private sealed class ContextCheckerOption : Option
        {
            private readonly string _eName;
            private readonly string _eValue;
            private readonly int _index;

            public ContextCheckerOption(string p, string d, string eName, string eValue, int index)
                : base(p, d, 1)
            {
                _eName = eName;
                _eValue = eValue;
                _index = index;
            }

            protected override void OnParseComplete(OptionContext c)
            {
                Assert.Equal(1, c.OptionValues.Count);
                Assert.Equal(c.OptionValues[0], _eValue);
                Assert.Equal(c.OptionName, _eName);
                Assert.Equal(c.OptionIndex, _index);
                Assert.Equal(c.Option, this);
                Assert.Equal(c.Option.Description, Description);
            }
        }

        [Fact]
        public void OptionContext()
        {
            var optionSet = new OptionSet
            {
                new ContextCheckerOption ("a=", "a desc", "/a",   "a-val", 1),
                new ContextCheckerOption ("b",  "b desc", "--b+", "--b+",  2),
                new ContextCheckerOption ("c=", "c desc", "--c",  "C",     3),
                new ContextCheckerOption ("d",  "d desc", "/d-",  null,    4)
            };
            Assert.Equal(4, optionSet.Count);
            optionSet.Parse(new[] { "/a", "a-val", "--b+", "--c=C", "/d-" });
        }

        [Fact]
        public void DefaultHandler()
        {
            var extra = new List<string>();
            var optionSet = new OptionSet
            {
                { "<>", "", v => extra.Add (v) }
            };
            var e = optionSet.Parse(new[] { "-a", "b", "--c=D", "E" });
            Assert.Empty(e);
            Assert.Equal(4, extra.Count);
            Assert.Equal("-a", extra[0]);
            Assert.Equal("b", extra[1]);
            Assert.Equal("--c=D", extra[2]);
            Assert.Equal("E", extra[3]);
        }

        [Fact]
        public void MixedDefaultHandler()
        {
            var tests = new List<string>();
            var optionSet = new OptionSet
            {
                { "t|<>=", "", v => tests.Add (v) }
            };
            var e = optionSet.Parse(new[] { "-tA", "-t:B", "-t=C", "D", "--E=F" });
            Assert.Empty(e);
            Assert.Equal(5, tests.Count);
            Assert.Equal("A", tests[0]);
            Assert.Equal("B", tests[1]);
            Assert.Equal("C", tests[2]);
            Assert.Equal("D", tests[3]);
            Assert.Equal("--E=F", tests[4]);
        }

        [Fact]
        public void DefaultHandlerRuns()
        {
            var formats = new Dictionary<string, List<string>>();
            string format = "foo";

            var optionSet = new OptionSet
            {
                { "f|format=", "", v => format = v },
                { "<>",
                    "", v => {
                        if (!formats.TryGetValue (format, out var f)) {
                            f = new List<string> ();
                            formats.Add (format, f);
                        }
                        f.Add (v);
                    } }
            };

            var e = optionSet.Parse(new[] { "a", "b", "-fbar", "c", "d", "--format=baz", "e", "f" });
            Assert.Empty(e);
            Assert.Equal(3, formats.Count);
            Assert.Equal(2, formats["foo"].Count);
            Assert.Equal("a", formats["foo"][0]);
            Assert.Equal("b", formats["foo"][1]);
            Assert.Equal(2, formats["bar"].Count);
            Assert.Equal("c",formats["bar"][0]);
            Assert.Equal("d",formats["bar"][1]);
            Assert.Equal(2, formats["baz"].Count);
            Assert.Equal("e", formats["baz"][0]);
            Assert.Equal("f", formats["baz"][1]);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Foo
        {
            private readonly string _s;

            private Foo(string s) { _s = s; }
            public override string ToString() { return _s; }
        }
    }
}
