using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnomalyDection.Core.Definitions
{

    public class CMABase
    {
        public string Artifact_id { get; set; }
        public string AndNode_Id { get; set; }
    }

    public class CWWA : CMABase
    {
        public List<List<string>> ParrallelWriteNodes { get; set; } = new List<List<string>>();

    }

    public class CWKA : CMABase
    {
        public List<List<string>> WriteNodes { get; set; } = new List<List<string>>();
        public List<List<string>> KillNodes { get; set; } = new List<List<string>>();

    }

    public class CKRA : CMABase
    {
        public List<List<string>> ReadNodes { get; set; } = new List<List<string>>();
        public List<List<string>> KillNodes { get; set; } = new List<List<string>>();

    }

    public class CWRA : CMABase
    {
        public List<List<string>> ReadNodes { get; set; } = new List<List<string>>();
        public List<List<string>> WriteNodes { get; set; } = new List<List<string>>();

    }
}
