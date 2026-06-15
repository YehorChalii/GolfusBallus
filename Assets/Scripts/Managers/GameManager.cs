using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Pending, RainSetup, GolfTurn, WindSelect, MonsterTurn, Resolution }
    private GameState _currentState = GameState.Pending;
    private GameState _pendingState = GameState.Pending;

    [Header("References")]
    [SerializeField] private CameraController _cameraController;

    [Header("Players")]
    [SerializeField] private GolfBallController _golfBall;
    [SerializeField] private MudMonsterController _mudMonster;

    [Header("Managers")]
    [SerializeField] private EnvironmentManager _environmentManager;
    [SerializeField] private GameUIManager _gameUIManager;
    [SerializeField] private WindUIManager _windUIManager;

    [Header("Mud Monster Rounds Settings")]
    [SerializeField] private int _roundsPerWindChange;
    private int _completedRounds = 0;

    private Vector3 _currentWindDirection;

    [Header("Loading Settings")]
    [SerializeField] private float _landedDelay;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        _cameraController.OnTransitionFinished += HandleCameraTransitionComplete;

        GolfBallEvents.OnGolfBallStopped += HandleBallStopped;

        RoundEvents.OnRainSetupComplete += HandleRainSetupComplete;
        RoundEvents.OnWindConfirmed += HandleWindConfirmed;

        MudEvents.OnMudPuddleLand += HandleMudPuddleLand;

        RoundEvents.OnGameOver += HandleGameOver;
        RoundEvents.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        _cameraController.OnTransitionFinished -= HandleCameraTransitionComplete;

        GolfBallEvents.OnGolfBallStopped -= HandleBallStopped;

        RoundEvents.OnRainSetupComplete -= HandleRainSetupComplete;
        RoundEvents.OnWindConfirmed -= HandleWindConfirmed;

        MudEvents.OnMudPuddleLand -= HandleMudPuddleLand;

        RoundEvents.OnGameOver -= HandleGameOver;
        RoundEvents.OnGameOver -= HandleGameOver;
    }

    public void InitializeAssignedPlayers(InputDevice player1Device, InputDevice player2Device)
    {
        _golfBall.InitializeDevice(player1Device);
        _cameraController.InitializeDevice(player1Device);
        _mudMonster.InitializeDevice(player2Device);
        _gameUIManager.AssignUIDevices(player2Device);

        StartGame();
    }

    private void Start()
    {
        SetGameplaySystemsActive(false);
        _gameUIManager.InitializeUI();
    }

    public void StartGame()
    {
        _currentState = GameState.RainSetup;
        _cameraController.ChangeCameraMode(CameraController.CameraState.MudMonster);
        _environmentManager.SpawnRainProjectiles();
    }

    private void HandleRainSetupComplete()
    {
        if (_currentState != GameState.RainSetup) return;
        StartCoroutine(DelayedInitiateGolfTurn());
    }

    private void HandleMudPuddleLand()
    {
        if (_currentState == GameState.MonsterTurn)
        {
            _mudMonster.enabled = false;
            StartCoroutine(DelayedInitiateGolfTurn());
        }
    }

    private IEnumerator DelayedInitiateGolfTurn()
    {
        yield return new WaitForSeconds(_landedDelay);
        InitiateGolfTurnTransition();
    }

    private void InitiateGolfTurnTransition()
    {
        if (_currentState == GameState.Resolution) return;
        SetGameplaySystemsActive(false);
        _pendingState = GameState.GolfTurn;
        _cameraController.ChangeCameraMode(CameraController.CameraState.GolfBall);
    }

    public void HandleBallStopped()
    {
        if (_currentState != GameState.GolfTurn) return;
        SetGameplaySystemsActive(false);
        _completedRounds++;

        if (_completedRounds % _roundsPerWindChange == 0) _pendingState = GameState.WindSelect;
        else _pendingState = GameState.MonsterTurn;

        _cameraController.ChangeCameraMode(CameraController.CameraState.MudMonster);
    }

    private void HandleCameraTransitionComplete()
    {
        if (_currentState == GameState.Resolution) return;

        switch (_pendingState)
        {
            case GameState.GolfTurn: ExecuteGolfTurn(); break;
            case GameState.WindSelect: ExecuteWindSelectPhase(); break;
            case GameState.MonsterTurn: ExecuteMonsterTurn(); break;
        }
    }

    private void ExecuteGolfTurn()
    {
        _currentState = GameState.GolfTurn;
        _golfBall.enabled = true;
        _gameUIManager.SetGolfHUDActive(true);
    }

    private void ExecuteWindSelectPhase()
    {
        _currentState = GameState.WindSelect;
        _gameUIManager.SetWindUIActive(true);
    }

    private void HandleWindConfirmed(Vector3 chosenDirection)
    {
        _currentWindDirection = chosenDirection;

        _golfBall.UpdateWindDirection(_currentWindDirection);
        _mudMonster.UpdateWindDirection(_currentWindDirection);

        _gameUIManager.SetWindUIActive(false);
        _environmentManager.ApplyWindParticlesToggle(chosenDirection);

        ExecuteMonsterTurn();
    }

    private void ExecuteMonsterTurn()
    {
        _currentState = GameState.MonsterTurn;
        PrepareMonsterShot();
    }

    private void PrepareMonsterShot()
    {
        _mudMonster.enabled = true;
        _gameUIManager.SetupMonsterAim();
    }

    private void HandleGameOver(RoundEvents.WinnerType winner)
    {
        _currentState = GameState.Resolution;
        SetGameplaySystemsActive(false);
    }

    private void SetGameplaySystemsActive(bool isActive)
    {
        _golfBall.enabled = isActive;
        _mudMonster.enabled = isActive;
        _gameUIManager.SetGlobalGameplayUIActive(isActive);
    }
}