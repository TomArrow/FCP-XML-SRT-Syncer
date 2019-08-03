using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        public override int GetHashCode() {

            var mystring = ToString();
            MD5 md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(mystring));
            var ivalue = BitConverter.ToInt32(hashed, 0);
            return ivalue;
        }
        public bool Equals(SRTEntry obj) {
            return obj.startTime == this.startTime && obj.endTime == this.endTime && obj.text == this.text;
        }
    }
}
