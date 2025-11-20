using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    private const string PLAYER_PREFS_FULLSCREEN = "Fullscreen";
    private const string PLAYER_PREFS_RESOLUTION = "ScreenResolutionIndex";

    [SerializeField] private Button soundEffectsButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Toggle fullscreenToggle;

    [SerializeField] private TextMeshProUGUI soundEffectsText;
    [SerializeField] private TextMeshProUGUI musicText;
    [SerializeField] private TextMeshProUGUI fullscreenText;

    [SerializeField] private TMP_Dropdown resolutionDropdown;

    private Resolution[] resolutions;

    private void Awake()
    {
        soundEffectsButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.ChangeVolume();
            UpdateVisual();
        });

        musicButton.onClick.AddListener(() =>
        {
            MusicManager.Instance.ChangeVolume();
            UpdateVisual();
        });

        fullscreenToggle.onValueChanged.AddListener((bool isOn) =>
        {
            Screen.fullScreen = isOn;
            PlayerPrefs.SetInt(PLAYER_PREFS_FULLSCREEN, isOn ? 1 : 0);
            PlayerPrefs.Save();
            UpdateVisual();
        });

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    private void Start()
    {
        LoadResolutions();

        // Fullscreen padrão e carregamento
        if (!PlayerPrefs.HasKey(PLAYER_PREFS_FULLSCREEN))
        {
            Screen.fullScreen = true;
            fullscreenToggle.isOn = true;
            PlayerPrefs.SetInt(PLAYER_PREFS_FULLSCREEN, 1);
        }
        else
        {
            bool full = PlayerPrefs.GetInt(PLAYER_PREFS_FULLSCREEN) == 1;
            Screen.fullScreen = full;
            fullscreenToggle.isOn = full;
        }

        UpdateVisual();
    }

    private void LoadResolutions()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        int savedResolutionIndex = PlayerPrefs.GetInt(PLAYER_PREFS_RESOLUTION, -1);
        int currentResolutionIndex = 0;

        var options = new System.Collections.Generic.List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            // Encontrar a resolução atual da tela
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        // Se primeira vez jogando, usa resolução atual
        if (savedResolutionIndex == -1)
        {
            resolutionDropdown.value = currentResolutionIndex;
            PlayerPrefs.SetInt(PLAYER_PREFS_RESOLUTION, currentResolutionIndex);
            PlayerPrefs.Save();
        }
        else
        {
            resolutionDropdown.value = savedResolutionIndex;
            ApplyResolution(savedResolutionIndex);
        }

        resolutionDropdown.RefreshShownValue();
    }

    private void OnResolutionChanged(int resolutionIndex)
    {
        ApplyResolution(resolutionIndex);
        PlayerPrefs.SetInt(PLAYER_PREFS_RESOLUTION, resolutionIndex);
        PlayerPrefs.Save();
    }

    private void ApplyResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    private void UpdateVisual()
    {
        soundEffectsText.text = "Sound Effects: " + Mathf.Round(SoundManager.Instance.GetVolume() * 10f);
        musicText.text = "Music: " + Mathf.Round(MusicManager.Instance.GetVolume() * 10f);

        fullscreenText.text = fullscreenToggle.isOn ? "Fullscreen: ON" : "Fullscreen: OFF";
    }


}
