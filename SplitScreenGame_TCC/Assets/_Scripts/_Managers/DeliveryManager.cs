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
        public bool freezeTimerOnFirstRecipe;
        public float spawnInterval = 8f;
    }

    [Serializable]
    public class WaitingRecipe
    {
        public RecipeSO recipeSO;
        public float timer;

        public WaitingRecipe(RecipeSO recipeSO, float timer)
        {
            this.recipeSO = recipeSO;
            this.timer = timer;
        }
    }

    public static DeliveryManager Instance { get; private set; }

    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSucess;
    public event EventHandler OnRecipeFailed;

    [Header("Configuração de Fases")]
    [SerializeField] private List<PhaseConfig> phasesConfig;
    [SerializeField] private string currentPhaseName;

    [Header("Configuração de Pedidos")]
    [SerializeField] private int waitingRecipesMax = 4;
    [SerializeField] private float recipeMaxTime = 15f;
    [SerializeField] private int penaltyOnExpire = -5;

    private List<WaitingRecipe> waitingRecipeList = new List<WaitingRecipe>();
    private float spawnRecipeTimer;
    private int successfulRecipesAmount;
    private bool firstRecipeDelivered = false;
    private bool secondRecipeSpawned = false;
    private PhaseConfig currentPhase;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetPhase(currentPhaseName);
    }

    public void SetPhase(string phaseName)
    {
        currentPhaseName = phaseName;
        currentPhase = phasesConfig.Find(p => p.phaseName == phaseName);

        waitingRecipeList.Clear();
        firstRecipeDelivered = false;
        secondRecipeSpawned = false;

        if (currentPhase != null)
        {
            Debug.Log($"[DeliveryManager] Fase iniciada: {currentPhase.phaseName} | freezeTimerOnFirstRecipe = {currentPhase.freezeTimerOnFirstRecipe}");

            if (currentPhase.firstRecipe != null)
            {
                waitingRecipeList.Add(new WaitingRecipe(currentPhase.firstRecipe, recipeMaxTime));
                OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
                Debug.Log($"[DeliveryManager] Primeiro pedido: {currentPhase.firstRecipe.recipeName}");
            }

            if (currentPhase.freezeTimerOnFirstRecipe)
                KitchenGameManager.Instance.FreezePhaseTime();
            else
                KitchenGameManager.Instance.StartPhaseTime();

            spawnRecipeTimer = currentPhase.spawnInterval;
        }
        else
        {
            Debug.LogWarning($"Fase '{phaseName}' não encontrada na configuração do DeliveryManager!");
        }
    }

    private void Update()
    {
        if (currentPhase == null) return;

        // Se estiver congelado e o primeiro pedido ainda não foi entregue, pausa tudo.
        if (currentPhase.freezeTimerOnFirstRecipe && !firstRecipeDelivered)
            return;

        // Atualiza timers dos pedidos
        for (int i = waitingRecipeList.Count - 1; i >= 0; i--)
        {
            waitingRecipeList[i].timer -= Time.deltaTime;
            if (waitingRecipeList[i].timer <= 0f)
            {
                RecipeSO expiredRecipe = waitingRecipeList[i].recipeSO;
                waitingRecipeList.RemoveAt(i);

                ScoreManager.Instance?.AddScore(penaltyOnExpire);
                OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                OnRecipeFailed?.Invoke(this, EventArgs.Empty);

                Debug.Log($"[DeliveryManager] Pedido expirado: {expiredRecipe.recipeName}. Penalidade {penaltyOnExpire}");
            }
        }

        // Corrige o problema da terceira fase:
        // Se o primeiro pedido não foi entregue e não há freeze, o segundo pode surgir automaticamente.
        if (!secondRecipeSpawned)
        {
            spawnRecipeTimer -= Time.deltaTime;
            if (spawnRecipeTimer <= 0f)
            {
                if (waitingRecipeList.Count < waitingRecipesMax && currentPhase.availableRecipes.Count > 0)
                {
                    RecipeSO recipe = currentPhase.availableRecipes[UnityEngine.Random.Range(0, currentPhase.availableRecipes.Count)];
                    waitingRecipeList.Add(new WaitingRecipe(recipe, recipeMaxTime));
                    OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
                    Debug.Log($"[DeliveryManager] Segundo pedido gerado automaticamente: {recipe.recipeName}");
                }
                secondRecipeSpawned = true;
                spawnRecipeTimer = currentPhase.spawnInterval;
            }
        }
        // Depois do segundo, segue o ciclo normal de geração
        else
        {
            spawnRecipeTimer -= Time.deltaTime;
            if (spawnRecipeTimer <= 0f)
            {
                spawnRecipeTimer = currentPhase.spawnInterval;
                if (waitingRecipeList.Count < waitingRecipesMax && currentPhase.availableRecipes.Count > 0)
                {
                    RecipeSO recipe = currentPhase.availableRecipes[UnityEngine.Random.Range(0, currentPhase.availableRecipes.Count)];
                    waitingRecipeList.Add(new WaitingRecipe(recipe, recipeMaxTime));
                    OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
                    Debug.Log($"[DeliveryManager] Novo pedido gerado (3+): {recipe.recipeName}");
                }
            }
        }
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipeList.Count; i++)
        {
            WaitingRecipe waitingRecipe = waitingRecipeList[i];
            RecipeSO waitingRecipeSO = waitingRecipe.recipeSO;

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
                    if (!found)
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    successfulRecipesAmount++;
                    waitingRecipeList.RemoveAt(i);

                    ScoreManager.Instance?.AddScore(30);
                    OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                    OnRecipeSucess?.Invoke(this, EventArgs.Empty);

                    if (currentPhase.freezeTimerOnFirstRecipe && !firstRecipeDelivered &&
                        waitingRecipeSO == currentPhase.firstRecipe)
                    {
                        firstRecipeDelivered = true;
                        KitchenGameManager.Instance.StartPhaseTime();
                        Debug.Log("[DeliveryManager] Primeiro pedido entregue — fase descongelada.");
                    }

                    if (!secondRecipeSpawned)
                    {
                        if (waitingRecipeList.Count < waitingRecipesMax && currentPhase.availableRecipes.Count > 0)
                        {
                            RecipeSO recipe = currentPhase.availableRecipes[UnityEngine.Random.Range(0, currentPhase.availableRecipes.Count)];
                            waitingRecipeList.Add(new WaitingRecipe(recipe, recipeMaxTime));
                            OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
                            Debug.Log($"[DeliveryManager] Segundo pedido gerado imediatamente: {recipe.recipeName}");
                        }
                        secondRecipeSpawned = true;
                        spawnRecipeTimer = currentPhase.spawnInterval;
                    }

                    return;
                }
            }
        }

        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    public List<WaitingRecipe> GetWaitingRecipeList() => waitingRecipeList;
    public int GetSuccessfulRecipesAmount() => successfulRecipesAmount;
    public float GetRecipeMaxTime() => recipeMaxTime;
}
