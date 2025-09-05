using System.Collections.Generic;

namespace uPSG
{
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
