using UnityEngine;
using TMPro;
using System;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ordersDeliveredText;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;

    private void Start()
    {
        KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;

        KitchenGameManager.Instance.OnGameWon += (sender, e) => ShowVictoryPanel();
        KitchenGameManager.Instance.OnGameLost += (sender, e) => ShowDefeatPanel();

        HideAll();
    }

    private void KitchenGameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (KitchenGameManager.Instance.IsGameOver())
        {
            ordersDeliveredText.text = DeliveryManager.Instance.GetSuccessfulRecipesAmount().ToString();
        }
        else
        {
            HideAll();
        }
    }

    private void ShowVictoryPanel()
    {
        HideAll();
        victoryPanel.SetActive(true);
    }

    private void ShowDefeatPanel()
    {
        HideAll();
        defeatPanel.SetActive(true);
    }

    private void HideAll()
    {
        victoryPanel.SetActive(false);
        defeatPanel.SetActive(false);
        gameObject.SetActive(true);
    }
}
