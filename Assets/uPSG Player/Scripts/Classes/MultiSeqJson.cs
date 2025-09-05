using System.Collections.Generic;

namespace uPSG
{
    [System.Serializable]
    public class MultiSeqJson
    {
        public List<SeqJson> seqJsonList;
        public MultiSeqJson()
        {
            seqJsonList = new();
        }
    }
}