using System.Collections;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Config")]
    public GameObject[] popUps;
    public float delayBeforeFirstPopUp = 1.5f;

    [Header("Input References")]
    [SerializeField] private GameInput keyboardInput;
    [SerializeField] private GameInput gamepadInput;

    private int popUpIndex;

    private void Start()
    {
        StartCoroutine(ShowFirstPopUpWithDelay());

        // Assinar eventos de input para o "Interact" de ambos os dispositivos
        keyboardInput.OnInteractAction += OnInteractInput;
        gamepadInput.OnInteractAction += OnInteractInput;
    }

    private void OnDestroy()
    {
        // Remover assinaturas de eventos para evitar memory leaks
        keyboardInput.OnInteractAction -= OnInteractInput;
        gamepadInput.OnInteractAction -= OnInteractInput;
    }

    private IEnumerator ShowFirstPopUpWithDelay()
    {
        yield return new WaitForSeconds(delayBeforeFirstPopUp);
        ShowCurrentPopUp();
    }

    private void OnInteractInput(object sender, GameInput.InputActionEventArgs e)
    {
        if (popUpIndex < popUps.Length)
        {
            AdvanceTutorial();
        }
    }

    private void AdvanceTutorial()
    {
        popUpIndex++;

        if (popUpIndex < popUps.Length)
        {
            ShowCurrentPopUp();
        }
        else
        {
            HideAllPopUps();
            // Notifica o KitchenGameManager que o tutorial foi concluído
            KitchenGameManager.Instance.CompleteTutorial();
        }
    }

    private void ShowCurrentPopUp()
    {
        for (int i = 0; i < popUps.Length; i++)
        {
            popUps[i].SetActive(i == popUpIndex);
        }
    }

    private void HideAllPopUps()
    {
        foreach (GameObject popUp in popUps)
        {
            popUp.SetActive(false);
        }
    }
}
