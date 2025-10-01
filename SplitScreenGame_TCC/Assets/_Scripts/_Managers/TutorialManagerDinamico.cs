using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManagerDinamico : MonoBehaviour
{
    public static TutorialManagerDinamico Instance { get; private set; }

    [Header("Configuração de Passos")]
    [SerializeField] private List<TutorialStep> steps; // lista configurável no inspetor

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
        CuttingCounter.OnAnyCut += HandleCuttingUsed;
        StoveCounter.OnAnyCook += HandleStoveUsed;
        DeliveryManager.Instance.OnRecipeSucess += HandleRecipeDelivered;

        ShowStepInstruction();
    }

    private void OnDestroy()
    {
        ContainerCounter.OnAnyContainerUsed -= HandleContainerUsed;
        CuttingCounter.OnAnyCut -= HandleCuttingUsed;
        StoveCounter.OnAnyCook -= HandleStoveUsed;
        if (DeliveryManager.Instance != null)
            DeliveryManager.Instance.OnRecipeSucess -= HandleRecipeDelivered;
    }

    private void ShowStepInstruction()
    {
        if (currentStep < steps.Count)
        {
            var step = steps[currentStep];
            if (tutorialText != null)
                tutorialText.text = step.instruction;
        }
    }

    private void CompleteStep(TutorialStep.StepType type)
    {
        if (currentStep >= steps.Count) return;

        if (steps[currentStep].stepType == type)
        {
            Debug.Log($"[Tutorial] Passo {type} concluído!");
            currentStep++;
            if (currentStep >= steps.Count)
            {
                Debug.Log("[Tutorial] Finalizado!");
                if (tutorialText != null)
                    tutorialText.text = "Tutorial concluído!";

                KitchenGameManager.Instance.CompleteTutorial();
                return;
            }
            ShowStepInstruction();
        }
    }

    private void HandleContainerUsed(object sender, EventArgs e)
    {
        CompleteStep(TutorialStep.StepType.Container);
    }

    private void HandleCuttingUsed(object sender, EventArgs e)
    {
        CompleteStep(TutorialStep.StepType.Cutting);
    }

    private void HandleStoveUsed(object sender, EventArgs e)
    {
        CompleteStep(TutorialStep.StepType.Stove);
    }

    private void HandleRecipeDelivered(object sender, EventArgs e)
    {
        CompleteStep(TutorialStep.StepType.Delivery);
    }
}
