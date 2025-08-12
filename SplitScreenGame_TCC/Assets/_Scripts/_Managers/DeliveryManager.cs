using System;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    [Serializable]
    public class PhaseConfig
    {
        public string phaseName;
        public List<RecipeSO> availableRecipes;
        public RecipeSO firstRecipe;
        public bool freezeTimerOnFirstRecipe; // Se true, congela o timer até o primeiro pedido ser entregue
    }

    public static DeliveryManager Instance { get; private set; }

    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSucess;
    public event EventHandler OnRecipeFailed;

    [Header("Configuração de Fases")]
    [SerializeField] private List<PhaseConfig> phasesConfig;
    [SerializeField] private string currentPhaseName;

    private List<RecipeSO> waitingRecipeSOList = new List<RecipeSO>();
    private float spawnRecipeTimer;
    [SerializeField] private float spawnRecipeTimerMax = 4f;
    [SerializeField] private int waitingRecipesMax = 4;
    private int successfulRecipesAmount;

    private bool firstRecipeDelivered = false;
    private PhaseConfig currentPhase;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        spawnRecipeTimer = spawnRecipeTimerMax;
        SetPhase(currentPhaseName);
    }

    public void SetPhase(string phaseName)
    {
        currentPhaseName = phaseName;
        currentPhase = phasesConfig.Find(p => p.phaseName == phaseName);

        waitingRecipeSOList.Clear();
        firstRecipeDelivered = false;

        if (currentPhase != null)
        {
            // Sempre começa com o primeiro pedido
            if (currentPhase.firstRecipe != null)
            {
                waitingRecipeSOList.Add(currentPhase.firstRecipe);
                OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
            }

            // Se a fase congela o timer até o primeiro pedido ser entregue
            if (currentPhase.freezeTimerOnFirstRecipe)
                KitchenGameManager.Instance.FreezePhaseTime();
            else
                KitchenGameManager.Instance.StartPhaseTime();
        }
        else
        {
            Debug.LogWarning($"Fase '{phaseName}' não encontrada na configuração do DeliveryManager!");
        }
    }

    private void Update()
    {
        if (currentPhase == null) return;

        // Se a fase congela spawns até o primeiro pedido ser entregue
        if (currentPhase.freezeTimerOnFirstRecipe && !firstRecipeDelivered)
            return;

        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (waitingRecipeSOList.Count < waitingRecipesMax && currentPhase.availableRecipes.Count > 0)
            {
                RecipeSO recipe = currentPhase.availableRecipes[UnityEngine.Random.Range(0, currentPhase.availableRecipes.Count)];
                waitingRecipeSOList.Add(recipe);
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
                bool matches = true;

                foreach (KitchenObjectSO recipeIngredient in waitingRecipeSO.kitchenObjectSOList)
                {
                    bool found = false;
                    foreach (KitchenObjectSO plateIngredient in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        if (plateIngredient == recipeIngredient)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found) { matches = false; break; }
                }

                if (matches)
                {
                    successfulRecipesAmount++;
                    waitingRecipeSOList.RemoveAt(i);

                    OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                    OnRecipeSucess?.Invoke(this, EventArgs.Empty);

                    // Se for o primeiro pedido de uma fase que congela timer, libera o tempo
                    if (currentPhase.freezeTimerOnFirstRecipe && !firstRecipeDelivered &&
                        waitingRecipeSO == currentPhase.firstRecipe)
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

    public List<RecipeSO> GetWaitingRecipeSOList() => waitingRecipeSOList;
    public int GetSuccessfulRecipesAmount() => successfulRecipesAmount;
}
