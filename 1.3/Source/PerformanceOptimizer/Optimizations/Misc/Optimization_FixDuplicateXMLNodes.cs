using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using Verse;
using Verse.Sound;
using Verse.Steam;
using static Verse.XmlInheritance;

namespace PerformanceOptimizer
{
    public class Optimization_FixDuplicateXMLNodes : Optimization
    {
        public override OptimizationType OptimizationType => OptimizationType.Misc;
        public override string Name => "PO.FixCheckForDuplicateNodes".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(XmlInheritance), "CheckForDuplicateNodes", GetMethod(nameof(Prefix)));
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(XmlNode node, XmlNode root)
        {
            CheckForDuplicateNodes(node, root);
            return false;
        }
        private static void CheckForDuplicateNodes(XmlNode node, XmlNode root)
        {
            tempUsedNodeNames.Clear();
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.NodeType == XmlNodeType.Element && !IsListElement(childNode))
                {
                    if (tempUsedNodeNames.Contains(childNode.Name))
                    {
                        var outerNode = GetFirstMatchingNode(node.ChildNodes, childNode);
                        if (!(outerNode is null))
                        {
                            if (outerNode.ChildNodes?[0]?.NodeType == XmlNodeType.Element) // if this element is not a value
                            {
                                //Log.Warning("Fixing duplicate XML node name " + childNode.Name + " in this XML block: " + node.OuterXml + ((node != root) ? ("\n\nRoot node: " + root.OuterXml) : ""));
                                for (var i = 0; i < childNode.ChildNodes.Count; i++)
                                {
                                    outerNode.AppendChild(childNode.ChildNodes[i]);
                                }
                            }
                            //else if (outerNode.InnerText != childNode.InnerText)
                            //{
                            //    Log.Error("Duplicate XML node name " + childNode.Name + " - outerNode.InnerText: " + outerNode.InnerText 
                            //        + " childNode.InnerText: " + childNode.InnerText + " in this XML block: " + node.OuterXml + ((node != root) ? ("\n\nRoot node: " + root.OuterXml) : ""));
                            //}
                            node.RemoveChild(childNode);
                        }
                    }
                    else
                    {
                        tempUsedNodeNames.Add(childNode.Name);
                    }
                }
            }
            tempUsedNodeNames.Clear();
            foreach (XmlNode childNode2 in node.ChildNodes)
            {
                if (childNode2.NodeType == XmlNodeType.Element)
                {
                    CheckForDuplicateNodes(childNode2, root);
                }
            }
        }

        public static XmlNode GetFirstMatchingNode(XmlNodeList xmlNodeList, XmlNode childNode)
        {
            for (var i = 0; i < xmlNodeList.Count; i++)
            {
                var node = xmlNodeList[i];
                if (node != childNode && node.Name == childNode.Name)
                {
                    return node;
                }
            }
            return null;
        }
    }
}
