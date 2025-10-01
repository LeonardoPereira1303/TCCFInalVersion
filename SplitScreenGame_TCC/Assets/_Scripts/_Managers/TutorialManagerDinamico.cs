using System;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManagerDinamico : MonoBehaviour
{
    public static TutorialManagerDinamico Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Text tutorialText;

    private int currentStep = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Debug.Log("[Tutorial] Iniciado");

        // Inscrever eventos globais
        ContainerCounter.OnAnyContainerUsed += HandleContainerUsed;
        CuttingCounter.OnAnyCut += HandleCuttingOrStoveUsed;
        StoveCounter.OnAnyCook += HandleCuttingOrStoveUsed;
        DeliveryManager.Instance.OnRecipeSucess += HandleRecipeDelivered;

        ShowStepInstruction();
    }

    private void OnDestroy()
    {
        ContainerCounter.OnAnyContainerUsed -= HandleContainerUsed;
        CuttingCounter.OnAnyCut -= HandleCuttingOrStoveUsed;
        StoveCounter.OnAnyCook -= HandleCuttingOrStoveUsed;
        if (DeliveryManager.Instance != null)
            DeliveryManager.Instance.OnRecipeSucess -= HandleRecipeDelivered;
    }

    private void ShowStepInstruction()
    {
        switch (currentStep)
        {
            case 0:
                SetText("Pegue um ingrediente no Container.");
                break;
            case 1:
                SetText("Leve o ingrediente até a bancada de corte ou fogão.");
                break;
            case 2:
                SetText("Agora entregue o prato no balcão de Delivery.");
                break;
            case 3:
                SetText("Parabéns! Tutorial concluído!");
                break;
        }
    }

    private void SetText(string msg)
    {
        if (tutorialText != null)
            tutorialText.text = msg;
    }

    private void HandleContainerUsed(object sender, EventArgs e)
    {
        if (currentStep == 0)
        {
            Debug.Log("[Tutorial] Ingrediente coletado!");
            NextStep();
        }
    }

    private void HandleCuttingOrStoveUsed(object sender, EventArgs e)
    {
        if (currentStep == 1)
        {
            Debug.Log("[Tutorial] Ingrediente preparado!");
            NextStep();
        }
    }

    private void HandleRecipeDelivered(object sender, EventArgs e)
    {
        if (currentStep == 2)
        {
            Debug.Log("[Tutorial] Pedido entregue!");
            NextStep();
            KitchenGameManager.Instance.CompleteTutorial();
        }
    }

    private void NextStep()
    {
        currentStep++;
        ShowStepInstruction();
    }
}
