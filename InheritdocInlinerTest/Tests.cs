using System.IO;
using System.Reflection;
using Artees.Tools.InheritdocInliner;
using NUnit.Framework;

namespace InheritdocInlinerTest
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void Test()
        {
            var projectPath = Directory.GetParent(Directory
                .GetParent(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName)
                .FullName);
            var xmlPath = $"{projectPath}/TestCase/ShouldAssertions.xml";
            var dllPath = $"{projectPath}/TestCase/ShouldAssertions.dll";
            var outputPath = $"{projectPath}/TestCase/Output.xml";
            Inliner.Inline(xmlPath, dllPath, outputPath);
            var output = File.ReadAllText(outputPath);
            var expected = File.ReadAllText($"{projectPath}/TestCase/Expected.xml");
            Assert.AreEqual(expected, output);
        }
    }
}