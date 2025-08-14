using System;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        Paused
    }

    private State state;
    private State previousState;
    private float waitingToStartTimer = 1f;
    private float countdownToStartTimer = 3f;
    private float gamePlayingTimer;
    public float gamePlayingTimerMax = 120f;

    private bool tutorialCompleted = false;
    private bool phaseTimeStarted = true;

    private void Awake()
    {
        Instance = this;
        state = State.WaitingToStart;
    }

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            // Log de pontuação
            ScoreManager.Instance.OnScoreChanged += (sender, args) =>
            {
                //Debug.Log($"[KitchenGameManager] Pontos: {ScoreManager.Instance.GetScore()} / {ScoreManager.Instance.GetScoreGoal()}");
            };

            // Ao atingir o objetivo
            ScoreManager.Instance.OnGoalReached += (sender, args) =>
            {
                Debug.Log("[KitchenGameManager] Objetivo de pontos atingido! Encerrando a fase...");
                state = State.GameOver;
                OnStateChanged?.Invoke(this, EventArgs.Empty);
            };
        }
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
                if (countdownToStartTimer <= 0f)
                {
                    countdownToStartTimer = 0f;
                    state = State.GamePlaying;
                    gamePlayingTimer = gamePlayingTimerMax;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;

            case State.GamePlaying:
                if (!phaseTimeStarted) return;
                gamePlayingTimer -= Time.deltaTime;
                if (gamePlayingTimer <= 0f)
                {
                    gamePlayingTimer = 0f;
                    state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
        }
    }

    public void FreezePhaseTime() => phaseTimeStarted = false;
    public void StartPhaseTime() => phaseTimeStarted = true;

    public void CompleteTutorial()
    {
        tutorialCompleted = true;
        IsTutorialActive = false;
    }

    public bool IsGamePlaying() => state == State.GamePlaying;
    public bool IsCountdownToStartActive() => state == State.CountdownToStart;
    public float GetCountdownToStartTimer() => countdownToStartTimer;
    public bool IsGameOver() => state == State.GameOver;
    public float GetGamePlayingTimerNormalized() => 1 - (gamePlayingTimer / gamePlayingTimerMax);

    public void GoBackToMainMenu() => SceneManager.LoadScene("MainMenu");
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TogglePauseGame()
    {
        if (state == State.Paused) ResumeGame();
        else PauseGame();
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

    public bool IsGamePaused() => state == State.Paused;

    public bool CanPlayersMove()
    {
        // Jogadores só podem se mover se o estado é GamePlaying
        // Não importa se o timer da fase está congelado — ainda podem se mover nesse caso
        return state == State.GamePlaying;
    }
}
