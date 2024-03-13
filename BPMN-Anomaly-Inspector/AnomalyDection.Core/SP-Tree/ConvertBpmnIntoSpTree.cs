using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;


namespace AnomalyDection.Core.SP_Tree
{
    public class ConvertBpmnIntoSpTree
    {
        private const string EMPTY_NODE_ID = "emptyNode";
        static int level;
        static XmlNode xml_joint_successor;
        static XmlNode RootNode;
        static XDocument bpmnDoc;
        static string? nearstSplit = null;
        static List<LoopIdentifiers> loopIdentifiersList = new List<LoopIdentifiers>();

        public static XmlNamespaceManager nsmgr { get; set; }
        public static SpTree_Node? Parse(string xmlFilePath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFilePath);
            bpmnDoc = XDocument.Load(xmlFilePath);

            return TranslateIntoSpTree(xmlDoc);
        }

        public static SpTree_Node? Parse(Stream xmlFileStream)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFileStream);

            xmlFileStream.Position = 0;
            bpmnDoc = XDocument.Load(xmlFileStream);

            return TranslateIntoSpTree(xmlDoc);
        }

        private static SpTree_Node? TranslateIntoSpTree(XmlDocument xmlDoc)
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            namespaceManager.AddNamespace("bpmn", "http://www.omg.org/spec/BPMN/20100524/MODEL");
            namespaceManager.AddNamespace("camunda", "http://camunda.org/schema/1.0/bpmn");

            XNamespace bpmnNs = "http://www.omg.org/spec/BPMN/20100524/MODEL";
            nsmgr = namespaceManager;

            XmlNodeList processNodes = xmlDoc.SelectNodes("//bpmn:process", namespaceManager);
            RootNode = processNodes[0];
            XmlAttribute idAttribute = RootNode.Attributes["id"];

            if (idAttribute != null)
            {
                Console.WriteLine($"Process ID: {idAttribute.Value}");

                var startNode = FindNodeByName(RootNode, "bpmn:startEvent");
                var snode = bpmnDoc.Descendants().Where(x => x.Name.LocalName == "startEvent").FirstOrDefault();
                DetectCyclicUtil(snode, new Dictionary<string, bool>(), new Dictionary<string, bool>());

                var tree = ConvertBpmnToSptree(startNode, new SpTree_Node());

                return tree;
            }
            return null;
        }

        private static SpTree_Node ConvertBpmnToSptree(XmlNode n, SpTree_Node parent)
        {
            SpTree_Node node = new()
            {
                parent = parent,
                node_id = n.Attributes["id"].Value,
                node_type = MapBpmnToTypesOfSpTree(n.Name)
            };
            switch (n.LocalName)
            {
                case "startEvent":
                    var outgoingFlow = FindNodeById(RootNode, n.ChildNodes[0].ChildNodes[0].InnerText);
                    var target = FindNodeById(RootNode, outgoingFlow.Attributes["targetRef"].Value);

                    node.left_child = ConvertBpmnToSptree(target, node);
                    break;
                case "endEvent":
                    node.left_child = null;
                    break;
                case "task":
                case "serviceTask":
                case "intermediateCatchEvent":
                    node.level = level;

                    var nodesArtifactsOp = GetArtifacts(n);
                    if (nodesArtifactsOp != null && nodesArtifactsOp.Any())
                        node.operation_sequence.AddRange(nodesArtifactsOp);

                    outgoingFlow = FindNodeByName(n, "bpmn:outgoing");
                    outgoingFlow = FindNodeById(RootNode, outgoingFlow.InnerText);
                    target = FindNodeById(RootNode, outgoingFlow.Attributes["targetRef"].Value);
                    node.left_child = ConvertBpmnToSptree(target, node);
                    break;
                case "parallelGateway":
                case "exclusiveGateway":
                    if ((GetGateType(n) == GateType.Split || loopIdentifiersList.Where(x => x.JoinNodeId == node.node_id).Any()) &&
                        !loopIdentifiersList.Where(x => x.SplitNodeId == node.node_id).Any())
                    {
                        level = level + 1;
                        node.level = level;
                        if (loopIdentifiersList.Where(x => x.JoinNodeId == node.node_id).Any())
                        {
                            node.loop_type = loop_types.REPEAT;
                        }

                        XmlNodeList? listOf_successors = n.SelectNodes("bpmn:outgoing", nsmgr);

                        for (int i = 0; i < listOf_successors.Count; i++)
                        {
                            var success_split = FindNodeById(RootNode, listOf_successors[i].InnerText);

                            var artifactOperations = GetArtifactsFromFlow(success_split);
                            if (artifactOperations != null && artifactOperations.Any())
                                node.operation_sequence.AddRange(artifactOperations);

                            target = FindNodeById(RootNode, success_split.Attributes["targetRef"].Value);
                            SpTree_Node successor = ConvertBpmnToSptree(target, node);
                            if (successor != null)
                            {
                                if (node.right_child == null)
                                    node.right_child = new List<SpTree_Node>();

                                node.right_child.Add(successor);
                            }
                        }
                        if (xml_joint_successor != null)
                            node.left_child = ConvertBpmnToSptree(xml_joint_successor, node);

                        if (loopIdentifiersList.Where(x => x.JoinNodeId == node.node_id).Any())
                        {
                            var corrspondingSplit = loopIdentifiersList.Where(x => x.JoinNodeId == node.node_id);
                            var loop_split = FindNodeById(RootNode, corrspondingSplit.First().SplitNodeId);
                            ConvertBpmnToSptree(loop_split, node);

                            if (xml_joint_successor != null)
                                node.left_child = ConvertBpmnToSptree(xml_joint_successor, node);
                        }

                    }
                    else //if (GetGateType(n) == GateType.Join || loopIdentifiersList.Where(x => x.SplitNodeId == node.node_id).Any())
                    {
                        //var flowid = FindNodeByName(n, "bpmn:outgoing");
                        var nodes = n.SelectNodes("bpmn:outgoing", nsmgr);
                        var success_joint = FindNodeById(RootNode, nodes[0].InnerText);
                        target = FindNodeById(RootNode, success_joint.Attributes["targetRef"].Value);

                        if (GetGateType(target) != GateType.Join &&
                                                !loopIdentifiersList.Where(x => x.SplitNodeId == target.Attributes["id"].Value).Any())
                            xml_joint_successor = target;
                        else
                            xml_joint_successor = null;


                        if (loopIdentifiersList.Where(x => x.SplitNodeId == node.node_id).Any())
                        {
                            for (int i = 0; i < nodes.Count; i++)
                            {
                                var theFlow = FindNodeById(RootNode, nodes[i].InnerText);

                                var artifactOperations = GetArtifactsFromFlow(theFlow);
                                if (artifactOperations != null && artifactOperations.Any())
                                    node.parent.operation_sequence.AddRange(artifactOperations);

                                target = FindNodeById(RootNode, theFlow.Attributes["targetRef"].Value);

                                if (!target.Attributes["id"].Value
                                    .Equals(loopIdentifiersList.Where(x => x.SplitNodeId == node.node_id).First().JoinNodeId))
                                {
                                    if (GetGateType(target) != GateType.Join)
                                    {
                                        xml_joint_successor = target;
                                        break;
                                    }
                                }
                            }
                        }

                        if (node.parent.node_type == node_types.XOR &&
                                                !loopIdentifiersList.Where(x => x.JoinNodeId == node.parent.node_id).Any())
                        {
                            //new empty ACTIVITY sptree_node;
                            SpTree_Node emptyNOde = new SpTree_Node();
                            emptyNOde.parent = parent;
                            emptyNOde.node_id = EMPTY_NODE_ID;
                            emptyNOde.node_type = node_types.ACTIVITY;
                            return emptyNOde;
                        }
                        node.parent = null;
                        if (level > 0)
                            level = level - 1;
                        return null;
                    }

                    break;
            }

            return node;
        }

        private static IEnumerable<SpTree_Node.Opt_Element> GetArtifacts(XmlNode n)
        {
            var allOperations = new List<SpTree_Node.Opt_Element>();

            //input Parameters are the Read
            XmlNode inputOutputNode = n.SelectSingleNode("bpmn:extensionElements/camunda:inputOutput", nsmgr);

            if (inputOutputNode != null)
            {
                XmlNodeList inputNodes = inputOutputNode.SelectNodes("camunda:inputParameter", nsmgr);
                foreach (XmlNode inputNode in inputNodes)
                {
                    allOperations.Add(new SpTree_Node.Opt_Element() { artifact_id = inputNode.Attributes["name"].Value, operation = operation_types.Read });
                }

                XmlNodeList outputNodes = inputOutputNode.SelectNodes("camunda:outputParameter", nsmgr);
                foreach (XmlNode outputNode in outputNodes)
                {
                    if (string.IsNullOrEmpty(outputNode.InnerText))
                        allOperations.Add(new SpTree_Node.Opt_Element() { artifact_id = outputNode.Attributes["name"].Value, operation = operation_types.Kill });
                    else
                        allOperations.Add(new SpTree_Node.Opt_Element() { artifact_id = outputNode.Attributes["name"].Value, operation = operation_types.Write });
                }
            }
            return allOperations;
        }

        private static IEnumerable<SpTree_Node.Opt_Element> GetArtifactsFromFlow(XmlNode flow)
        {
            var allOperations = new List<SpTree_Node.Opt_Element>();

            // Find the conditionExpression element for this sequence flow
            XmlNode conditionExpression = flow.SelectSingleNode("bpmn:conditionExpression", nsmgr);

            if (conditionExpression != null)
            {
                //Get all Variables in the expression.
                // Parse the code into a syntax tree
                SyntaxTree tree = CSharpSyntaxTree.ParseText(conditionExpression.InnerText);

                // Extract and print all variables from the expression
                IEnumerable<IdentifierNameSyntax> variables = tree.GetRoot().DescendantNodes()
                    .OfType<IdentifierNameSyntax>();

                foreach (var variable in variables)
                {
                    allOperations.Add(new SpTree_Node.Opt_Element()
                    {
                        artifact_id = variable.Identifier.ValueText,
                        operation = operation_types.Read,
                        flowId = flow.Attributes["id"].Value,
                    });
                }
            }

            return allOperations;
        }

        private static bool DetectCyclicUtil(XElement n, Dictionary<string, bool> visited,
                               Dictionary<string, bool> recStack)
        {
            // Mark the current node as visited and
            // part of recursion stack

            string currentNodeId = n.Attributes("id").FirstOrDefault().Value;

            string? currentNodeName = n.Attributes("name")?.FirstOrDefault()?.Value;
            string? typeOfNode = n.Name.LocalName;


            if (typeOfNode.Equals("exclusiveGateway") && IsSplitNode(currentNodeId))
                nearstSplit = currentNodeId;

            if (recStack.ContainsKey(currentNodeId) && recStack[currentNodeId])
            {
                loopIdentifiersList.Add(new LoopIdentifiers(nearstSplit, currentNodeId));
                return true;
            }

            if (visited.ContainsKey(currentNodeId))
                return false;

            visited.Add(currentNodeId, true);
            recStack.Add(currentNodeId, true);

            var outgoingFlows = bpmnDoc.Descendants()
                                 .Where(x => x.Name.LocalName == "sequenceFlow" && x.Attribute("sourceRef")?.Value == currentNodeId)
                                 .Select(x => x.Attribute("targetRef")?.Value)
                                 .ToList();

            // Now, find the actual successor nodes using the IDs from the outgoing sequence flows
            var successorNodes = new List<XElement>();
            foreach (var targetId in outgoingFlows)
            {
                var targetNode = bpmnDoc.Descendants()
                                        .FirstOrDefault(x => x.Attributes().Any(a => a.Name == "id" && a.Value == targetId));
                if (targetNode != null)
                {
                    successorNodes.Add(targetNode);
                }
            }

            for (int i = 0; i < successorNodes.Count; i++)
            {
                DetectCyclicUtil(successorNodes[i], visited, recStack);
                //    return true;
            }

            recStack[currentNodeId] = false;

            return false;
        }

        private static bool IsSplitNode(string nodeId)
        {

            // Count incoming and outgoing sequence flows for the node
            var incomingFlows = bpmnDoc.Descendants()
                                       .Where(x => x.Name.LocalName == "sequenceFlow" && x.Attribute("targetRef")?.Value == nodeId)
                                       .Count();

            var outgoingFlows = bpmnDoc.Descendants()
                                       .Where(x => x.Name.LocalName == "sequenceFlow" && x.Attribute("sourceRef")?.Value == nodeId)
                                       .Count();

            // Determine if the node is a Split or Join
            if (outgoingFlows > incomingFlows)
            {
                return true;
            }
            else if (incomingFlows > outgoingFlows)
            {
                return false;
            }
            else
            {
                throw new Exception($"Node {nodeId} has an equal number of incoming and outgoing sequence flows, so it's neither a pure Split nor a pure Join.");
            }
        }

        private static GateType GetGateType(XmlNode node)
        {
            var incomingFlows = node.SelectNodes("bpmn:incoming", nsmgr).Count;
            var outgoingFlows = node.SelectNodes("bpmn:outgoing", nsmgr).Count;

            if (outgoingFlows > 1)
            {
                return GateType.Split;
            }
            else if (incomingFlows > 1)
            {
                return GateType.Join;
            }

            return GateType.None;
        }
        private static node_types MapBpmnToTypesOfSpTree(string elementName)
        {
            switch (elementName)
            {
                case "bpmn:startEvent":
                    return node_types.START;
                case "bpmn:endEvent":
                    return node_types.END;
                case "bpmn:exclusiveGateway":
                    {
                        return node_types.XOR;
                    }
                case "bpmn:task":
                    return node_types.ACTIVITY;
                case "bpmn:serviceTask":
                    return node_types.ACTIVITY;
                case "bpmn:sequenceFlow":
                    return node_types.Flow;
                case "bpmn:intermediateCatchEvent":
                    return node_types.ACTIVITY;
                case "bpmn:parallelGateway":
                    return node_types.AND;
                default:
                    throw new Exception("can't find type");

            }
        }

        private struct LoopIdentifiers
        {
            public string SplitNodeId { get; set; }
            public string JoinNodeId { get; set; }

            public LoopIdentifiers(string splitNodeId, string joinNodeId)
            {
                SplitNodeId = splitNodeId;
                JoinNodeId = joinNodeId;
            }
        }

        static XmlNode FindNodeByName(XmlNode parentNode, string nodeName)
        {
            foreach (XmlNode childNode in parentNode.ChildNodes)
            {
                if (childNode.Name == nodeName)
                {
                    return childNode;
                }
            }

            return null; // Return null if no node is found
        }

        static XmlNode FindNodeById(XmlNode parentNode, string idValue)
        {
            foreach (XmlNode childNode in parentNode.ChildNodes)
            {
                if (childNode.Attributes != null && childNode.Attributes["id"] != null
                    && childNode.Attributes["id"].Value == idValue)
                {
                    return childNode;
                }
            }

            return null; // Return null if no node is found
        }

        private enum GateType
        {
            Split,
            Join,
            None
        }

    }
}
