using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Artees.Diagnostics.BDD;

namespace Artees.Tools.InheritdocInliner
{
    internal class Inheritdoc
    {
        private readonly XmlNode _node;
        private readonly XmlDocument _docs;
        private readonly Assembly _dll;
        private readonly XmlNode _source;

        public Inheritdoc(XmlNode node, XmlDocument docs, Assembly dll)
        {
            _node = node;
            _docs = docs;
            _dll = dll;
            _source = GetSource();
        }

        private XmlNode GetSource()
        {
            var parentNode = _node.ParentNode;
            parentNode.Aka("Node parent").Should().Not().BeNull();
            if (parentNode == null) return null;
            parentNode.Name.Should().BeEqual("member");
            parentNode.Attributes.Aka("Attributes").Should().Not().BeNull();
            var fullParentName = GetNameAttribute(parentNode);
            var parentName = fullParentName?.Split(':');
            parentName.Aka("Parent name").Should().Not().BeNull();
            if (parentName == null) return null;
            var cref = GetAttribute(_node, "cref");
            if (cref != null) return GetSourceByCref(cref.Value, fullParentName);
            return parentName[0] == "T"
                ? GetSourceByParentType(parentName[1], fullParentName)
                : GetSourceByParentMember(parentName, fullParentName);
        }

        private static string GetNameAttribute(XmlNode node)
        {
            return GetAttributeValue(node, "name");
        }

        private static string GetAttributeValue(XmlNode node, string attributeName)
        {
            return GetAttribute(node, attributeName)?.Value;
        }

        private static XmlAttribute GetAttribute(XmlNode node, string attributeName)
        {
            return node.Attributes?[attributeName];
        }

        private XmlNode GetSourceByCref(string cref, string inheritdocMember)
        {
            var members = _docs.GetElementsByTagName("member");
            for (var i = 0; i < members.Count; i++)
            {
                var member = members.Item(i);
                if (GetNameAttribute(member) == cref) return member;
            }

            var docsName = _docs.BaseURI.Split('/').Last();
            var m = $"{cref} not found in {docsName}. " +
                    $"The <inheritdoc/> tag will be removed from {inheritdocMember}.";
            ShouldAssertions.Fail(m);
            return null;
        }

        private XmlNode GetSourceByParentType(string parentName, string inheritdocMember)
        {
            var parentTypeName = GetParentTypeName(parentName);
            var cref = $"T:{parentTypeName}";
            var source = GetSourceByCref(cref, inheritdocMember);
            return source;
        }

        private string GetParentTypeName(string parentName)
        {
            var parentType = _dll.GetType(parentName);
            parentType.Aka(parentName).Should().Not().BeNull();
            if (parentType == null) return null;
            var baseType = parentType.BaseType;
            return GetTypeName(baseType);
        }

        private static string GetTypeName(Type type)
        {
            return type == null ? null : $"{type.Namespace}.{type.Name}";
        }

        private XmlNode GetSourceByParentMember(IReadOnlyList<string> parentName,
            string inheritdocMember)
        {
            var ib = parentName[1].LastIndexOf('(');
            var l = parentName[1].Length;
            var ibe = ib == -1 ? l - 1 : ib;
            var ibl = ib == -1 ? l : ib;
            var id = parentName[1].LastIndexOf('.', ibe, ibe);
            var parentWithoutMember = parentName[1].Substring(0, id);
            var memberWithoutArgs = parentName[1].Substring(id + 1, ibl - id - 1);
            var parentTypeName = GetParentTypeName(parentWithoutMember, memberWithoutArgs);
            var memberName = parentName[1].Substring(id, l - id);
            var cref = $"{parentName[0]}:{parentTypeName}{memberName}";
            var source = GetSourceByCref(cref, inheritdocMember);
            return source;
        }

        private string GetParentTypeName(string parentName, string memberName)
        {
            var parentType = _dll.GetType(parentName);
            parentType.Aka(parentName).Should().Not().BeNull();
            if (parentType == null) return null;
            var baseType = GetBaseTypeOrInterfaceWithMember(parentType, memberName);
            return GetTypeName(baseType);
        }

        private static Type GetBaseTypeOrInterfaceWithMember(Type parentType, string memberName)
        {
            var type = parentType.BaseType;
            if (type != null && type.GetMember(memberName).Length != 0) return type;
            foreach (var interfaceType in parentType.GetInterfaces())
            {
                if (interfaceType.GetMember(memberName).Length == 0) continue;
                type = interfaceType;
                break;
            }

            return type;
        }

        public bool Inline()
        {
            var inheritdocParent = _node.ParentNode;
            inheritdocParent.Aka("Inheritdoc parent").Should().Not().BeNull();
            if (inheritdocParent == null) return true;
            if (RemoveInheritdocThatIsFound(inheritdocParent)) return true;
            var sourceChildren = _source.ChildNodes;
            var sourceHasInheritdoc =
                HasChild(sourceChildren, "inheritdoc", string.Empty, string.Empty);
            if (sourceHasInheritdoc) return false;
            var isInlined = InlineInheritdoc(inheritdocParent, sourceChildren);
            return isInlined;
        }

        private bool RemoveInheritdocThatIsFound(XmlNode inheritdocParent)
        {
            if (_source != null) return false;
            inheritdocParent.RemoveChild(_node);
            return true;
        }

        private static bool HasChild(XmlNodeList childNodes, XmlNode child)
        {
            var nameAttribute = GetNameAttribute(child);
            return HasChild(childNodes, child.Name, nameAttribute, child.OuterXml);
        }

        private static bool HasChild(XmlNodeList childNodes, string childName,
            string nameAttribute, string childXml)
        {
            for (var i = 0; i < childNodes.Count; i++)
            {
                var iChild = childNodes.Item(i);
                if (iChild == null || iChild.Name != childName) continue;
                var name = GetNameAttribute(iChild);
                var iXml = iChild.OuterXml;
                if (name == null || name == nameAttribute || iXml == childXml) return true;
            }

            return false;
        }

        private bool InlineInheritdoc(XmlNode inheritdocParent, XmlNodeList sourceChildren)
        {
            var parentChildren = inheritdocParent.ChildNodes;
            for (var i = 0; i < sourceChildren.Count; i++)
            {
                var sourceChild = sourceChildren.Item(i);
                sourceChild.Aka("Child").Should().Not().BeNull();
                if (sourceChild == null) continue;
                if (HasChild(parentChildren, sourceChild)) continue;
                inheritdocParent.AppendChild(sourceChild.Clone());
            }

            inheritdocParent.RemoveChild(_node);
            return true;
        }
    }
}