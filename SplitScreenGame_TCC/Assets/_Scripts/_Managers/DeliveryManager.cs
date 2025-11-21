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
    [SerializeField] private Animator recipeAlertAnimator;

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
        OnRecipeSpawned += (sender, e) =>
        {
            Debug.Log("TRIGGER DISPARADO: AlertNewRecipe");
            recipeAlertAnimator.SetTrigger("AlertNewRecipe");
        };

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

        if (KitchenGameManager.Instance.IsGameOver() || KitchenGameManager.Instance.IsGamePaused())
            return;

        // 🔒 Impede contagem e spawn antes do jogo começar (antes do Countdown terminar)
        if (!KitchenGameManager.Instance.IsGamePlaying())
            return;

        // 🧊 Se a fase estiver congelada e o primeiro pedido ainda não foi entregue, pausa tudo.
        if (currentPhase.freezeTimerOnFirstRecipe && !firstRecipeDelivered)
            return;

        // ⏱️ Atualiza timers dos pedidos
        for (int i = waitingRecipeList.Count - 1; i >= 0; i--)
        {
            WaitingRecipe wr = waitingRecipeList[i];
            wr.timer -= Time.deltaTime;

            // 🔔 ALERTA DE TEMPO BAIXO
            if (wr.timer <= 5f && wr.timer + Time.deltaTime > 5f)
            {
                // Toca só UMA VEZ por pedido
                recipeAlertAnimator.SetTrigger("AlertLosingRecipe");
            }

            if (wr.timer <= 0f)
            {
                RecipeSO expiredRecipe = wr.recipeSO;
                waitingRecipeList.RemoveAt(i);

                ScoreManager.Instance?.AddScore(penaltyOnExpire);
                OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                OnRecipeFailed?.Invoke(this, EventArgs.Empty);

                Debug.Log($"[DeliveryManager] Pedido expirado: {expiredRecipe.recipeName}. Penalidade {penaltyOnExpire}");
            }
        }

        // 🧮 Controle de spawn dos próximos pedidos
        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = currentPhase.spawnInterval;

            // ✅ Agora, se o freezeTimer estiver desativado, novos pedidos aparecem mesmo sem entregar o primeiro
            if (waitingRecipeList.Count < waitingRecipesMax && currentPhase.availableRecipes.Count > 0)
            {
                RecipeSO recipe = currentPhase.availableRecipes[UnityEngine.Random.Range(0, currentPhase.availableRecipes.Count)];
                waitingRecipeList.Add(new WaitingRecipe(recipe, recipeMaxTime));
                OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
                Debug.Log($"[DeliveryManager] Novo pedido gerado: {recipe.recipeName}");
            }

            // Marcação para o segundo pedido — usada apenas se for necessário manter lógica antiga
            if (!secondRecipeSpawned)
                secondRecipeSpawned = true;
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
