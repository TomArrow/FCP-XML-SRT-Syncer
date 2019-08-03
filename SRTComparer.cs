using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCP_XML_SRT_Syncer
{
    class SRTComparer : IEqualityComparer<SRTEntry>
    {
        public bool Equals(SRTEntry x, SRTEntry y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(SRTEntry obj)
        {
            return obj.GetHashCode();
        }
    }
}
