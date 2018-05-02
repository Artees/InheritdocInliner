using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Artees.Diagnostics.BDD;

namespace Artees.Tools.InheritdocInliner
{
    public static class Inliner
    {
        public static void Inline(string xmlPath, string dllPath, string outputPath)
        {
            xmlPath.Aka("XML").Should().Not().BeNull();
            if (xmlPath == null) return;
            var xml = new XmlDocument();
            xml.Load(xmlPath);
            var fullDllPath = Path.GetFullPath(dllPath);
            var dll = Assembly.LoadFile(fullDllPath);
            var inheritdocs = GetInheritdocs(xml, dll);
            Inline(inheritdocs);
            SaveXml(xml, outputPath);
        }

        private static void SaveXml(XmlDocument xml, string newPath)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t"
            };
            using (var writer = XmlWriter.Create(newPath, settings))
            {
                xml.Save(writer);
            }
        }

        private static List<Inheritdoc> GetInheritdocs(XmlDocument xml, Assembly dll)
        {
            var inheritdocs = new List<Inheritdoc>();
            var inheritdocNodes = xml.GetElementsByTagName("inheritdoc");
            for (var i = 0; i < inheritdocNodes.Count; i++)
            {
                var inheritdoc = new Inheritdoc(inheritdocNodes.Item(i), xml, dll);
                inheritdocs.Add(inheritdoc);
            }

            return inheritdocs;
        }

        private static void Inline(ICollection<Inheritdoc> inheritdocs)
        {
            while (inheritdocs.Count > 0)
            {
                foreach (var inheritdoc in inheritdocs.ToList())
                {
                    var isInlined = inheritdoc.Inline();
                    if (isInlined)
                    {
                        inheritdocs.Remove(inheritdoc).Should().BeTrue();
                    }
                }
            }
        }
    }
}