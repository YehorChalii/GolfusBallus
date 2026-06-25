using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static Action OnGameOver;

    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, RainSetup, MonsterSetup, GolfTurn, WindSelect, MonsterTurn, Resolution }
    private GameState _currentState = GameState.MainMenu;
    private GameState _pendingState = GameState.MainMenu;

    [Header("Systems")]
    [SerializeField] private EnvironmentManager _environmentManager;
    [SerializeField] private CameraController _cameraController;

    [Header("Players")]
    [SerializeField] private GolfBallController _golfBall;
    [SerializeField] private MudMonsterController _mudMonster;

    private int _rainLandedCount = 0;

    [Header("Monster Setup Settings")]
    [SerializeField] private int _monsterSetupShots;
    private int _monsterSetupFired = 0;

    [Header("Wind Settings")]
    [SerializeField] private int _roundsPerWindChange;
    private int _completedRounds = 0;
    public Vector3 CurrentWindDirection { get; private set; }

    [Header("Settings")]
    [SerializeField] private float _landedDelay;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        WindUIManager.OnDirectionConfirmed += HandleWindSelectionConfirmed;
    }

    private void OnEnable()
    {
        MudProjectile.OnMudPuddleLand += HandlePuddleLanded;
        CameraController.OnTransitionFinished += HandleCameraTransitionComplete;

        GolfBallController.OnBallStoppedMoving += OnBallStopped;
        GolfBallController.OnBallLose += OnGolfBallLose;
        GolfBallController.OnBallHitMonster += OnMudMonsterLose;
    }

    private void OnDisable()
    {
        MudProjectile.OnMudPuddleLand -= HandlePuddleLanded;
        CameraController.OnTransitionFinished -= HandleCameraTransitionComplete;

        GolfBallController.OnBallStoppedMoving -= OnBallStopped;
        GolfBallController.OnBallLose -= OnGolfBallLose;
        GolfBallController.OnBallHitMonster -= OnMudMonsterLose;
    }

    private void OnDestroy()
    {
        WindUIManager.OnDirectionConfirmed -= HandleWindSelectionConfirmed;
    }

    public void InitializeAssignedPlayers(InputDevice player1Device, InputDevice player2Device)
    {
        _golfBall.InitializeDevice(player1Device);
        _cameraController.InitializeDevice(player1Device);
        _mudMonster.InitializeDevice(player2Device);

        GameUIManager.Instance.AssignUIDevices(player2Device);

        StartGame();
    }

    private void Start()
    {
        SetGameplaySystemsActive(false);
        GameUIManager.Instance.InitializeUI();
    }

    public void StartGame()
    {
        _currentState = GameState.RainSetup;
        _rainLandedCount = 0;
        _cameraController.ChangeCameraMode(CameraController.CameraState.MudMonster);
        _environmentManager.SpawnRainProjectiles();

        SoundManager.PlayMainMusic();
    }

    private void HandlePuddleLanded()
    {
        if (_currentState == GameState.RainSetup)
        {
            _rainLandedCount++;
            if (_rainLandedCount >= _environmentManager.RainProjectileCount)
            {
                StartMonsterSetup();
            }
        }
        else if (_currentState == GameState.MonsterSetup)
        {
            _monsterSetupFired++;
            if (_monsterSetupFired < _monsterSetupShots)
            {
                PrepareMonsterShot();
            }
            else
            {
                StartCoroutine(DelayedInitiateGolfTurn());
            }
        }
        else if (_currentState == GameState.MonsterTurn)
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

    public void OnBallStopped()
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
        GameUIManager.Instance.SetGolfHUDActive(true);
    }

    private void ExecuteWindSelectPhase()
    {
        _currentState = GameState.WindSelect;
        GameUIManager.Instance.SetWindUIActive(true);
    }

    private void HandleWindSelectionConfirmed(Vector3 chosenDirection)
    {
        CurrentWindDirection = chosenDirection;
        _golfBall.UpdateWindDirection(CurrentWindDirection);
        GameUIManager.Instance.SetWindUIActive(false);

        _environmentManager.ApplyWindParticlesToggle(chosenDirection);
        ExecuteMonsterTurn();
    }

    private void ExecuteMonsterTurn()
    {
        _currentState = GameState.MonsterTurn;
        PrepareMonsterShot();
    }

    private void StartMonsterSetup()
    {
        _currentState = GameState.MonsterSetup;
        _monsterSetupFired = 0;
        PrepareMonsterShot();
    }

    private void PrepareMonsterShot()
    {
        _mudMonster.enabled = true;
        GameUIManager.Instance.SetupMonsterAim();
    }

    public void OnMonsterShoot() => GameUIManager.Instance.HideMonsterAim();

    public void OnGolfBallLose() { GameUIManager.Instance.ShowMonsterWin(); EndGame(); }
    public void OnMudMonsterLose() { GameUIManager.Instance.ShowGolfWin(); EndGame(); }

    private void EndGame()
    {
        _currentState = GameState.Resolution;
        SetGameplaySystemsActive(false);

        OnGameOver?.Invoke();
    }

    private void SetGameplaySystemsActive(bool isActive)
    {
        _golfBall.enabled = isActive;
        _mudMonster.enabled = isActive;
        GameUIManager.Instance.SetGlobalGameplayUIActive(isActive);
    }
}