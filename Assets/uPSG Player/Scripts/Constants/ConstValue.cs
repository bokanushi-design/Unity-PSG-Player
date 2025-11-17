namespace uPSG
{
    /**** v0.9.6beta ****/

    public static class ConstValue
    {
        public const int SEMITONES_IN_OCTAVE = 12;
        public const int CENT_IN_OCTAVE = SEMITONES_IN_OCTAVE * 100;
        public const int DEFAULT_SAMPLE_RATE = 32000;
        public const int DEFAULT_AUDIO_CLIP_SIZE = 1000;
        public const int AUDIO_CLIP_SIZE_MIN = 20;
        public const int AUDIO_CLIP_SIZE_MAX = 1000;
        public const int DEFAULT_A4_FREQ = 440;
        public const int A4_FREQ_MIN = 400;
        public const int A4_FREQ_MAX = 480;
        public const int DEFAULT_TICK_PER_NOTE = 960;
        public const int TRIANGLE_TABLE_LENGTH = 32;
        public const int A4_NOTE_NUM = 69;
        public const int NOTE_NUM_MIN = 12;
        public const int NOTE_NUM_MAX = 120;
        public const int DEFAULT_TEMPO = 120;
        public const int TEMPO_MIN = 1;
        public const int TEMPO_MAX = 255;
        public const float DEFAULT_GATE_STEP_RATE = 1f;
        public const int SEQ_VOL_MAX = 15;
        public const int NOISE_REG = 0x8000;
        public const double NES_FREQ = 894886.25d;
        public const int DEFAULT_OCTAVE = 4;
        public const int OCTAVE_MIN = 2;
        public const int OCTAVE_MAX = 8;
        public const int DEFAULT_PROGRAM_CHANGE = 2;
        public const int PROGRAM_CHANGE_MAX = 6;
        public const int DEFAULT_LENGTH = 4;
        public const int LENGTH_MIN = 1;
        public const int LENGTH_MAX = 128;
        public const int DEFAULT_NOTE_NUM = 60;
        public const int SWEEP_MIN = -1200;
        public const int SWEEP_MAX = 1200;
        public const int REPEAT_MAX = 128;
        public const int GATE_MAX = 100;
        public const int LFO_MAX = 255;
        public const int DEFAULT_ASYNC_INTERRUPT = 1000;
    }
}