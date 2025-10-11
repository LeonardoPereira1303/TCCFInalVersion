using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextAnim : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] float timeBtwChars;
    [SerializeField] float timeBtwWords;

    public string[] storyTexts;
    int i = 0;

    void Start()
    {
        EndCheck();
    }

    private IEnumerator TextVisible()
    {
        textMeshPro.ForceMeshUpdate();
        int totalVisibleCharacters = textMeshPro.textInfo.characterCount; // Get # of Visible Character in text object
        int counter = 0;

        while (true)
        {
            int visibleCount = counter % (totalVisibleCharacters + 1);
            textMeshPro.maxVisibleCharacters = visibleCount;

            if (visibleCount >= totalVisibleCharacters)
            {
                i += 1;
                Invoke("EndCheck", timeBtwWords);
                break;
            }
            counter += 1;
            yield return new WaitForSeconds(timeBtwChars);
        }
    }

    public void EndCheck()
    {
        if(i<=storyTexts.Length-1)
        {
            textMeshPro.text = storyTexts[i];
            StartCoroutine(TextVisible());
        }
    }
}
