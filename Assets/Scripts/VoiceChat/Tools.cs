using System;
using System.Collections.Generic;

namespace WekenDev.VoiceChat
{
    public static class Tools
    {
        /// <summary>
        /// Convert float array of RAW samples into bytes array
        /// </summary>
        public static byte[] FloatToByte(float[] samples)
        {
            short[] intData = new short[samples.Length];
            byte[] bytesData = new byte[samples.Length * 2];
            for (int i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * 32767);
                byte[] byteArr = System.BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }
            return bytesData;
        }

        public static short[] FloatToShort(float[] samples)
        {
            short[] intData = new short[samples.Length];

            for (int i = 0; i < samples.Length; i++)
            {
                float sample = samples[i];
                // Защита от выхода за границы
                if (sample > 1.0f) sample = 1.0f;
                else if (sample < -1.0f) sample = -1.0f;

                intData[i] = (short)(sample * 32767);
            }

            return intData;
        }

        public static float[] ShortToFloat(short[] intData)
        {
            float[] floatData = new float[intData.Length];

            for (int i = 0; i < intData.Length; i++)
            {
                floatData[i] = intData[i] / 32768f;
            }

            return floatData;
        }


        /// <summary>
        /// Convert float array of RAW samples into bytes array
        /// </summary>
        public static byte[] FloatToByte(List<float> samples)
        {
            short[] intData = new short[samples.Count];
            byte[] bytesData = new byte[samples.Count * 2];
            for (int i = 0; i < samples.Count; i++)
            {
                intData[i] = (short)(samples[i] * 32767);
                byte[] byteArr = System.BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }
            return bytesData;
        }


        /// <summary>
        /// Converts list of bytes to float array by using 32767 rescale factor
        /// </summary>
        public static float[] ByteToFloat(byte[] data)
        {
            int length = data.Length / 2;
            float[] samples = new float[length];
            for (int i = 0; i < length; i++)
                samples[i] = (float)BitConverter.ToInt16(data, i * 2) / 32767;
            return samples;
        }

        /// <summary>
        /// Converts list of bytes to float array by using 32767 rescale factor
        /// </summary>
        public static float[] ByteToFloat(byte[] data, int startIndex, int dataLenght)
        {
            int length = dataLenght / 2;
            float[] samples = new float[length];
            for (int i = 0; i < length; i++)
                samples[i] = (float)BitConverter.ToInt16(data, startIndex + i * 2) / 32767;
            return samples;
        }
    }
}
