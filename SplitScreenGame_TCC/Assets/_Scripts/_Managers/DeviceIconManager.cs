using UnityEngine;
using UnityEngine.UI;

public class DeviceIconManager : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Image targetImage;

    [Header("Ícones")]
    [SerializeField] private Sprite keyboardIcon;
    [SerializeField] private Sprite xboxIcon;
    [SerializeField] private Sprite playstationIcon;

    private void Start()
    {
        if (gameInput != null)
        {
            gameInput.OnDeviceChanged += UpdateIcon;
            UpdateIcon(gameInput.GetCurrentDeviceType());
        }
    }

    private void OnDestroy()
    {
        if (gameInput != null)
            gameInput.OnDeviceChanged -= UpdateIcon;
    }

    private void UpdateIcon(GameInput.DeviceType type)
    {
        switch (type)
        {
            case GameInput.DeviceType.KeyboardMouse:
                targetImage.sprite = keyboardIcon;
                break;
            case GameInput.DeviceType.GamepadXbox:
                targetImage.sprite = xboxIcon;
                break;
            case GameInput.DeviceType.GamepadPlayStation:
                targetImage.sprite = playstationIcon;
                break;
        }
    }
}
