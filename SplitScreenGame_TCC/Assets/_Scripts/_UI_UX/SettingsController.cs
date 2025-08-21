using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsController : MonoBehaviour
{
    [Header("Volume Settings")]
    [SerializeField] private TMP_Text volumeTextValue = null;
    [SerializeField] private Slider volumeSlider = null;
    [SerializeField] private float defaultVolume = 1.0f;

    [Header("Graphics Settings")]
    private bool _isFullScreen;
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;

    [Space(10)]
    [SerializeField] private Toggle fullScreenToggle;

    [SerializeField] private GameObject confirmationPrompt = null;

    private void Start() 
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        if (volumeTextValue != null)
            volumeTextValue.text = volume.ToString("0.0");
    }

    public void SetFullScreen(bool isFullScreen)
    {
        _isFullScreen = isFullScreen;
    }

    public void GraphicsApply()
    {
        Screen.fullScreen = _isFullScreen;
        PlayerPrefs.SetInt("fullscreen", _isFullScreen ? 1 : 0);
        StartCoroutine(ConfirmationBox());
    }

    public void VolumeApply()
    {
        PlayerPrefs.SetFloat("masterVolume", AudioListener.volume);
        StartCoroutine(ConfirmationBox());
    }

    public void ResetButton()
    {
        // Volume
        AudioListener.volume = defaultVolume;
        if (volumeSlider != null)
            volumeSlider.value = defaultVolume;
        if (volumeTextValue != null)
            volumeTextValue.text = defaultVolume.ToString("0.0");
        VolumeApply();

        // Tela cheia
        fullScreenToggle.isOn = false;
        Screen.fullScreen = false;

        // Resolução
        Resolution currentResolution = Screen.currentResolution;
        Screen.SetResolution(currentResolution.width, currentResolution.height, Screen.fullScreen);
        resolutionDropdown.value = resolutions.Length - 1;
        GraphicsApply();
    }

    private IEnumerator ConfirmationBox()
    {
        confirmationPrompt.SetActive(true);
        yield return new WaitForSeconds(2f);
        confirmationPrompt.SetActive(false);
    }
}
