using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GamePlayingClockUI : MonoBehaviour
{
    [SerializeField] private Image timerImage;
    [SerializeField] private TextMeshProUGUI timerText;

    private bool halfwaySoundPlayed = false;
    private bool finalSecondsSoundPlayed = false;

    private void Update()
    {
        // Garante que só toca quando o jogo está em execução (não no menu, pausa ou contagem inicial)
        if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            ResetFlags(); // reseta quando o jogo ainda não começou
            return;
        }

        float normalizedTime = KitchenGameManager.Instance.GetGamePlayingTimerNormalized();
        timerImage.fillAmount = normalizedTime;

        float totalTime = KitchenGameManager.Instance.gamePlayingTimerMax;
        float remainingTime = totalTime * (1 - normalizedTime);

        // 🔸 Toca na metade do tempo apenas 1 vez
        if (!halfwaySoundPlayed && Mathf.Abs(remainingTime - totalTime / 2f) < Time.deltaTime)
        {
            halfwaySoundPlayed = true;
            SoundManager.Instance.PlayTimerHalfwaySound(Camera.main.transform.position);
        }

        // 🔸 Toca nos 5 segundos finais apenas 1 vez
        if (!finalSecondsSoundPlayed && remainingTime <= 5f + Time.deltaTime && remainingTime > 5f - Time.deltaTime)
        {
            finalSecondsSoundPlayed = true;
            SoundManager.Instance.PlayTimerFinalSound(Camera.main.transform.position);
        }

        // Formata o tempo
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void ResetFlags()
    {
        halfwaySoundPlayed = false;
        finalSecondsSoundPlayed = false;
    }
}
