using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerInputAssigner : MonoBehaviour
{
    [SerializeField] private float _matchStartDelay;

    private InputDevice _player1Device;
    private InputDevice _player2Device;

    private bool _player1Connected = false;
    private bool _player2Connected = false;

    private InputSystem_Actions _controls;

    void OnEnable()
    {
        _controls = new InputSystem_Actions();
        _controls.Menu.Confirm.started += OnPlayerJoined;
        _controls.Menu.Enable();
    }

    void OnDisable()
    {
        if (_controls != null)
        {
            _controls.Menu.Confirm.started -= OnPlayerJoined;
            _controls.Menu.Disable();
        }
    }

    private void OnPlayerJoined(InputAction.CallbackContext ctx)
    {
        if (ctx.control == null) return;

        InputDevice device = ctx.control.device;
        if (!(device is Gamepad)) return;

        if (!_player1Connected)
        {
            _player1Device = device;
            _player1Connected = true;
            Debug.Log($"Player 1 (Golf Ball) Connected via device: {device.name}");

            GameUIManager.Instance.UpdateJoinedPlayerVisuals(1);
            SoundManager.PlayUISound();

            return;
        }

        if (!_player2Connected && device != _player1Device)
        {
            _player2Device = device;
            _player2Connected = true;
            Debug.Log($"Player 2 (Mud Monster) Connected via device: {device.name}");

            GameUIManager.Instance.UpdateJoinedPlayerVisuals(2);
            SoundManager.PlayUISound();

            StartCoroutine(DelayedStartMatch());
        }
    }

    private IEnumerator DelayedStartMatch()
    {
        if (_controls != null) _controls.Menu.Disable();

        yield return new WaitForSecondsRealtime(_matchStartDelay);

        StartMatch();
    }

    private void StartMatch()
    {
        GameUIManager.Instance.HideJoinMenuPanel();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.InitializeAssignedPlayers(_player1Device, _player2Device);
        }

        Destroy(gameObject);
    }
}