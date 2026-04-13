using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Slider volumeSlider;

    public void SetVolume()
    {
        AudioListener.volume = volumeSlider.value;
    }

    public GameObject settingsUI;

    public void ToggleSettings()
    {
        settingsUI.SetActive(!settingsUI.activeSelf);
    }
}