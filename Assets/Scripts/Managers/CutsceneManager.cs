using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private int _gameplaySceneIndex = 1;

    [Header("Cutscene Sequence")]
    [SerializeField] private List<CanvasGroup> _cutscenePanels;
    [SerializeField] private float _timePerPanel;

    [Header("Fade Settings")]
    [SerializeField] private float _fadeDuration;

    private InputSystem_Actions _controls;
    private Coroutine _cutsceneSequenceCoroutine;
    private bool _isTransitioning = false;

    private void Awake()
    {
        _controls = new InputSystem_Actions();
    }

    void OnEnable()
    {
        _controls.Menu.Confirm.started += OnSkipPressed;
        _controls.Menu.Enable();
    }

    void OnDisable()
    {
        if (_controls != null)
        {
            _controls.Menu.Confirm.started -= OnSkipPressed;
            _controls.Menu.Disable();
        }
    }

    void Start()
    {
        foreach (CanvasGroup panel in _cutscenePanels)
        {
            panel.alpha = 0f;
            panel.gameObject.SetActive(true);
        }

        _cutsceneSequenceCoroutine = StartCoroutine(PlayCutsceneSequence());
    }

    private IEnumerator PlayCutsceneSequence()
    {
        for (int i = 0; i < _cutscenePanels.Count; i++)
        {
            CanvasGroup currentPanel = _cutscenePanels[i];
            if (currentPanel == null) continue;

            yield return StartCoroutine(FadeCanvasGroup(currentPanel, 0f, 1f));

            yield return new WaitForSeconds(_timePerPanel);

            yield return StartCoroutine(FadeCanvasGroup(currentPanel, 1f, 0f));
        }

        ProceedToGame();
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / _fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    private void OnSkipPressed(InputAction.CallbackContext ctx)
    {
        ProceedToGame();
    }

    private void ProceedToGame()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;

        if (_cutsceneSequenceCoroutine != null)
        {
            StopCoroutine(_cutsceneSequenceCoroutine);
        }

        SceneManager.LoadScene(_gameplaySceneIndex);
    }
}