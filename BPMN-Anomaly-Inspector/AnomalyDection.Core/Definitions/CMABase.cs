using Newtonsoft.Json;
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
        public string AnomalyType { get; set; } = "Concurrent";
    }

    public class CWWA : CMABase
    {
        [JsonProperty(PropertyName = "WriteNodes")]
        public List<List<string>> ParrallelWriteNodes { get; set; } = new List<List<string>>();
        public string Level { get; set; } = "Warning";
        public string Code { get; set; } = "ConWW";
    }

    public class CWKA : CMABase
    {
        public List<List<string>> WriteNodes { get; set; } = new List<List<string>>();
        public List<List<string>> KillNodes { get; set; } = new List<List<string>>();
        public string Level { get; set; } = "Warning";
        public string Code { get; set; } = "ConWK";
    }

    public class CKRA : CMABase
    {
        public List<List<string>> ReadNodes { get; set; } = new List<List<string>>();
        public List<List<string>> KillNodes { get; set; } = new List<List<string>>();
        public string Level { get; set; } = "Error";
        public string Code { get; set; } = "ConRA";
    }

    public class CWRA : CMABase
    {
        public List<List<string>> ReadNodes { get; set; } = new List<List<string>>();
        public List<List<string>> WriteNodes { get; set; } = new List<List<string>>();
        public string Level { get; set; } = "Error";
        public string Code { get; set; } = "ConWR";

    }

    public class CorruptedSend : CMABase
    {
        public string Level { get; set; } = "Error";
        public string Code { get; set; } = "CorruptedSend";
        public List<List<string>> ReadNodes { get; set; }
    }
}
