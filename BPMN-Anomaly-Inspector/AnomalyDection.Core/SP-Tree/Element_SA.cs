using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnomalyDection.Core.SP_Tree
{

    public class Element_SA
    {
        public string artifact_id;
        public string node_id;

        public Element_SA(string artifact_id, string node_id)
        {
            this.artifact_id = artifact_id;
            this.node_id = node_id;
        }
    }
}
