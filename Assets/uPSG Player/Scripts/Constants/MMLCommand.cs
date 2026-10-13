namespace uPSG
{
    /**** v0.9.9beta ****/
    public static class MMLCommand
    {
        public const char MML_COMMENT_LINE = ';';
        public const char MML_COMMENT_BLOCK_1 = '/';
        public const char MML_COMMENT_BLOCK_2 = '*';
        public const char MML_TEMPO = 't';
        public const char MML_VOICE = '@';
        public const char MML_DPCM = 'p';
        public const char MML_VOLUME = 'v';
        public const char MML_OCTAVE = 'o';
        public const char MML_OCTAVE_UP = '>';
        public const char MML_OCTAVE_DOWN = '<';
        public const char MML_LENGTH = 'l';
        public const char MML_LENGTH_DOT = '.';
        public const char MML_NOTE_NUM = 'n';
        public const char MML_FREQ = 'z';
        public const char MML_NOTE_A = 'a';
        public const char MML_NOTE_G = 'g';
        public const char MML_NOTE_SHARP = '+';
        public const char MML_NOTE_FLAT = '-';
        public const char MML_REST = 'r';
        public const char MML_TIE = '&';
        public const char MML_REPEAT_START = '[';
        public const char MML_REPEAT_END = ']';
        public const char MML_LOOP = 'L';
        public const char MML_TUNE = 'T';
        public const char MML_GATE = 'G';
        public const char MML_SWEEP = 'S';
        public const char MML_ENVELOPE = 'V';
        public const char MML_LFO = 'M';
        public const char MML_CH_SYNC = '!';
        public const char NUM_ZERO = '0';
        public const char NUM_NINE = '9';
        public const char NUM_PLUS = '+';
        public const char NUM_MINUS = '-';
        public const char PARAM_START = '{';
        public const char PARAM_END = '}';
        public const char CHR_COMMA = ',';
        public const char CHR_PIPE = '|';
    }
}