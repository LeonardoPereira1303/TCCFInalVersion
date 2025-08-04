using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    [SerializeField] private string deviceName = "Keyboard"; // "Keyboard", "Gamepad"

    public class InputActionEventArgs : EventArgs
    {
        public string Device { get; set; }
    }

    public event EventHandler<InputActionEventArgs> OnInteractAction;
    public event EventHandler<InputActionEventArgs> OnInteractAlternateAction;
    public event EventHandler<InputActionEventArgs> OnDashAction;
    public event EventHandler<InputActionEventArgs> OnPauseAction;

    private PlayerInputActions playerInputActions;
    private InputDevice assignedDevice;
public void InitializeInput()
    {
        playerInputActions = new PlayerInputActions();

        // Encontrar e vincular ao dispositivo apropriado
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
        };

        playerInputActions.Player.InteractAlternate.performed += ctx =>
        {
            if (ctx.control.device == assignedDevice)
                OnInteractAlternateAction?.Invoke(this, new InputActionEventArgs { Device = deviceName });
        };

        playerInputActions.Player.Dash.performed += ctx =>
        {
            if (ctx.control.device == assignedDevice)
                OnDashAction?.Invoke(this, new InputActionEventArgs { Device = deviceName });
        };

        playerInputActions.Player.Pause.performed += ctx =>
        {
            if (ctx.control.device == assignedDevice)
                OnPauseAction?.Invoke(this, new InputActionEventArgs { Device = deviceName });
        };
    }

    public Vector2 GetMovementVectorNormalized()
    {
        if (assignedDevice == null) return Vector2.zero;

        // Leitura baseada em dispositivo
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector.normalized;
    }

    private void OnDestroy()
    {
        if (playerInputActions != null)
        {
            playerInputActions.Dispose();
        }
    }
}
