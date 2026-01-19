using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using WekenDev.InputSystem;
using System.Collections;

namespace WekenDev.VoiceChat
{
    public class Recorder : MonoBehaviour
    {
        public static Recorder Instance;
        public event Action<byte[]> OnSendDataToNetwork;
        public bool IsMuted = false;

        [Header("Debug")]
        [SerializeField] private bool _debugEcho = false;
        [SerializeField] private Speaker _echoSpeaker;

        // Приватные поля
        private int _lastPosition = 0;
        private List<float> _buffer;
        private AudioClip _workingClip;
        private string _currentMicrophone;
        private bool _isRecording = false;
        private InputAction _voiceAction;
        private bool _isVoiceActiv;
        private float _voiceDetectionThreshold = 0.0005f;
        private int _chunkSize;

        // Статистика
        private int _totalChunksSent = 0;
        private int _totalBytesSent = 0;
        private float _lastStatLogTime = 0f;
        private const int STATS_LOG_INTERVAL = 10;
        private const int MICROPHONE_BUFFER_SECONDS = 10;

        // Для отправки тишины
        private float _silenceTimer = 0f;
        private const float SILENCE_SEND_INTERVAL = 0.02f; // Отправлять тишину каждые 20ms
        private byte[] _cachedSilenceData; // Кэшированные данные тишины

        private void Awake() => InitializeSingleton();
        private void Update() => ProcessFrame();

        public void Init() => Initialize();
        public void SetMicrophone(string microphone) => ChangeMicrophone(microphone);
        public void SetMode(bool enabled) => SetVoiceActivationMode(enabled);
        public void SetResponseMicrophone(float value) => _voiceDetectionThreshold = Mathf.Lerp(0.0005f, 0.02f, value / 100f);
        public void SetCheckMicrophone(bool enabled) => CheckMic(enabled);


        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Initialize()
        {
            _voiceAction = InputManager.Instance.Actions.Player.Voice;

            if (_voiceAction != null)
            {
                _voiceAction.started += OnVoiceStarted;
                _voiceAction.canceled += OnVoiceCanceled;
            }

            _buffer = new List<float>();
            _chunkSize = Settings.FRAME_SIZE;

            RequestMicrophonePermission();

            _echoSpeaker?.Init();

            StartCoroutine(DelayedMicrophoneInitialization());
        }

        private void CheckMic(bool enabled)
        {
            _debugEcho = enabled;

            if (_debugEcho == false)
            {
                for (int i = 0; i < 15; i++)
                {
                    _echoSpeaker.AddVoiceData(_cachedSilenceData);
                }
            }
        }

        private IEnumerator DelayedMicrophoneInitialization()
        {
            yield return new WaitForSeconds(0.5f);

            if (HasConnectedMicrophoneDevices())
            {
                _currentMicrophone = Microphone.devices[0];
                Debug.Log($"[Recorder] Default microphone set: {_currentMicrophone}");
            }
            else
            {
                Debug.Log($"[Recorder] Can't set any microphone.");
            }

            SwitchState();
        }

        //Установка микрофона
        private void ChangeMicrophone(string microphone)
        {
            if (_currentMicrophone == microphone)
                return;

            bool wasRecording = _isRecording;

            StopRecord();

            _currentMicrophone = microphone;

            if (wasRecording && !string.IsNullOrEmpty(_currentMicrophone))
            {
                StartCoroutine(StartRecordCoroutine());
            }
        }

        //Установка режима активации записи
        private void SetVoiceActivationMode(bool enabled)
        {
            _isVoiceActiv = enabled;

            SwitchState();
        }

        /// <summary>
        /// Switch ON / OFF state.
        /// </summary>
        private void OnVoiceStarted(InputAction.CallbackContext context)
        {
            if (_isVoiceActiv) return;

            StartCoroutine(StartRecordCoroutine());
        }

        private void OnVoiceCanceled(InputAction.CallbackContext context)
        {
            if (_isVoiceActiv) return;

            StopRecord();
        }

        private void SwitchState()
        {
            if (_voiceAction == null) return;

            if (_isVoiceActiv)
            {
                _voiceAction.Disable();
                StartCoroutine(StartRecordCoroutine());
            }
            else
            {
                _voiceAction.Enable();
                StopRecord();
            }
        }

        private void ProcessFrame()
        {
            if (!_isVoiceActiv && !_isRecording)
            {
                SendSilenceIfNeeded();
            }

            if (Time.time - _lastStatLogTime > STATS_LOG_INTERVAL)
            {
                LogStats();
            }

            if (_isRecording && !string.IsNullOrEmpty(_currentMicrophone))
            {
                ProcessRecording();
            }
        }

