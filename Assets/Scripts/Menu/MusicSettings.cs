using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MusicSettings : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider musicSlider;

    const string MUSIC_PARAM = "MusicVolume";

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(MUSIC_PARAM, 0.5f);
        musicSlider.value = savedVolume;
        SetMusicVolume(savedVolume);
    }

    public void SetMusicVolume(float value)
    {
        mixer.SetFloat(MUSIC_PARAM, Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(MUSIC_PARAM, value);
    }
}