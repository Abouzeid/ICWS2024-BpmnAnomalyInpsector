using AnomalyDection.Core.SP_Tree;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnomalyDection.Core.ConcurrentAnomaly
{
    public class Prev_SP_Tree_Approach
    {
        List<ConCurrentAnomlayBase> anomalies = new List<ConCurrentAnomlayBase>();
        public List<ConCurrentAnomlayBase> GetAnomalies()
        {
            return anomalies;
        }

        public List<artifact_operation_in_node> Original_sptree_CAD(SpTree_Node spnode)
        {
            if (spnode.node_type != node_types.START && spnode.node_type != node_types.END
                && spnode.node_type != node_types.AND)
            {
                if (spnode.operation_sequence.Any())
                    foreach (var item in spnode.operation_sequence)
                    {
                        artifact_operation_in_node newElement = new artifact_operation_in_node();

                        newElement.node_id = spnode.node_id;
                        newElement.artifact_id = item.artifact_id;
                        newElement.Operation = item.operation;

                        if (spnode.artifact_set == null)
                            spnode.artifact_set = new List<artifact_operation_in_node>();

                        spnode.artifact_set.Add(newElement);
                    }
            }

            switch (spnode.node_type)
            {
                case node_types.START:
                    return Original_sptree_CAD(spnode.left_child);
                case node_types.END:
                    return new List<artifact_operation_in_node>();
                case node_types.ACTIVITY:

                    CheckICCA_Infected(spnode);

                    if (spnode.left_child != null)
                    {
                        var val = Original_sptree_CAD(spnode.left_child);
                        spnode.artifact_set.AddRange(val);
                    }
                    return spnode.artifact_set;
                case node_types.LOOP:
                    {
                        CheckICCA_Infected(spnode);

                        spnode.artifact_set.AddRange(Original_sptree_CAD(spnode.right_child.FirstOrDefault()));

                        spnode.artifact_set.AddRange(Original_sptree_CAD(spnode.left_child));
                        return spnode.artifact_set;
                    }
                case node_types.XOR:
                    {
                        CheckICCA_Infected(spnode);
                        if (spnode.node_id.Contains("joint"))
                            return new List<artifact_operation_in_node>();

                        if (spnode.right_child != null)
                        {
                            for (int i = 0; i < spnode.right_child.Count; i++)
                            {
                                spnode.artifact_set.AddRange(Original_sptree_CAD(spnode.right_child[i]));
                            }
                        }

                        if (spnode.left_child != null)
                        {
                            spnode.artifact_set.AddRange(Original_sptree_CAD(spnode.left_child));
                        }

                        return spnode.artifact_set;
                    }
                case node_types.AND:
                    {
                        if (spnode.node_id.Contains("joint"))
                            return new List<artifact_operation_in_node>();

                        List<artifact_operation_in_node>[] branch_artifact_set = new List<artifact_operation_in_node>[spnode.right_child.Count];
                        for (int i = 0; i < spnode.right_child.Count; i++)
                        {
                            branch_artifact_set[i] = Original_sptree_CAD(spnode.right_child[i]);
                            spnode.artifact_set.AddRange(branch_artifact_set[i]);
                        }

                        for (int i = 0; i < branch_artifact_set.Length; i++)
                        {
                            for (int j = i + 1; j < branch_artifact_set.Length; j++)
                            {
                                foreach (var item_i in branch_artifact_set[i])
                                {
                                    foreach (var item_j in branch_artifact_set[j])
                                    {
                                        if (item_i.Operation == operation_types.Write && item_j.Operation == operation_types.Write && item_i.artifact_id == item_j.artifact_id)
                                        {
                                            anomalies.Add(new CCA1() { artifactId = item_i.artifact_id, WriteNode1 = item_i.node_id, WriteNode2 = item_j.node_id });
                                            ////Console.WriteLine("CCA1 ______W and W/////");
                                            ////Console.WriteLine($"{item_i.Operation}, {item_j.Operation},{item_i.node_id}, {item_j.node_id}, artifact {item_i.artifact_id}");
                                        }

                                        if ((item_i.Operation == operation_types.Write && item_j.Operation == operation_types.Kill ||
                                            item_i.Operation == operation_types.Kill && item_j.Operation == operation_types.Write) && item_i.artifact_id == item_j.artifact_id)
                                        {
                                            var cca2 = new CCA2() { artifactId = item_i.artifact_id };
                                            if (item_i.Operation == operation_types.Write)
                                            {
                                                cca2.WriteNode = item_i.node_id;
                                                cca2.KillNode = item_j.node_id;
                                            }
                                            else
                                            {
                                                cca2.WriteNode = item_j.node_id;
                                                cca2.KillNode = item_i.node_id;
                                            }
                                            anomalies.Add(cca2);
                                            ////Console.WriteLine("CCA2 ______W and K/////");
                                            ////Console.WriteLine($"{item_i.Operation}, {item_j.Operation}, {item_j.node_id}, artifact {item_i.artifact_id}");
                                        }

                                        if ((item_i.Operation == operation_types.Read && item_j.Operation == operation_types.Kill ||
                                            item_i.Operation == operation_types.Kill && item_j.Operation == operation_types.Read) && item_i.artifact_id == item_j.artifact_id)
                                        {
                                            var cca3 = new CCA3() { artifactId = item_i.artifact_id };
                                            if (item_i.Operation == operation_types.Read)
                                            {
                                                cca3.ReadNode = item_i.node_id;
                                                cca3.KillNode = item_j.node_id;
                                            }
                                            else
                                            {
                                                cca3.ReadNode = item_j.node_id;
                                                cca3.KillNode = item_i.node_id;
                                            }
                                            anomalies.Add(cca3);
                                            ////Console.WriteLine("CCA3 ______R and K/////");
                                            ////Console.WriteLine($"{item_i.Operation}, {item_j.Operation}, {item_i.node_id}, {item_j.node_id}, artifact {item_i.artifact_id}");
                                        }


                                        //if ((item_i.Operation == operation_types.Write && item_j.Operation == operation_types.Read ||
                                        //    item_i.Operation == operation_types.Read && item_j.Operation == operation_types.Write) && item_i.artifact_id == item_j.artifact_id)
                                        //{

                                        //    Console.WriteLine($"CCA4 ______R and W/////--------------->{spnode.node_id}");
                                        //    Console.WriteLine($"{item_i.Operation}, {item_j.Operation},{item_i.node_id},  {item_j.node_id}, artifact {item_i.artifact_id}");
                                        //}
                                    }
                                }
                            }

                        }

                        if (spnode.left_child != null)
                        {
                            spnode.artifact_set.AddRange(Original_sptree_CAD(spnode.left_child));
                        }

                        return spnode.artifact_set;
                    }
                default:
                    break;
            }
            return null;
        }

        private void CheckICCA_Infected(SpTree_Node spnode)
        {
            foreach (var item in spnode.artifact_set)
            {
                var isInfected = GetAnomalies().Any(x => x.artifactId == item.artifact_id
                 && item.Operation == operation_types.Read);

                if (isInfected)
                    anomalies.Add(new Infected()
                    {
                        artifactId = item.artifact_id,
                        ReadNode = spnode.node_id
                    });
            }
        }
    }

    public class ConCurrentAnomlayBase
    {
        [JsonProperty(PropertyName = "ArtifactId")]
        public string artifactId { get; set; }
        public string Type { get; set; } = "Concurrent";
    }

    public class CCA1 : ConCurrentAnomlayBase
    {
        [JsonProperty(PropertyName = "WriteNode")]
        public string WriteNode1 { get; set; }
        [JsonProperty(PropertyName = "WriteNode2")]
        public string WriteNode2 { get; set; }
        public string Code { get; set; } = "CCA1";

    }

    public class CCA2 : ConCurrentAnomlayBase
    {
        public string WriteNode { get; set; }
        public string KillNode { get; set; }
        public string Code { get; set; } = "CCA2";
    }

    public class CCA3 : ConCurrentAnomlayBase
    {
        public string ReadNode { get; set; }
        public string KillNode { get; set; }
        public string Code { get; set; } = "CCA3";
    }

    public class Infected : ConCurrentAnomlayBase
    {
        public string ReadNode { get; set; }
        public string Code { get; set; } = "Infected";
    }

}