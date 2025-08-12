using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSucess;
    public event EventHandler OnRecipeFailed;

    public static DeliveryManager Instance { get; private set; }

    [Header("Receitas")]
    [SerializeField] private RecipeListSO recipeListSO;
    [SerializeField] private RecipeSO firstTutorialRecipeSO; // Pedido inicial obrigatório

    private List<RecipeSO> waitingRecipeSOList;

    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipesMax = 4;
    private int successfulRecipesAmount;

    private bool firstRecipeDelivered = false; // Controle do primeiro pedido entregue

    private void Awake()
    {
        Instance = this;
        waitingRecipeSOList = new List<RecipeSO>();
    }

    private void Start()
    {
        // Gera apenas o primeiro pedido no início
        waitingRecipeSOList.Add(firstTutorialRecipeSO);
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        // Bloqueia spawn se o tutorial estiver ativo ou o primeiro pedido não foi entregue
        if (KitchenGameManager.Instance.IsTutorialActive || !firstRecipeDelivered)
            return;

        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (waitingRecipeSOList.Count < waitingRecipesMax)
            {
                RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)];
                waitingRecipeSOList.Add(waitingRecipeSO);
                OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                bool plateContentsMatchesRecipe = true;
                foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList)
                {
                    bool ingredientFound = false;
                    foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        if (plateKitchenObjectSO == recipeKitchenObjectSO)
                        {
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound)
                    {
                        plateContentsMatchesRecipe = false;
                    }
                }

                if (plateContentsMatchesRecipe)
                {
                    successfulRecipesAmount++;
                    waitingRecipeSOList.RemoveAt(i);

                    OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                    OnRecipeSucess?.Invoke(this, EventArgs.Empty);

                    // Marca o primeiro pedido como entregue e libera o tempo da fase
                    if (!firstRecipeDelivered && waitingRecipeSO == firstTutorialRecipeSO)
                    {
                        firstRecipeDelivered = true;
                        KitchenGameManager.Instance.StartPhaseTime();
                    }

                    return;
                }
            }
        }

        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipeSOList;
    }

    public int GetSuccessfulRecipesAmount()
    {
        return successfulRecipesAmount;
    }
}
