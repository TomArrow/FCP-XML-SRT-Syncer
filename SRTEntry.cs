using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCP_XML_SRT_Syncer
{
    class SRTEntry
    {

        // These all in seconds
        public double startTime = 0;
        public double endTime = 0;
        public string text = "";

        public SRTEntry(string textA, double originalStartTimeA, double originalEndTimeA)
        {
            text = textA;
            startTime = originalStartTimeA;
            endTime = originalEndTimeA;
        }

        public override string ToString()
        {
            return "SRTEntry (" + startTime + "," + endTime + "," + text + ")";
        }
    }
}
