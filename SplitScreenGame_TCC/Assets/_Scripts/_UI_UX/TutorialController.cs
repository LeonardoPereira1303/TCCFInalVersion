using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class TutorialController : MonoBehaviour
{
    [Header("Level Selection")]
    public string _GameLevel;

    public void StartGame()
    {
        SceneManager.LoadScene(_GameLevel);
    }

    public void ExitTutorial()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
