using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuLoader : MonoBehaviour
{
    [Header("Loading Settings")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject mainMenu;

    [Header("Loading UI")]
    [SerializeField] private Slider loadingSlider;

    [Header("Configuração de Loading")]
    [SerializeField] private float fakeLoadTime = 5f; // tempo fixo para a barra encher

    public void LoadLevelButton(string levelToLoad)
    {
        mainMenu.SetActive(false);
        loadingScreen.SetActive(true);

        StartCoroutine(LoadLevelAsync(levelToLoad));
    }

    private IEnumerator LoadLevelAsync(string levelToLoad)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        loadOperation.allowSceneActivation = false; // só libera quando a barra terminar

        float elapsedTime = 0f;

        // Simula a barra enchendo em tempo fixo
        while (elapsedTime < fakeLoadTime)
        {
            elapsedTime += Time.deltaTime;
            loadingSlider.value = Mathf.Clamp01(elapsedTime / fakeLoadTime);
            yield return null;
        }

        // Ao terminar, ativa a cena
        loadOperation.allowSceneActivation = true;
    }

    public void ExitGame()
    {
        Debug.Log("[AsyncLoader] Saindo do jogo...");
        Application.Quit();
    }
}
