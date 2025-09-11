namespace uPSG
{
    [System.Serializable]
    public enum SEQ_CMD
    {
        PROGRAM_CHANGE = 00,
        SET_TEMPO = 01,
        TUNE = 02,
        NOTE_ON = 10,
        REST = 11,
        NOTE_TIE = 12,
        GATE_STEP_RATE = 13,
        DIRECT_FREQ = 14,
        VOLUME = 20,
        ENV_ON = 21,
        ENV_PARAM_START = 22,
        ENV_PARAM = 23,
        ENV_PARAM_END = 24,
        SWEEP = 30,
        LFO_SET = 31,
        LFO_DELAY = 32,
        LFO_SPEED = 33,
        LFO_DEAPTH = 34,
        LFO_PARAM_END = 35,
        LOOP_POINT = 40,
        REPEAT_START = 41,
        REPEAT_END = 42,
        END_OF_SEQ = 99,
    }
}
