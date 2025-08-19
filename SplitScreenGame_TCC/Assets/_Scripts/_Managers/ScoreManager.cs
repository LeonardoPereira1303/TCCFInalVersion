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

        // Nunca deixar pontuação negativa
        if (currentScore < 0)
            currentScore = 0;

        OnScoreChanged?.Invoke(this, EventArgs.Empty);

        if (currentScore >= scoreGoal)
        {
            OnGoalReached?.Invoke(this, EventArgs.Empty);
        }
    }

    public int GetScore() => currentScore;
    public int GetScoreGoal() => scoreGoal;
}
