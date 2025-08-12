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

        keyboardInput.OnInteractAction += OnInteractInput;
        gamepadInput.OnInteractAction += OnInteractInput;
    }

    private void OnDestroy()
    {
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
