namespace uPSG
{
    /**** v0.9.9beta ****/

    [System.Serializable]
    public enum DPCM_RATE_INDEX
    {
        Rate_4_18kHz = 0,   // $0
        Rate_4_71kHz = 1,   // $1
        Rate_5_32kHz = 2,   // $2
        Rate_5_98kHz = 3,   // $3
        Rate_6_74kHz = 4,   // $4
        Rate_7_58kHz = 5,   // $5
        Rate_8_56kHz = 6,   // $6
        Rate_9_68kHz = 7,   // $7
        Rate_10_79kHz = 8,  // $8
        Rate_12_19kHz = 9,  // $9
        Rate_14_15kHz = 10, // $A (Commonly found on drum toms, etc.)
        Rate_16_92kHz = 11, // $B
        Rate_19_68kHz = 12, // $C
        Rate_24_28kHz = 13, // $D
        Rate_28_53kHz = 14, // $E
        Rate_33_14kHz = 15  // $F (A staple for top-quality sound, voice acting, etc.)
    }

    public static class DpcmValue
    {
        public static readonly float[] DPCM_FREQ_TABLE = new float[16]
        {
            4181.71f,  4709.93f,  5323.04f,  5977.14f,
            6741.41f,  7581.40f,  8555.77f,  9675.25f,
            10793.81f, 12193.30f, 14149.33f, 16919.19f,
            19678.96f, 24283.47f, 28532.74f, 33144.13f
        };
    }
}