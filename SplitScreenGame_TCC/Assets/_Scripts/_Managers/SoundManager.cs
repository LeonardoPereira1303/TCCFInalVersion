using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";

    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    private float volume = 1f;

    private void Awake()
    {
        Instance = this;

        volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, 1f);
    }

    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSucess += DeliveryManager_OnRecipeSucess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
        BaseCounter.OnAnyObjectPlacedHere += BaseCounter_OnAnyObjectPlacedHere;
        TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
        StoveCounter.OnAnyStoveStateChanged += StoveCounter_OnAnyStoveStateChanged; // 🔥 novo evento

        // Localiza o Player na cena
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.OnPickedSomething += Player_OnPickedSomething;
        }
        else
        {
            Debug.LogWarning("Player não encontrado na cena.");
        }
    }

    private void TrashCounter_OnAnyObjectTrashed(object sender, EventArgs e)
    {
        TrashCounter trashCounter = sender as TrashCounter;
        PlaySound(audioClipRefsSO.trash, trashCounter.transform.position);
    }

    private void BaseCounter_OnAnyObjectPlacedHere(object sender, EventArgs e)
    {
        BaseCounter baseCounter = sender as BaseCounter;
        PlaySound(audioClipRefsSO.objectDrop, baseCounter.transform.position);
    }

    private void Player_OnPickedSomething(object sender, EventArgs e)
    {
        if (sender is Player player)
        {
            PlaySound(audioClipRefsSO.objectPickup, player.transform.position);
        }
    }

    private void CuttingCounter_OnAnyCut(object sender, EventArgs e)
    {
        CuttingCounter cuttingCounter = sender as CuttingCounter;
        PlaySound(audioClipRefsSO.repair, cuttingCounter.transform.position);
    }

    private void DeliveryManager_OnRecipeFailed(object sender, EventArgs e)
    {
        DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
        PlaySound(audioClipRefsSO.deliveryFail, deliveryCounter.transform.position);
    }

    private void DeliveryManager_OnRecipeSucess(object sender, EventArgs e)
    {
        DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
        PlaySound(audioClipRefsSO.deliverySuccess, deliveryCounter.transform.position);
    }

    // 🔥 Novo: som do fogão conforme estado
    private void StoveCounter_OnAnyStoveStateChanged(object sender, StoveCounter.OnAnyStoveStateChangedEventArgs e)
    {
        switch (e.state)
        {
            case StoveCounter.State.Frying:
                PlaySound(audioClipRefsSO.stoveFrying, e.position);
                break;

            case StoveCounter.State.Burned:
                PlaySound(audioClipRefsSO.stoveBurned, e.position);
                break;

            default:
                // Idle: sem som
                break;
        }
    }

    public void PlayDashSound(Vector3 position)
    {
        if (audioClipRefsSO.dash != null && audioClipRefsSO.dash.Length > 0)
        {
            AudioClip clip = audioClipRefsSO.dash[Random.Range(0, audioClipRefsSO.dash.Length)];
            AudioSource.PlayClipAtPoint(clip, position);
        }
    }

    public void PlayCountdownSound(Vector3 position)
    {
        if (audioClipRefsSO.countdown != null)
        {
            AudioSource.PlayClipAtPoint(audioClipRefsSO.countdown, position);
        }
    }

    private void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volume = 1f)
    {
        if (audioClipArray == null || audioClipArray.Length == 0) return;
        PlaySound(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volume);
    }

    private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
    {
        if (audioClip == null) return;
        AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplier * volume);
    }

    public void PlayTimerHalfwaySound(Vector3 position, float volume = 0.2f)
    {
        if (audioClipRefsSO.timerHalfway != null && audioClipRefsSO.timerHalfway.Length > 0)
        {
            AudioClip clip = audioClipRefsSO.timerHalfway[UnityEngine.Random.Range(0, audioClipRefsSO.timerHalfway.Length)];
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }

    public void PlayTimerFinalSound(Vector3 position, float volume = 0.2f)
    {
        if (audioClipRefsSO.timerFinal != null && audioClipRefsSO.timerFinal.Length > 0)
        {
            AudioClip clip = audioClipRefsSO.timerFinal[UnityEngine.Random.Range(0, audioClipRefsSO.timerFinal.Length)];
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }

    public void ChangeVolume()
    {
        volume += .1f;
        if (volume > 1f)
            volume = 0f;

        PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return volume;
    }
}
