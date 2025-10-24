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
        Time.timeScale = 1f;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        loadOperation.allowSceneActivation = false;

        float elapsedTime = 0f;

        // Usa unscaledDeltaTime para ignorar pausas
        while (elapsedTime < fakeLoadTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            loadingSlider.value = Mathf.Clamp01(elapsedTime / fakeLoadTime);
            yield return null;
        }

        loadOperation.allowSceneActivation = true;

    }

    public void ExitGame()
    {
        Debug.Log("[AsyncLoader] Saindo do jogo...");
        Application.Quit();
    }
}
