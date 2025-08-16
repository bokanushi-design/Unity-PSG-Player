namespace uPSG
{

    [System.Serializable]
    public class SeqEvent
    {
        public SEQ_CMD seqCmd;
        public int seqParam;
        public int seqStep;
        public SeqEvent(SEQ_CMD _cmd, int _param, int _step)
        {
            seqCmd = _cmd;
            seqParam = _param;
            seqStep = _step;
        }
    }
}
