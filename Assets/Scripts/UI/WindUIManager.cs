using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class WindUIManager : MonoBehaviour
{
    public static Action<Vector3> OnDirectionConfirmed;

    [Header("Arrow Images")]
    [SerializeField] private Image _upArrow;
    [SerializeField] private Image _downArrow;
    [SerializeField] private Image _leftArrow;
    [SerializeField] private Image _rightArrow;

    [Header("Sprites")]
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _pressedSprite;
    [SerializeField] private Sprite _disabledSprite;

    private InputSystem_Actions _controls;

    private Vector3 _currentSelectedDirection = Vector3.zero;
    private Vector3 _lockedDirection = Vector3.zero;
    private Image _currentlyPressedImage;

    public void InitializeDevice(InputDevice assignedDevice)
    {
        _controls = new InputSystem_Actions();

        if (assignedDevice != null)
        {
            _controls.devices = new[] { assignedDevice };
        }

        _controls.Monster.WindSelect.performed += OnDpadPressed;
        _controls.Monster.Shoot.performed += OnConfirmPressed;
    }

    void OnEnable()
    {
        if (_controls != null)
        {
            _controls.Monster.Enable();
        }

        ResetSelectionState();
    }

    void OnDisable()
    {
        if (_controls != null)
        {
            _controls.Monster.Disable();
        }
    }

    private void ResetSelectionState()
    {
        SetImageSprite(_upArrow, _lockedDirection == -Vector3.forward ? _disabledSprite : _defaultSprite);
        SetImageSprite(_downArrow, _lockedDirection == -Vector3.back ? _disabledSprite : _defaultSprite);
        SetImageSprite(_leftArrow, _lockedDirection == -Vector3.left ? _disabledSprite : _defaultSprite);
        SetImageSprite(_rightArrow, _lockedDirection == -Vector3.right ? _disabledSprite : _defaultSprite);

        _currentSelectedDirection = Vector3.zero;
        _currentlyPressedImage = null;
    }

    private void OnDpadPressed(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            if (input.x > 0)
            {
                SetWindDirection(Vector3.right, _rightArrow);
            }
            else
            {
                SetWindDirection(Vector3.left, _leftArrow);
            }
        }
        else if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
        {
            if (input.y > 0)
            {
                SetWindDirection(Vector3.forward, _upArrow);
            }
            else
            {
                SetWindDirection(Vector3.back, _downArrow);
            }
        }
    }

    private void SetWindDirection(Vector3 direction, Image targetImage)
    {
        if (targetImage == null) return;

        if (-direction == _lockedDirection)
        {
            return;
        }

        SetImageSprite(_currentlyPressedImage, _defaultSprite);

        _currentSelectedDirection = -direction;
        _currentlyPressedImage = targetImage;

        SetImageSprite(_currentlyPressedImage, _pressedSprite);
    }

    private void SetImageSprite(Image image, Sprite targetSprite)
    {
        if (image != null && targetSprite != null)
        {
            image.sprite = targetSprite;
        }
    }

    private void OnConfirmPressed(InputAction.CallbackContext ctx)
    {
        if (_currentSelectedDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        OnDirectionConfirmed?.Invoke(_currentSelectedDirection.normalized);
        SoundManager.PlayMusic(SoundType.Music_Wind, 0.4f);

        _lockedDirection = _currentSelectedDirection;

        ResetSelectionState();
    }
}