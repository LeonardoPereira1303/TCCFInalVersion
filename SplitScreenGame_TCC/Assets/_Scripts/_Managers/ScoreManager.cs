using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public event EventHandler OnScoreChanged;
    public event EventHandler OnGoalReached;

    [SerializeField] private int scoreGoal = 50; // Pontos necessários para vencer (teste)
    private int currentScore = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        OnScoreChanged?.Invoke(this, EventArgs.Empty);

        Debug.Log($"[ScoreManager] Pontos atuais: {currentScore}");

        if (currentScore >= scoreGoal)
        {
            Debug.Log("[ScoreManager] Objetivo de pontos alcançado!");
            OnGoalReached?.Invoke(this, EventArgs.Empty);
        }
    }

    public int GetScore() => currentScore;
    public int GetScoreGoal() => scoreGoal;
}
