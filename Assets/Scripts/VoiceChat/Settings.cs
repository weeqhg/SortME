
namespace WekenDev.VoiceChat { 
    public static class Settings {

        /// <summary>
        /// Should we compress audio stream before sending via network?
        /// This value should be the same for the listener and speaker.
        /// </summary>
        public const bool compression = true;

        /// <summary>
        /// Piece time (milliseconds)
        /// </summary>
        public const int FRAME_TIME_MS = 20;

        /// <summary>
        /// The sampling rate used for audio recording and playback (8000, 16000, 32000).
        /// Make this value smaller when you have troubles sending big values via network.
        /// </summary>
        public const int SAMPLE_RATE = 16000; //8000;

        /// <summary>
        /// Size of data which is sent via network.
        /// </summary>
        public const int FRAME_SIZE = (int)(SAMPLE_RATE * ((float)FRAME_TIME_MS / 1000f)); // send with interval ~ pieceDuration ms

        public const int CHANNELS = 1;  

        /// <summary>
        /// What is size of audio clip, used by microphone (seconds). Audio clip is looped and rewritten from beginning when overflowed.
        /// </summary>
        public const int audioClipDuration = 1;
    }
}