        private void SendSilenceIfNeeded()
        {
            _silenceTimer += Time.deltaTime;

            if (_silenceTimer >= SILENCE_SEND_INTERVAL)
            {
                SendSilenceChunk();
                _silenceTimer = 0f;
            }
        }

        private void LogStats()
        {
            float avgChunkSize = _totalBytesSent / (float)Mathf.Max(1, _totalChunksSent);
            float bytesPerSecond = _totalBytesSent / (Time.time - _lastStatLogTime);
            float kbps = (bytesPerSecond * 8f) / 1000f;

            Debug.Log($"📊 VOIP Stats (last 3s): " +
                     $"{_totalChunksSent} chunks, " +
                     $"{_totalBytesSent} bytes total, " +
                     $"Avg: {avgChunkSize:F0} bytes/chunk, " +
                     $"Rate: {bytesPerSecond / 1024:F1} KB/s ({kbps:F1} kbps)");

            _totalChunksSent = 0;
            _totalBytesSent = 0;
            _lastStatLogTime = Time.time;
        }

        private IEnumerator StartRecordCoroutine()
        {
            // Проверить доступность микрофона
            if (string.IsNullOrEmpty(_currentMicrophone))
            {
                if (!HasConnectedMicrophoneDevices())
                {
                    yield break;
                }
                _currentMicrophone = Microphone.devices[0];
            }

            if (Microphone.IsRecording(_currentMicrophone))
            {
                Microphone.End(_currentMicrophone);
            }

            if (Microphone.IsRecording(_currentMicrophone))
            {
                Microphone.End(_currentMicrophone);
                yield return new WaitForSeconds(0.05f);
            }

            _buffer?.Clear();

            if (_workingClip != null)
            {
                Destroy(_workingClip);
                _workingClip = null;
            }

            _workingClip = Microphone.Start(_currentMicrophone, true, MICROPHONE_BUFFER_SECONDS, Settings.SAMPLE_RATE);

            int attempts = 0;
            while (Microphone.GetPosition(_currentMicrophone) == 0 && attempts < 10)
            {
                attempts++;
                yield return new WaitForSeconds(0.01f);
            }

            _lastPosition = 0;
            _isRecording = true;
        }

        private bool StopRecord()
        {
            if (!_isRecording)
                return false;

            if (_buffer.Count > 0)
            {
                while (_buffer.Count < _chunkSize)
                {
                    _buffer.Add(0f);
                }
                SendChunk(_buffer.GetRange(0, Mathf.Min(_chunkSize, _buffer.Count)));
                _buffer.Clear();
            }

            if (_workingClip != null)
            {
                Destroy(_workingClip);
                _workingClip = null;
            }

            _lastPosition = 0;
            _buffer?.Clear();

            _isRecording = false;

            return true;
        }



        /// <summary>
        /// Get audio data from microphone and prepare it for sending via network.
        /// </summary>
        private void ProcessRecording()
        {
            if (_workingClip == null) return;
            int currentPosition = Microphone.GetPosition(_currentMicrophone);
            int newSamples = GetNewSamplesCount(currentPosition, _lastPosition, _workingClip.samples);

            if (newSamples > 0)
            {
                if (_isRecording)
                {
                    if (!IsMuted)
                    {
                        ReadNewSamples(currentPosition, newSamples); //Читаем и отправляем данные
                    }
                }
            }

            _lastPosition = currentPosition;
        }

        private int GetNewSamplesCount(int currentPos, int lastPos, int clipLength)
        {
            if (currentPos >= lastPos)
                return currentPos - lastPos;
            else
                return (clipLength - lastPos) + currentPos;
        }

        private void ReadNewSamples(int currentPos, int newSamples)
        {
            // Читаем данные порциями по CHUNK_SIZE
            int samplesRead = 0;

            while (samplesRead < newSamples)
            {
                int samplesToRead = Mathf.Min(_chunkSize, newSamples - samplesRead);

                // Рассчитываем позицию в AudioClip
                int readPos = (_lastPosition + samplesRead) % _workingClip.samples;

                // Читаем порцию данных
                if (readPos + samplesToRead <= _workingClip.samples)
                {
                    // Обычное чтение
                    float[] chunk = new float[samplesToRead];
                    _workingClip.GetData(chunk, readPos);
                    ProcessAudioChunk(chunk);
                }
                else
                {
                    // Чтение через границу буфера
                    int firstPart = _workingClip.samples - readPos;
                    float[] chunk1 = new float[firstPart];
                    _workingClip.GetData(chunk1, readPos);

                    int secondPart = samplesToRead - firstPart;
                    float[] chunk2 = new float[secondPart];
                    _workingClip.GetData(chunk2, 0);

                    // Объединяем
                    float[] fullChunk = new float[samplesToRead];
                    Array.Copy(chunk1, 0, fullChunk, 0, firstPart);
                    Array.Copy(chunk2, 0, fullChunk, firstPart, secondPart);

                    ProcessAudioChunk(fullChunk);
                }

                samplesRead += samplesToRead;
            }
        }

