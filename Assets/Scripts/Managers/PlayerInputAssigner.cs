using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputAssigner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private GameUIManager _gameUIManager;

    [Header("Settings")]
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

            _gameUIManager.UpdateJoinedPlayerVisuals(1);

            return;
        }

        if (!_player2Connected && device != _player1Device)
        {
            _player2Device = device;
            _player2Connected = true;
            Debug.Log($"Player 2 (Mud Monster) Connected via device: {device.name}");

            _gameUIManager.UpdateJoinedPlayerVisuals(2);

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
        _gameUIManager.HideJoinMenuPanel();
        _gameManager.InitializeAssignedPlayers(_player1Device, _player2Device);

        Destroy(gameObject);
    }
}