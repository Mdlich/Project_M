using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class OptionsManager : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundSLider;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    void Start()
    {
        musicSlider.SetValueWithoutNotify( AudioManager.MusicVolume );
        soundSLider.SetValueWithoutNotify( AudioManager.SoundVolume );
        qualityDropdown.SetValueWithoutNotify( GameManager.Quality );
    }

    public void SetMusicVolume(float volume )
    {
        AudioManager.ChangeMusicVolume( volume );
    }

    public void SetSoundVolume( float volume )
    {
        AudioManager.ChangeSoundsVolume( volume );
    }

    public void SetQuality(int state )
    {
        GameManager.Quality = state;
    }
}
