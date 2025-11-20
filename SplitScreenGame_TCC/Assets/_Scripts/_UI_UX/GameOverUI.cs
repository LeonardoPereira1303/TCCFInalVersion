using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ordersDeliveredText;

    [Space(10)]
    [Header("Panels")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;

    [Space(10)]
    [Header("Buttons")]
    [SerializeField] private Button victoryFirstButton;
    [SerializeField] private Button defeatFirstButton;

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

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(victoryFirstButton.gameObject);
    }

    private void ShowDefeatPanel()
    {
        HideAll();
        defeatPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(defeatFirstButton.gameObject);
    }

    private void HideAll()
    {
        victoryPanel.SetActive(false);
        defeatPanel.SetActive(false);
        gameObject.SetActive(true);
    }
}
