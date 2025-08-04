using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class GamePlayingClockUI : MonoBehaviour
{
    [SerializeField] private Image timerImage;
    [SerializeField] private TextMeshProUGUI timerText;

    private void Update() {
        float normalizedTime = KitchenGameManager.Instance.GetGamePlayingTimerNormalized();
        timerImage.fillAmount = KitchenGameManager.Instance.GetGamePlayingTimerNormalized();

        float remainingTime = KitchenGameManager.Instance.gamePlayingTimerMax * (1 - normalizedTime);

        // Formata o tempo em minutos e segundos
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
