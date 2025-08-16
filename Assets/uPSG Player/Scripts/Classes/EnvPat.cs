using System.Collections.Generic;

namespace uPSG
{
    [System.Serializable]
    public class EnvPat
    {
        public int envId;
        public List<int> envVolList = new List<int>();
        public int envLoopIndex;
        public EnvPat(int _id, List<int> _envParams, int _envloopIndex)
        {
            envId = _id;
            envVolList.Clear();
            envVolList.AddRange(_envParams);
            envLoopIndex = _envloopIndex;
        }
    }

}
