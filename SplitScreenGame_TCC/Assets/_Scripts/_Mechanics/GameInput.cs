using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    [SerializeField] private string deviceName = "Keyboard"; // "Keyboard" ou "Gamepad"

    public class InputActionEventArgs : EventArgs
    {
        public string Device { get; set; }
    }

    public enum DeviceType
    {
        KeyboardMouse,
        GamepadXbox,
        GamepadPlayStation,
        Unknown
    }

    public event EventHandler<InputActionEventArgs> OnInteractAction;
    public event EventHandler<InputActionEventArgs> OnInteractAlternateAction;
    public event EventHandler<InputActionEventArgs> OnDashAction;
    public event EventHandler<InputActionEventArgs> OnPauseAction;

    public event Action<DeviceType> OnDeviceChanged;

    private PlayerInputActions playerInputActions;
    private InputDevice assignedDevice;
    private DeviceType currentDeviceType = DeviceType.KeyboardMouse;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pauseSound;

    public void InitializeInput()
    {
        playerInputActions = new PlayerInputActions();

        if (deviceName.ToLower() == "keyboard")
        {
            assignedDevice = Keyboard.current;
        }
        else if (deviceName.ToLower() == "gamepad")
        {
            assignedDevice = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;
        }

        if (assignedDevice != null)
        {
            playerInputActions.devices = new[] { assignedDevice };
        }

        playerInputActions.Player.Enable();

        playerInputActions.Player.Interact.performed += ctx =>
        {
            if (ctx.control.device == assignedDevice)
                OnInteractAction?.Invoke(this, new InputActionEventArgs { Device = deviceName });

            CheckDeviceChange(ctx.control.device);
        };

        playerInputActions.Player.InteractAlternate.performed += ctx =>
        {
            if (ctx.control.device == assignedDevice)
                OnInteractAlternateAction?.Invoke(this, new InputActionEventArgs { Device = deviceName });

            CheckDeviceChange(ctx.control.device);
        };

        playerInputActions.Player.Dash.performed += ctx =>
        {
            if (ctx.control.device == assignedDevice)
                OnDashAction?.Invoke(this, new InputActionEventArgs { Device = deviceName });

            CheckDeviceChange(ctx.control.device);
        };

        playerInputActions.Player.Pause.performed += ctx =>
        {
            if (ctx.control.device == assignedDevice)
            {
                OnPauseAction?.Invoke(this, new InputActionEventArgs { Device = deviceName });
                if (audioSource != null && pauseSound != null)
                {
                    audioSource.PlayOneShot(pauseSound);
                }
            }

            CheckDeviceChange(ctx.control.device);
        };
    }

    private void CheckDeviceChange(InputDevice device)
    {
        if (device == null) return;

        DeviceType newDeviceType = GetDeviceType(device);

        if (newDeviceType != currentDeviceType)
        {
            currentDeviceType = newDeviceType;
            OnDeviceChanged?.Invoke(currentDeviceType);

            // Debug opcional
            Debug.Log("Novo dispositivo detectado: " + device.displayName + " -> " + newDeviceType);
        }
    }

    private DeviceType GetDeviceType(InputDevice device)
    {
        if (device is Keyboard || device is Mouse)
            return DeviceType.KeyboardMouse;

        if (device is Gamepad gamepad)
        {
            string name = gamepad.displayName.ToLower();

            // Detectar Xbox
            if (name.Contains("xbox"))
                return DeviceType.GamepadXbox;

            // Detectar controles PlayStation corretamente
            if (name.Contains("dualshock") ||
                name.Contains("playstation") ||
                name.Contains("wireless controller") ||  // Nome comum do DualShock 4
                name.Contains("sony") ||                 // Segurança extra
                name.Contains("dualsense"))              // Caso apareça como PS5
            {
                return DeviceType.GamepadPlayStation;
            }

            return DeviceType.Unknown;
        }

        return DeviceType.Unknown;
    }

    public DeviceType GetCurrentDeviceType() => currentDeviceType;

    public Vector2 GetMovementVectorNormalized()
    {
        if (assignedDevice == null) return Vector2.zero;
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector.normalized;
    }

    private void OnDestroy()
    {
        if (playerInputActions != null)
            playerInputActions.Dispose();
    }
}