        private void ProcessAudioChunk(float[] chunk)
        {
            if (!VoiceIsDetected(chunk))
            {
                SendSilenceChunk();
                return;
            }

            _buffer.AddRange(chunk);

            while (_buffer.Count >= _chunkSize)
            {
                var chunkToSend = _buffer.GetRange(0, _chunkSize);
                SendChunk(chunkToSend);

                _buffer.RemoveRange(0, _chunkSize);
            }
        }

        private void SendSilenceChunk()
        {
            // Создаем или используем кэшированные данные тишины
            if (_cachedSilenceData == null)
            {
                List<float> silenceChunk = new List<float>();
                for (int i = 0; i < _chunkSize; i++)
                {
                    silenceChunk.Add(0f);
                }

                short[] shorts = new short[_chunkSize];
                for (int i = 0; i < _chunkSize; i++)
                {
                    shorts[i] = 0;
                }

                _cachedSilenceData = AudioOpusCompressor.Compress(shorts);
            }

            // Статистика
            _totalChunksSent++;
            _totalBytesSent += _cachedSilenceData.Length;

            // Эхо (опционально)
            if (_debugEcho && _echoSpeaker != null)
                _echoSpeaker.AddVoiceData(_cachedSilenceData);

            // Отправка
            OnSendDataToNetwork?.Invoke(_cachedSilenceData);
        }


        private void SendChunk(List<float> chunkSamples)
        {
            if (chunkSamples.Count != 320)
            {
                Debug.LogError($"Wrong chunk size: {chunkSamples.Count}, expected 320");
                return;
            }

            short[] shorts = new short[_chunkSize];
            for (int i = 0; i < 320; i++)
            {
                float sample = Mathf.Clamp(chunkSamples[i], -1f, 1f);
                shorts[i] = (short)(sample * 32767);
            }

            byte[] compressed = AudioOpusCompressor.Compress(shorts);

            //Статистика
            _totalChunksSent++;
            _totalBytesSent += compressed.Length;

            //Эхо
            if (_debugEcho && _echoSpeaker != null)
                _echoSpeaker.AddVoiceData(compressed);

            //Отправка
            OnSendDataToNetwork?.Invoke(compressed);
        }


        bool HasConnectedMicrophoneDevices()
        {
            return Microphone.devices.Length > 0;
        }

        void RequestMicrophonePermission()
        {
            if (!HasMicrophonePermission())
            {
#if UNITY_ANDROID
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
#elif UNITY_IOS
				Application.RequestUserAuthorization(UserAuthorization.Microphone);
#elif UNITY_WEBGL && !UNITY_EDITOR && FG_MPRO
				FrostweepGames.MicrophonePro.Microphone.Instance.RequestPermission();
#endif
            }
        }

        bool HasMicrophonePermission()
        {
#if UNITY_ANDROID
            return UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone);
#elif UNITY_IOS
			return Application.HasUserAuthorization(UserAuthorization.Microphone);
#else
            return true;
#endif
        }

        bool VoiceIsDetected(float[] samples)
        {
            //bool detected = false;
            //double sumTwo = 0;
            //double tempValue;

            //for (int index = 0; index < samples.Length; index++)
            //{
            // tempValue = samples[index];
            // sumTwo += tempValue * tempValue;
            // if (tempValue > _voiceDetectionThreshold)
            //  detected = true;
            // }
            // sumTwo /= samples.Length;

            //return detected || sumTwo > _voiceDetectionThreshold;

            float sumSquares = 0f;
            float peakValue = 0f;

            for (int i = 0; i < samples.Length; i++)
            {
                float absSample = Mathf.Abs(samples[i]);
                sumSquares += absSample * absSample;
                if (absSample > peakValue)
                    peakValue = absSample;
            }

            float rms = Mathf.Sqrt(sumSquares / samples.Length);

            // 2. Динамические пороги на основе текущего threshold
            float energyThreshold = _voiceDetectionThreshold; // Порог для RMS
            float peakThreshold = _voiceDetectionThreshold * 3f; // Порог для пиков может быть выше

            // 3. Логика детекции:
            // - Либо достаточно высокая общая энергия (RMS)
            // - Либо есть заметные пики
            bool hasEnergy = rms > energyThreshold;
            bool hasPeaks = peakValue > peakThreshold;

            return hasEnergy || hasPeaks;
        }

        private void OnDisable()
        {
            if (_voiceAction != null)
            {
                _voiceAction.started -= OnVoiceStarted;
                _voiceAction.canceled -= OnVoiceCanceled;
                _voiceAction.Disable();
            }
        }
    }
}

