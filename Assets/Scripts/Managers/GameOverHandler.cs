using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameOverHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameUIManager _gameUIManager;

    [Header("Scene Settings")]
    [SerializeField] private int _gameplaySceneIndex = 1;
    [SerializeField] private float _restartDelay;

    private InputSystem_Actions _controls;
    private bool _canRestart = false;
    private bool _isTransitioning = false;

    void Awake()
    {
        _controls = new InputSystem_Actions();
    }

    void OnEnable()
    {
        RoundEvents.OnGameOver += EnableRestart;
        _controls.Menu.Confirm.started += OnRestartPressed;
    }

    void OnDisable()
    {
        RoundEvents.OnGameOver -= EnableRestart;
        _controls.Menu.Confirm.started -= OnRestartPressed;
        _controls.Menu.Disable();
    }

    private void EnableRestart(RoundEvents.WinnerType winner)
    {
        _canRestart = true;
        _controls.Menu.Enable();
    }

    private void OnRestartPressed(InputAction.CallbackContext ctx)
    {
        if (!_canRestart || _isTransitioning) return;

        StartCoroutine(RestartSequence());
    }

    private IEnumerator RestartSequence()
    {
        _isTransitioning = true;
        _canRestart = false;
        _controls.Menu.Disable();

        _gameUIManager.UpdateRestartVisuals();

        yield return new WaitForSeconds(_restartDelay);

        SceneManager.LoadScene(_gameplaySceneIndex);
    }
}