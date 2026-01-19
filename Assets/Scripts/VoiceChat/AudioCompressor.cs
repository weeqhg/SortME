using Concentus.Enums;
using Concentus.Structs;
using System;

namespace WekenDev.VoiceChat
{
    public static class AudioOpusCompressor
    {
        private static int _sampleRate = 0;
        private static int _channels = 0;
        private static int _frameSize = 0;
        private static OpusEncoder _encoder;
        private static OpusDecoder _decoder;

        private static void InitializeSettings()
        {
            if (_sampleRate == 0)
            {
                _sampleRate = Settings.SAMPLE_RATE;
                _channels = Settings.CHANNELS;
                _frameSize = Settings.FRAME_SIZE;
            }
        }

        private static OpusEncoder GetEncoder()
        {
            InitializeSettings();

            if (_encoder == null)
            {
                _encoder = new OpusEncoder(_sampleRate, _channels, OpusApplication.OPUS_APPLICATION_VOIP);
                _encoder.Bitrate = 16000; // 16 кбит/с
            }
            return _encoder;
        }

        private static OpusDecoder GetDecoder()
        {
            if (_decoder == null)
            {
                _decoder = new OpusDecoder(_sampleRate, _channels);
            }
            return _decoder;
        }

        public static byte[] Compress(short[] pcmAudio)
        {
            var encoder = GetEncoder();
            byte[] outputBuffer = new byte[1275]; // Максимальный размер пакета Opus

            int encodedBytes = encoder.Encode(
                pcmAudio.AsSpan(0, _frameSize),  // ReadOnlySpan<short> in_pcm
                _frameSize,                      // int frame_size
                outputBuffer.AsSpan(),           // Span<byte> out_data
                outputBuffer.Length              // int max_data_bytes
            );

            byte[] result = new byte[encodedBytes];
            outputBuffer.AsSpan(0, encodedBytes).CopyTo(result);
            return result;
        }

        public static float[] Decompress(byte[] opusData)
        {
            var decoder = GetDecoder();
            short[] pcmBuffer = new short[320];

            int decodedSamples = decoder.Decode(
                opusData.AsSpan(0, opusData.Length),  // ReadOnlySpan<byte>
                pcmBuffer.AsSpan(0, _frameSize),      // Span<short>
                _frameSize,                           // frame_size
                false                                 // decode_fec
            );

            if (decodedSamples <= 0)
                return new float[320];

            float[] floatBuffer = new float[decodedSamples];
            for (int i = 0; i < decodedSamples; i++)
            {
                floatBuffer[i] = pcmBuffer[i] / 32768f;
            }

            return floatBuffer;
        }
    }
}