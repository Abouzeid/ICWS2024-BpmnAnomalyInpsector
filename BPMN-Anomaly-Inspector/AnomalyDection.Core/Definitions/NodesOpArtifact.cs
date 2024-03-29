using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnomalyDection.Core.Definitions
{

    public class NodesOpArtifact
    {
        public List<string> KillNodes { get; set; } = new List<string>();
        public List<string> WriteNodes { get; set; } = new List<string>();
        public List<string> ReadNodes { get; set; } = new List<string>();
    }

    public class BranchesSet
    {
        public List<List<string>> Kill_in_Branches { get; set; } = new List<List<string>>();
        public List<List<string>> Read_in_Branches { get; set; } = new List<List<string>>();
        public List<List<string>> Write_in_Branches { get; set; } = new List<List<string>>();
    }

    public struct BranchesRWKinfo
    {
        public BranchesRWKinfo()
        {
        }
        // key is branch_id and value is Nodes that K/W/R on an artifact
        public Dictionary<int, List<string>> Kill { get; set; } = new Dictionary<int, List<string>>();
        public Dictionary<int, List<string>> Read { get; set; } = new Dictionary<int, List<string>>();
        public Dictionary<int, List<string>> Write { get; set; } = new Dictionary<int, List<string>>();
    }
}
