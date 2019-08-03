using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FCP_XML_SRT_Syncer
{
    class Snippet
    {
        public string sequenceId = "";

        // These all in milliseconds
        public double sourceStartsAt = 0;
        public double sourceEndsAt = 0;
        public double sourceAbsoluteInPoint = 0;
        public double sourceAbsoluteOutPoint = 0;

        public Snippet(string sequenceIdA, double sourceStartsAtA, double sourceEndsAtA, double sourceAbsoluteInPointA, double sourceAbsoluteOutPointA)
        {
            sequenceId = sequenceIdA;
            sourceStartsAt = sourceStartsAtA;
            sourceEndsAt = sourceEndsAtA;
            sourceAbsoluteInPoint = sourceAbsoluteInPointA;
            sourceAbsoluteOutPoint = sourceAbsoluteOutPointA;
        }

        public override string ToString()
        {
            return "Snippet (" + sequenceId + "," + sourceStartsAt + "," + sourceEndsAt + "," + sourceAbsoluteInPoint + "," + sourceAbsoluteOutPoint+")";
        }
    }
}
