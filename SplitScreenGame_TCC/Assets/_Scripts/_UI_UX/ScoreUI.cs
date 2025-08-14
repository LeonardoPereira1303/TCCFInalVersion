using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText; // Arraste o TMP_Text do Canvas aqui

    private void Start()
    {
        // Inscreve no evento de mudança de score
        ScoreManager.Instance.OnScoreChanged += ScoreManager_OnScoreChanged;

        // Atualiza imediatamente no início
        UpdateScoreText();
    }

    private void ScoreManager_OnScoreChanged(object sender, System.EventArgs e)
    {
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        int currentScore = ScoreManager.Instance.GetScore();
        int goal = ScoreManager.Instance.GetScoreGoal();
        scoreText.text = $"Score: {currentScore} / {goal}";
    }

    private void OnDestroy()
    {
        // Evita memory leaks removendo a inscrição no evento
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= ScoreManager_OnScoreChanged;
    }
}
