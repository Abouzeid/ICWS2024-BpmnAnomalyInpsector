﻿using AnomalyDection.Core.Definitions;
using AnomalyDection.Core.SP_Tree;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AnomalyDection.Core.ConcurrentAnomaly
{
    public class OurApproachToConcurrent
    {
        private List<CMABase> anomalyResult = new List<CMABase>();

        /// <summary>
        /// Returns detected anomalies as a json 
        /// </summary>
        /// <returns></returns>
        public List<CMABase> GetAnomalyResult()
        {
            return anomalyResult;
        }

        /// <summary>
        /// Detect Concurrent Anomaly new Statistical branch approach algorithm
        /// </summary>
        /// <param name="spnode"></param>
        /// <returns></returns>
        public Dictionary<string, NodesOpArtifact> AOC(SpTree_Node spnode)
        {

            //Console.WriteLine(spnode.node_id);

            if (spnode.node_type != node_types.START && spnode.node_type != node_types.END && spnode.node_type != node_types.AND)
            {
                if (spnode.operation_sequence.Any())
                {
                    foreach (var item in spnode.operation_sequence)
                    {
                        if (!spnode.ArtifactOpMap.ContainsKey(item.artifact_id))
                        {
                            spnode.ArtifactOpMap.Add(item.artifact_id, new NodesOpArtifact());
                        }

                        if (item.operation == operation_types.Read)
                        {
                            if (spnode.ArtifactOpMap[item.artifact_id].ReadNodes == null)
                                spnode.ArtifactOpMap[item.artifact_id].ReadNodes = new List<string>() { spnode.node_id };
                            else
                                spnode.ArtifactOpMap[item.artifact_id].ReadNodes?.Add(spnode.node_id);
                        }

                        if (item.operation == operation_types.Kill)
                        {
                            if (spnode.ArtifactOpMap[item.artifact_id].KillNodes == null)
                                spnode.ArtifactOpMap[item.artifact_id].KillNodes = new List<string>() { spnode.node_id };
                            else
                                spnode.ArtifactOpMap[item.artifact_id].KillNodes?.Add(spnode.node_id);
                        }

                        if (item.operation == operation_types.Write)
                        {
                            if (spnode.ArtifactOpMap[item.artifact_id].WriteNodes == null)
                                spnode.ArtifactOpMap[item.artifact_id].WriteNodes = new List<string>() { spnode.node_id };
                            else
                                spnode.ArtifactOpMap[item.artifact_id].WriteNodes?.Add(spnode.node_id);
                        }
                    }
                }
            }

            switch (spnode.node_type)
            {
                case node_types.START:
                    return AOC(spnode.left_child);
                case node_types.END:
                    return null;
                case node_types.ACTIVITY:
                    if (spnode.left_child != null)
                    {
                        Merge(spnode.ArtifactOpMap, AOC(spnode.left_child));
                    }
                    return spnode.ArtifactOpMap;
                case node_types.LOOP:
                    {
                        Merge(spnode.ArtifactOpMap, AOC(spnode.right_child.FirstOrDefault()));

                        Merge(spnode.ArtifactOpMap, AOC(spnode.left_child));

                        return spnode.ArtifactOpMap;
                    }
                case node_types.XOR:
                    {
                        if (spnode.right_child != null)
                        {
                            for (int i = 0; i < spnode.right_child.Count; i++)
                            {
                                Merge(spnode.ArtifactOpMap, AOC(spnode.right_child[i]));
                            }
                        }

                        if (spnode.left_child != null)
                        {
                            Merge(spnode.ArtifactOpMap, AOC(spnode.left_child));
                        }

                        return spnode.ArtifactOpMap;
                    }
                case node_types.AND:
                    {
                        var all_branchesData = new Dictionary<string, BranchesInfo>();

                        for (int i = 0; i < spnode.right_child.Count; i++)
                        {
                            var artifacts_info = AOC(spnode.right_child[i]);

                            foreach (var pair in artifacts_info)
                            {
                                if (all_branchesData.ContainsKey(pair.Key) == false)
                                {
                                    all_branchesData.Add(pair.Key, new BranchesInfo()
                                    {
                                        Kill = new Dictionary<int, List<string>>(),
                                        Read = new Dictionary<int, List<string>>(),
                                        Write = new Dictionary<int, List<string>>()
                                    });
                                }

                                {
                                    if (pair.Value.WriteNodes.Count > 0)
                                    {
                                        all_branchesData[pair.Key].Write.Add(i, pair.Value.WriteNodes);
                                    }

                                    if (pair.Value.ReadNodes.Count > 0)
                                    {
                                        all_branchesData[pair.Key].Read.Add(i, pair.Value.ReadNodes);
                                    }

                                    if (pair.Value.KillNodes.Count > 0)
                                    {
                                        all_branchesData[pair.Key].Kill.Add(i, pair.Value.KillNodes);
                                    }
                                }
                            }

                            Merge(spnode.ArtifactOpMap, artifacts_info);
                        }

                        DetectFor_Current_AND(spnode, all_branchesData);

                        if (spnode.left_child != null)
                        {
                            var temp = AOC(spnode.left_child);
                            Merge(spnode.ArtifactOpMap, temp);
                        }

                        return spnode.ArtifactOpMap;
                    }
                default:
                    break;
            }
            return null;
        }      

        private void DetectFor_Current_AND(SpTree_Node and_spnode, Dictionary<string, BranchesInfo> andBranchesStats)
        {
            foreach (var element in andBranchesStats)
            {
                var artifact = element.Value;
                var key = element.Key;

                // Get Write and Write
                if (artifact.Write.Count > 1)
                {
                    //Console.WriteLine($"Concurrent Write Anomaly on Artifact ==> {key}");

                    var cwwa = new CWWA()
                    {
                        AndNode_Id = and_spnode.node_id,
                        Artifact_id = key,
                        ParrallelWriteNodes = artifact.Write.Values.ToList()
                    };


                    //Console.WriteLine("======================CWWA================================");
                  //  var serilaized = JsonConvert.SerializeObject(cwwa, Newtonsoft.Json.Formatting.Indented);
                   anomalyResult.Add(cwwa);
                    //Console.WriteLine(serilaized);
                    //Console.WriteLine("==========================================================");
                }

                if (artifact.Write.Count > 0 && artifact.Read.Count >= 1)
                {
                    var cwra = new CWRA()
                    {
                        AndNode_Id = and_spnode.node_id,
                        Artifact_id = key,
                    };

                    foreach (var writeItem in artifact.Write)
                    {
                        if (artifact.Read.ContainsKey(writeItem.Key) && artifact.Read.Keys.Count > 1)
                        {
                            cwra.WriteNodes.Add(writeItem.Value);
                        }
                        else if (!artifact.Read.ContainsKey(writeItem.Key) && artifact.Read.Keys.Count == 1) // read got only one
                        {
                            cwra.WriteNodes.Add(writeItem.Value);
                        }
                    }

                    foreach (var readItem in artifact.Read)
                    {
                        if (artifact.Write.ContainsKey(readItem.Key) && artifact.Write.Keys.Count > 1)
                        {
                            cwra.ReadNodes.Add(readItem.Value);
                        }
                        else if (!artifact.Write.ContainsKey(readItem.Key) && artifact.Write.Keys.Count == 1)
                        {
                            cwra.ReadNodes.Add(readItem.Value);
                        }
                    }

                    if (cwra.ReadNodes.Count > 0)
                    {
                        //Console.WriteLine("=========Write=============CWRA===============READ====");
                       // var serilaized = JsonConvert.SerializeObject(cwra, Newtonsoft.Json.Formatting.Indented);
                        anomalyResult.Add(cwra);
                        //Console.WriteLine(serilaized);
                        //Console.WriteLine("======================================================");
                    }
                }

                if (artifact.Write.Count >= 1 && artifact.Kill.Count >= 1)
                {
                    var cwka = new CWKA()
                    {
                        AndNode_Id = and_spnode.node_id,
                        Artifact_id = key,
                    };

                    foreach (var writeItem in artifact.Write)
                    {
                        if (artifact.Kill.ContainsKey(writeItem.Key) && artifact.Kill.Keys.Count > 1)
                        {
                            cwka.WriteNodes.Add(writeItem.Value);
                        }
                        else if (!artifact.Kill.ContainsKey(writeItem.Key) && artifact.Kill.Keys.Count == 1)
                        {
                            cwka.WriteNodes.Add(writeItem.Value);
                        }
                    }

                    foreach (var killItem in artifact.Kill)
                    {
                        if (artifact.Write.ContainsKey(killItem.Key) && artifact.Write.Keys.Count > 1)
                        {
                            cwka.KillNodes.Add(killItem.Value);
                        }
                        else if (!artifact.Write.ContainsKey(killItem.Key) && artifact.Write.Keys.Count == 1)
                        {
                            cwka.KillNodes.Add(killItem.Value);
                        }
                    }

                    if (cwka.WriteNodes.Count > 0)
                    {
                        //Console.WriteLine("=========Write=============CWKA===============Kill====");
                       // var serilaized = JsonConvert.SerializeObject(cwka, Newtonsoft.Json.Formatting.Indented);
                        anomalyResult.Add(cwka);
                        //Console.WriteLine(serilaized); 
                        //Console.WriteLine("======================================================");
                    }
                }

                if (artifact.Kill.Count >= 1 && artifact.Read.Count >= 1)
                {
                    var ckra = new CKRA()
                    {
                        AndNode_Id = and_spnode.node_id,
                        Artifact_id = key,
                    };

                    foreach (var killItem in artifact.Kill)
                    {
                        if (artifact.Read.ContainsKey(killItem.Key) && artifact.Read.Keys.Count > 1)
                        {
                            ckra.KillNodes.Add(killItem.Value);
                        }
                        else if (!artifact.Read.ContainsKey(killItem.Key) && artifact.Read.Keys.Count == 1)
                        {
                            ckra.KillNodes.Add(killItem.Value);
                        }
                    }

                    foreach (var readItem in artifact.Read)
                    {
                        if (artifact.Kill.ContainsKey(readItem.Key) && artifact.Kill.Keys.Count > 1)
                        {
                            ckra.ReadNodes.Add(readItem.Value);
                        }
                        else if (!artifact.Kill.ContainsKey(readItem.Key) && artifact.Kill.Keys.Count == 1)
                        {
                            ckra.ReadNodes.Add(readItem.Value);
                        }
                    }

                    if (ckra.ReadNodes.Count > 0)
                    {
                        //Console.WriteLine("=========Write=============CWKA===============Kill=====================");
                        //var serilaized = JsonConvert.SerializeObject(ckra, Newtonsoft.Json.Formatting.Indented);
                        anomalyResult.Add(ckra);
                        //Console.WriteLine(serilaized); 
                        //Console.WriteLine("==========================================================");
                    }
                }
            }
        }

        private static void Merge(Dictionary<string, NodesOpArtifact> dest, Dictionary<string, NodesOpArtifact> source)
        {
            if (source == null)
                return;

            foreach (var item in source)
            {
                if (dest.ContainsKey(item.Key) == false)
                {
                    dest.Add(item.Key, item.Value);
                }
                else
                {
                    if (item.Value.ReadNodes.Count > 0)
                    {
                        var orignialList = dest[item.Key].ReadNodes;
                        var newList = item.Value.ReadNodes;

                        var desiredResult = orignialList.Union(newList);

                        dest[item.Key].ReadNodes = desiredResult.ToList();
                    }
                    if (item.Value.KillNodes.Count > 0)
                    {
                        var orignialList = dest[item.Key].KillNodes;
                        var newList = item.Value.KillNodes;

                        var desiredResult = orignialList.Union(newList);

                        dest[item.Key].KillNodes = desiredResult.ToList();
                    }
                    if (item.Value.WriteNodes.Count > 0)
                    {
                        var orignialList = dest[item.Key].WriteNodes;
                        var newList = item.Value.WriteNodes;

                        var desiredResult = orignialList.Union(newList);

                        dest[item.Key].WriteNodes = desiredResult.ToList();
                    }
                }
            }
        }

    }
}
