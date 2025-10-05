using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialManagerDinamico : MonoBehaviour {
    public static TutorialManagerDinamico Instance { get; private set; }

    [Header("Configuração de Passos")]
    [SerializeField] private List<TutorialStep> steps;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI tutorialTextPlayer1;
    [SerializeField] private TextMeshProUGUI tutorialTextPlayer2;

    private int currentStep = 0;
    private HighlightableCounter highlightedCounter;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() {
        ContainerCounter.OnAnyContainerUsed += HandleContainerUsed;
        CuttingCounter.OnAnyCut += HandleCuttingUsed;
        StoveCounter.OnAnyCook += HandleStoveUsed;
        DeliveryManager.Instance.OnRecipeSucess += HandleRecipeDelivered;

        ShowStepInstruction();
    }

    private void OnDestroy() {
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

            if (tutorialTextPlayer1 != null)
                tutorialTextPlayer1.text = step.instruction;

            if (tutorialTextPlayer2 != null)
                tutorialTextPlayer2.text = step.instruction;

            HighlightCounter(step.highlightTarget);
        }
    }


    private void HighlightCounter(HighlightableCounter counter) {
        if (highlightedCounter != null) highlightedCounter.DisableHighlight();
        highlightedCounter = counter;
        if (highlightedCounter != null) highlightedCounter.EnableHighlight();
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
                // Quando termina o tutorial
                if (tutorialTextPlayer1 != null)
                    tutorialTextPlayer1.text = "Tutorial concluído!";
                if (tutorialTextPlayer2 != null)
                    tutorialTextPlayer2.text = "Tutorial concluído!";

                HighlightCounter(null);
                KitchenGameManager.Instance.CompleteTutorial();
                return;
            }

            // Atualiza instrução dos dois jogadores
            if (tutorialTextPlayer1 != null)
                tutorialTextPlayer1.text = steps[currentStep].instruction;
            if (tutorialTextPlayer2 != null)
                tutorialTextPlayer2.text = steps[currentStep].instruction;

            ShowStepInstruction();
        }
    }


    private void HandleContainerUsed(object sender, EventArgs e) => CompleteStep(TutorialStep.StepType.Container);
    private void HandleCuttingUsed(object sender, EventArgs e) => CompleteStep(TutorialStep.StepType.Cutting);
    private void HandleStoveUsed(object sender, EventArgs e) => CompleteStep(TutorialStep.StepType.Stove);
    private void HandleRecipeDelivered(object sender, EventArgs e) => CompleteStep(TutorialStep.StepType.Delivery);
}
