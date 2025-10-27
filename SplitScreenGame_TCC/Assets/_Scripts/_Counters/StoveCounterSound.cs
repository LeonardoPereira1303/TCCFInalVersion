using System;
using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{
    [SerializeField] private StoveCounter stoveCounter;
    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true; // som contínuo para o fritar
    }

    private void Start()
    {
        stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
    }

    private void StoveCounter_OnStateChanged(object sender, StoveCounter.OnStateChangedEventArgs e)
    {
        switch (e.state)
        {
            case StoveCounter.State.Frying:
                PlayLoop(audioClipRefsSO.stoveFrying); // som contínuo de fritura
                break;

            case StoveCounter.State.Fried:
                PlayLoop(audioClipRefsSO.stoveFrying); // som contínuo de fritura
                break;

            case StoveCounter.State.Burned:
                StopLoop();
                PlayRandom(audioClipRefsSO.warning); // som aleatório de queimado
                break;

            default:
                StopLoop();
                break;
        }
    }

    private void PlayLoop(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.clip = clip;
        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    private void StopLoop()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
    }

    private void PlayOneShot(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip clip = clips[UnityEngine.Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    private void PlayRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip clip = clips[UnityEngine.Random.Range(0, clips.Length)];
        AudioSource.PlayClipAtPoint(clip, transform.position);
    }
}
