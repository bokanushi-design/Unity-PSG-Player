using System.Collections.Generic;

namespace uPSG
{
    /**** v0.9.1beta ****/

    [System.Serializable]
    public class SeqJson
    {
        public int jsonTickPerNote;
        public List<SeqEvent> jsonSeqList;
        public SeqJson()
        {
            jsonSeqList = new();
        }
    }
}
