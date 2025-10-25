using System.Collections;
using UnityEngine;
using System;

public class TeleportClearCounter : ClearCounter
{
    public static event EventHandler OnAnyTeleport;

    [Tooltip("Tempo em segundos para o objeto ser teletransportado")]
    [SerializeField] private float teleportDelay = 3f;

    [Tooltip("Bancada de destino para onde o objeto será teletransportado")]
    [SerializeField] private BaseCounter targetCounter;

    [Header("VFX")]
    [SerializeField] private GameObject teleportVFXPrefab; // Prefab do efeito (ex: CFX_Portal, CFX_MagicPoof)
    [SerializeField] private Transform vfxSpawnPoint; // Local opcional de spawn
    [SerializeField] private float vfxDuration = 1.2f; // Duração do efeito
    [SerializeField] private float vfxScale = 0.6f; // Escala reduzida do efeito

    [Header("SFX")]
    [SerializeField] private AudioClip teleportSound; // Som do teletransporte
    [SerializeField] private float teleportSoundVolume = 0.7f; // Volume do som

    private Coroutine teleportCoroutine;

    public override void Interact(Player player)
    {
        bool playerHadObject = player.HasKitchenObject();

        // Executa a interação padrão (coloca ou pega o item)
        base.Interact(player);

        // Se o jogador colocou um item na bancada
        if (playerHadObject && HasKitchenObject())
        {
            // VFX no momento que o item é colocado
            SpawnTeleportVFX();

            // Inicia a coroutine do teleporte
            if (teleportCoroutine == null)
            {
                teleportCoroutine = StartCoroutine(TeleportAfterDelay());
            }
        }
    }

    private IEnumerator TeleportAfterDelay()
    {
        yield return new WaitForSeconds(teleportDelay);

        if (HasKitchenObject() && targetCounter != null && !targetCounter.HasKitchenObject())
        {
            KitchenObject kitchenObject = GetKitchenObject();
            kitchenObject.SetKitchenObjectParent(targetCounter);

            // VFX de teleporte na origem e destino
            SpawnTeleportVFX();

            if (targetCounter is TeleportClearCounter destinationTeleport)
            {
                destinationTeleport.SpawnTeleportVFX();
            }

            // 🎵 Tocar som de teletransporte
            PlayTeleportSound();

            OnAnyTeleport?.Invoke(this, EventArgs.Empty);
        }

        teleportCoroutine = null;
    }

    private void SpawnTeleportVFX()
    {
        if (teleportVFXPrefab == null) return;

        Vector3 spawnPos = vfxSpawnPoint != null ? vfxSpawnPoint.position : transform.position + Vector3.up * 0.7f;
        GameObject vfxInstance = Instantiate(teleportVFXPrefab, spawnPos, Quaternion.identity);
        vfxInstance.transform.localScale *= vfxScale;
        Destroy(vfxInstance, vfxDuration);
    }

    private void PlayTeleportSound()
    {
        if (teleportSound != null)
        {
            Vector3 soundPos = vfxSpawnPoint != null ? vfxSpawnPoint.position : transform.position;
            AudioSource.PlayClipAtPoint(teleportSound, soundPos, teleportSoundVolume);
        }
    }
}
