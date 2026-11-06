using System.Collections.Generic;

namespace uPSG
{
    /**** v0.9.1beta ****/

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