using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DeliveryManagerSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform iconContainer;
    [SerializeField] private Transform iconTemplate;
    [SerializeField] private Image timerBar;

    private DeliveryManager.WaitingRecipe currentRecipe;

    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
    }

    public void SetWaitingRecipe(DeliveryManager.WaitingRecipe waitingRecipe)
    {
        currentRecipe = waitingRecipe;
        recipeNameText.text = waitingRecipe.recipeSO.recipeName;

        foreach (Transform child in iconContainer)
        {
            if (child == iconTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (KitchenObjectSO kitchenObjectSO in waitingRecipe.recipeSO.kitchenObjectSOList)
        {
            Transform iconTransform = Instantiate(iconTemplate, iconContainer);
            iconTransform.gameObject.SetActive(true);
            iconTransform.GetComponent<Image>().sprite = kitchenObjectSO.sprite;
        }
    }

    private void Update()
    {
        if (currentRecipe != null)
        {
            float normalized = currentRecipe.timer / DeliveryManager.Instance.GetRecipeMaxTime();
            timerBar.fillAmount = normalized;

            if (normalized > 0.5f) timerBar.color = Color.green;
            else if (normalized > 0.25f) timerBar.color = Color.yellow;
            else timerBar.color = Color.red;
        }
    }
}
