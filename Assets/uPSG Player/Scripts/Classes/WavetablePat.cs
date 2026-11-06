using System.Collections.Generic;

namespace uPSG
{
    /**** v0.9.10beta ****/

    [System.Serializable]
    public class WavetablePat
    {
        public int wtId;
        public List<int> wtParamList = new();

        public WavetablePat(int _id, List<int> wtParams)
        {
            wtId = _id;
            wtParamList.Clear();
            wtParamList.AddRange(wtParams);
        }
    }

}
