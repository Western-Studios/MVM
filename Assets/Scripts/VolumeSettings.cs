using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider     masterSlider;
    [SerializeField] private Slider     musicSlider;
    [SerializeField] private Slider     sfxSlider;

    private const string MasterKey = "vol_master";
    private const string MusicKey  = "vol_music";
    private const string SFXKey    = "vol_sfx";

    private void Start()
    {
        masterSlider.value = PlayerPrefs.GetFloat(MasterKey, 1f);
        musicSlider.value  = PlayerPrefs.GetFloat(MusicKey,  1f);
        sfxSlider.value    = PlayerPrefs.GetFloat(SFXKey,    1f);

        ApplyMaster(masterSlider.value);
        ApplyMusic(musicSlider.value);
        ApplySFX(sfxSlider.value);

        masterSlider.onValueChanged.AddListener(ApplyMaster);
        musicSlider.onValueChanged.AddListener(ApplyMusic);
        sfxSlider.onValueChanged.AddListener(ApplySFX);
    }

    public void ApplyMaster(float value) => Apply("MasterVolume", MasterKey, value);
    public void ApplyMusic(float value)  => Apply("MusicVolume",  MusicKey,  value);
    public void ApplySFX(float value)    => Apply("SFXVolume",    SFXKey,    value);

    private void Apply(string param, string prefKey, float value)
    {
        mixer.SetFloat(param, ToDb(value));
        PlayerPrefs.SetFloat(prefKey, value);
    }

    // Convert 0-1 slider to decibels. Clamped to avoid log10(0) = -infinity.
    private static float ToDb(float value) => Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
}
