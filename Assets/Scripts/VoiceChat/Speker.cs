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

        [SerializeField, Range(0.1f, 5f)] private float _gain = 1f;
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

            for (int i = 0; i < samples.Length; i++)
            {
                float s = samples[i] * _gain;
                if (s > 1f) s = 1f;
                else if (s < -1f) s = -1f;

                _clipData[_writePos] = s;
                _writePos = (_writePos + 1) % CLIP_SIZE;
            }

            _clip.SetData(_clipData, 0);
        }

        private void OnDestroy()
        {
            _source = null;
        }
    }
}