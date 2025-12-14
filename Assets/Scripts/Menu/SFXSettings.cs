using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SFXSettings : MonoBehaviour
{
    public AudioMixer mixer;
    const string SFX_PARAM = "SFXVolume";
     public Slider sfxSlider;

    void Start()
    {
        float saved = PlayerPrefs.GetFloat(SFX_PARAM, 0.8f);
        sfxSlider.value = saved;
        SetSFXVolume(saved);
    }

    public void SetSFXVolume(float value)
    {
        mixer.SetFloat(SFX_PARAM, Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(SFX_PARAM, value);
    }
}