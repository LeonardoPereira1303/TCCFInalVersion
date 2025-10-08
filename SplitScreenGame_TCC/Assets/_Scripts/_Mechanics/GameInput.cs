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

    // Novo evento de mudança de dispositivo
    public event Action<DeviceType> OnDeviceChanged;

    private PlayerInputActions playerInputActions;
    private InputDevice assignedDevice;
    private DeviceType currentDeviceType = DeviceType.KeyboardMouse;

    public void InitializeInput()
    {
        playerInputActions = new PlayerInputActions();

        // Detecta e vincula o dispositivo inicial
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
                OnPauseAction?.Invoke(this, new InputActionEventArgs { Device = deviceName });

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
        }
    }

    private DeviceType GetDeviceType(InputDevice device)
    {
        if (device is Keyboard || device is Mouse)
            return DeviceType.KeyboardMouse;

        if (device is Gamepad gamepad)
        {
            string name = gamepad.displayName.ToLower();
            if (name.Contains("xbox"))
                return DeviceType.GamepadXbox;
            if (name.Contains("dualshock") || name.Contains("playstation"))
                return DeviceType.GamepadPlayStation;
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
