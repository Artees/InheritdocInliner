using System.Collections.Generic;
using System.Diagnostics;
using Artees.Diagnostics.BDD;
using CommandLine;

namespace Artees.Tools.InheritdocInliner
{
    internal static class Program
    {
        // ReSharper disable once ClassNeverInstantiated.Local
        private class Options
        {
            // ReSharper disable UnusedAutoPropertyAccessor.Local, MemberCanBePrivate.Local
            [Option('x', "xml", Hidden = true)] public string XmlOption { private get; set; }

            [Value(0, MetaName = "-x, --xml", HelpText = "The XML document to be processed.")]
            public string XmlValue { private get; set; }

            public string Xml => XmlOption ?? XmlValue;

            [Option('d', "dll", Hidden = true)] public string DllOption { private get; set; }

            [Value(1, MetaName = "-d, --dll",
                HelpText = "The assembly file for which the XML document is processed. " +
                           "If not specified, the path of the XML document will be used.")]
            public string DllValue { private get; set; }

            public string Dll => DllOption ?? DllValue ?? Xml.Remove(Xml.Length - 4) + ".dll";

            [Option('o', "output", Hidden = true)] public string OutputOption { private get; set; }

            [Value(2, MetaName = "-o, --output",
                HelpText = "The path of the output XML document. " +
                           "If not specified, the original document will be overwritten.")]
            public string OutputValue { private get; set; }

            public string Output => OutputOption ?? OutputValue ?? Xml;
            // ReSharper restore UnusedAutoPropertyAccessor.Local, MemberCanBePrivate.Local
        }

        public static void Main(string[] args)
        {
            using (var shouldListener = new WarningShouldListener())
            {
                ShouldAssertions.Listeners.Add(shouldListener);
                using (var traceListener = new ConsoleTraceListener())
                {
                    Trace.Listeners.Add(traceListener);
                    Inline(args);
                }
            }
        }

        private static void Inline(IEnumerable<string> args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(Inline)
                .WithNotParsed(Fail);
        }

        private static void Inline(Options options)
        {
            Inliner.Inline(options.Xml, options.Dll, options.Output);
        }

        private static void Fail(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                if (error.Tag == ErrorType.HelpRequestedError) continue;
                ShouldAssertions.Fail(error.ToString());
            }
        }
    }
}