using UnityEngine;
using WekenDev.AudioManagerGame;



public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioMusicController _musicController;
    private AudioUiController _uiController;

    private void Awake() => InitializeSingleton();
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

    public void Init()
    {
        _musicController = GetComponentInChildren<AudioMusicController>();
        _musicController.Init();
        ChangeMusic(AudioDesign.Calm);

        _uiController = GetComponentInChildren<AudioUiController>();
        _uiController.Init();
    }


    public void ChangeMusic(AudioDesign audioDesign)
    {
        _musicController.ChangeAudioDesign(audioDesign);
    }
    public void PlayAudioUI(TypeUiAudio type)
    {
        _uiController.PlayAudioUI(type);
    }

}

