using UnityEngine;

namespace WekenDev.VoiceChat
{
    public class Speaker : MonoBehaviour
    {
        private AudioSource _source;
        private AudioClip _clip;
        private float[] _clipData;
        private int _writePos = 0;
        private const int CLIP_SIZE = 4800;


        public void Init()
        {
            _source = gameObject.GetComponent<AudioSource>();
            _source.loop = true;

            _clipData = new float[CLIP_SIZE];
            _clip = AudioClip.Create("Voice", CLIP_SIZE, 1, Settings.SAMPLE_RATE, false);
            _clip.SetData(_clipData, 0);

            _source.clip = _clip;
            _source.Play();

            Debug.Log($"Big Clip Speaker: {CLIP_SIZE} samples ({CLIP_SIZE / 16}ms)");
        }

        public void AddVoiceData(byte[] voiceData)
        {
            if (voiceData == null || voiceData.Length == 0 || _clip == null || _clipData == null)
            {
                Debug.LogWarning("AddVoiceData: voiceData is null or empty or clip null or clipData null");
                return;
            }

            float[] samples = AudioOpusCompressor.Decompress(voiceData);

            foreach (float sample in samples)
            {
                _clipData[_writePos] = sample;
                _writePos = (_writePos + 1) % CLIP_SIZE;
            }

            _clip.SetData(_clipData, 0);
        }
    }
}