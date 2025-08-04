using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Portal destinationPortal;
    [SerializeField] private Canvas portalInteractCanvas;
    [SerializeField] private float detectionDistance = 1.5f;
    public float teleportOffset = 2.0f;

    private bool isTeleporting = false;

    private void Update()
    {
        Player[] allPlayers = FindObjectsOfType<Player>();

        bool anyPlayerInRange = false;

        foreach (Player player in allPlayers)
        {
            float dist = Vector3.Distance(player.transform.position, transform.position);

            if (dist < detectionDistance && !isTeleporting)
            {
                anyPlayerInRange = true;
            }
        }

        if (portalInteractCanvas != null)
        {
            if (portalInteractCanvas.gameObject.activeSelf != anyPlayerInRange)
            {
                portalInteractCanvas.gameObject.SetActive(anyPlayerInRange);
            }
        }
    }

    public void TryInteractTeleport(Player player)
    {
        if (isTeleporting)
        {
            Debug.Log("Portal: Em cooldown, não pode teleportar.");
            return;
        }

        float dist = Vector3.Distance(player.transform.position, transform.position);
        if (dist < detectionDistance)
        {
            TeleportPlayer(player);
        }
    }

    private void TeleportPlayer(Player player)
    {
        isTeleporting = true;
        if (destinationPortal != null)
            destinationPortal.StartTeleportCooldown();

        Vector3 newPos = destinationPortal.transform.position + (destinationPortal.transform.forward * teleportOffset);
        newPos.y = player.transform.position.y;
        player.transform.position = newPos;

        StartTeleportCooldown();
    }

    public void StartTeleportCooldown()
    {
        isTeleporting = true;
        StartCoroutine(ResetTeleportCooldown());
    }

    private IEnumerator ResetTeleportCooldown()
    {
        yield return new WaitForSeconds(0.5f);
        isTeleporting = false;
    }
}