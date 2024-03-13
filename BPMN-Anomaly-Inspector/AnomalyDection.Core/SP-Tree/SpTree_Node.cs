using AnomalyDection.Core.Definitions;

namespace AnomalyDection.Core.SP_Tree
{

    public enum operation_types
    {
        Read, Write, Kill
    }

    public enum loop_types
    {
        NONE, WHILE, FOR, REPEAT
    }

    public enum node_types
    {
        START,
        END,
        ACTIVITY,
        AND,
        XOR,
        LOOP,
        Flow
    }


    public class artifact_operation_in_node
    {
        public string artifact_id { get; set; }
        public string node_id { get; set; }
        public operation_types? Operation { get; set; }
        public double EBT { get; set; }
        public double LCT { get; set; }
        public node_types? node_type { get; set; }
    }


    public class SpTree_Node
    {
        public struct Opt_Element
        {
            public string artifact_id;
            public operation_types operation;
            public string flowId;
        }

        public string node_id { get; set; }      
        public int level { get; set; }
        public node_types node_type { get; set; }
        public loop_types loop_type { get; set; }
        public SpTree_Node left_child { get; set; }
        public SpTree_Node parent { get; set; }
        public List<SpTree_Node> right_child { get; set; }

        public List<Opt_Element> operation_sequence { get; set; } = new List<Opt_Element>();

        public List<artifact_operation_in_node> artifact_set { get; set; } = new List<artifact_operation_in_node>();

        public Dictionary<string, ConcurrentArtifact> conCurrentArtifacts { get; set; } = new Dictionary<string, ConcurrentArtifact>();

        public Dictionary<string, NodesOpArtifact> ArtifactOpMap { get; set; } = new Dictionary<string, NodesOpArtifact>();

        public SAFW SAFW { get; set; } = new SAFW();
        public SAFR SAFR { get; set; } = new SAFR();
        public SAFK SAFK { get; set; } = new SAFK();

        public SALW SALW { get; set; } = new SALW();
        public SALK SALK { get; set; } = new SALK();

        public NLK NLK { get; set; } = new NLK();
        public NLW NLW { get; set; } = new NLW();

        public LK LK { get; set; } = new LK();
        public LW LW { get; set; } = new LW();
    }

}
