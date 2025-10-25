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

    private Coroutine teleportCoroutine;

    public override void Interact(Player player)
    {
        // Verifica se o jogador está segurando algo
        bool playerHasObject = player.HasKitchenObject();

        // Chama a lógica padrão do ClearCounter (colocar ou pegar objeto)
        base.Interact(player);

        // Se o jogador estava segurando algo e colocou o item na bancada,
        // agora a bancada passa a ter um KitchenObject
        if (playerHasObject && HasKitchenObject())
        {
            // Ativar VFX no momento que o item é colocado
            SpawnTeleportVFX();

            // Inicia a contagem para o teleporte (como antes)
            if (teleportCoroutine == null)
            {
                teleportCoroutine = StartCoroutine(TeleportAfterDelay());
            }
        }
    }

    private IEnumerator TeleportAfterDelay()
    {
        yield return new WaitForSeconds(teleportDelay);

        // Teleporta o objeto, se ainda existir e o destino estiver livre
        if (HasKitchenObject() && targetCounter != null && !targetCounter.HasKitchenObject())
        {
            KitchenObject kitchenObject = GetKitchenObject();
            kitchenObject.SetKitchenObjectParent(targetCounter);

            // VFX nas duas bancadas: origem e destino
            SpawnTeleportVFX();

            if (targetCounter is TeleportClearCounter destinationTeleport)
            {
                destinationTeleport.SpawnTeleportVFX();
            }

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
}
