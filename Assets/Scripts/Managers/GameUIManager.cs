using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private AimBarUIManager _aimBarUIManager;
    [SerializeField] private WindUIManager _windUIManager;

    [Header("Golf Ball HUD")]
    [SerializeField] private GameObject _golfBallHUD;

    [Header("Win Panels")]
    [SerializeField] private GameObject _mudMonsterWinPanel;
    [SerializeField] private GameObject _golfBallWinPanel;
    [SerializeField] private Image _restartButton;

    [Header("Join Players Panel")]
    [SerializeField] private GameObject _joinMenuPanel;
    [SerializeField] private Image _golfBallLaunchImage;
    [SerializeField] private Image _mudMonsterLaunchImage;

    [SerializeField] private Sprite _pressedSprite;

    void OnEnable()
    {
        GolfBallEvents.OnGolfBallAimStarted += HandleGolfAimStarted;
        GolfBallEvents.OnGolfBallLaunched += HandleGolfBallLaunched;

        MudEvents.OnMudMonsterShot += HandleMonsterShot;

        RoundEvents.OnGameOver += HandleGameOver;
    }

    void OnDisable()
    {
        GolfBallEvents.OnGolfBallAimStarted -= HandleGolfAimStarted;
        GolfBallEvents.OnGolfBallLaunched -= HandleGolfBallLaunched;

        MudEvents.OnMudMonsterShot -= HandleMonsterShot;

        RoundEvents.OnGameOver -= HandleGameOver;
    }

    public void InitializeUI()
    {
        _mudMonsterWinPanel.SetActive(false);
        _golfBallWinPanel.SetActive(false);
        _restartButton.gameObject.SetActive(false);
        _golfBallHUD.SetActive(false);
        _windUIManager.gameObject.SetActive(false);
        _aimBarUIManager.gameObject.SetActive(false);
    }

    public void AssignUIDevices(InputDevice mudMonsterDevice)
    {
        _windUIManager.InitializeDevice(mudMonsterDevice);
    }

    public void UpdateJoinedPlayerVisuals(int playerNumber)
    {
        Image targetImage = (playerNumber == 1) ? _golfBallLaunchImage : _mudMonsterLaunchImage;
        targetImage.sprite = _pressedSprite;
    }

    public void UpdateRestartVisuals()
    {
        _restartButton.sprite = _pressedSprite;
    }

    public void SetGolfHUDActive(bool isActive) => _golfBallHUD.SetActive(isActive);
    public void SetWindUIActive(bool isActive) => _windUIManager.gameObject.SetActive(isActive);
    public void HideJoinMenuPanel() => _joinMenuPanel.gameObject.SetActive(false);

    public void SetupMonsterAim()
    {
        _aimBarUIManager.gameObject.SetActive(true);
        _aimBarUIManager.ResetSystem();
    }

    public void HideMonsterAim() => _aimBarUIManager.gameObject.SetActive(false);

    public void SetGlobalGameplayUIActive(bool isActive)
    {
        _aimBarUIManager.gameObject.SetActive(isActive);
        _golfBallHUD.SetActive(false);
    }

    private void HandleGolfAimStarted() => SetGolfHUDActive(true);
    private void HandleGolfBallLaunched(Vector3 launchDir) => SetGolfHUDActive(false);
    private void HandleMonsterShot() => HideMonsterAim();

    private void HandleGameOver(RoundEvents.WinnerType winner)
    {
        if(winner == RoundEvents.WinnerType.GolfBall)
        {
            _golfBallWinPanel.SetActive(true);
        }
        if (winner == RoundEvents.WinnerType.MudMonster)
        {
            _mudMonsterWinPanel.SetActive(true);
        }

        _restartButton.gameObject.SetActive(true);
    }
}