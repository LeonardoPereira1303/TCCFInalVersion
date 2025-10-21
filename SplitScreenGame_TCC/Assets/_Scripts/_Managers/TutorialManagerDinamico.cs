using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialManagerDinamico : MonoBehaviour {
    public static TutorialManagerDinamico Instance { get; private set; }

    [SerializeField] private ObjectiveArrowSingle arrowPointer;

    //[SerializeField] private WorldArrowPointer arrowPointer;

    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;


    [Header("Configuração de Passos")]
    [SerializeField] private List<TutorialStep> steps;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI tutorialTextPlayer1;
    [SerializeField] private Image tutorialImage;

    private int currentStep = 0;
    private HighlightableCounter highlightedCounter;
    private bool tutorialActive = false;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Começa desativado
        SetTutorialActive(false);
    }

    private void Start() {
        // Aguarda o jogo começar
        KitchenGameManager.Instance.OnStateChanged += HandleGameStateChanged;

        // Eventos de interação
        ContainerCounter.OnAnyContainerUsed += HandleContainerUsed;
        CuttingCounter.OnAnyCut += HandleCuttingUsed;
        StoveCounter.OnAnyCook += HandleStoveUsed;
        DeliveryManager.Instance.OnRecipeSucess += HandleRecipeDelivered;
        BaseCounter.OnAnyCounterInteracted += HandleCounterInteracted;
        TeleportClearCounter.OnAnyTeleport += HandleTeleportUsed;
    }

    private void OnDestroy() {
        if (KitchenGameManager.Instance != null)
            KitchenGameManager.Instance.OnStateChanged -= HandleGameStateChanged;

        ContainerCounter.OnAnyContainerUsed -= HandleContainerUsed;
        CuttingCounter.OnAnyCut -= HandleCuttingUsed;
        StoveCounter.OnAnyCook -= HandleStoveUsed;
        DeliveryManager.Instance.OnRecipeSucess -= HandleRecipeDelivered;
        BaseCounter.OnAnyCounterInteracted -= HandleCounterInteracted;
        TeleportClearCounter.OnAnyTeleport -= HandleTeleportUsed;

    }

    private void SetTutorialActive(bool active) {
        tutorialActive = active;

        if (tutorialTextPlayer1 != null)
            tutorialTextPlayer1.gameObject.SetActive(active);

        if (tutorialImage != null)
            tutorialImage.gameObject.SetActive(active);

        if (!active && highlightedCounter != null)
            highlightedCounter.DisableHighlight();
    }

    private void HandleGameStateChanged(object sender, EventArgs e) {
        if (KitchenGameManager.Instance.IsGamePlaying()) {
            Debug.Log("[Tutorial] Contagem regressiva finalizada — iniciando tutorial dinâmico.");
            StartTutorial();
        }
    }

    private void StartTutorial() {
        if (tutorialActive) return;

        currentStep = 0;
        SetTutorialActive(true);
        ShowStepInstruction();
    }

    private void ShowStepInstruction() {
        if (!tutorialActive || currentStep >= steps.Count)
            return;

        var step = steps[currentStep];

        if (tutorialTextPlayer1 != null)
            tutorialTextPlayer1.text = step.instruction;

        if (tutorialImage != null)
            tutorialImage.sprite = step.image;

        HighlightCounter(step.highlightTarget);
    }

    private void HighlightCounter(HighlightableCounter counter)
    {
        if (highlightedCounter != null)
            highlightedCounter.DisableHighlight();

        highlightedCounter = counter;

        if (highlightedCounter != null)
        {
            highlightedCounter.EnableHighlight();

            if (arrowPointer != null)
                arrowPointer.SetTarget(highlightedCounter.transform);
        }
        else
        {
            if (arrowPointer != null)
                arrowPointer.SetTarget(null);
        }
    }

    private void CompleteStep(TutorialStep.StepType type) {
        if (!tutorialActive || currentStep >= steps.Count)
            return;

        if (steps[currentStep].stepType == type) {
            Debug.Log($"[Tutorial] Passo {type} concluído!");
            currentStep++;

            if (currentStep >= steps.Count) {
                if (tutorialTextPlayer1 != null)
                    tutorialTextPlayer1.text = "Tutorial concluído!";

                if (tutorialImage != null)
                    tutorialImage.sprite = null;

                HighlightCounter(null);
                SetTutorialActive(false);
                return;
            }

            ShowStepInstruction();
        }
    }

    // Handlers dos eventos de passos
    private void HandleContainerUsed(object sender, EventArgs e) => CompleteStep(TutorialStep.StepType.Container);
    private void HandleCuttingUsed(object sender, EventArgs e) => CompleteStep(TutorialStep.StepType.Cutting);
    private void HandleStoveUsed(object sender, EventArgs e) => CompleteStep(TutorialStep.StepType.Stove);
    private void HandleRecipeDelivered(object sender, EventArgs e) => CompleteStep(TutorialStep.StepType.Delivery);
    private void HandleCounterInteracted(object sender, EventArgs e) => CompleteStep(TutorialStep.StepType.Counter);
    private void HandleTeleportUsed(object sender, EventArgs e) => CompleteStep(TutorialStep.StepType.Teleport);

}
