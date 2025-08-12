using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TMPro;

public class KitchenGameManager : MonoBehaviour
{
    public static KitchenGameManager Instance { get; private set; }
    public event EventHandler OnStateChanged;
    public bool IsTutorialActive { get; private set; } = true;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
        Paused,
    }

    private State state;
    private State previousState;
    private float waitingToStartTimer = 1f;
    private float countdownToStartTimer = 3f;
    private float gamePlayingTimer;
    public float gamePlayingTimerMax = 120f;

    private bool tutorialCompleted = false;
    private bool phaseTimeStarted = false; // Controle do início do tempo real da fase

    private void Awake()
    {
        Instance = this;
        state = State.WaitingToStart;
    }

    private void Update()
    {
        if (state == State.Paused) return;

        switch (state)
        {
            case State.WaitingToStart:
                waitingToStartTimer -= Time.deltaTime;
                if (waitingToStartTimer < 0f && tutorialCompleted)
                {
                    state = State.CountdownToStart;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;

            case State.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer < 0f)
                {
                    // garante valor não-negativo e notifica a UI da mudança de estado
                    countdownToStartTimer = 0f;
                    state = State.GamePlaying;
                    gamePlayingTimer = gamePlayingTimerMax;
                    OnStateChanged?.Invoke(this, EventArgs.Empty); // <-- CORREÇÃO: notifica a UI para esconder o contador
                }
                break;

            case State.GamePlaying:
                if (!phaseTimeStarted) return; // Congela o tempo até a liberação
                gamePlayingTimer -= Time.deltaTime;
                if (gamePlayingTimer < 0f)
                {
                    state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;

            case State.GameOver:
                break;
        }
    }

    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
    }

    public bool IsCountdownToStartActive()
    {
        return state == State.CountdownToStart;
    }

    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer;
    }

    public bool IsGameOver()
    {
        return state == State.GameOver;
    }

    public float GetGamePlayingTimerNormalized()
    {
        return 1 - (gamePlayingTimer / gamePlayingTimerMax);
    }

    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CompleteTutorial()
    {
        tutorialCompleted = true;
        IsTutorialActive = false;
    }

    public void StartPhaseTime()
    {
        phaseTimeStarted = true;
    }

    public void TogglePauseGame()
    {
        if (state == State.Paused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        previousState = state;
        state = State.Paused;
        Time.timeScale = 0f;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ResumeGame()
    {
        state = previousState;
        Time.timeScale = 1f;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsGamePaused()
    {
        return state == State.Paused;
    }

    public bool CanPlayersMove()
    {
        // Movimento só permitido quando estado é GamePlaying e a contagem regressiva já acabou
        return state == State.GamePlaying;
    }
}
