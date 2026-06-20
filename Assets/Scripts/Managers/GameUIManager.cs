using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("Golf Ball HUD")]
    [SerializeField] private GameObject _golfBallHUD;

    [Header("Win Panels")]
    [SerializeField] private GameObject _mudMonsterWinPanel;
    [SerializeField] private GameObject _golfBallWinPanel;
    [SerializeField] private Image _restartButton;

    [Header("UI Managers")]
    [SerializeField] private AimBarUIManager _aimBarUIManager;
    [SerializeField] private WindUIManager _windUIManager;

    [Header("Join Players Panel")]
    [SerializeField] private GameObject _joinMenuPanel;
    [SerializeField] private Image _golfBallLaunchImage;
    [SerializeField] private Image _mudMonsterLaunchImage;
    [SerializeField] private Sprite _pressedSprite;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

    public void ShowMonsterWin()
    {
        _mudMonsterWinPanel.SetActive(true);
        _restartButton.gameObject.SetActive(true);

        SoundManager.StopAllSounds();
        SoundManager.StopMainMusic();
        SoundManager.PlaySound(SoundType.Music_MudMonsterWins, 0.3f);
    }

    public void ShowGolfWin() 
    {
        _golfBallWinPanel.SetActive(true);
        _restartButton.gameObject.SetActive(true);

        SoundManager.StopAllSounds();
        SoundManager.StopMainMusic();
        SoundManager.PlaySound(SoundType.Music_GolfBallWins, 0.3f);
    }  

    public void SetWindUIActive(bool isActive) => _windUIManager.gameObject.SetActive(isActive);

    public void SetupMonsterAim()
    {
        _aimBarUIManager.gameObject.SetActive(true);
        _aimBarUIManager.ResetSystem();
    }

    public void HideMonsterAim() => _aimBarUIManager.gameObject.SetActive(false);

    public void HideJoinMenuPanel() => _joinMenuPanel.gameObject.SetActive(false);

    public void SetGlobalGameplayUIActive(bool isActive)
    {
        _aimBarUIManager.gameObject.SetActive(isActive);
        _golfBallHUD.SetActive(false);
    }
}