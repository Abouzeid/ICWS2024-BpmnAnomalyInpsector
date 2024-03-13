using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnomalyDection.Core.SP_Tree
{

    #region SAFW/K/R Sets
    public class SAFW
    {
        public List<Element_SA> Elements { get; set; } = new List<Element_SA>();
    }

    public class SAFR
    {
        public List<Element_SA> Elements { get; set; } = new List<Element_SA>();
    }

    public class SAFK
    {
        public List<Element_SA> Elements { get; set; } = new List<Element_SA>();
    }


    public class SALW
    {
        public List<Element_SA> Elements { get; set; } = new List<Element_SA>();
    }

    public class SALK
    {
        public List<Element_SA> Elements { get; set; } = new List<Element_SA>();
    }

    public class NLK
    {
        public List<Element_SA> Elements { get; set; } = new List<Element_SA>();
    }

    public class NLW
    {
        public List<Element_SA> Elements { get; set; } = new List<Element_SA>();
    }

    public class LK
    {
        public List<Element_SA> Elements { get; set; } = new List<Element_SA>();
    }

    public class LW
    {
        public List<Element_SA> Elements { get; set; } = new List<Element_SA>();
    }
    #endregion
}
