using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnomalyDection.Core.Definitions
{
    public class ConcurrentArtifact
    {
        public int WriteCount { get; set; }
        public int KillCount { get; set; }
        public int ReadCount { get; set; }

        public List<List<string>> KillBranches { get; set; } = new List<List<string>>();
        public List<List<string>> ReadBranches { get; set; } = new List<List<string>>();
        public List<List<string>> WriteBranches { get; set; } = new List<List<string>>();

        public List<string>? KillNodes { get; set; } = new List<string>();
        public List<string>? WriteNodes { get; set; } = new List<string>();
        public List<string>? ReadNodes { get; set; } = new List<string>();

        public bool CheckWrite { get; set; } = true;
        public bool CheckRead { get; set; } = true;
        public bool CheckKill { get; set; } = true;

        public int WriteBranchId { get; set; }
        public int ReadBranchId { get; set; }
        public int KillBranchId { get; set; }
    }
}
